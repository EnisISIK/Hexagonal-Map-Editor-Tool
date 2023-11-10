﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;
using System.Linq;
using System;
using System.Threading.Tasks;

public class World : MonoBehaviour
{
    private int seed = 45;
    private ChunkDataGenerator _chunkDataGenerator;
    public BiomeAttributes[] biomes;

    public BiomeAttributes OceanBiome;

    Dictionary<Vector3Int, BiomeAttributes> voronoiBiomeAttributesDict = new Dictionary<Vector3Int, BiomeAttributes>();
    public static Dictionary<Vector3Int, Vector3> voronoiCentersDictionary;
    public static Dictionary<Vector3Int, VoronoiSeed> voronoiCentersVoronoi;

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
    public static Dictionary<Vector3Int, HexState[,,]> chunksDataDictionary;

    HashSet<ChunkCoord> activeChunks = new HashSet<ChunkCoord>();
    public static Dictionary<Vector2Int, ChunkCoord> activeChunksDictionary;

    public ConcurrentQueue<ConcurrentQueue<HexMod>> modifications = new ConcurrentQueue<ConcurrentQueue<HexMod>>();

    public ChunkCoord playerCurrentChunkCoord;
    ChunkCoord playerLastChunkCoord;
    HashSet<ChunkCoord> chunksToCreate= new HashSet<ChunkCoord>();
    HashSet<Vector3Int> dataToCreate = new HashSet<Vector3Int>();
    HashSet<Chunk> chunksToUpdateHashSet = new HashSet<Chunk>();

    public ConcurrentQueue<Chunk> chunksToUpdate = new ConcurrentQueue<Chunk>();
    public HashSet<Chunk> chunksToUpdateHash = new HashSet<Chunk>();

    public GameObject debugScreen;

    private bool isCreatingChunks;
    public bool isUpdatingChunks;
    public bool isApplyingModifications;
    private bool isCreatingData;

    public float HumidityOffset = 100f;

    [Range(0.95f, 0f)]
    public float globalLightLevel;

    // TODO: make blocktypes enum or a scriptable object
    // TODO: add creation stack
    // TODO: remove some of the coroutines
    // TODO: hash veya başka bir yapıya çevir hangisine bilmiyorum. Ve yield return null nereye koyduğun önemli. Blockingi önlemek için

    private void Start()
    {
        _chunkDataGenerator = new ChunkDataGenerator(this,domainWarping);
        chunksDictionary = new Dictionary<Vector3Int, Chunk>();
        chunksDataDictionary = new Dictionary<Vector3Int, HexState[,,]>();
        activeChunksDictionary = new Dictionary<Vector2Int, ChunkCoord>();
        voronoiCentersDictionary = new Dictionary<Vector3Int, Vector3>();
        voronoiCentersVoronoi = new Dictionary<Vector3Int, VoronoiSeed>();
        voronoiBiomeAttributesDict = new Dictionary<Vector3Int, BiomeAttributes>();
        CalculateSpawnPosition();

        player.position = spawnPosition;
        CheckViewDistance();
        playerLastChunkCoord = PositionHelper.GetChunkCoordFromVector3(PositionHelper.PixelToHex(player.position));
    }
    private void Update()
    {
        playerCurrentChunkCoord = PositionHelper.GetChunkCoordFromVector3(PositionHelper.PixelToHex(player.position));
        if (dataToCreate.Count > 0 && !isCreatingData)
            StartCoroutine(CreateDataCoroutine());  //işe yaradı. Its alive its alive
        if (chunksToCreate.Count > 0 && !isCreatingChunks)
        {
            CreateChunks();
        }
        if (chunksToUpdate.Count > 0 && !isUpdatingChunks)//&&!(dataToCreate.Count > 0)
        {
            StartCoroutine(UpdateChunksT());
        }
        if (modifications.Count > 0 && !isApplyingModifications)
            StartCoroutine(ApplyModifications());
        if (Input.GetKeyDown(KeyCode.F3))
        {
            debugScreen.SetActive(!debugScreen.activeSelf);
        }
        if (!playerCurrentChunkCoord.Equals(playerLastChunkCoord))
        {
            CheckViewDistance();
            playerLastChunkCoord = playerCurrentChunkCoord;
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
                if (chunksToUpdate.TryPeek(out Chunk var)) {
                        Vector3Int checkDir = var.position / HexData.ChunkWidth;
                        if (chunksDataDictionary.TryGetValue(checkDir, out HexState[,,] hexMap))
                        {
                                // if(chunksDataDictionary.TryGetValue(var.position / HexData.ChunkWidth, out HexState[,,] hexMap))
                                if (chunksToUpdate.TryDequeue(out Chunk var1))
                                {
                                    //StartCoroutine(var1.UpdateChunk(hexMap));
                                    //yield return null; 
                                    yield return var1.UpdateChunk(hexMap);  //burayı değiştirdik bir tık yavaş çalışıyor
                                }
                        }
                }
            }
        //yield return ApplyModifications();  //silebilirsin denemelik
        isUpdatingChunks = false;
    }

