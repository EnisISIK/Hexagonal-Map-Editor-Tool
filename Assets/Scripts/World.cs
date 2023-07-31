using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public Material material;
    public BlockType[] blocktypes;

    Chunk[,] chunks = new Chunk[HexData.WorldSizeInChunks, HexData.WorldSizeInChunks];

    private void Start()
    {
        GenerateWorld();
    }

    void GenerateWorld()
    {
        for(int x = 0; x < HexData.WorldSizeInChunks; x++)
        {
            for (int z = 0; z < HexData.WorldSizeInChunks; z++)
            {
                CreateNewChunk(x, z);
            }
        }
    }

    public byte GetHex(Vector3 pos)
    {
        //face check ekleme zamanın gelmiş o olmadığı için çalışmıyor
        //ayrıca efficiency de getirir bize
        if (!IsHexInWorld(pos))
        {
            return 0;
        }
        else
        {
            return 3;
        }
    }
    void CreateNewChunk(int _x, int _z)
    {
        chunks[_x,_z] = new Chunk(new ChunkCoord(_x, _z), this);
    }

    bool IsChunkInWorld(ChunkCoord coord)
    {
        if (coord.x>0 && coord.x< HexData.WorldSizeInChunks - 1 && coord.z > 0 && coord.z < HexData.WorldSizeInChunks - 1) //5'ler world size in cunks
        {
            return true; 
        }
        else { return false; }
    }

    bool IsHexInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < HexData.WorldSizeInBlocks && pos.y >= 0 && pos.y < HexData.ChunkHeight  && pos.z >= 0 && pos.z < HexData.WorldSizeInBlocks ) //25'ler world size in hex 10 da chunk height
        { //düzgün çalışmıyore ayrıca triangles da çalışmıyore
            return true;
        }
        else
        {
            return false;
        }
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
