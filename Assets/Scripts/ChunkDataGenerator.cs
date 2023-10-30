using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

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

    public IEnumerator GenerateData(Vector3 chunkPos, Dictionary<Vector3Int,Vector3> biomeCenters, Dictionary<Vector3Int, BiomeAttributes> biomeAttributes, System.Action<HexState[,,]> callback)
    {
        HexState[,,] tempData = new HexState[HexData.ChunkWidth, HexData.ChunkHeight, HexData.ChunkWidth];

        Task t = Task.Factory.StartNew(delegate
        {
            Dictionary<Vector3Int,Vector3> biomeCentersyedek = biomeCenters;
            Dictionary<Vector3Int, BiomeAttributes> biomeAttributesyedek = biomeAttributes;

            for (int x = 0; x < HexData.ChunkWidth; x++)
            {
                for (int z = 0; z < HexData.ChunkWidth; z++)
                {
                    BiomeSelector biomeSelection = SelectBiomeAttributesFromDict(new Vector3(chunkPos.x + x, 0, chunkPos.z + z), biomeCentersyedek, biomeAttributesyedek);
                    for (int y = 0; y < HexData.ChunkHeight; y++)
                    {
                        tempData[x, y, z] = new HexState(_world.GetHex(new Vector3(x, y, z) + chunkPos, biomeSelection));
                        //tempData[x, y, z] = _world.GetHex(new Vector3(x, y, z) + chunkPos);
                    }
                }
            }
        });

        yield return new WaitUntil(() =>
        {
            return t.IsCompleted;
        });

        if (t.Exception != null)
        {
            Debug.LogError(t.Exception);
        }

        callback(tempData);
    }
    public IEnumerator GenerateData(Vector3 chunkPos, Dictionary<Vector3Int, VoronoiSeed> biomeCenters, System.Action<HexState[,,]> callback)
    {
        HexState[,,] tempData = new HexState[HexData.ChunkWidth, HexData.ChunkHeight, HexData.ChunkWidth];

        Task t = Task.Factory.StartNew(delegate
        {
            Dictionary<Vector3Int, VoronoiSeed> biomeCentersVoronoi = biomeCenters;

            for (int x = 0; x < HexData.ChunkWidth; x++)
            {
                for (int z = 0; z < HexData.ChunkWidth; z++)
                {
                    BiomeSelector biomeSelection = SelectBiomeAttributesFromDict(new Vector3(chunkPos.x + x, 0, chunkPos.z + z), biomeCentersVoronoi);
                    for (int y = 0; y < HexData.ChunkHeight; y++)
                    {
                        tempData[x, y, z] = new HexState(_world.GetHex(new Vector3(x, y, z) + chunkPos, biomeSelection));
                        //tempData[x, y, z] = _world.GetHex(new Vector3(x, y, z) + chunkPos);
                    }
                }
            }
        });

        yield return new WaitUntil(() =>
        {
            return t.IsCompleted;
        });

        if (t.Exception != null)
        {
            Debug.LogError(t.Exception);
        }

        callback(tempData);
    }
    private BiomeSelector SelectBiomeAttributesFromDict(Vector3 position,Dictionary<Vector3Int, Vector3> biomeCenters, Dictionary<Vector3Int, BiomeAttributes> biomeAttributes)
    {
        int gridX = Mathf.FloorToInt(position.x / 16);
        int gridZ = Mathf.FloorToInt(position.z / 16); //eğer burası 64 olacaksa dictionaryden doğrudan i j yazamazsın.

        float nearestDistance = Mathf.Infinity;
        Vector3Int nearestPoint = new Vector3Int();

        float distortedX = position.x + Noise.Map01Int(0,15, Mathf.PerlinNoise(position.x * 0.1f, position.z * 0.1f));
        float distortedZ = position.z + Noise.Map01Int(0, 15, Mathf.PerlinNoise(position.x * 0.1f, position.z * 0.1f));
        Dictionary<Vector3Int, float> distDict = new Dictionary<Vector3Int, float>();
        for (int a = -2; a < 3; a++)
        {
            for (int b = -2; b < 3; b++)
            {

                int i = gridX + a;
                int j = gridZ + b;
                if(biomeCenters.TryGetValue(new Vector3Int(i,0,j), out Vector3 var)) { 
                    float distance = Vector3.Distance(new Vector3(distortedX, 0 , distortedZ), var);
                    distDict.Add(new Vector3Int(i, 0, j), distance);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestPoint = new Vector3Int(i, 0, j);
                    }
                }

            }
        }
        //Dictionary<Vector3Int, float> weights = new Dictionary<Vector3Int, float>();
        //float totalWeight = 0f;
        //var bot5 = distDict.OrderBy(pair => pair.Value).Take(3).ToDictionary(pair => pair.Key, pair => pair.Value);
        //foreach (var localDistance in bot5)
        //{
        //    if (localDistance.Value == 0f) { 
        //        weights.Add(localDistance.Key,1f);
        //        totalWeight += 1f;
        //    }
        //    else
        //    {
        //        weights.Add(localDistance.Key, 1f / (localDistance.Value* localDistance.Value));//Mathf.Sqrt(localDistance.Value));
        //        totalWeight += 1f / (localDistance.Value* localDistance.Value);//Mathf.Sqrt(localDistance.Value);
        //    }

        //}

        //Dictionary<Vector3Int, int> heights = new Dictionary<Vector3Int, int>();

        //foreach(var biomePoint in bot5)
        //{
        //    BiomeAttributes attributes0 = null;
        //    if (biomeAttributes.TryGetValue(biomePoint.Key, out BiomeAttributes var0))
        //        attributes0 = var0;

        //    if (var0.biomeName == "Ocean")//geçici çözüm
        //        heights.Add(biomePoint.Key, 4);
        //    else
        //        heights.Add(biomePoint.Key, PositionHelper.GetSurfaceHeightNoise(position.x,position.z,attributes0,domainWarping));

        //    weights[biomePoint.Key] /= totalWeight;
        //}
        //float terrainHeightfloat = 0;

        //foreach(var height in heights)
        //{
        //    terrainHeightfloat += height.Value * weights[height.Key];
        //}

        //int terrainHeight = Mathf.RoundToInt(terrainHeightfloat);

        BiomeAttributes attributes1 =null;
        if(biomeAttributes.TryGetValue(new Vector3Int(nearestPoint.x, 0, nearestPoint.z), out BiomeAttributes var1))
            attributes1 = var1;//liste yerine dictionary de yapılabilir. Eğer çalışmıyorsa tabii ki de

        return new BiomeSelector(attributes1);
        //return new BiomeSelector(attributes1,terrainHeight);
    }
    private BiomeSelector SelectBiomeAttributesFromDict(Vector3 position, Dictionary<Vector3Int, VoronoiSeed> biomeCenters)
    {
        int gridX = Mathf.FloorToInt(position.x / 16);
        int gridZ = Mathf.FloorToInt(position.z / 16); //eğer burası 64 olacaksa dictionaryden doğrudan i j yazamazsın.

        float nearestDistance = Mathf.Infinity;
        Vector3Int nearestPoint = new Vector3Int();

        float distortedX = position.x + Noise.Map01Int(0, 15, Mathf.PerlinNoise(position.x * 0.1f, position.z * 0.1f));
        float distortedZ = position.z + Noise.Map01Int(0, 15, Mathf.PerlinNoise(position.x * 0.1f, position.z * 0.1f));
        Dictionary<Vector3Int, float> distDict = new Dictionary<Vector3Int, float>();
        for (int a = -2; a < 3; a++)
        {
            for (int b = -2; b < 3; b++)
            {

                int i = gridX + a;
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

        BiomeAttributes attributes1 = null;
        if (biomeCenters.TryGetValue(new Vector3Int(nearestPoint.x, 0, nearestPoint.z), out VoronoiSeed var1))
            attributes1 = var1.voronoiBiome;//liste yerine dictionary de yapılabilir. Eğer çalışmıyorsa tabii ki de

        return new BiomeSelector(attributes1);
        //return new BiomeSelector(attributes1,terrainHeight);
    }

}