    IEnumerator UpdateChunksT()
    {
        isUpdatingChunks = true;
        while (chunksToUpdate.Count > 0)
        {
            if (chunksToUpdate.TryDequeue(out Chunk var))
            {
                Vector3Int checkDir = var.position / HexData.ChunkWidth;
                if (chunksDataDictionary.TryGetValue(checkDir, out HexState[,,] hexMap)&&
                    chunksDataDictionary.ContainsKey(new Vector3Int(checkDir.x, 0, checkDir.z-1))&&
                    chunksDataDictionary.ContainsKey(new Vector3Int(checkDir.x, 0, checkDir.z+1)) &&
                    chunksDataDictionary.ContainsKey(new Vector3Int(checkDir.x-1, 0, checkDir.z)) &&
                    chunksDataDictionary.ContainsKey(new Vector3Int(checkDir.x+1, 0, checkDir.z)) &&
                    chunksDataDictionary.ContainsKey(new Vector3Int(checkDir.x-1, 0, checkDir.z-1)) &&
                    chunksDataDictionary.ContainsKey(new Vector3Int(checkDir.x - 1, 0, checkDir.z + 1)) &&
                    chunksDataDictionary.ContainsKey(new Vector3Int(checkDir.x + 1, 0, checkDir.z - 1)) &&
                    chunksDataDictionary.ContainsKey(new Vector3Int(checkDir.x + 1, 0, checkDir.z+1)))
                {
                        yield return var.UpdateChunk(hexMap);
                }
                else
                {
                    chunksToUpdate.Enqueue(var);
                }
            }
            yield return null;
        }
        isUpdatingChunks = false;
    }

