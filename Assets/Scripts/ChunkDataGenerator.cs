using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;

public class ChunkDataGenerator
{
	public World _world;

    public DomainWarping domainWarping;

    public ChunkDataGenerator(World world)
    {
		_world = world;
    }
    public ChunkDataGenerator(World world, DomainWarping domainWarping)
    {
        _world = world;
        this.domainWarping = domainWarping;
    }

    //void ComposableGenerator()
    //{
    //    HexState[,,] tempData = new HexState[HexData.ChunkWidth, HexData.ChunkHeight, HexData.ChunkWidth];
    //    for (int z = 0; z < HexData.ChunkWidth; z++)
    //    {
    //        for (int x = 0; x < HexData.ChunkWidth; x++)
    //        {
    //            for (int y = 0; y < HexData.ChunkHeight; y++)
    //            {
    //                tempData[x, y, z] = new HexState(_world.GetHex(new Vector3(x, y, z) + chunkPos, biomeSelection));

    //            }
    //        }  // for x
    //    }  // for z
    //}

    private HexState[,,] ComposableGenerator(Vector3 chunkPos, Dictionary<Vector3Int, VoronoiSeed> biomeCenters,System.Action<BiomeSelector[,]> callback)
    {
        HexState[,,] tempData = new HexState[HexData.ChunkWidth, HexData.ChunkHeight, HexData.ChunkWidth];
        BiomeSelector[,] biomeSelectionData = new BiomeSelector[HexData.ChunkWidth, HexData.ChunkWidth];
        Dictionary<Vector3Int, VoronoiSeed> biomeCentersVoronoi = biomeCenters;
        BiomeSelector biomeSelection;
        for (int x = 0; x < HexData.ChunkWidth; x++)
        {
            for (int z = 0; z < HexData.ChunkWidth; z++)
            {
                biomeSelection = SelectBiomeAttributesFromDict(new Vector3(chunkPos.x + x, 0, chunkPos.z + z), biomeCentersVoronoi);
                biomeSelectionData[x, z] = biomeSelection;
                for (int y = 0; y < HexData.ChunkHeight; y++)
                {
                    byte id = (byte)((Mathf.FloorToInt(y + chunkPos.y) > biomeSelection.terrainSurfaceNoise.Value) ? 0 : 1);
                    tempData[x, y, z] = new HexState(id);
                    //tempData[x, y, z] = new HexState(_world.GetHex(new Vector3(x, y, z) + chunkPos, biomeSelection));
                    //if(tempData[x, y, z].id == 0 && tempSurfaceData[x, y, z] != null)
                    //{
                    //    tempSurfaceData[x, y, z] = new HexState(0,biomeSelection.biomeAttributes);
                    //}
                }
            }
        }
        callback(biomeSelectionData);
        return tempData;
    }
    public HexState[,,] ComposeTerrain(Vector3 chunkPos, HexState[,,] tempData, BiomeSelector[,] biomeSelectionData)
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
                    byte block = tempData[x,y,z].id;
                    if (block == 1)
                    {
                        if(y == terrainHeight)
                        {
                            tempData[x, y, z].id = biome.surfaceBlock;
                        }
                        else if (y < terrainHeight && y > terrainHeight-4)
                        {
                            tempData[x, y, z].id = biome.subSurfaceBlock;
                        }
                        else
                        {
                            foreach (Lode lode in biome.lodes)
                            {
                                if (y > lode.minHeight && y < lode.maxHeight)
                                {
                                    if (Noise.Get3DPerlin(new Vector3(x, y, z) + chunkPos, lode.noiseOffset, lode.scale, lode.threshold))
                                    {
                                        tempData[x, y, z].id = lode.blockID;
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
    public HexState[,,] AddFinishGen(Vector3 chunkPos, HexState[,,] tempData, BiomeSelector[,] biomeSelectionData)
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
                    if (y != terrainHeight) continue;
                    if (y == terrainHeight && biome.placeFlora)
                    {
                        Vector2 pos = new Vector2(x+chunkPos.x, z + chunkPos.z);
                        if (Noise.Get2DPerlin(pos, 0, biome.floraZoneScale) > biome.floraZoneThreshold)
                        {

                            if (Noise.Get2DPerlin(pos, 0, biome.floraPlacementScale) > biome.floraPlacementThreshold)
                            {
                                ConcurrentQueue<HexMod> queue = new ConcurrentQueue<HexMod>();
                                int height = (int)(biome.maxFloraHeight * Noise.Get2DPerlin(pos, 4005f, 2f));

                                if (height < biome.maxFloraHeight)
                                    height = biome.minFloraHeight;

                                for (int i = 1; i < height; i++)
                                {
                                    tempData[x,y+i,z].id = 3;
                                }

                                for (int trunkx = -2; trunkx < 3; trunkx++)
                                {
                                    for (int trunky = 0; trunky < 3; trunky++)
                                    {
                                        for (int trunkz = -2; trunkz < 3; trunkz++)
                                        {
                                            if (x + trunkx > 0 && x + trunkx < HexData.ChunkWidth &&
                                                y + trunky + height > 0 && y + trunky + height < HexData.ChunkHeight &&
                                                z + trunkz > 0 && z + trunkz < HexData.ChunkWidth)
                                                tempData[x + trunkx, y + trunky + height, z + trunkz].id = 6;
                                            else
                                                queue.Enqueue(new HexMod(new Vector3(x + trunkx+chunkPos.x, y + trunky + height +chunkPos.y, z + trunkz + chunkPos.z), 6));
                                        }
                                    }
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
    public HexState[,,] AddFinishGenTrees(Vector3 chunkPos, HexState[,,] tempData, BiomeSelector[,] biomeSelectionData)  //tree değil de string ve biome olarak yap
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
                    if (tempData[x, y+1, z].id == 0)
                        Debug.Log(block.id);

                    tempData[x, y+1, z].id = block.id;
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
                                        tempData[modX, y + modY, modZ].id = mod.id;
                                    else
                                        queue.Enqueue(new HexMod(new Vector3(modX + chunkPos.x, y+modY ,  modZ + chunkPos.z), mod.id));
                                }
                                _world.finishersToAdd.Enqueue(queue);
                            }
                            //else
                            //{
                            //    ConcurrentQueue<HexMod> queue = new ConcurrentQueue<HexMod>();
                            //    ConcurrentQueue<HexMod> structure = Structure.GenerateMajorFlora(4, new Vector3(x, 0, z), biome.minFloraHeight, biome.maxFloraHeight);
                            //    while (structure.Count > 0)
                            //    {
                            //        structure.TryDequeue(out HexMod mod);
                            //        int modX = (int)mod.position.x;
                            //        int modY = (int)mod.position.y;
                            //        int modZ = (int)mod.position.z;

                            //        if (modX > 0 && modX < HexData.ChunkWidth &&
                            //            y + modY > 0 && y + modY < HexData.ChunkHeight &&
                            //            modZ > 0 && modZ < HexData.ChunkWidth)
                            //            tempData[modX, y + modY, modZ].id = mod.id;
                            //        else
                            //            queue.Enqueue(new HexMod(new Vector3(modX + chunkPos.x, y + modY, modZ + chunkPos.z), mod.id));
                            //    }
                            //    _world.finishersToAdd.Enqueue(queue);
                            //}
                        }
                    }
                }
            }
        }

        return tempData;
    }

    public IEnumerator GenerateData(Vector3 chunkPos, Dictionary<Vector3Int, VoronoiSeed> biomeCenters, System.Action<HexState[,,]> callback)//, System.Action<HexState[,,]> surfaceCallback
    {

        BiomeSelector[,] biomeSelectionData = new BiomeSelector[HexData.ChunkWidth, HexData.ChunkWidth];
        HexState[,,] tempData = new HexState[HexData.ChunkWidth, HexData.ChunkHeight, HexData.ChunkWidth];
        //HexState[,,] tempSurfaceData = new HexState[HexData.ChunkWidth, 1, HexData.ChunkWidth];
        Dictionary<Vector3Int, VoronoiSeed> biomeCentersVoronoi = biomeCenters;
        HexState[,,] tempData2 = new HexState[HexData.ChunkWidth, HexData.ChunkHeight, HexData.ChunkWidth];

        Task t = Task.Factory.StartNew(delegate
        {
            tempData2 = ComposableGenerator(chunkPos, biomeCentersVoronoi, x => biomeSelectionData = x);

            tempData2 = ComposeTerrain(chunkPos, tempData2, biomeSelectionData);

            tempData2 = AddFinishGenTrees(chunkPos, tempData2, biomeSelectionData);
            //BiomeSelector biomeSelection;
            //for (int x = 0; x < HexData.ChunkWidth; x++)
            //{
            //    for (int z = 0; z < HexData.ChunkWidth; z++)
            //    {
            //        biomeSelection = SelectBiomeAttributesFromDict(new Vector3(chunkPos.x + x, 0, chunkPos.z + z), biomeCentersVoronoi);
            //        for (int y = 0; y < HexData.ChunkHeight; y++)
            //        {
            //            tempData[x, y, z] = new HexState(_world.GetHex(new Vector3(x, y, z) + chunkPos, biomeSelection));
            //            //if(tempData[x, y, z].id == 0 && tempSurfaceData[x, y, z] != null)
            //            //{
            //            //    tempSurfaceData[x, y, z] = new HexState(0,biomeSelection.biomeAttributes);
            //            //}
            //        }
            //    }
            //}
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
        //surfaceCallback(tempSurfaceData);
    }
    
    private BiomeSelector SelectBiomeAttributesFromDict(Vector3 position, Dictionary<Vector3Int, VoronoiSeed> biomeCenters)
    {
        int gridX = Mathf.FloorToInt(position.x / 64);
        int gridZ = Mathf.FloorToInt(position.z / 64);

        float nearestDistance = Mathf.Infinity;
        Vector3Int nearestPoint = new Vector3Int();

        float distortedX = position.x + Noise.Map01Int(0, 16, Mathf.PerlinNoise(position.x * 0.1f, position.z * 0.1f));
        float distortedZ = position.z + Noise.Map01Int(0, 16, Mathf.PerlinNoise(position.x * 0.1f, position.z * 0.1f));
        
        Dictionary<Vector3Int, float> distDict = new Dictionary<Vector3Int, float>();
        for (int a = -1; a < 2; a++)
        {
            for (int b = -1; b < 2; b++)
            {

                int i = gridX + a;  //burayı incele 9.11
                int j = gridZ + b;
                if (biomeCenters.TryGetValue(new Vector3Int(i, 0, j), out VoronoiSeed var))
                {
                    float distance = Vector3.Distance(new Vector3(distortedX, 0, distortedZ), var.voronoiPosition);
                    distDict.Add(new Vector3Int(i, 0, j), distance);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestPoint = new Vector3Int(i, 0, j);
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

            if (biomeCenters.TryGetValue(localDistance.Key, out VoronoiSeed seed))
            {
                attributes0 = seed.voronoiBiome;
            }
            if(attributes0!=null)
            {
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
                    terrainHeightfloat += PositionHelper.GetSurfaceHeightNoise(position.x, position.z, attributes0, domainWarping) * weight;
                }
            }

        }
       
        int terrainHeight = Mathf.RoundToInt(terrainHeightfloat / totalWeight);

        BiomeAttributes attributes1 = null;
        if (biomeCenters.TryGetValue(new Vector3Int(nearestPoint.x, 0, nearestPoint.z), out VoronoiSeed var1))
            attributes1 = var1.voronoiBiome;

        if (attributes1.biomeName == "Ocean") {

            if (hasLandBiome && terrainHeight > 4)
            {
                attributes1 = secondClosestBiome;
            }
            else if ((!hasLandBiome) || (hasLandBiome && terrainHeight <= 4))
            {
                terrainHeight = 4;
            }
        }
        else if(hasOceanBiome && terrainHeight <= 4)
        {
            terrainHeight = 4;
        }
        
        return new BiomeSelector(attributes1, terrainHeight);
    }

}
