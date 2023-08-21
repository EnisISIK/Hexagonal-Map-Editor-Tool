using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class HexPrism
{

    public Vector3Int GetChunkFromChunkVector3(Vector3 pos)
    {

        int x = Mathf.FloorToInt(pos.x / HexData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / HexData.ChunkWidth);

        return new Vector3Int(x, 0, z);//call as vector3 for chunksdictionary

    }

    public Vector3 AxialToOddr(Vector2 hex)
    {
        float x = hex.x;
        float y = hex.y + (hex.x - (Mathf.FloorToInt(hex.x) & 1)) / 2;

        return new Vector3(y, 0, x);
    }
    public Vector2 CubeToAxial(Vector3 hex)
    {
        float q = hex.z;
        float r = hex.x;

        return new Vector2(r, q);
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
        float q = (Mathf.Sqrt(3) / 3 * pos.x - 1.0f / 3 * pos.z);
        float r = (2.0f / 3 * pos.z);

        return AxialToOddr(AxialRound(new Vector2(r, q)));
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

}
