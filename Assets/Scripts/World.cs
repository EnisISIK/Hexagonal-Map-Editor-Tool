using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;
using System.Linq;
using System;

public class World : MonoBehaviour
{
    private ChunkDataGenerator _chunkDataGenerator;
    public BiomeAttributes[] biomes;

    public BiomeAttributes OceanBiome;

    Dictionary<Vector3Int, BiomeAttributes> voronoiBiomeAttributesDict = new Dictionary<Vector3Int, BiomeAttributes>();
    public static Dictionary<Vector3Int, Vector3> voronoiCentersDictionary;

    [SerializeField]
    private NoiseSettings biomeNoiseSettings;

    public DomainWarping domainWarping;
    public DomainWarping biomeDomainWarping;

    [SerializeField]
    private List<BiomeData> biomeAttributesData = new List<BiomeData>();

    public Transform player;
    public Vector3 spawnPosition;

    public Material material;
    public Material transparentMaterial;
    public Material waterMaterial;

    public BlockType[] blocktypes;

    public static Dictionary<Vector3Int, Chunk> chunksDictionary;
    public static Dictionary<Vector3Int, byte[,,]> chunksDataDictionary;

    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    public static Dictionary<Vector2Int, ChunkCoord> activeChunksDictionary;

    public ConcurrentQueue<ConcurrentQueue<HexMod>> modifications = new ConcurrentQueue<ConcurrentQueue<HexMod>>();

    public ChunkCoord playerCurrentChunkCoord;
    ChunkCoord playerLastChunkCoord;
    HashSet<ChunkCoord> chunksToCreate= new HashSet<ChunkCoord>();

    public ConcurrentQueue<Chunk> chunksToUpdate = new ConcurrentQueue<Chunk>();

    public GameObject debugScreen;

    private bool isCreatingChunks;
    public bool isUpdatingChunks;

    public float HumidityOffset = 100f;

    [Range(0.95f, 0f)]
    public float globalLightLevel;

    // TODO: make blocktypes enum or a scriptable object
    // TODO: add creation stack
    // TODO: work on random generated seeds
    // FIX: perlin noise is same for values below zero
    // TODO: clean code and turn repeating codes to functions

    private void Start()
    {
        _chunkDataGenerator = new ChunkDataGenerator(this,domainWarping);
        chunksDictionary = new Dictionary<Vector3Int, Chunk>();
        chunksDataDictionary = new Dictionary<Vector3Int, byte[,,]>();
        activeChunksDictionary = new Dictionary<Vector2Int, ChunkCoord>();
        voronoiCentersDictionary = new Dictionary<Vector3Int, Vector3>();
        voronoiBiomeAttributesDict = new Dictionary<Vector3Int, BiomeAttributes>();
        CalculateSpawnPosition();

        player.position = spawnPosition;
        CheckViewDistance();
        playerLastChunkCoord = PositionHelper.GetChunkCoordFromVector3(PositionHelper.PixelToHex(player.position));
    }
    private void Update()
    {
        playerCurrentChunkCoord = PositionHelper.GetChunkCoordFromVector3(PositionHelper.PixelToHex(player.position));

        Shader.SetGlobalFloat("GlobalLightLevel",globalLightLevel);

        if (!playerCurrentChunkCoord.Equals(playerLastChunkCoord))
        { 
            CheckViewDistance();
            playerLastChunkCoord = playerCurrentChunkCoord; 
        }
        if (chunksToCreate.Count > 0 && !isCreatingChunks)
        {
            StartCoroutine(CreateChunks());
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            debugScreen.SetActive(!debugScreen.activeSelf);
        }
    }
    private void FixedUpdate()
    {
        if (chunksToUpdate.Count > 0 && !isUpdatingChunks)
        {
            StartCoroutine(UpdateChunks());
        }
    }

    private void CalculateSpawnPosition()
    {
        int centerChunk = (HexData.WorldSizeInChunks * HexData.ChunkWidth) / 2;
        spawnPosition.x = (centerChunk + centerChunk * 0.5f - centerChunk / 2) * (HexData.innerRadius * 2f);
        spawnPosition.y = HexData.ChunkHeight - 50f;
        spawnPosition.z = centerChunk * (HexData.outerRadius * 1.5f);
    }

