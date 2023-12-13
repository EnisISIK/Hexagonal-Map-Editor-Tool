using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class PositionHelper
{

    public static bool IsHexInChunk(float _x, float _y, float _z)
    {
        return !(_x < 0 || _x > HexData.ChunkWidth - 1 || _y < 0 || _y > HexData.ChunkHeight - 1 || _z < 0 || _z > HexData.ChunkWidth - 1);

    }


    public static Vector3Int GetChunkFromVector3(Vector3 pos)
    {

        int x = Mathf.FloorToInt(pos.x / HexData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / HexData.ChunkWidth);

        return new Vector3Int(x, 0, z);
    }


    public static ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {

        int x = Mathf.FloorToInt(pos.x / HexData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / HexData.ChunkWidth);

        return new ChunkCoord(x, z);

    }


    public static Vector3Int GetInChunkPosition(Vector3 pos, Vector3 chunkPos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkPos.x * HexData.ChunkWidth);
        zCheck -= Mathf.FloorToInt(chunkPos.z * HexData.ChunkWidth);

        return new Vector3Int(xCheck, yCheck, zCheck);
    }


    public static Vector3 AxialToOddr(Vector2 hex)
    {
        float x = hex.x;
        float y = hex.y + (hex.x - (Mathf.FloorToInt(hex.x) & 1)) / 2;

        return new Vector3(y, 0, x);
    }


    public static Vector2 CubeToAxial(Vector3 hex)
    {
        float q = hex.z;
        float r = hex.x;

        return new Vector2(r, q);
    }


    public static Vector3 AxialToCube(Vector2 hex)
    {
        float q = hex.y;
        float r = hex.x;
        float s = -q - r;

        return new Vector3(r, s, q);
    }


    //Converts Pixel Position to Hex Position
    public static Vector3 PixelToHex(Vector3 pos)
    {
        float q = (Mathf.Sqrt(3) / 3 * pos.x - 1.0f / 3 * pos.z);
        float r = (2.0f / 3 * pos.z);

        Vector3 hexPos = AxialToOddr(AxialRound(new Vector2(r, q)));
        hexPos.y = pos.y;

        return hexPos;
    }


    //Converts Hex Position to Pixel Position
    public static Vector3 HexToPixel(Vector3 pos)
    {
        int _x = Mathf.FloorToInt(pos.x);
        int _y = Mathf.FloorToInt(pos.y);
        int _z = Mathf.FloorToInt(pos.z);

        Vector3 pixelPos = new Vector3
        {
            x = (((_x) + (_z) * 0.5f - (_z) / 2) * ((Mathf.Sqrt(3.00f) / 2.00f) * 2f)),
            y = 1f * _y,
            z = ((_z) * (HexData.outerRadius * 1.5f))
        };

        return pixelPos;
    }


    public static Vector3 CubeRound(Vector3 pos)
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


    public static Vector2 AxialRound(Vector2 pos)
    {
        return CubeToAxial(CubeRound(AxialToCube(pos)));
    }



}
