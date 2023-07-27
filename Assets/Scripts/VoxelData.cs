﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData {

	public static readonly int ChunkWidth = 5;
	public static readonly int ChunkHeight = 10;

	public static readonly int TextureAtlasSizeInBlocks = 4;
	public static float NormalizedBlockTextureSize
    {
        get { return 1f / (float)TextureAtlasSizeInBlocks; }
    }

	public static readonly int WorldSizeInChunks = 50;
	public static int WorldSizeInBlocks
	{

		get { return WorldSizeInChunks * ChunkWidth; }

	}
	public const float outerRadius = 1f;

	public const float innerRadius = outerRadius * 0.866025404f;

	public static readonly Vector3 up = Vector3.up;
	public static readonly Vector3 down = Vector3.down;
	public static readonly Vector3 right = Vector3.right;
	public static readonly Vector3 left = Vector3.left;
	public static readonly Vector3 frontRight = new Vector3(0.50f, 0, -((Mathf.Sqrt(3.00f)) / 2.00f));
	public static readonly Vector3 frontLeft = new Vector3(-0.50f, 0, -((Mathf.Sqrt(3.00f)) / 2.00f));
	public static readonly Vector3 backRight = new Vector3(0.50f, 0, ((Mathf.Sqrt(3.00f)) / 2.00f));
	public static readonly Vector3 backLeft = new Vector3(-0.50f, 0, ((Mathf.Sqrt(3.00f)) / 2.00f));

	public static readonly Vector3 p00 = new Vector3(0.00f, 0.50f, 1.00f);
	public static readonly Vector3 p01 = new Vector3(Mathf.Sqrt(3.00f) / 2.00f, 0.50f, (1.00f / 2.00f));
	public static readonly Vector3 p02 = new Vector3(Mathf.Sqrt(3.00f) / 2.00f, 0.50f, -(1.00f / 2.00f));
	public static readonly Vector3 p03 = new Vector3(0.00f, 0.50f, -1.00f);
	public static readonly Vector3 p04 = new Vector3(-Mathf.Sqrt(3.00f) / 2.00f, 0.50f, -(1.00f / 2.00f));
	public static readonly Vector3 p05 = new Vector3(-Mathf.Sqrt(3.00f) / 2.00f, 0.50f, (1.00f / 2.00f));
	public static readonly Vector3 p06 = new Vector3(0.00f, -0.50f, 1.00f);
	public static readonly Vector3 p07 = new Vector3(Mathf.Sqrt(3.00f) / 2.00f, -0.50f, (1.00f / 2.00f));
	public static readonly Vector3 p08 = new Vector3(Mathf.Sqrt(3.00f) / 2.00f, -0.50f, -(1.00f / 2.00f));
	public static readonly Vector3 p09 = new Vector3(0.00f, -0.50f, -1.00f);
	public static readonly Vector3 p10 = new Vector3(-Mathf.Sqrt(3.00f) / 2.00f, -0.50f, -(1.00f / 2.00f));
	public static readonly Vector3 p11 = new Vector3(-Mathf.Sqrt(3.00f) / 2.00f, -0.50f, (1.00f / 2.00f));

	#region Vertices clockwise top to bottom
	public static readonly Vector3[] hexVertices = new Vector3[]
	{
		// Top
		p00, p01, p02, p03, p04, p05,
		
		// Bottom
		p06, p07, p08, p09, p10, p11,
		
		// Right
		p01, p02, p07, p08,
		
		// Left
		p04, p05, p10, p11,
		
		// Front Right
		p02, p03, p08, p09,
		
		// Front Left
		p03, p04, p09, p10,
		
		// Back Right
		p00, p01, p06, p07,
		
		// Back Left
		p05, p00, p11, p06
	};
	#endregion

	//buraya face değerlerini gir
	public static readonly Vector3 f00 = new Vector3(0.00f, 1.00f, 0.00f);
	public static readonly Vector3 f01 = new Vector3(0.00f, -1.00f, 0.00f);
	public static readonly Vector3 f02 = new Vector3(1.00f, 0.00f, 0.00f);
	public static readonly Vector3 f03 = new Vector3(-1.00f, 0.00f, 0.00f);
	public static readonly Vector3 f04 = new Vector3(0.50f, 0.00f, -((Mathf.Sqrt(3.00f)) / 2.00f));
	public static readonly Vector3 f05 = new Vector3(-0.50f, 0.00f, -((Mathf.Sqrt(3.00f)) / 2.00f));
	public static readonly Vector3 f06 = new Vector3(0.50f, 0.00f, ((Mathf.Sqrt(3.00f)) / 2.00f));
	public static readonly Vector3 f07 = new Vector3(-0.50f, 0.00f, ((Mathf.Sqrt(3.00f)) / 2.00f));

	#region Faces clockwise top to bottom
	public static readonly Vector3[] hexFaces = new Vector3[]
	{
		// Top
		f00,
		// Bottom
		f01,
		// Right
		f02,
		// Left
		f03,
		// Front Right
		f04,
		// Front Left
		f05,
		// Back Right
		f06,
		// Back Left
		f07,
	};
	#endregion

	public static readonly Vector2 _00 = new Vector2(0.50f, 1.00f);
	public static readonly Vector2 _01 = new Vector2(1.00f, 0.50f + (1.00f / (2.00f * Mathf.Sqrt(3.00f))));
	public static readonly Vector2 _02 = new Vector2(1.00f, (1.00f / (2.00f * Mathf.Sqrt(3.00f))));
	public static readonly Vector2 _03 = new Vector2(0.50f, 0.00f);
	public static readonly Vector2 _04 = new Vector2(0.00f, (1.00f / (2.00f * Mathf.Sqrt(3.00f))));
	public static readonly Vector2 _05 = new Vector2(0.00f, 0.50f + (1.00f / (2.00f * Mathf.Sqrt(3.00f))));

	public static readonly Vector2 _06 = new Vector2(0f, 0f);
	public static readonly Vector2 _07 = new Vector2(1f, 0f);
	public static readonly Vector2 _08 = new Vector2(0f, 1f);
	public static readonly Vector2 _09 = new Vector2(1f, 1f);

	#region UVs
	public static readonly Vector2[] hexUvs = new Vector2[]
	{
		// Top
		_00, _01, _02, _03, _04, _05,
		
		// Bottom
		_00, _01, _02, _03, _04, _05,
		
		// Right
		_06, _07, _08, _09,
		
		// Left
		_06, _07, _08, _09,
		
		// Front Right
		_06, _07, _08, _09,
		
		// Front Left
		_06, _07, _08, _09,
		
		// Back Right
		_06, _07, _08, _09,
		
		// Back Left
		_06, _07, _08, _09,
	};
	#endregion

	#region Triangles
	public static readonly int[] hexTriangles = new int[]
	{
		// Top
		0, 1, 3,
		1, 2, 3,
		0, 3, 4,
		0, 4, 5,		
		
		// Bottom
		9, 7, 6,
		9, 8, 7,
		10, 9, 6,
		11, 10, 6,
		
		// Right
		14, 13, 12,
		14, 15, 13,
		
		// Left
		18, 17, 16,
		18, 19, 17,
		
		// Front Right
		22, 21, 20,
		22, 23, 21,
		
		// Front Left
		26, 25, 24,
		26, 27, 25,
		
		// Back Right
		30, 29, 28,
		30, 31, 29,
		
		// Back Left
		34, 33, 32,
		34, 35, 33,


	};
	#endregion
}