    IEnumerator UpdateChunks()
    {
        isUpdatingChunks = true;
        while (chunksToUpdate.Count > 0)
        {
            if (chunksToUpdate.TryPeek(out Chunk var))
                if (chunksDataDictionary.ContainsKey(var.position/HexData.ChunkWidth))
                {
                    Vector3Int chunkPos = PositionHelper.GetChunkFromVector3(var.position);
                    if (!chunksDataDictionary.ContainsKey(chunkPos))
                        chunksDataDictionary.Add(chunkPos, chunksDictionary[chunkPos].hexMap);
                    if (chunksToUpdate.TryDequeue(out Chunk var1))
                    {
                        StartCoroutine(var1.UpdateChunk());
                        yield return null;
                    }
                }
        }
        isUpdatingChunks = false;
    }
    public IEnumerator ApplyModifications()
    {
        while (modifications.Count > 0)
        {

            ConcurrentQueue<HexMod> queue;
            modifications.TryDequeue(out queue);

            while (queue.Count > 0)
            {

                HexMod v;
                queue.TryDequeue(out v);

                ChunkCoord c = PositionHelper.GetChunkCoordFromVector3(v.position);

                if (chunksDictionary.TryGetValue(new Vector3Int(c.x, 0, c.z), out Chunk var))
                {
                    var.modifications.Enqueue(v);
                }



            }
            yield return null;
        }
    }

    IEnumerator CreateChunks()
    {
        isCreatingChunks = true;

        while (chunksToCreate.Count > 0)
        {
            HashSet<ChunkCoord> chunksToCreateCopy = new HashSet<ChunkCoord>(chunksToCreate);
            foreach (ChunkCoord chunkPos in chunksToCreateCopy) {
                if (chunksDataDictionary.ContainsKey(new Vector3Int(chunkPos.x, 0, chunkPos.z))) { 

                    chunksDictionary[new Vector3Int(chunkPos.x, 0, chunkPos.z)].Init(chunksDataDictionary[new Vector3Int(chunkPos.x, 0, chunkPos.z)]);
                    chunksToCreate.Remove(chunkPos);
                }
            }
            yield return null;
        }
        isCreatingChunks = false;
    }

    private void Populate(Vector3Int chunkPos)
    {
        StartCoroutine(PopulateCoroutine(chunkPos));
    }

    private IEnumerator PopulateCoroutine(Vector3Int chunkPos)
    {
        byte[,,] tmpData=null;
        if (tmpData == null)
        {
            StartCoroutine(_chunkDataGenerator.GenerateData(chunkPos * HexData.ChunkWidth, voronoiCentersDictionary,voronoiBiomeAttributesDict, x => tmpData = x));
            yield return new WaitUntil(() => tmpData != null);

        }
        if (!chunksDataDictionary.ContainsKey(chunkPos))
            chunksDataDictionary.Add(chunkPos, tmpData);
    }

    public byte[,,] RequestChunk(Vector3 chunkPos)
    {
        Vector3Int coord = PositionHelper.GetChunkFromVector3(chunkPos);
        return chunksDataDictionary[coord];
    }

    public Chunk GetChunkFromChunkVector3(Vector3 pos)
    {

        int x = Mathf.FloorToInt(pos.x / HexData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / HexData.ChunkWidth);

        return chunksDictionary[new Vector3Int(x, 0, z)];

    }

