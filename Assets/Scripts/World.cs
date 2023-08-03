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

    Chunk[,] chunks = new Chunk[HexData.WorldSizeInChunks, HexData.WorldSizeInChunks];

    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    ChunkCoord playerCurrentChunkCoord;
    ChunkCoord playerLastChunkCoord;

    // TODO: may implement a system for chunk that holds real calculated position and position relative to other chunks separate
    // TODO: make blocktypes enum or a scriptable object
    // TODO: add creation stack
    // TODO: work on random generated seeds

    private void Start()
    {
        Random.InitState(seed);
        int centerChunk = (HexData.WorldSizeInChunks * HexData.ChunkWidth) / 2;
        spawnPosition.x = (centerChunk + centerChunk * 0.5f - centerChunk / 2) * (HexData.innerRadius * 2f);
        spawnPosition.y = HexData.ChunkHeight + 2f;
        spawnPosition.z = centerChunk * (HexData.outerRadius * 1.5f);
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
    }
    private void Update()
    {
        playerCurrentChunkCoord = GetChunkCoordFromVector3(player.position);
        if (!playerCurrentChunkCoord.Equals(playerLastChunkCoord))
            CheckViewDistance();
            playerLastChunkCoord = playerCurrentChunkCoord;
    }
    void GenerateWorld()
    {
        for(int x = (HexData.WorldSizeInChunks / 2)-HexData.ViewDistanceinChunks; x < (HexData.WorldSizeInChunks / 2) + HexData.ViewDistanceinChunks; x++)
        {
            for (int z = (HexData.WorldSizeInChunks / 2) - HexData.ViewDistanceinChunks; z < (HexData.WorldSizeInChunks / 2) + HexData.ViewDistanceinChunks; z++)
            {
                CreateNewChunk(x, z);
            }
        }
        player.position = spawnPosition;
    }
    
    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        float coordplacex = (pos.x / (HexData.innerRadius * 2f)) - (pos.z * 0.5f) + (pos.z / 2);
        float coordplacez = (pos.z / (HexData.outerRadius * 1.5f));
        int x = Mathf.FloorToInt(coordplacex / HexData.ChunkWidth);
        int z = Mathf.FloorToInt(coordplacez / HexData.ChunkWidth);
        return new ChunkCoord(x, z);

    }
    void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        for (int x = coord.x - HexData.ViewDistanceinChunks; x < coord.x + HexData.ViewDistanceinChunks; x++)
        {
            for (int z = coord.z - HexData.ViewDistanceinChunks; z < coord.z + HexData.ViewDistanceinChunks; z++)
            {
                if (IsChunkInWorld(new ChunkCoord(x,z)))
                {
                    if(chunks[x,z]== null)
                    {
                        CreateNewChunk(x,z);
                    }
                    else if(!chunks[x,z].isActive)
                    {
                        chunks[x, z].isActive = true;
                        activeChunks.Add(new ChunkCoord(x, z));
                    }
                }
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
            chunks[_chunk.x, _chunk.z].isActive = false;
        }
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
    void CreateNewChunk(int _x, int _z)
    {
        chunks[_x,_z] = new Chunk(new ChunkCoord(_x, _z), this);
        activeChunks.Add(new ChunkCoord(_x, _z));
    }

    bool IsChunkInWorld(ChunkCoord coord)
    {
        return coord.x >= 0 && coord.x < HexData.WorldSizeInChunks && coord.z >= 0 && coord.z < HexData.WorldSizeInChunks; //5'ler world size in cunks

    }

    bool IsHexInWorld(Vector3 pos)
    {
        return pos.x >= 0 && pos.x < HexData.WorldSizeInBlocks && pos.y >= 0 && pos.y < HexData.ChunkHeight && pos.z >= 0 && pos.z < HexData.WorldSizeInBlocks; //25'ler world size in hex 10 da chunk height

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