    public IEnumerator ApplyModifications()
    {
        isApplyingModifications = true;

        HashSet<ChunkCoord> coordHashes = new HashSet<ChunkCoord>();
        while (modifications.Count > 0)
        {
            
                ConcurrentQueue<HexMod> queueNotFinished = new ConcurrentQueue<HexMod>();
                modifications.TryDequeue(out ConcurrentQueue<HexMod> queue);
                ChunkCoord c;  //bu c'yi hashset yapıp queue sona erdikten sonra ekleyebilirsin aslında kaç tane farklı chunk varsa
                while (queue.Count > 0)
                {
                    queue.TryDequeue(out HexMod v);

                    c = PositionHelper.GetChunkCoordFromVector3(v.position);

                    if (chunksDataDictionary.TryGetValue(new Vector3Int(c.x, 0, c.z), out HexState[,,] var))
                    {
                        Vector3Int newPos = new Vector3Int(Mathf.FloorToInt(v.position.x - (c.x * HexData.ChunkWidth)), Mathf.FloorToInt(v.position.y), Mathf.FloorToInt(v.position.z - (c.z * HexData.ChunkWidth)));
                        if (!blocktypes[var[newPos.x, newPos.y, newPos.z].id].isSolid)
                        {
                            coordHashes.Add(c);
                            var[newPos.x, newPos.y, newPos.z].id = v.id;
                        }
                    }
                    else
                    {
                        queueNotFinished.Enqueue(v);
                    }
                }  // queue while

                if (queueNotFinished.Count > 0)
                    modifications.Enqueue(queueNotFinished);

            yield return null;
        }  // modifications while
        HashSet<ChunkCoord> modifiedChunksCopy = new HashSet<ChunkCoord>(coordHashes);
        foreach (ChunkCoord chunkCoord in modifiedChunksCopy)
        {
            if (!chunksToCreate.Contains(chunkCoord))
            {
                if (chunksDictionary.TryGetValue(new Vector3Int(chunkCoord.x, 0, chunkCoord.z), out Chunk chunk))
                {
                    //continue; //burayı düzeltirsen tamamdır galiba
                    bool itemExists = false;
                    foreach (Chunk item in chunksToUpdate)
                    {
                        if (item.Equals(chunk))
                        {
                            itemExists = true;
                            break;
                        }
                    }
                    if (itemExists == false) chunksToUpdate.Enqueue(chunk); //burayı değiştirdik bir tık yavaş çalışıyor update chunksı değiştirdim ama düzelecek
                    coordHashes.Remove(chunkCoord);  //yerinden tam emin değilim
                }
            }
            else
            {
                //chunksToCreate.Add(chunkCoord);
            }
        }
        isApplyingModifications = false;
    }

    private void CreateChunks()
    {
        StartCoroutine(CreateChunksCoroutine());
    }

    private IEnumerator CreateChunksCoroutine()
    {
        isCreatingChunks = true;

        while (chunksToCreate.Count > 0)
        {
            HashSet<ChunkCoord> chunksToCreateCopy = new HashSet<ChunkCoord>(chunksToCreate);
            foreach (ChunkCoord chunkPos in chunksToCreateCopy) {
                if (chunksDataDictionary.ContainsKey(new Vector3Int(chunkPos.x, 0, chunkPos.z))) {
                    if(!chunksDictionary.ContainsKey(new Vector3Int(chunkPos.x, 0, chunkPos.z)))
                    {
                        chunksDictionary.Add(new Vector3Int(chunkPos.x, 0, chunkPos.z), new Chunk(new ChunkCoord(chunkPos.x, chunkPos.z), this));
                        chunksDictionary[new Vector3Int(chunkPos.x, 0, chunkPos.z)].Init();
                        chunksToCreate.Remove(chunkPos);
                    }
                }
                yield return null;
            }
        }
        isCreatingChunks = false;
    }

    private IEnumerator PopulateCoroutine(Vector3Int chunkPos)
    {
        HexState[,,] tmpData = null;
        
        yield return _chunkDataGenerator.GenerateData(chunkPos * HexData.ChunkWidth, voronoiCentersVoronoi, x => tmpData = x);
        
        if (!chunksDataDictionary.ContainsKey(chunkPos))
            chunksDataDictionary.Add(chunkPos, tmpData);
    }

    private IEnumerator CreateDataCoroutine()
    {
        isCreatingData = true;

        while (dataToCreate.Count > 0)
        {
            HashSet<Vector3Int> chunksToCreateCopy = new HashSet<Vector3Int>(dataToCreate);
            foreach (Vector3Int chunkPos in chunksToCreateCopy)
            {
                yield return PopulateCoroutine(chunkPos);
                dataToCreate.Remove(chunkPos);
            }
            yield return null;
        }
        isCreatingData = false;
    }

    public HexState[,,] RequestChunk(Vector3 chunkPos)
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

