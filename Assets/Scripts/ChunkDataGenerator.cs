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

    public IEnumerator GenerateData(Vector3 chunkPos, Dictionary<Vector3Int, VoronoiSeed> biomeCenters, System.Action<HexState[,,]> callback)
    {
        HexState[,,] tempData = new HexState[HexData.ChunkWidth, HexData.ChunkHeight, HexData.ChunkWidth];
        Dictionary<Vector3Int, VoronoiSeed> biomeCentersVoronoi = biomeCenters;

        Task t = Task.Factory.StartNew(delegate
        {
            BiomeSelector biomeSelection;
            for (int x = 0; x < HexData.ChunkWidth; x++)
            {
                for (int z = 0; z < HexData.ChunkWidth; z++)
                {
                    biomeSelection = SelectBiomeAttributesFromDict(new Vector3(chunkPos.x + x, 0, chunkPos.z + z), biomeCentersVoronoi);
                    for (int y = 0; y < HexData.ChunkHeight; y++)
                    {
                        tempData[x, y, z] = new HexState(_world.GetHex(new Vector3(x, y, z) + chunkPos, biomeSelection));
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
    
    private BiomeSelector SelectBiomeAttributesFromDict(Vector3 position, Dictionary<Vector3Int, VoronoiSeed> biomeCenters)
    {
        int gridX = Mathf.FloorToInt(position.x / 64); //bu chunkwidth olmak zorunda
        int gridZ = Mathf.FloorToInt(position.z / 64); //eğer burası 64 olacaksa dictionaryden doğrudan i j yazamazsın.

        float nearestDistance = Mathf.Infinity;
        Vector3Int nearestPoint = new Vector3Int();

        float distortedX = position.x + Noise.Map01Int(0, 16, Mathf.PerlinNoise(position.x * 0.1f, position.z * 0.1f)); //değişiklik çok az olunca aynı lerp oluyor olabilir. Al bunu 4le çarp
        float distortedZ = position.z + Noise.Map01Int(0, 16, Mathf.PerlinNoise(position.x * 0.1f, position.z * 0.1f)); //burayı biraz kurcala
        
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

        BiomeAttributes attributes1 = null;
        if (biomeCenters.TryGetValue(new Vector3Int(nearestPoint.x, 0, nearestPoint.z), out VoronoiSeed var1))
            attributes1 = var1.voronoiBiome;//liste yerine dictionary de yapılabilir. Eğer çalışmıyorsa tabii ki de

        return new BiomeSelector(attributes1);
        //return new BiomeSelector(attributes1,terrainHeight);
    }

}
