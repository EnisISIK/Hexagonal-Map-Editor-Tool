using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

public class ChunkDataGenerator
{
	public World _world;

	[SerializeField]
	List<Vector3Int> biomeCenters = new List<Vector3Int>();
	List<float> biomeNoise = new List<float>();

	public DomainWarping domainWarping;
	public DomainWarping biomeDomainWarping;

    [SerializeField]
    private List<BiomeData> biomeAttributesData = new List<BiomeData>();

    public ChunkDataGenerator(World world)
    {
		_world = world;
    }
    public ChunkDataGenerator(World world, List<BiomeData> biomeAttributesData, DomainWarping domainWarping, DomainWarping biomeDomainWarping)
    {
        _world = world;
        this.biomeAttributesData = biomeAttributesData;
        this.domainWarping = domainWarping;
        this.biomeDomainWarping = biomeDomainWarping;
    }

    public IEnumerator GenerateData(Vector3 chunkPos, System.Action<byte[,,]> callback)
	{
		byte[,,] tempData = new byte[HexData.ChunkWidth, HexData.ChunkHeight, HexData.ChunkWidth];

		Task t = Task.Factory.StartNew(delegate
		{
            //BiomeSelector biomeSelection = SelectBiomeAttributes(chunkPos);

            for (int y = 0; y < HexData.ChunkHeight; y++)
			{
				for (int z = 0; z < HexData.ChunkWidth; z++)
				{
					for (int x = 0; x < HexData.ChunkWidth; x++)
					{
                        //tempData[x, y, z] = _world.GetHex(new Vector3(x, y, z) + chunkPos, biomeSelection);
                        tempData[x, y, z] = _world.GetHex(new Vector3(x, y, z) + chunkPos);
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

    public IEnumerator GenerateData(Vector3 chunkPos, List<Vector3Int> biomeCenters , List<float> biomeNoise, System.Action<byte[,,]> callback)
    {
        byte[,,] tempData = new byte[HexData.ChunkWidth, HexData.ChunkHeight, HexData.ChunkWidth];

        Task t = Task.Factory.StartNew(delegate
        {
            this.biomeCenters = biomeCenters;
            this.biomeNoise = biomeNoise;


            for (int x = 0; x < HexData.ChunkWidth; x++) 
            {
                for (int z = 0; z < HexData.ChunkWidth; z++)
                {
                    BiomeSelector biomeSelection = SelectBiomeAttributes(new Vector3(chunkPos.x + x,0, chunkPos.z + z));
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

    private BiomeSelector SelectBiomeAttributes(Vector3 position, bool useDomainWarping = true)
    {
        if (useDomainWarping)
        {

            Vector2Int domainOffset = Vector2Int.RoundToInt(biomeDomainWarping.GenerateDomainOffset((int)position.x, (int)position.z));
            position += new Vector3Int(domainOffset.x, 0, domainOffset.y);
        }

        List<BiomeSelectionHelper> biomeSelectionHelpers = GetBiomeSelectionHelpers(position);
        BiomeAttributes attributes_1 = SelectBiome(biomeSelectionHelpers[0].Index);
        BiomeAttributes attributes_2 = SelectBiome(biomeSelectionHelpers[1].Index);

        float distance = Vector3.Distance(biomeCenters[biomeSelectionHelpers[0].Index], biomeCenters[biomeSelectionHelpers[1].Index]);
        float weight_0 = biomeSelectionHelpers[0].Distance / distance;
        float weight_1 = 1 - weight_0;
        int terrainHeightNoise_0 = GetSurfaceHeightNoise(position.x, position.z, attributes_1);
        int terrainHeightNoise_1 = GetSurfaceHeightNoise(position.x, position.z, attributes_2);
        return new BiomeSelector(attributes_1, Mathf.RoundToInt(terrainHeightNoise_0 * weight_0 + terrainHeightNoise_1 * weight_1));

    }

    private int GetSurfaceHeightNoise(float x, float z, BiomeAttributes attributes_1)
    {
        float height = domainWarping.GenerateDomainNoise(new Vector2(x, z), attributes_1.noiseSettings[0]);
        height = Noise.Redistribution(height, attributes_1.noiseSettings[0]);
        int terrainHeight = Noise.Map01Int(0, HexData.ChunkHeight, height);

        return terrainHeight;
    }

    private BiomeAttributes SelectBiome(int index)
    {
        float temp = biomeNoise[index];
        foreach (var data in biomeAttributesData)
        {
            if (temp > data.temperatureStartThreshold && temp < data.temperatureEndThreshold)
            {
                return data.Biome;
            }
        }
        return biomeAttributesData[0].Biome;
    }

    private List<BiomeSelectionHelper> GetBiomeSelectionHelpers(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x);
        int y = 0;
        int z = Mathf.FloorToInt(position.z);

        return GetClosestBiomeIndex(new Vector3Int(x, y, z));
    }

    private List<BiomeSelectionHelper> GetClosestBiomeIndex(Vector3Int position)
    {
        return biomeCenters.Select((center, index) =>
        new BiomeSelectionHelper
        {
            Index = index,
            Distance = Vector3.Distance(center, position)
        }).OrderBy(helper => helper.Distance).Take(4).ToList();
    }

    private struct BiomeSelectionHelper
    {
        public int Index;
        public float Distance;
    }

}
