using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HexData {

	//A Single Hexagon's metric values
	public const float outerRadius = 1f;

	public const float innerRadius = outerRadius * 0.866025404f;

	#region World size metrics
	public static readonly int ChunkWidth = 16;
	public static readonly int ChunkHeight = 128;

	public static readonly int ViewDistanceinChunks = 5;

	public static readonly int WorldSizeInChunks = 10;
	public static int WorldSizeInBlocks
	{

		get { return WorldSizeInChunks * ChunkWidth; }

	}
    #endregion

    #region TextureAtlas metrics
    public static readonly int TextureAtlasSizeInBlocks = 4;
	public static float NormalizedBlockTextureSize
    {
        get { return 1f / (float)TextureAtlasSizeInBlocks; }
    }
    #endregion

	#region Normals
	public static readonly Vector3 up = Vector3.up;
	public static readonly Vector3 down = Vector3.down;
	public static readonly Vector3 right = Vector3.right;
	public static readonly Vector3 left = Vector3.left;
	public static readonly Vector3 frontRight = new Vector3(0.50f, 0, -((Mathf.Sqrt(3.00f)) / 2.00f));
	public static readonly Vector3 frontLeft = new Vector3(-0.50f, 0, -((Mathf.Sqrt(3.00f)) / 2.00f));
	public static readonly Vector3 backRight = new Vector3(0.50f, 0, ((Mathf.Sqrt(3.00f)) / 2.00f));
	public static readonly Vector3 backLeft = new Vector3(-0.50f, 0, ((Mathf.Sqrt(3.00f)) / 2.00f));
    #endregion

    #region Vertices clockwise top to bottom
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

	
	public static readonly Vector3[] topVertices = new Vector3[]
	{
		// Top
		p00, p01, p02, p03, p04, p05,
		
	};

	public static readonly Vector3[] bottomVertices = new Vector3[]
	{
		// Bottom
		p06, p07, p08, p09, p10, p11,
	};

	public static readonly Vector3[] rightVertices = new Vector3[]
	{
		// Right
		p01, p02, p07, p08,
	};

	public static readonly Vector3[] leftVertices = new Vector3[]
	{
		// Left
		p04, p05, p10, p11,
	};

	public static readonly Vector3[] frontRightVertices = new Vector3[]
	{	
		// Front Right
		p02, p03, p08, p09,
	};

	public static readonly Vector3[] frontLeftVertices = new Vector3[]
	{
		// Front Left
		p03, p04, p09, p10,
	};

	public static readonly Vector3[] backRightVertices = new Vector3[]
	{
		// Back Right
		p00, p01, p06, p07,
	};

	public static readonly Vector3[] backLeftVertices = new Vector3[]
	{
		// Back Left
		p05, p00, p11, p06
	};
    #endregion

    #region Face values as a compass 
    public static readonly Vector3 fu = new Vector3(0.00f, 1.00f, 0.00f);
	public static readonly Vector3 fd = new Vector3(0.00f, -1.00f, 0.00f);
	public static readonly Vector3 fe = new Vector3(1.00f, 0.00f, 0.00f);
	public static readonly Vector3 fw = new Vector3(-1.00f, 0.00f, 0.00f);

	public static readonly Vector3 fne = new Vector3(1.00f, 0.00f, 1.00f);
	public static readonly Vector3 fsw = new Vector3(-1.00f, 0.00f, -1.00f);

	public static readonly Vector3 fnw = new Vector3(-1.00f, 0.00f, 1.00f);
	public static readonly Vector3 fse = new Vector3(1.00f, 0.00f, -1.00f);
	#endregion

	#region UVs clockwise top to bottom
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

	public static readonly Vector2[] topUvs = new Vector2[]
	{
		// Top
		_00, _01, _02, _03, _04, _05,
	};

	public static readonly Vector2[] bottomUvs = new Vector2[]
	{
		// Bottom
		_00, _01, _02, _03, _04, _05,
	};

	public static readonly Vector2[] rightUvs = new Vector2[]
	{	
		// Right
		_06, _07, _08, _09,
	};

	public static readonly Vector2[] leftUvs = new Vector2[]
	{	
		// Left
		_06, _07, _08, _09,
	};

	public static readonly Vector2[] frontRightUvs = new Vector2[]
	{		
		// Front Right
		_06, _07, _08, _09,
	};

	public static readonly Vector2[] frontLeftUvs = new Vector2[]
	{
		// Front Left
		_06, _07, _08, _09,
	};

	public static readonly Vector2[] backRightUvs = new Vector2[]
	{
		// Back Right
		_06, _07, _08, _09,
	};

	public static readonly Vector2[] backLeftUvs = new Vector2[]
	{	
		// Back Left
		_06, _07, _08, _09,
	};
	#endregion

	#region Triangles clockwise top to bottom
	public static readonly int[] topTriangles = new int[]
	{
		// Top
		0, 1, 3,
		1, 2, 3,
		0, 3, 4,
		0, 4, 5,
	};
	
	public static readonly int[] bottomTriangles = new int[]
	{
		// Bottom
		9, 7, 6,
		9, 8, 7,
		10, 9, 6,
		11, 10, 6,
	};
	
	public static readonly int[] rightTriangles = new int[]
	{	
		// Right
		14, 13, 12,
		14, 15, 13,
	};
	
	public static readonly int[] leftTriangles = new int[]
	{
		// Left
		18, 17, 16,
		18, 19, 17,
	};
	
	public static readonly int[] frontRightTriangles = new int[]
	{
		// Front Right
		22, 21, 20,
		22, 23, 21,
	};
	
	public static readonly int[] frontLeftTriangles = new int[]
	{
		// Front Left
		26, 25, 24,
		26, 27, 25,
	};
	
	public static readonly int[] backRightTriangles = new int[]
	{
		// Back Right
		30, 29, 28,
		30, 31, 29,
	};
	
	public static readonly int[] backLeftTriangles = new int[]
	{
		// Back Left
		34, 33, 32,
		34, 35, 33,
	};
	#endregion
}