    void CheckViewDistance()
    {
        ChunkCoord coord = PositionHelper.GetChunkCoordFromVector3(PositionHelper.PixelToHex(player.position));
        GenerateVoronoiSeeds(coord, HexData.ViewDistanceinChunks, HexData.ChunkWidth);
        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        activeChunks.Clear();
        for (int x = coord.x - HexData.ViewDistanceinChunks-1; x < coord.x + HexData.ViewDistanceinChunks+1; x++)
        {
            for (int z = coord.z - HexData.ViewDistanceinChunks-1; z < coord.z + HexData.ViewDistanceinChunks+1; z++)
            {
                Vector3Int chunkPos = new Vector3Int(x, 0, z);
                if (chunksDataDictionary.ContainsKey(chunkPos)) continue;
                
                Populate(chunkPos);
            }
        }

        for (int x = coord.x - HexData.ViewDistanceinChunks; x < coord.x + HexData.ViewDistanceinChunks; x++)
        {
            for (int z = coord.z - HexData.ViewDistanceinChunks; z < coord.z + HexData.ViewDistanceinChunks; z++)
            {
                Vector3Int chunkPos = new Vector3Int(x, 0, z);
                if (!chunksDictionary.TryGetValue(chunkPos, out Chunk var))
                {
                    chunksDictionary.Add(chunkPos, new Chunk(new ChunkCoord(x, z), this));
                    chunksToCreate.Add(new ChunkCoord(x, z));
                }
                else if (!var.isActive)
                {
                    var.isActive = true;
                }
                activeChunks.Add(new ChunkCoord(x, z));

                for(int i= 0; i < previouslyActiveChunks.Count; i++){
                    if (previouslyActiveChunks[i].Equals(new ChunkCoord(x,z)))
                        previouslyActiveChunks.RemoveAt(i);
                }
            }
        }
        foreach(ChunkCoord _chunk in previouslyActiveChunks)
        {
            chunksDictionary[new Vector3Int(_chunk.x, 0, _chunk.z)].isActive = false;
            activeChunks.Remove(new ChunkCoord(_chunk.x, _chunk.z));
        }
    }
    private Vector3Int GetInChunkPosition(Vector3 pos, Vector3 chunkPos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkPos.x*HexData.ChunkWidth);
        zCheck -= Mathf.FloorToInt(chunkPos.z*HexData.ChunkWidth);

