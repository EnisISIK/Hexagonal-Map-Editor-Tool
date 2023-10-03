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

    public IEnumerator GenerateData(Vector3 chunkPos, Dictionary<Vector3Int,Vector3> biomeCenters, Dictionary<Vector3Int, BiomeAttributes> biomeAttributes, System.Action<byte[,,]> callback)
    {
        byte[,,] tempData = new byte[HexData.ChunkWidth, HexData.ChunkHeight, HexData.ChunkWidth];

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
                        tempData[x, y, z] = _world.GetHex(new Vector3(x, y, z) + chunkPos, biomeSelection);
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
        int gridX = Mathf.FloorToInt(position.x / 64);
        int gridZ = Mathf.FloorToInt(position.z / 64); // sanırım işe yaradı en nihayetinde. Bu 16 iken 32 oldu. Center finderda ise cellsize'i 2 katına çıkarmak durumunda kaldım

        float nearestDistance = Mathf.Infinity;
        Vector3Int nearestPoint = new Vector3Int();

        float distortedX = position.x + Noise.Map01Int(0,15, Mathf.PerlinNoise(position.x * 0.1f, position.z * 0.1f));
        float distortedZ = position.z + Noise.Map01Int(0, 15, Mathf.PerlinNoise(position.x * 0.1f, position.z * 0.1f));

        for (int a = -2; a < 3; a++)
        {
            for (int b = -2; b < 3; b++)
            {

                int i = gridX + a;
                int j = gridZ + b;
                if(biomeCenters.TryGetValue(new Vector3Int(i,0,j), out Vector3 var)) { 
                    float distance = Vector3.Distance(new Vector3(distortedX, 0 , distortedZ), var);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestPoint = new Vector3Int(i, 0, j);
                    }
                }

            }
        }
        BiomeAttributes attributes1 =null;
        if(biomeAttributes.TryGetValue(new Vector3Int(nearestPoint.x, 0, nearestPoint.z), out BiomeAttributes var1))
            attributes1 = var1;//liste yerine dictionary de yapılabilir. Eğer çalışmıyorsa tabii ki de

        return new BiomeSelector(attributes1);
    }
    private int GetSurfaceHeightNoise(float x, float z, BiomeAttributes attributes_1)
    {
        float height = domainWarping.GenerateDomainNoise(new Vector2(x, z), attributes_1.noiseSettings[0]);
        height = Noise.Redistribution(height, attributes_1.noiseSettings[0]);
        int terrainHeight = Noise.Map01Int(0, HexData.ChunkHeight, height);

        return terrainHeight;
    }

}
