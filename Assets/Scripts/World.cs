using System.Collections;
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

    [SerializeField]
    private BiomeAttributes standartBiome;

    public BiomeAttributes OceanBiome;

    [SerializeField]
    private NoiseSettings biomeNoiseSettings;

    [SerializeField]
    private DomainWarping domainWarping;

    [SerializeField]
    private List<BiomeData> biomeTable = new List<BiomeData>();

    public Transform player;

    public Material material;
    public Material transparentMaterial;
    public Material waterMaterial;

    public BlockType[] blocktypes;

    public static Dictionary<Vector3Int, Chunk> chunksDictionary;
    public static Dictionary<Vector3Int, byte[,,]> chunksDataDictionary;
    public static Dictionary<Vector3Int, VoronoiSeed> biomeCentersDictionary;

    private HashSet<ChunkCoord> chunksToCreate = new HashSet<ChunkCoord>();
    private HashSet<Vector3Int> dataToCreate = new HashSet<Vector3Int>();
    public ConcurrentQueue<ConcurrentQueue<HexMod>> finishersToAdd = new ConcurrentQueue<ConcurrentQueue<HexMod>>();
    public ConcurrentQueue<Chunk> chunksToUpdate = new ConcurrentQueue<Chunk>();

    private HashSet<ChunkCoord> activeChunks = new HashSet<ChunkCoord>();

    private List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>();

    public ChunkCoord playerCurrentChunkCoord;
    private ChunkCoord playerLastChunkCoord;

    public GameObject debugScreen;

    private bool isCreatingData;
    private bool isCreatingChunks;
    private bool isUpdatingChunks;
    private bool isAddingFinishers;

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
        chunksDataDictionary = new Dictionary<Vector3Int, byte[,,]>();
        biomeCentersDictionary = new Dictionary<Vector3Int, VoronoiSeed>();

        player.position = CalculateSpawnPosition();
        CheckViewDistance();
        playerLastChunkCoord = PositionHelper.GetChunkCoordFromVector3(PositionHelper.PixelToHex(player.position));
    }

    private void Update()
    {
        playerCurrentChunkCoord = PositionHelper.GetChunkCoordFromVector3(PositionHelper.PixelToHex(player.position));
        if (dataToCreate.Count > 0 && !isCreatingData)
        {
            StartCoroutine(CreateDataCoroutine());
        }
        if (chunksToCreate.Count > 0 && !isCreatingChunks)
        {
            StartCoroutine(CreateChunksCoroutine());
        }
        if (chunksToUpdate.Count > 0 && !isUpdatingChunks)
        {
            StartCoroutine(UpdateChunksCoroutineALTERNATIVE());
        }
        if (finishersToAdd.Count > 0 && !isAddingFinishers)
            StartCoroutine(AddFinishersCoroutine());
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

    private Vector3 CalculateSpawnPosition()
    {
        int centerChunk = (HexData.WorldSizeInChunks * HexData.ChunkWidth) / 2;
        float x = (centerChunk + centerChunk * 0.5f - centerChunk / 2) * (HexData.innerRadius * 2f);
        float y = HexData.ChunkHeight - 50f;
        float z = centerChunk * (HexData.outerRadius * 1.5f);

        return new Vector3(x, y, z);
    }

    private IEnumerator CreateChunksCoroutine()
    {
        isCreatingChunks = true;
        while (chunksToCreate.Count > 0)
        {
            List<ChunkCoord> chunksToCreateList = new List<ChunkCoord>(chunksToCreate);

            foreach (ChunkCoord chunkPos in chunksToCreateList)
            {
                Vector3Int chunkPosition = new Vector3Int(chunkPos.x, 0, chunkPos.z);

                if (chunksDataDictionary.ContainsKey(chunkPosition) && !chunksDictionary.ContainsKey(chunkPosition))
                {
                    Chunk newChunk = new Chunk(chunkPos, this);
                    chunksDictionary.Add(chunkPosition, newChunk);
                    newChunk.Init();
                    chunksToCreate.Remove(chunkPos);
                }
            }
            yield return null;
        }

        isCreatingChunks = false;
    }

    private IEnumerator UpdateChunksCoroutineALTERNATIVE()
    {
        isUpdatingChunks = true;

        int maxConcurrentCoroutines = 8;
        int runningCoroutines = 0;

        int updateCount = chunksToUpdate.Count;
        while (updateCount > 0)
        {
            if (runningCoroutines < maxConcurrentCoroutines) {
                if (chunksToUpdate.TryDequeue(out Chunk var))
                {
                    Vector3Int checkDirection = var.position / HexData.ChunkWidth;
                    if (chunksDataDictionary.TryGetValue(checkDirection, out byte[,,] hexMap) &&
                        DoesNeighbouringDataExists(checkDirection))
                    {
                        //yield return var.UpdateChunk(hexMap);
                        runningCoroutines++;
                        StartCoroutine(var.UpdateChunk(hexMap, () => runningCoroutines--));
                    }
                    else
                    {
                        chunksToUpdate.Enqueue(var);
                    }
                }
            }
            yield return null;
        }
        isUpdatingChunks = false;
    }
    private bool DoesNeighbouringDataExists(Vector3Int checkDirection)
    {
        List<Vector3Int> neighboringKeys = new List<Vector3Int>
        {
        new Vector3Int(checkDirection.x, 0, checkDirection.z - 1),
        new Vector3Int(checkDirection.x, 0, checkDirection.z + 1),
        new Vector3Int(checkDirection.x - 1, 0, checkDirection.z),
        new Vector3Int(checkDirection.x + 1, 0, checkDirection.z),
        new Vector3Int(checkDirection.x - 1, 0, checkDirection.z - 1),
        new Vector3Int(checkDirection.x - 1, 0, checkDirection.z + 1),
        new Vector3Int(checkDirection.x + 1, 0, checkDirection.z - 1),
        new Vector3Int(checkDirection.x + 1, 0, checkDirection.z + 1),
        };

        foreach (Vector3Int key in neighboringKeys)
        {
            if (!chunksDataDictionary.ContainsKey(key))
            {
                return false;
            }
        }

        return true;
    }
    public bool DoesDataExists(Vector3Int chunkPosition)
    {
        if (!chunksDataDictionary.ContainsKey(chunkPosition)) return false;

        return true;
    }

    private IEnumerator AddFinishersCoroutine()
    {
        isAddingFinishers = true;

        Task t = Task.Factory.StartNew(delegate
        {
            HashSet<ChunkCoord> coordHashes = new HashSet<ChunkCoord>();
            ConcurrentQueue<HexMod> queueNotFinished = new ConcurrentQueue<HexMod>();
            finishersToAdd.TryDequeue(out ConcurrentQueue<HexMod> queue);
            while (queue.Count > 0)
            {
                queue.TryDequeue(out HexMod v);

                ChunkCoord c = PositionHelper.GetChunkCoordFromVector3(v.position);

                if (chunksDataDictionary.TryGetValue(new Vector3Int(c.x, 0, c.z), out byte[,,] var))
                {
                    Vector3Int newPos = new Vector3Int(Mathf.FloorToInt(v.position.x - (c.x * HexData.ChunkWidth)), Mathf.FloorToInt(v.position.y), Mathf.FloorToInt(v.position.z - (c.z * HexData.ChunkWidth)));
                    if (!blocktypes[var[newPos.x, newPos.y, newPos.z]].isSolid)
                    {
                        coordHashes.Add(c);
                        var[newPos.x, newPos.y, newPos.z] = v.id;
                    }
                }
                else
                {
                    queueNotFinished.Enqueue(v);
                }
            }  // queue while

            if (queueNotFinished.Count > 0)
            {
                finishersToAdd.Enqueue(queueNotFinished);
            }
            List<ChunkCoord> modifiedChunksCopy = new List<ChunkCoord>(coordHashes);
            foreach (ChunkCoord chunkCoord in modifiedChunksCopy)
            {
                if (chunksToCreate.Contains(chunkCoord)) continue;
                if (!chunksDictionary.TryGetValue(new Vector3Int(chunkCoord.x, 0, chunkCoord.z), out Chunk chunk)) continue;

                bool itemExists = false;
                foreach (Chunk item in chunksToUpdate)
                {
                    if (item.Equals(chunk))
                    {
                        itemExists = true;
                        break;
                    }
                }
                if (itemExists == false) chunksToUpdate.Enqueue(chunk);
                coordHashes.Remove(chunkCoord);

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
        isAddingFinishers = false;
    }

    private IEnumerator CreateDataCoroutine()
    {
        isCreatingData = true;

        int maxConcurrentCoroutines = 8;
        int runningCoroutines = 0;

        while (dataToCreate.Count > 0)
        {
            List<Vector3Int> chunksToCreateCopy = new List<Vector3Int>(dataToCreate);
            foreach (Vector3Int chunkPos in chunksToCreateCopy)
            {
                if (runningCoroutines < maxConcurrentCoroutines)
                {
                    runningCoroutines++;
                    StartCoroutine(PopulateDataCoroutine(chunkPos, () => runningCoroutines--));
                    dataToCreate.Remove(chunkPos);
                }
            }
            yield return null;

        }
        isCreatingData = false;
    }

    private IEnumerator PopulateDataCoroutine(Vector3Int chunkPos,System.Action onComplete)
    {
        byte[,,] chunkData = null;
        //HexState[,,] chunkSurfaceData = null;

        yield return _chunkDataGenerator.GenerateData(chunkPos * HexData.ChunkWidth, biomeCentersDictionary, x => chunkData = x);//,s=>chunkSurfaceData=s

        //StartCoroutine=>AddFoliage(chunkSurfaceData) or Function=>AddFoliage(chunkSurfaceData)
        if (!chunksDataDictionary.ContainsKey(chunkPos))
            chunksDataDictionary.Add(chunkPos, chunkData);
        
        onComplete?.Invoke();
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

    private void CheckViewDistance()
    {

        ChunkCoord playerCoord = PositionHelper.GetChunkCoordFromVector3(PositionHelper.PixelToHex(player.position));
        GenerateVoronoiCenters(playerCoord, HexData.ChunkWidth);
        //List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        if (previouslyActiveChunks.Count == 0)
           previouslyActiveChunks = new List<ChunkCoord>(activeChunks);
        else
            previouslyActiveChunks.AddRange(activeChunks);

        activeChunks.Clear();

        Vector3Int chunkPos;
        ChunkCoord chunkCoord;
        int lowerDistanceX = playerCoord.x - HexData.ViewDistanceinChunks;
        int higherDistanceX = playerCoord.x + HexData.ViewDistanceinChunks;
        int lowerDistanceZ = playerCoord.z - HexData.ViewDistanceinChunks;
        int higherDistanceZ = playerCoord.z + HexData.ViewDistanceinChunks;

        for (int x = lowerDistanceX - 1; x < higherDistanceX + 1; x++)
        {
            for (int z = lowerDistanceZ - 1; z < higherDistanceZ + 1; z++)
            {
                chunkPos = new Vector3Int(x, 0, z);
                if (!chunksDataDictionary.ContainsKey(chunkPos)) dataToCreate.Add(chunkPos);
            }
        }

        for (int x = lowerDistanceX; x < higherDistanceX; x++)
        {
            for (int z = lowerDistanceZ; z < higherDistanceZ; z++)
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

        //foreach (ChunkCoord chunkCoord1 in activeChunks) { 
        //    for (int i = 0; i < previouslyActiveChunks.Count; i++)
        //        if (previouslyActiveChunks[i].Equals(chunkCoord1))
        //            previouslyActiveChunks.RemoveAt(i);
        //}
        foreach (ChunkCoord chunkCoord1 in activeChunks)
        {
            previouslyActiveChunks.RemoveAll(coord => coord.Equals(chunkCoord1));
        }

        //Deactivates the chunks Outside Render Distance
        foreach (ChunkCoord chunkCoordToDeactivate in previouslyActiveChunks)
        {
            if (chunksDictionary.TryGetValue(new Vector3Int(chunkCoordToDeactivate.x, 0, chunkCoordToDeactivate.z), out Chunk chunk)) 
                chunk.isActive = false;

            activeChunks.Remove(new ChunkCoord(chunkCoordToDeactivate.x, chunkCoordToDeactivate.z));
        }
    }


    //Destroys or places block
    public void EditHex(Vector3 pos, byte newID)
    {
        Vector3Int chunkPos = PositionHelper.GetChunkFromVector3(pos);
        if (!chunksDataDictionary.TryGetValue(chunkPos, out byte[,,] chunkMap)) 
            return;

        Vector3Int inChunkPosition = PositionHelper.GetInChunkPosition(pos, chunkPos);
        chunkMap[inChunkPosition.x, inChunkPosition.y, inChunkPosition.z] = newID;

        if (!chunksDictionary.TryGetValue(chunkPos, out Chunk chunk)) 
            return;

        StartCoroutine(chunk.UpdateSurroundingHex(inChunkPosition.x, inChunkPosition.y, inChunkPosition.z, newID));
    }


    //Checks if Hexagon is a Solid Block
    public bool CheckForHex(Vector3 pos)
    {
        if (!IsHexInWorld(pos)) 
            return false;

        Vector3Int chunkPos = PositionHelper.GetChunkFromVector3(pos);
        if (!chunksDataDictionary.TryGetValue(chunkPos,out byte[,,] var)) 
            return false;

        Vector3Int inChunkPos = PositionHelper.GetInChunkPosition(pos,chunkPos);
        return blocktypes[var[inChunkPos.x, inChunkPos.y, inChunkPos.z]].isSolid;
    }


    //Checks if Hexagon is a Transparent Block
    public bool CheckForTransparentHex(Vector3 pos)
    {
        if (!IsHexInWorld(pos))
            return false;

        Vector3Int chunkPos = PositionHelper.GetChunkFromVector3(pos);
        if (!chunksDataDictionary.TryGetValue(chunkPos, out byte[,,] var))
            return false;

        Vector3Int inChunkPos = PositionHelper.GetInChunkPosition(pos, chunkPos);
        return blocktypes[var[inChunkPos.x, inChunkPos.y, inChunkPos.z]].isTransparent;
    }


    //Checks if Hexagon is a Water Block
    public bool CheckForWaterHex(Vector3 pos)
    {
        if (!IsHexInWorld(pos)) 
            return false;

        Vector3Int chunkPos = PositionHelper.GetChunkFromVector3(pos);
        if (!chunksDataDictionary.TryGetValue(chunkPos, out byte[,,] var)) 
            return false;

        Vector3Int inChunkPos = PositionHelper.GetInChunkPosition(pos, chunkPos);
        return blocktypes[var[inChunkPos.x, inChunkPos.y, inChunkPos.z]].isWater;
    }


    //Scale of a a single biome. Calculated as biomeScale*ChunkWidth. Default is 4x16 -> 64
    private const int biomeScale = 4;
    //Generates Distorted Voronoi Centers
    private void GenerateVoronoiCenters(ChunkCoord playerCoord, int pixelsPerChunk)
    {
        Dictionary<Vector3Int, VoronoiSeed> generatedBiomeCenters = BiomeCenterFinder.GenerateBiomeCenters(this, pixelsPerChunk, HexData.MaxRenderChunks, playerCoord, biomeScale);

        foreach (var seedCenter in generatedBiomeCenters)
        {
            if (biomeCentersDictionary.ContainsKey(seedCenter.Key)) 
                continue;

            biomeCentersDictionary.Add(seedCenter.Key, seedCenter.Value);
        }
    }


    //Selects biome based on BiomeTable
    public BiomeAttributes SelectBiomes(float temperature, float moisture)
    {
        //return biome based on BiomeTable
        foreach (var data in biomeTable)
        {
            if (temperature >= data.temperatureStartThreshold && temperature <= data.temperatureEndThreshold
                && moisture >= data.humidityStartThreshold && moisture <= data.humidityEndThreshold)
            {
                return data.Biome;
            }
        }
        //return basic biome
        return biomeTable[0].Biome;
    }


    //Checks if Hexagon's position is inside the world's boundaries
    private bool IsHexInWorld(Vector3 pos)
    {
        return pos.y >= 0 && pos.y < HexData.ChunkHeight;
    }


}

[System.Serializable]
public class BlockType
{
    public BlockDataTypes blockDataType;
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
    public Vector3Int voronoiPosition;

    public VoronoiSeed(BiomeAttributes voronoiBiome, Vector3Int voronoiPosition)
    {
        this.voronoiBiome = voronoiBiome;
        this.voronoiPosition = voronoiPosition;
    }
}