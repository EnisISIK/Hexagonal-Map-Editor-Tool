using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;
using System;

public class World : MonoBehaviour
{
    public BiomeAttributes[] biomes;

    [SerializeField]
    List<Vector3Int> biomeCenters = new List<Vector3Int>();
    List<float> biomeNoise = new List<float>();

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
    List<ChunkCoord> chunksToCreate= new List<ChunkCoord>();

    public ConcurrentQueue<Chunk> chunksToUpdate = new ConcurrentQueue<Chunk>();

    public GameObject debugScreen;

    private bool isCreatingChunks;
    public bool isUpdatingChunks;

    // TODO: make blocktypes enum or a scriptable object
    // TODO: add creation stack
    // TODO: work on random generated seeds
    // FIX: perlin noise is same for values below zero
    // TODO: clean code and turn repeating codes to functions

    private void Start()
    {

        chunksDictionary = new Dictionary<Vector3Int, Chunk>();
        chunksDataDictionary = new Dictionary<Vector3Int, byte[,,]>();
        activeChunksDictionary = new Dictionary<Vector2Int, ChunkCoord>();
        CalculateSpawnPosition();

        player.position = spawnPosition;
        CheckViewDistance();
        playerLastChunkCoord = PositionHelper.GetChunkCoordFromVector3(PositionHelper.PixelToHex(player.position));
    }
    private void Update()
    {
        playerCurrentChunkCoord = PositionHelper.GetChunkCoordFromVector3(PositionHelper.PixelToHex(player.position));
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
                if (var.isHexMapPopulated)
                    if (chunksToUpdate.TryDequeue(out Chunk var1)) { 
                        StartCoroutine(var1.UpdateChunk());
                        yield return null;
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

                if (chunksDictionary.ContainsKey(new Vector3Int(c.x,0,c.z)))
                {
                    chunksDictionary[new Vector3Int(c.x, 0, c.z)].modifications.Enqueue(v);
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
            chunksDictionary[new Vector3Int(chunksToCreate[0].x, 0, chunksToCreate[0].z)].Init();
            chunksToCreate.RemoveAt(0);
            yield return null;
        }
        isCreatingChunks = false;
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
        GenerateBiomePoints(PositionHelper.PixelToHex(player.position), HexData.ViewDistanceinChunks, HexData.ChunkWidth);
        ChunkCoord coord = PositionHelper.GetChunkCoordFromVector3(PositionHelper.PixelToHex(player.position)); 

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        activeChunks.Clear();

        for (int x = coord.x - HexData.ViewDistanceinChunks; x < coord.x + HexData.ViewDistanceinChunks; x++)
        {
            for (int z = coord.z - HexData.ViewDistanceinChunks; z < coord.z + HexData.ViewDistanceinChunks; z++)
            {
                if (!chunksDictionary.ContainsKey(new Vector3Int(x, 0, z))){
                    chunksDictionary.Add(new Vector3Int(x, 0, z), new Chunk(new ChunkCoord(x, z), this));
                    chunksToCreate.Add(new ChunkCoord(x, z));
                }
                else if (!chunksDictionary[new Vector3Int(x, 0, z)].isActive){
                    chunksDictionary[new Vector3Int(x, 0, z)].isActive = true;
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

    public bool CheckForHex(Vector3 pos)
    {
        ChunkCoord thisChunk = PositionHelper.GetChunkCoordFromVector3(pos);

        if (!IsHexInWorld(pos)) return false;
       
        Vector3Int chunkPos = new Vector3Int(thisChunk.x, 0, thisChunk.z);
        if (chunksDictionary.ContainsKey(chunkPos) && chunksDictionary[chunkPos].isHexMapPopulated) 
        {
            return blocktypes[chunksDictionary[chunkPos].GetHexFromGlobalVector3(pos)].isSolid; 
        }
        return blocktypes[GetHex(pos)].isSolid;
    }
    public bool CheckForTransparentHex(Vector3 pos)
    {
        ChunkCoord thisChunk = PositionHelper.GetChunkCoordFromVector3(pos);

        if (!IsHexInWorld(pos)) return false;

        Vector3Int chunkPos = new Vector3Int(thisChunk.x, 0, thisChunk.z);
        if (chunksDictionary.ContainsKey(chunkPos) && chunksDictionary[chunkPos].isHexMapPopulated)
        {
            return blocktypes[chunksDictionary[chunkPos].GetHexFromGlobalVector3(pos)].isTransparent;
        }
        return blocktypes[GetHex(pos)].isTransparent;
    }
    public bool CheckForWaterHex(Vector3 pos)
    {
        ChunkCoord thisChunk = PositionHelper.GetChunkCoordFromVector3(pos);

        if (!IsHexInWorld(pos)) return false;

        Vector3Int chunkPos = new Vector3Int(thisChunk.x, 0, thisChunk.z);
        if (chunksDictionary.ContainsKey(chunkPos) && chunksDictionary[chunkPos].isHexMapPopulated)
        {
            return blocktypes[chunksDictionary[chunkPos].GetHexFromGlobalVector3(pos)].isWater;
        }
        return blocktypes[GetHex(pos)].isWater;
    }

    public void GenerateBiomePoints(Vector3 playerPosition,int drawRange,int mapSize)
    {
        biomeCenters = new List<Vector3Int>();
        biomeCenters = BiomeCenterFinder.CalculateBiomeCenters(playerPosition,drawRange,mapSize);

        for(int i = 0; i<biomeCenters.Count; i++)
        {
            Vector2Int domainWarpingOffset = biomeDomainWarping.GenerateDomainOffsetInt(biomeCenters[i].x, biomeCenters[i].y);
            biomeCenters[i] += new Vector3Int(domainWarpingOffset.x, 0, domainWarpingOffset.y);
        }

        biomeNoise = CalculateBiomeNoise(biomeCenters);
    }

    private List<float> CalculateBiomeNoise(List<Vector3Int> biomeCenters)
    {
        return biomeCenters.Select(center => Noise.OctavePerlin(new Vector2(center.x, center.z), biomeNoiseSettings)).ToList();
    }
    public byte GetHex(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);
        if (!IsHexInWorld(pos)) return 0;

        /* BIOME SELECTION Pass*/

        //BiomeSelector biomeSelection = SelectBiomeAttributes(pos);

        //BiomeAttributes biome = biomeSelection.biomeAttributes;


        //int terrainHeight;
        //if (biomeSelection.terrainSurfaceNoise.HasValue == false)
        //{
        //    terrainHeight = GetSurfaceHeightNoise(pos.x, pos.z, biome);
        //}
        //else
        //{
        //    terrainHeight = biomeSelection.terrainSurfaceNoise.Value;
        //}
        //terrainHeight = GetSurfaceHeightNoise(pos.x, pos.z, biome);
        /* Basic Terrain Pass*/

        BiomeAttributes biome = biomes[0];

        int terrainHeight = GetSurfaceHeightNoise(pos.x, pos.z, biome);

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
            if (yPos <= 7)
                return 8;
            else
                return 0;
        }
        else
        {
            voxelValue = 1;
        }
        if (terrainHeight < 7)
        {
            voxelValue = 7;
        }
        /*Second Pass*/

        if(voxelValue == 1)
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

    private BiomeSelector SelectBiomeAttributes(Vector3 position,bool useDomainWarping = true)
    {
        if (useDomainWarping) {

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
        foreach(var data in biomeAttributesData)
        {
            if(temp > data.temperatureStartThreshold && temp< data.temperatureEndThreshold)
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
        new BiomeSelectionHelper {
            Index = index,
            Distance = Vector3.Distance(center,position)
        }).OrderBy(helper=> helper.Distance).Take(4).ToList();
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
        Gizmos.color = Color.blue;

        foreach(var biomeCenterPoint in biomeCenters)
        {
            Gizmos.DrawLine(biomeCenterPoint, biomeCenterPoint + Vector3.up * 255);
        }
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