    private void CheckViewDistance()
    {

        ChunkCoord coord = PositionHelper.GetChunkCoordFromVector3(PositionHelper.PixelToHex(player.position));
        GenerateVoronoiSeeds(coord, HexData.ViewDistanceinChunks, HexData.ChunkWidth);
        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        activeChunks.Clear();

        Vector3Int chunkPos;
        ChunkCoord chunkCoord;
        for (int x = coord.x - HexData.ViewDistanceinChunks - 1; x < coord.x + HexData.ViewDistanceinChunks + 1; x++)
        {
            for (int z = coord.z - HexData.ViewDistanceinChunks - 1; z < coord.z + HexData.ViewDistanceinChunks + 1; z++)
            {
                chunkPos = new Vector3Int(x, 0, z);
                if (!chunksDataDictionary.ContainsKey(chunkPos)) dataToCreate.Add(chunkPos);//Populate(chunkPos); // add to queue//bunu belki tekrar ayrı fora koyarsın belki daha hızlanır
            }
        }

        for (int x = coord.x - HexData.ViewDistanceinChunks ; x < coord.x + HexData.ViewDistanceinChunks; x++)
        {
            for (int z = coord.z - HexData.ViewDistanceinChunks ; z < coord.z + HexData.ViewDistanceinChunks; z++)
            {
                chunkPos = new Vector3Int(x, 0, z);
                chunkCoord = new ChunkCoord(x, z);
                
                if (!chunksDictionary.TryGetValue(chunkPos, out Chunk var))
                {
                    chunksToCreate.Add(chunkCoord);
                }
                else if (!var.isActive)
                {
                    var.isActive = true;
                }
                activeChunks.Add(chunkCoord);
            }
        }

        foreach (ChunkCoord chunkCoord1 in activeChunks) { 
            for (int i = 0; i < previouslyActiveChunks.Count; i++)
                if (previouslyActiveChunks[i].Equals(chunkCoord1))
                    previouslyActiveChunks.RemoveAt(i);
        }

        foreach (ChunkCoord _chunk in previouslyActiveChunks)
        {
            if (chunksDictionary.TryGetValue(new Vector3Int(_chunk.x, 0, _chunk.z), out Chunk var)) var.isActive = false;

            activeChunks.Remove(new ChunkCoord(_chunk.x, _chunk.z));
        }
    }

    public void EditHex(Vector3 pos, byte newID)
    {
        Vector3Int chunkPos = PositionHelper.GetChunkFromVector3(pos);
        Vector3Int inChunkPosition = PositionHelper.GetInChunkPosition(pos,chunkPos);

        if (!chunksDataDictionary.TryGetValue(chunkPos, out HexState[,,] chunkMap)) return;
        chunkMap[inChunkPosition.x, inChunkPosition.y, inChunkPosition.z].id = newID;

        if (!chunksDictionary.TryGetValue(chunkPos, out Chunk chunk)) return;
        StartCoroutine(chunk.UpdateSurroundingHex(inChunkPosition.x, inChunkPosition.y, inChunkPosition.z, newID));
    }

    public bool CheckForHex(Vector3 pos)
    {
        if (!IsHexInWorld(pos)) 
            return false;

        Vector3Int chunkPos = PositionHelper.GetChunkFromVector3(pos);
        if (!chunksDataDictionary.TryGetValue(chunkPos,out HexState[,,] var)) 
            return false;

        Vector3Int inChunkPos = PositionHelper.GetInChunkPosition(pos,chunkPos);
        return blocktypes[var[inChunkPos.x, inChunkPos.y, inChunkPos.z].id].isSolid;
    }

    //yukarıdaki 3 fonksiyon birleşecek
    public bool CheckForWaterHex(Vector3 pos)
    {
        if (!IsHexInWorld(pos)) return false;

        Vector3Int chunkPos = PositionHelper.GetChunkFromVector3(pos);
        if (!chunksDataDictionary.TryGetValue(chunkPos, out HexState[,,] var)) return false;

        Vector3Int inChunkPos = PositionHelper.GetInChunkPosition(pos, chunkPos);
        return blocktypes[var[inChunkPos.x, inChunkPos.y, inChunkPos.z].id].isWater;
    }

