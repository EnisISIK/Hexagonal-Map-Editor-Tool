using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;

public class ChunkDataGenerator
{
    //TODO: Turn 3D arrays to flat arrays. Extra calculation logic needed.

    private World _world;

    public DomainWarping domainWarping;

    private readonly int biomeScale = 4;

    public ChunkDataGenerator(World world)
    {
		_world = world;
    }


    public ChunkDataGenerator(World world, DomainWarping domainWarping, int biomeScale)
    {
        _world = world;
        this.domainWarping = domainWarping;
        this.biomeScale = biomeScale;
    }


    //Generates the shape with either a stone or air block based on the block's biome height
    private byte[,,] GenerateShape(Vector3Int chunkPos, Dictionary<Vector3Int, VoronoiSeed> biomeCenters,System.Action<BiomeSelector[,]> callback)
    {
        byte[,,] tempData = new byte[HexData.ChunkWidth, HexData.ChunkHeight, HexData.ChunkWidth];
        BiomeSelector[,] biomeSelectionData = new BiomeSelector[HexData.ChunkWidth, HexData.ChunkWidth];
        
        for (int x = 0; x < HexData.ChunkWidth; x++)
        {
            for (int z = 0; z < HexData.ChunkWidth; z++)
            {
                BiomeSelector biomeSelection = SelectBiomeAttributes(new Vector3Int(chunkPos.x + x, 0, chunkPos.z + z), biomeCenters);
                
                biomeSelectionData[x, z] = biomeSelection;
                for (int y = 0; y < HexData.ChunkHeight; y++)
                {
                    
                    byte id = (byte)((y + chunkPos.y > biomeSelection.terrainSurfaceNoise.Value) ? 0 : 1);

                    tempData[x, y, z] = id;
                }
            }
        }

        callback(biomeSelectionData);
        return tempData;
    }


