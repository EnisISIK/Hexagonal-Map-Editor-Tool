using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockData
{
    public readonly Vector3[][] hexVertices;
    public readonly Vector2[][] hexUvs;
    public readonly Vector3[][] normals;
    public readonly int[][] hexTriangles;
    public int facesCount;

    public BlockData()
    {
        this.hexVertices = HexData.hexVertices;
        this.hexUvs = HexData.hexUvs;
        this.hexTriangles = HexData.hexTriangles;
        this.facesCount = 8;
        this.normals = HexData.normals;
    }

    public BlockData(Vector3[][] hexVertices, Vector2[][] hexUvs, int[][] hexTriangles, Vector3[][] normals, int facesCount)
    {
        this.hexVertices = hexVertices;
        this.hexUvs = hexUvs;
        this.hexTriangles = hexTriangles;
        this.normals = normals;
        this.facesCount = facesCount;
    }
}
