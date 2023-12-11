﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ChunkMeshRenderer
{
	private World _world;
	public readonly List<Vector3> vertices = new List<Vector3>();
	public readonly List<int> triangles = new List<int>();
	public readonly List<int> transparentTriangles = new List<int>();
	public readonly List<int> waterTriangles = new List<int>();
	public readonly List<Color> colors = new List<Color>();

	public readonly List<Vector3> normals = new List<Vector3>();
	public readonly List<Vector2> uvs = new List<Vector2>();
	private readonly Vector3Int chunkPosition;

	public Vector3[] arrayvertices;
	public int[] arraytriangles;
	public int[] arraytransparentTriangles;
	public int[] arraywaterTriangles;
	public Color[] arraycolors;

	public Vector3[] arraynormals;
	public Vector2[] arrayuvs;

	public ChunkMeshRenderer(World world, Vector3Int chunkPosition)
    {
		_world = world;
		this.chunkPosition = chunkPosition;
    }

	internal void AddHex(Vector3 pos, Vector3[] hexVert, int[] hexTri, Vector3[] hexNormals, bool isWater,bool isTransparent, float lightLevel)
	{
		Vector3 _position = PositionHelper.HexToPixel(pos);
		_position.x += (chunkPosition.x * 2f * HexData.innerRadius - chunkPosition.x);
		_position.z += (chunkPosition.z * 1.5f * HexData.outerRadius - chunkPosition.z);

		int triangleOffsetValue = vertices.Count;

		normals.AddRange(hexNormals);

		vertices.AddRange(hexVert.Select(v => v + _position));
		colors.AddRange(hexVert.Select(v => new Color(0, 0, 0, lightLevel)));

        if (!isWater)
        {
			if (!isTransparent)
			{
				triangles.AddRange(hexTri.Select(t => t + triangleOffsetValue));
			}
			else
			{
				transparentTriangles.AddRange(hexTri.Select(t => t + triangleOffsetValue));
			}
		}
		else
		{
			waterTriangles.AddRange(hexTri.Select(t => t + triangleOffsetValue));
		}

		if (hexTri.Length == 12) triangleOffsetValue += 6;
		else if (hexTri.Length == 6) triangleOffsetValue += 4;
	}

	internal void AddUvs(int blockID, Vector2[] hexUV, int textureIDNum,bool isWater)
	{
		for (int i = 0; i < hexUV.Length; i++)
		{
			int textureId = _world.blocktypes[blockID].GetTextureID(textureIDNum);
			float y = textureId / HexData.TextureAtlasSizeInBlocks;
			float x = textureId - (y * HexData.TextureAtlasSizeInBlocks);

			x *= HexData.NormalizedBlockTextureSize;
			y *= HexData.NormalizedBlockTextureSize;

			y = 1f - y - HexData.NormalizedBlockTextureSize;

			Vector2 textureUV = hexUV[i];
			textureUV.x = (textureUV.x * HexData.NormalizedBlockTextureSize + x);
			textureUV.y = (textureUV.y * HexData.NormalizedBlockTextureSize + y);
			if(!isWater)
				uvs.Add(textureUV);
			if (isWater)
				uvs.Add(hexUV[i]);

		}
	}

	public void SetListsToArrays()
    {
		arrayvertices = vertices.ToArray();
		arrayuvs = uvs.ToArray();
		arraynormals = normals.ToArray();
		arraycolors = colors.ToArray();

		arraytriangles = triangles.ToArray();
		arraytransparentTriangles = transparentTriangles.ToArray();
		arraywaterTriangles = waterTriangles.ToArray();
	}
}