    //Converts stone blocks to biome blocks
    private byte[,,] ComposeTerrain(Vector3 chunkPos, byte[,,] tempData, BiomeSelector[,] biomeSelectionData)
    {
        for (int x = 0; x < HexData.ChunkWidth; x++)
        {
            for (int z = 0; z < HexData.ChunkWidth; z++)
            {
                BiomeSelector biomeSelector = biomeSelectionData[x, z];
                BiomeAttributes biome = biomeSelector.biomeAttributes;
                int terrainHeight = biomeSelector.terrainSurfaceNoise.Value;
                for (int y = 0; y < HexData.ChunkHeight; y++)
                {
                    byte block = tempData[x,y,z];
                    if (block == 1)
                    {
                        if(y == terrainHeight)
                        {
                            tempData[x, y, z] = biome.surfaceBlock;
                        }
                        else if (y < terrainHeight && y > terrainHeight-4)
                        {
                            tempData[x, y, z] = biome.subSurfaceBlock;
                        }
                        else
                        {
                            foreach (Lode lode in biome.lodes)
                            {
                                if (y > lode.minHeight && y < lode.maxHeight)
                                {
                                    if (Noise.Get3DPerlin(new Vector3(x, y, z) + chunkPos, lode.noiseOffset, lode.scale, lode.threshold))
                                    {
                                        tempData[x, y, z] = lode.blockID;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return tempData;
    }


    //Adds structure to surface blocks
    private byte[,,] AddFinishGen(Vector3 chunkPos, byte[,,] tempData, BiomeSelector[,] biomeSelectionData)  //tree değil de string ve biome olarak yap
    {

        ConcurrentQueue<HexMod> teststructure = Structure.GenerateMajorFlora(5, chunkPos, 0, 0);
        while (teststructure.Count > 0)
        {
            teststructure.TryDequeue(out HexMod block);
            int x = (int) block.position.x;
            int z = (int)block.position.z;
            BiomeSelector biomeSelector = biomeSelectionData[x, z];
            BiomeAttributes biome = biomeSelector.biomeAttributes;
            int terrainHeight = biomeSelector.terrainSurfaceNoise.Value;
            for (int y = 0; y < HexData.ChunkHeight; y++)
            {
                if (y != terrainHeight) continue;
                if (y == terrainHeight && biome.placeFlora)
                {
                    if (tempData[x, y + 1, z] != 0) continue;

                    tempData[x, y+1, z] = block.id;

                    float flowerThreshold = Noise.Get2DPerlin(new Vector2(x, z), 0f, 10f);

                    if (flowerThreshold > 0.2f && flowerThreshold < 0.3f) tempData[x, y + 1, z] = 19;
                    else if(flowerThreshold>0.5f&& flowerThreshold < 0.6f) tempData[x, y + 1, z] = 20;
                    else if(flowerThreshold > 0.7f&& flowerThreshold < 0.8f) tempData[x, y + 1, z] = 21;
                }
            }

        }
        for (int x = 0; x < HexData.ChunkWidth; x++)
        {
            for (int z = 0; z < HexData.ChunkWidth; z++)
            {
                BiomeSelector biomeSelector = biomeSelectionData[x, z];
                BiomeAttributes biome = biomeSelector.biomeAttributes;
                int terrainHeight = biomeSelector.terrainSurfaceNoise.Value;
                for (int y = 0; y < HexData.ChunkHeight; y++)
                {
                    if (y != terrainHeight) continue;
                    if (y == terrainHeight && biome.placeFlora)
                    {
                        Vector2 pos = new Vector2(x + chunkPos.x, z + chunkPos.z);
                        if (Noise.Get2DPerlin(pos, 0, biome.floraZoneScale) > biome.floraZoneThreshold)
                        {
                            if (Noise.Get2DPerlin(pos, 0, biome.floraPlacementScale) > biome.floraPlacementThreshold)
                            {
                                ConcurrentQueue<HexMod> queue = new ConcurrentQueue<HexMod>();
                                ConcurrentQueue<HexMod> structure = Structure.GenerateMajorFlora(biome.floraIndex, new Vector3(x,0,z), biome.minFloraHeight, biome.maxFloraHeight);
                                while (structure.Count > 0)
                                {
                                    structure.TryDequeue(out HexMod mod);
                                    int modX = (int) mod.position.x;
                                    int modY = (int) mod.position.y;
                                    int modZ = (int) mod.position.z;

                                    if (modX > 0 && modX < HexData.ChunkWidth &&
                                        y + modY > 0 && y + modY < HexData.ChunkHeight &&
                                        modZ > 0 && modZ < HexData.ChunkWidth)
                                        tempData[modX, y + modY, modZ] = mod.id;
                                    else
                                        queue.Enqueue(new HexMod(new Vector3(modX + chunkPos.x, y+modY ,  modZ + chunkPos.z), mod.id));
                                }
                                _world.finishersToAdd.Enqueue(queue);
                            }
                        }
                    }
                }
            }
        }

        return tempData;
    }


    //Generates terrain for single chunk
    public IEnumerator GenerateData(Vector3Int chunkPos, Dictionary<Vector3Int, VoronoiSeed> biomeCenters, System.Action<byte[,,]> callback)
    {
        byte[,,] tempData2 = null;

        Task t = Task.Factory.StartNew(delegate
        {
            BiomeSelector[,] biomeSelectionData = null;

            tempData2 = GenerateShape(chunkPos, biomeCenters, x => biomeSelectionData = x);

            tempData2 = ComposeTerrain(chunkPos, tempData2, biomeSelectionData);

            tempData2 = AddFinishGen(chunkPos, tempData2, biomeSelectionData);

        });

        yield return new WaitUntil(() =>
        {
            return t.IsCompleted;
        });

        if (t.Exception != null)
        {
            Debug.LogError(t.Exception);
        }

        callback(tempData2);
    }


    //Selects biome for each block
    private BiomeSelector SelectBiomeAttributes(Vector3Int position, Dictionary<Vector3Int, VoronoiSeed> biomeCenters)
    {

        int gridX = Mathf.FloorToInt(position.x / 64);
        int gridZ = Mathf.FloorToInt(position.z / 64);

        float nearestDistance = Mathf.Infinity;
        Vector3Int nearestPoint = new Vector3Int();

        int distortedX = position.x + Noise.Map01Int(0, 16, Mathf.PerlinNoise(position.x * 0.1f, position.z * 0.1f));
        int distortedZ = position.z + Noise.Map01Int(0, 16, Mathf.PerlinNoise(position.x * 0.1f, position.z * 0.1f));

        Dictionary<Vector3Int, float> distDict = new Dictionary<Vector3Int, float>();
        for (int a = -1; a < 2; a++)
        {
            for (int b = -1; b < 2; b++)
            {

                int i = gridX + a;
                int j = gridZ + b;

                Vector3Int currentVoronoiSeed = new Vector3Int(i, 0, j);

                if (biomeCenters.TryGetValue(currentVoronoiSeed, out VoronoiSeed var))
                {
                    float distance = Vector3Int.Distance(new Vector3Int(distortedX, 0, distortedZ), var.voronoiPosition);

                    distDict.Add(currentVoronoiSeed, distance);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestPoint = currentVoronoiSeed;
                    }
                }

            }
        }

        int closestBiomeReach = 3;
        var closestBiomes = distDict.OrderBy(pair => pair.Value).Take(closestBiomeReach).ToDictionary(pair => pair.Key, pair => pair.Value);
        BiomeAttributes secondClosestBiome = null;

        bool hasLandBiome = false;
        bool hasOceanBiome = false;

        float totalWeight = 0f;
        float weight = 0;

        float terrainHeightfloat = 0;
        int count = 0;
        foreach (var localDistance in closestBiomes)
        {
            count++;
            if (localDistance.Value == 0f)
            {
                weight = 1f;
                totalWeight += 1f;
            }
            else
            {
                weight = 1f / (localDistance.Value * localDistance.Value);
                totalWeight += weight;
            }

            BiomeAttributes attributes0 = null;

            if (!biomeCenters.TryGetValue(localDistance.Key, out VoronoiSeed seed)) continue;


            attributes0 = seed.voronoiBiome;

            if (attributes0.biomeName == "Ocean")
            {
                hasOceanBiome = true;
                terrainHeightfloat += 4 * weight;
            }
            else
            {
                if (!hasLandBiome)
                {
                    secondClosestBiome = attributes0;
                }
                hasLandBiome = true;
                terrainHeightfloat += GetSurfaceHeightNoise(position.x, position.z, attributes0, domainWarping) * weight;
            }

        }

        int terrainHeight = Mathf.RoundToInt(terrainHeightfloat / totalWeight);

        BiomeAttributes attributes1 = null;
        if (biomeCenters.TryGetValue(new Vector3Int(nearestPoint.x, 0, nearestPoint.z), out VoronoiSeed var1))
            attributes1 = var1.voronoiBiome;

        if (attributes1.biomeName == "Ocean")
        {

            if (hasLandBiome && terrainHeight > 4)
            {
                attributes1 = secondClosestBiome;
            }
            else if ((!hasLandBiome) || (hasLandBiome && terrainHeight <= 4))
            {
                terrainHeight = 4;
            }
        }
        else if (hasOceanBiome && terrainHeight <= 4)
        {
            terrainHeight = 4;
        }

        return new BiomeSelector(attributes1, terrainHeight);
    }


    //Generates terrain height for given column
    public static int GetSurfaceHeightNoise(float x, float z, BiomeAttributes attributes_1, DomainWarping domainWarping)
    {
        float height = domainWarping.GenerateDomainNoise(new Vector2(x, z), attributes_1.noiseSettings[0]);
        height = Noise.Redistribution(height, attributes_1.noiseSettings[0]);
        int terrainHeight = Noise.Map01Int(0, HexData.ChunkHeight, height);

        return terrainHeight;
    }

}