        return new Vector3Int(xCheck, yCheck, zCheck);
    }

    public bool CheckForHex(Vector3 pos)
    {
        ChunkCoord thisChunk = PositionHelper.GetChunkCoordFromVector3(pos);

        if (!IsHexInWorld(pos)) return false;

        Vector3Int chunkPos = new Vector3Int(thisChunk.x, 0, thisChunk.z);
        if (chunksDictionary.ContainsKey(chunkPos) && chunksDataDictionary.TryGetValue(chunkPos,out byte[,,] var))
        {
            Vector3Int inChunkPos = GetInChunkPosition(pos,chunkPos);
            return blocktypes[var[inChunkPos.x, inChunkPos.y, inChunkPos.z]].isSolid;
        }
        return blocktypes[GetHex(pos)].isSolid;
    }
    public bool CheckForTransparentHex(Vector3 pos)
    {
        ChunkCoord thisChunk = PositionHelper.GetChunkCoordFromVector3(pos);

        if (!IsHexInWorld(pos)) return false;

        Vector3Int chunkPos = new Vector3Int(thisChunk.x, 0, thisChunk.z);
        if (chunksDataDictionary.TryGetValue(chunkPos, out byte[,,] var))
        {
            Vector3Int inChunkPos = GetInChunkPosition(pos, chunkPos);
            return blocktypes[var[inChunkPos.x, inChunkPos.y, inChunkPos.z]].isTransparent;
        }
        if (!chunksDictionary.ContainsKey(chunkPos) && chunksDataDictionary.TryGetValue(chunkPos, out byte[,,] var1))
        {
            Vector3Int inChunkPos = GetInChunkPosition(pos, chunkPos);
            //if (blocktypes[var1[inChunkPos.x, inChunkPos.y, inChunkPos.z]].isTransparent && blocktypes[GetHex(pos)].isTransparent)
        } //bak bakalım chunkdatanin viewdistancetan 1 fazla olmasını kullanabiliyor musun gethex yerine
        return blocktypes[GetHex(pos)].isTransparent;
    }
    public bool CheckForWaterHex(Vector3 pos)
    {
        ChunkCoord thisChunk = PositionHelper.GetChunkCoordFromVector3(pos);

        if (!IsHexInWorld(pos)) return false;

        Vector3Int chunkPos = new Vector3Int(thisChunk.x, 0, thisChunk.z);
        if (chunksDataDictionary.TryGetValue(chunkPos, out byte[,,] var))
        {
            Vector3Int inChunkPos = GetInChunkPosition(pos, chunkPos);
            return blocktypes[var[inChunkPos.x, inChunkPos.y, inChunkPos.z]].isWater;
        }
        return blocktypes[GetHex(pos)].isWater;
    }

    public void GenerateVoronoiSeeds(ChunkCoord coord, int drawRange, int mapSize)
    {
        Dictionary<Vector3Int, Vector3> tempDict = BiomeCenterFinder.CalculateBiomeCentersDictionary(mapSize, drawRange, coord);

        foreach(var centerSeed in tempDict)
        {
            if (!voronoiCentersDictionary.ContainsKey(centerSeed.Key))
            {
                voronoiCentersDictionary.Add(centerSeed.Key, centerSeed.Value);
            }
        }

        List<float> tempTest = new List<float>();
        List<float> humTest = new List<float>();

        for (int x = coord.x - HexData.ViewDistanceinChunks - 4; x < coord.x + HexData.ViewDistanceinChunks + 4; x++)
        {
            for (int z = coord.z - HexData.ViewDistanceinChunks - 4; z < coord.z + HexData.ViewDistanceinChunks + 4; z++)
            {
                Vector3Int seedCoord = new Vector3Int(x, 0, z);
                if (!voronoiBiomeAttributesDict.ContainsKey(seedCoord))
                {
                    float land = Mathf.PerlinNoise((x + 100f) * 0.1f, (z + 100f) * 0.1f);
                    if (land < 0.50f) voronoiBiomeAttributesDict.Add(seedCoord, OceanBiome);
                    else{
                        float temperature = Mathf.PerlinNoise(x * 0.2f, z * 0.2f);
                        float humidity = Mathf.PerlinNoise((x + 160f) * 0.2f, (z + 160f) * 0.2f);
                        tempTest.Add(temperature);
                        humTest.Add(humidity);
                        voronoiBiomeAttributesDict.Add(seedCoord, SelectBiomes(temperature, humidity));
                    }
                }
            }
        }

    }

    private BiomeAttributes SelectBiomes(float tempbiomeNoiseyedek, float moisturebiomeNoiseyedek)
    {
        float temp = tempbiomeNoiseyedek;
        float humidity = moisturebiomeNoiseyedek;

        foreach (var data in biomeAttributesData)
        {
            if (temp > data.temperatureStartThreshold && temp < data.temperatureEndThreshold
                && humidity > data.humidityStartThreshold && humidity < data.humidityEndThreshold)
            {
                return data.Biome;
            }
        }
        return biomeAttributesData[0].Biome;
    }

    private List<float> CalculateBiomeNoise(List<Vector3Int> biomeCenters,Vector3Int offset)
    {
        return biomeCenters.Select(center =>Mathf.PerlinNoise((center.x+offset.x)*0.1f,(center.z+offset.z) * 0.1f) /*Noise.OctavePerlin(new Vector2(center.x+offset.x, center.z+offset.z), biomeNoiseSettings*/).ToList();
    }
    private List<float> CalculateBiomeTempNoise(List<Vector3> biomeCenters)
    {
        return biomeCenters.Select(center => Mathf.PerlinNoise((center.x + 500f) * 0.2f, (center.z+500f)*0.2f)).ToList();
    }
    private List<float> CalculateBiomeHumNoise(List<Vector3> biomeCenters)
    {
        return biomeCenters.Select(center => Mathf.PerlinNoise((center.x+1000f) * 0.2f, (center.z + 1000f) * 0.2f)).ToList();
    }
    private List<float> CalculateLandNoise(List<Vector3> biomeCenters)
    {
        return biomeCenters.Select(center => Mathf.PerlinNoise((center.x + 50f) * 0.1f, (center.z + 50f) * 0.1f)).ToList();
    }

    public byte GetHex(Vector3 pos,BiomeSelector biomeSelection=null)
    {
        int yPos = Mathf.FloorToInt(pos.y);
        if (!IsHexInWorld(pos)) return 0;

        /* Basic Terrain Pass*/
        int terrainHeight;
        BiomeAttributes biome;
        if (biomeSelection != null)
        {
            biome = biomeSelection.biomeAttributes;
            //if (biomeSelection.terrainSurfaceNoise.HasValue == false)
            //{
                terrainHeight = GetSurfaceHeightNoise(pos.x, pos.z, biome);
            //}
            //else
            //{
            //    terrainHeight = biomeSelection.terrainSurfaceNoise.Value;
            //}
        }
        else { biome = biomes[0];
            terrainHeight = GetSurfaceHeightNoise(pos.x, pos.z, biome);
        }
        //terrainHeight = GetSurfaceHeightNoise(pos.x, pos.z, biome);
        if (biome == OceanBiome)
        {
            terrainHeight = 4;
        }

        byte voxelValue;

        if (yPos == terrainHeight)
        {
            voxelValue = biome.surfaceBlock;
        }
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
        {
            voxelValue = biome.subSurfaceBlock;
        }
        else if (yPos > terrainHeight)
        {
            if (yPos <= 5)
                return 8;
            else
                return 0;
        }
        else
        {
            voxelValue = 1;
        }
        if (terrainHeight < 5)
        {
            voxelValue = 7;
        }
        /*Second Pass*/

        if (voxelValue == 1)
        {
            foreach(Lode lode in biome.lodes)
            {
                if(yPos > lode.minHeight && yPos < lode.maxHeight)
                {
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                    {
                        voxelValue = lode.blockID;
                    }
                }
            }
        }

        /* TREE PASS */

        if (yPos == terrainHeight && biome.placeFlora&&terrainHeight>=10)
        {

            if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.floraZoneScale) > biome.floraZoneThreshold)
            {

                if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.floraPlacementScale) > biome.floraPlacementThreshold)
                {
                    ConcurrentQueue<HexMod> structure = Structure.GenerateMajorFlora(biome.floraIndex,pos, biome.minFloraHeight, biome.maxFloraHeight);
                    modifications.Enqueue(structure);
                }
            }

        }
        return voxelValue;
    }


    private int GetSurfaceHeightNoise(float x, float z, BiomeAttributes attributes_1)
    {
        float height = domainWarping.GenerateDomainNoise(new Vector2(x, z), attributes_1.noiseSettings[0]);
        height = Noise.Redistribution(height, attributes_1.noiseSettings[0]);
        int terrainHeight = Noise.Map01Int(0, HexData.ChunkHeight, height);

        return terrainHeight;
    }

    private struct BiomeSelectionHelper
    {
        public int Index;
        public float Distance;
    }

    bool IsChunkInWorld(ChunkCoord coord)
    {
        return coord.x >= 0 && coord.x < HexData.WorldSizeInChunks && coord.z >= 0 && coord.z < HexData.WorldSizeInChunks; 

    }

    bool IsHexInWorld(Vector3 pos)
    {
        return pos.y >= 0 && pos.y < HexData.ChunkHeight;

    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.blue;

        //foreach(var biomeCenterPoint in voronoiCentersDictionary)
        //{
        //    Vector3 pixelPoint = PositionHelper.HexToPixel(biomeCenterPoint.Value);
        //    Gizmos.DrawLine(pixelPoint, pixelPoint+ Vector3.up * 255);
        //}

        //Gizmos.color = Color.red;
        //foreach (var biomeCenterPointyedek in biomeCentersyedek)
        //{
        //    Gizmos.DrawLine(biomeCenterPointyedek, biomeCenterPointyedek + Vector3.up * 255);
        //}
    }
}

