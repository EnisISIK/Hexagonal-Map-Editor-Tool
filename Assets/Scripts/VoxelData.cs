using System.Collections;
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

	public const float outerRadius = 1f;

	public const float innerRadius = outerRadius * 0.866025404f;

	public static readonly Vector3[] voxelVerts = new Vector3[14] {
		new Vector3(0f, 0f, 0f),
		new Vector3(0f, 0f, -outerRadius),
		new Vector3(innerRadius, 0f, -0.5f * outerRadius),
		new Vector3(innerRadius, 0f, 0.5f * outerRadius),
		new Vector3(0f, 0f, outerRadius),
		new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
		new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
		new Vector3(0f, 0.5f, 0f),
		new Vector3(0f, 0.5f, -outerRadius),
		new Vector3(innerRadius, 0.5f, -0.5f * outerRadius),
		new Vector3(innerRadius, 0.5f, 0.5f * outerRadius),
		new Vector3(0f, 0.5f, outerRadius),
		new Vector3(-innerRadius, 0.5f, 0.5f * outerRadius),
		new Vector3(-innerRadius, 0.5f, -0.5f * outerRadius)

	};

	public static readonly int[,] voxelTris = new int[8,18] {
		//-1 for none/skip  //bunlar aşağıdan yukarı gözüküyor galiba bunları değiştir tam tersi olcak şekilde
		{2,  0, 1, 1, 0,  6,  6, 0, 5, 5, 0, 4, 4, 0, 3, 3, 0, 2}, // Bottom Face
		{8,  7, 9, 9,  7, 10,10, 7,11,11, 7,12,12, 7,13,13, 7, 8}, // Top Face
		{1,  8, 2, 2,  8,  9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1}, // Front-Right Face
		{2,  9, 3, 3,  9, 10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1}, // Side-Right Face
		{3, 10, 4, 4, 10, 11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1}, // Back-Right Face
		{4, 11, 5, 5, 11, 12,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1}, // Back-Left Face
		{5, 12, 6, 6, 12, 13,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1}, // Side-Left Face
		{6, 13, 1, 1, 13,  8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1}  // Front-Left Face
	};

	public static readonly int[,] voxelTrisSpare = new int[8, 18] {
		//-1 for none/skip  //bunlar aşağıdan yukarı gözüküyor galiba bunları değiştir tam tersi olcak şekilde
		{0,  6, 5, 0, 5,  4,  0, 4, 3, 0, 3, 2, 0, 2, 1, 0, 1, 6}, // Bottom Face
		{7,  13, 12, 7,  12, 11,7, 11,10,7, 10,9,7, 9,8,7, 8, 13}, // Top Face
		{1,  8, 2, 2,  8,  9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1}, // Front-Right Face
		{2,  9, 3, 3,  9, 10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1}, // Side-Right Face
		{3, 10, 4, 4, 10, 11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1}, // Back-Right Face
		{4, 11, 5, 5, 11, 12,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1}, // Back-Left Face
		{5, 12, 6, 6, 12, 13,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1}, // Side-Left Face
		{6, 13, 1, 1, 13,  8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1}  // Front-Left Face
	};

	public static readonly Vector3[] hexFaceChecks = new Vector3[8]
	{
		new Vector3(0f, -1f, 0f), // Bottom Face
		new Vector3(0f, 1f, 0f), // Top Face
		new Vector3(-1f, 0f, -1f), // Front-Right Face
		new Vector3(1f, 0f, 0f), // Side-Right Face
		new Vector3(1f, 0f, 1f), // Back-Right Face
		new Vector3(1f, 0f, 1f), // Back-Left Face
		new Vector3(-1f, 0f, 0f), // Side-Left Face
		new Vector3(-1f, 0f, -1f)  // Front-Left Face
	};

	public static readonly Vector2[] voxelUvs = new Vector2[14] {

		new Vector2(0f,  0f),
		new Vector2(0f,  -outerRadius),
		new Vector2(innerRadius, -0.5f * outerRadius),
		new Vector2(innerRadius,  0.5f * outerRadius),
		new Vector2(0f,  outerRadius),
		new Vector2(-innerRadius,  0.5f * outerRadius),
		new Vector2(-innerRadius,  -0.5f * outerRadius),
		new Vector2(0f,  0f),
		new Vector2(0f,  -outerRadius),
		new Vector2(innerRadius,  -0.5f * outerRadius),
		new Vector2(innerRadius,  0.5f * outerRadius),
		new Vector2(0f,  outerRadius),
		new Vector2(-innerRadius, 0.5f * outerRadius),
		new Vector2(-innerRadius,  -0.5f * outerRadius)
	};


}