    //Biome Center Generatora taşınacak veya başka biyer
    private void GenerateVoronoiSeeds(ChunkCoord coord, int drawRange, int mapSize)
    {
        Dictionary<Vector3Int, VoronoiSeed> tempTest = BiomeCenterFinder.CalculateBiomeCentersCopy(mapSize, drawRange, coord, this);

        foreach (var centerSeed in tempTest)
        {
            if (voronoiCentersVoronoi.ContainsKey(centerSeed.Key)) continue;

            voronoiCentersVoronoi.Add(centerSeed.Key, centerSeed.Value);
        }
    }

    //Biome Center Generatora taşınacak veya başka biyer
    public BiomeAttributes SelectBiomes(float temp, float moisture)
    {
        foreach (var data in biomeAttributesData)
        {
            if (temp > data.temperatureStartThreshold && temp < data.temperatureEndThreshold
                && moisture > data.humidityStartThreshold && moisture < data.humidityEndThreshold)
            {
                return data.Biome;
            }
        }
        return biomeAttributesData[0].Biome;
    }

    public byte GetHex(Vector3 pos,BiomeSelector biomeSelection=null)  //bunu parçalayacan galiba
    {
        int yPos = Mathf.FloorToInt(pos.y);
        if (!IsHexInWorld(pos)) return 0;

        /* Basic Terrain Pass*/
        int terrainHeight;
        BiomeAttributes biome;
        if (biomeSelection != null)
        {
            biome = biomeSelection.biomeAttributes;
            if (biomeSelection.terrainSurfaceNoise.HasValue == false)
            {
                terrainHeight = PositionHelper.GetSurfaceHeightNoise(pos.x, pos.z, biome,domainWarping);
                terrainHeight += 5;
            }
            else
            {
               terrainHeight = biomeSelection.terrainSurfaceNoise.Value;
               terrainHeight += 5;
            }
        }
        else { biome = biomes[0];
            terrainHeight = PositionHelper.GetSurfaceHeightNoise(pos.x, pos.z, biome, domainWarping);
            terrainHeight += 5;
        }
        //terrainHeight = GetSurfaceHeightNoise(pos.x, pos.z, biome);
        if (biome == OceanBiome)
        {
            terrainHeight = 4;
            terrainHeight += 5;
        }

        byte voxelValue;

        if (yPos == terrainHeight)
        {
            voxelValue = biome.surfaceBlock;  //terrain composition
        }
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
        {
            voxelValue = biome.subSurfaceBlock; //terrain composition
        }
        else if (yPos > terrainHeight)
        {
            if (yPos <= 5&&biome==OceanBiome)
                return 8;
            else
                return 0; //terrain composition
        }
        else
        {
            voxelValue = 1;  //terrain composition
        }
        if (terrainHeight < 5 && biome == OceanBiome)
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

        if (yPos == terrainHeight && biome.placeFlora && biome != OceanBiome)
        {

            if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.floraZoneScale) > biome.floraZoneThreshold)
            {

                if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.floraPlacementScale) > biome.floraPlacementThreshold)
                {
                    ConcurrentQueue<HexMod> structure = Structure.GenerateMajorFlora(biome.floraIndex, pos, biome.minFloraHeight, biome.maxFloraHeight);
                    modifications.Enqueue(structure);
                }
            }
        }
        return voxelValue;
    }


    private bool IsHexInWorld(Vector3 pos)
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
    public bool renderNeighbourFaces;
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
public struct VoronoiSeed
{
    public BiomeAttributes voronoiBiome;
    public Vector3 voronoiPosition;

    public VoronoiSeed(BiomeAttributes voronoiBiome, Vector3 voronoiPosition)
    {
        this.voronoiBiome = voronoiBiome;
        this.voronoiPosition = voronoiPosition;
    }
}