[System.Serializable]
public class BlockType
{
    public BlockTypes blockTypeName;
    public string blockName;
    public bool isSolid;
    public bool isTransparent;
    public bool isWater;

    [Header("Texture Values")]
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int rightFaceTexture;
    public int leftFaceTexture;
    public int frontrightFaceTexture;
    public int frontleftFaceTexture;
    public int backrightFaceTexture;
    public int backleftFaceTexture;

    //Top, Bottom, Right, Left, Front Right, Front Left, Back Right, Back Left
    public int GetTextureID(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
                return topFaceTexture;
            case 1:
                return bottomFaceTexture;
            case 2:
                return rightFaceTexture;
            case 3:
                return leftFaceTexture;
            case 4:
                return frontrightFaceTexture;
            case 5:
                return frontleftFaceTexture;
            case 6:
                return backrightFaceTexture;
            case 7:
                return backleftFaceTexture;
            default:
                Debug.Log("Error in GetTextureID: invalid face index");
                return 0;

        } 
    }
}
public class HexMod
{
    public Vector3 position;
    public byte id;

    public HexMod()
    {
        position = new Vector3();
        id = 0;
    }

    public HexMod(Vector3 _position, byte _id)
    {
        position = _position;
        id = _id;
    }
}

[Serializable]
public struct BiomeData
{
    [Range(0f, 1f)]
    public float temperatureStartThreshold, temperatureEndThreshold;
    [Range(0f, 1f)]
    public float humidityStartThreshold, humidityEndThreshold;
    public BiomeAttributes Biome;
}

public class BiomeSelector
{
    public BiomeAttributes biomeAttributes = null;
    public int? terrainSurfaceNoise = null;

    public BiomeSelector(BiomeAttributes biomeAttributes,int? terrainSurfaceNoise = null)
    {
        this.biomeAttributes = biomeAttributes;
        this.terrainSurfaceNoise = terrainSurfaceNoise;
    }
}