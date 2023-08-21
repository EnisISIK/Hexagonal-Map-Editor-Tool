using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public int seed;
    public BiomeAttributes biome;

    public Transform player;
    public Vector3 spawnPosition;

    public Material material;
    public BlockType[] blocktypes;

    Chunk[,] chunks = new Chunk[HexData.WorldSizeInChunks+100, HexData.WorldSizeInChunks + 100];
    public static Dictionary<Vector3Int, Chunk> chunksDictionary;

    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    public static Dictionary<Vector2Int, ChunkCoord> activeChunksDictionary;

    public ChunkCoord playerCurrentChunkCoord;
    ChunkCoord playerLastChunkCoord;
    List<ChunkCoord> chunksToCreate= new List<ChunkCoord>();

    private bool isCreatingChunks;

    public GameObject debugScreen;
    private float[,] noiseMap;

    // TODO: may implement a system for chunk that holds real calculated position and position relative to other chunks separate(done, just need a bit cleaning)
    // TODO: make blocktypes enum or a scriptable object
    // TODO: add creation stack
    // TODO: work on random generated seeds
    // FIX: some of the chunks do not reload after getting into view distance when you go beyond that chunk disappears and never reloads
    // FIX: perlin noise is same for values below zero
    // TODO: clean code and turn repeating codes to functions

    private void Start()
    {

        chunksDictionary = new Dictionary<Vector3Int, Chunk>();
        activeChunksDictionary = new Dictionary<Vector2Int, ChunkCoord>();

        Random.InitState(seed);
        int centerChunk = (HexData.WorldSizeInChunks * HexData.ChunkWidth) / 2;
        spawnPosition.x = (centerChunk + centerChunk * 0.5f - centerChunk / 2) * (HexData.innerRadius * 2f);
        spawnPosition.y = HexData.ChunkHeight -50f;
        spawnPosition.z = centerChunk * (HexData.outerRadius * 1.5f);
        noiseMap = Noise.GenerateNoiseMap(HexData.WorldSizeInBlocks, HexData.ChunkHeight, biome.terrainScale);

        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
    }
    private void Update()
    {
        playerCurrentChunkCoord = GetChunkCoordFromVector3(player.position);
        if (!playerCurrentChunkCoord.Equals(playerLastChunkCoord))
        { 
            CheckViewDistance();
            playerLastChunkCoord = playerCurrentChunkCoord; 
        }

        if (chunksToCreate.Count > 0 && !isCreatingChunks)
        {
            StartCoroutine("CreateChunks");
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            debugScreen.SetActive(!debugScreen.activeSelf);
        }
    }
    void GenerateWorld()
    {
        for(int x = (HexData.WorldSizeInChunks / 2)-HexData.ViewDistanceinChunks; x < (HexData.WorldSizeInChunks / 2) + HexData.ViewDistanceinChunks; x++)
        {
            for (int z = (HexData.WorldSizeInChunks / 2) - HexData.ViewDistanceinChunks; z < (HexData.WorldSizeInChunks / 2) + HexData.ViewDistanceinChunks; z++)
            {
                chunksDictionary.Add(new Vector3Int(x, 0, z), new Chunk(new ChunkCoord(x, z), this, true));
                //chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, true);
                activeChunks.Add(new ChunkCoord(x, z));
            }
        }
        player.position = spawnPosition;
    }

    IEnumerator CreateChunks()
    {
        isCreatingChunks = true;
        
        while(chunksToCreate.Count > 0)
        {
            chunksDictionary[new Vector3Int(chunksToCreate[0].x, 0, chunksToCreate[0].z)].Init();
            //chunks[chunksToCreate[0].x, chunksToCreate[0].z].Init();
            chunksToCreate.RemoveAt(0);
            yield return null;
        }

        isCreatingChunks = false;
    }

    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        float coordplacex = (pos.x / (HexData.innerRadius * 2f)) - (pos.z * 0.5f) + (pos.z / 2);
        float coordplacez = (pos.z / (HexData.outerRadius * 1.5f));
        int x = Mathf.FloorToInt(coordplacex / HexData.ChunkWidth);
        int z = Mathf.FloorToInt(coordplacez / HexData.ChunkWidth);
        return new ChunkCoord(x, z);

    }

    public Vector3 GetVector3FromGlobalVector3(Vector3 pos)
    {
        float coordplacex = (pos.x / (HexData.innerRadius * 2f)) - (pos.z * 0.5f) + (pos.z / 2);
        float coordplacez = (pos.z / (HexData.outerRadius * 1.5f));
        int x = Mathf.RoundToInt(coordplacex);
        int z = Mathf.RoundToInt(coordplacez);

        return new Vector3(x, pos.y, z);

    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        float coordplacex = Mathf.RoundToInt((pos.x / (HexData.innerRadius * 2f)) - (pos.z * 0.5f) + (pos.z / 2));
        float coordplacez = Mathf.RoundToInt((pos.z / (HexData.outerRadius * 1.5f)));

        int x = Mathf.FloorToInt(coordplacex / HexData.ChunkWidth);
        int z = Mathf.FloorToInt(coordplacez / HexData.ChunkWidth);

        return chunksDictionary[new Vector3Int(x, 0, z)];

    }
    public Chunk GetChunkFromChunkVector3(Vector3 pos)
    {

        int x = Mathf.FloorToInt(pos.x / HexData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / HexData.ChunkWidth);

        return chunksDictionary[new Vector3Int(x, 0, z)];

    }

    public Vector3 AxialToOddr(Vector2 hex)
    {
        float x = hex.x;
        float y = hex.y + (hex.x - (Mathf.FloorToInt(hex.x) & 1)) / 2;

        return new Vector3(y,0,x);
    }
    public Vector2 CubeToAxial(Vector3 hex)
    {
        float q = hex.z;
        float r = hex.x;

        return new Vector2(r,q);
    }
    public Vector3 AxialToCube(Vector2 hex)
    {
        float q = hex.y;
        float r = hex.x;
        float s = -q - r;

        return new Vector3(r, s, q);
    }

    public Vector3 PixelToHex(Vector3 pos)
    {
        float q = (Mathf.Sqrt(3) / 3 * pos.x - 1.0f/3*pos.z);
        float r = (                            2.0f/3*pos.z);

        return AxialToOddr(AxialRound(new Vector2(r,q)));
    }

    public Vector3 CubeRound(Vector3 pos)
    {
        int q = Mathf.RoundToInt(pos.z);
        int r = Mathf.RoundToInt(pos.x);
        int s = Mathf.RoundToInt(pos.y);

        float qDiff = Mathf.Abs(q - pos.z);
        float rDiff = Mathf.Abs(r - pos.x);
        float sDiff = Mathf.Abs(s - pos.y);

        if (qDiff > rDiff && qDiff > sDiff)
            q = -r - s;
        else if (rDiff > sDiff)
            r = -q - s;
        else
            s = -q - r;

        return new Vector3(r, s, q);
    }

    public Vector2 AxialRound(Vector2 pos)
    {
        return CubeToAxial(CubeRound(AxialToCube(pos)));
    }

    void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkCoordFromVector3(player.position); //çalışıyor gibi ama active chunk reset olayına dikkat et. O sayıları değiştir bakıyım nolcak patlayacaz mı

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        activeChunks.Clear();

        for (int x = coord.x - HexData.ViewDistanceinChunks; x < coord.x + HexData.ViewDistanceinChunks; x++)
        {
            for (int z = coord.z - HexData.ViewDistanceinChunks; z < coord.z + HexData.ViewDistanceinChunks; z++)
            {
                //if (IsChunkInWorld(new ChunkCoord(x,z)))
                //{
                    if (!chunksDictionary.ContainsKey(new Vector3Int(x, 0, z))) //(chunks[x,z]== null)
                    {
                        chunksDictionary.Add(new Vector3Int(x, 0, z), new Chunk(new ChunkCoord(x, z), this, false));
                        //chunks[x, z] = new Chunk(new ChunkCoord(x, z), this,false);
                        chunksToCreate.Add(new ChunkCoord(x, z));
                    }
                    else if (!chunksDictionary[new Vector3Int(x, 0, z)].isActive) //(!chunks[x,z].isActive)
                    {
                        chunksDictionary[new Vector3Int(x, 0, z)].isActive = true;
                        //chunks[x, z].isActive = true;
                    }
                    activeChunks.Add(new ChunkCoord(x, z));
                //}
                for(int i= 0; i < previouslyActiveChunks.Count; i++)
                {
                    if (previouslyActiveChunks[i].Equals(new ChunkCoord(x,z)))
                    {
                        previouslyActiveChunks.RemoveAt(i);
                    }
                }
            }
        }
        foreach(ChunkCoord _chunk in previouslyActiveChunks)
        {
            chunksDictionary[new Vector3Int(_chunk.x, 0, _chunk.z)].isActive = false;
            //chunks[_chunk.x, _chunk.z].isActive = false;
            activeChunks.Remove(new ChunkCoord(_chunk.x, _chunk.z));
        }
    }
    public bool CheckForHex(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);//GetChunkCoordFromVector3(pos);   bunu değiştirdin çalıştı

        if (!IsHexInWorld(pos))
        {
            return false;
        }
        if(chunksDictionary.ContainsKey(new Vector3Int(thisChunk.x, 0, thisChunk.z))&& chunksDictionary[new Vector3Int(thisChunk.x, 0, thisChunk.z)].isHexMapPopulated) //(chunks[thisChunk.x, thisChunk.z]!=null&& chunks[thisChunk.x, thisChunk.z].isHexMapPopulated)
        {
            return blocktypes[chunksDictionary[new Vector3Int(thisChunk.x, 0, thisChunk.z)].GetHexFromGlobalVector3(pos)].isSolid; //blocktypes[chunks[thisChunk.x, thisChunk.z].GetHexFromGlobalVector3(pos)].isSolid;
        }
        return blocktypes[GetHex(pos)].isSolid;
    }
    public byte GetHex(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);
        if (!IsHexInWorld(pos))
        {
            return 0;
        }

        /* Basic Terrain Pass*/
        
        int terrainHeight = Mathf.FloorToInt(biome.terrainHeight*Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0 , biome.terrainScale)+biome.solidGroundHeight);
        //int terrainHeight = Mathf.FloorToInt(noiseMap[(int) pos.x,(int) pos.z]+ biome.solidGroundHeight);
        byte voxelValue = 0;

        if (yPos == terrainHeight)
        {
            voxelValue = 2;
        }
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
        {
            voxelValue = 4;
        }
        else if (yPos > terrainHeight)
        {
            return 0;
        }
        else
        {
            voxelValue = 1;
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
        return voxelValue;
    }

    bool IsChunkInWorld(ChunkCoord coord)
    {
        return coord.x >= 0 && coord.x < HexData.WorldSizeInChunks && coord.z >= 0 && coord.z < HexData.WorldSizeInChunks; //5'ler world size in cunks

    }

    bool IsHexInWorld(Vector3 pos)
    {
        return pos.x >= -800 && pos.x < HexData.WorldSizeInBlocks+ 800 && pos.y >= 0 && pos.y < HexData.ChunkHeight && pos.z >= -800 && pos.z < HexData.WorldSizeInBlocks+ 800; //25'ler world size in hex 10 da chunk height
        //return pos.y >= 0 && pos.y < HexData.ChunkHeight; //25'ler world size in hex 10 da chunk height

    }
}

[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;

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
