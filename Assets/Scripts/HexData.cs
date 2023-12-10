using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HexData {

	//A Single Hexagon's metric values
	public const float outerRadius = 1f;

	public const float radiusVal = 0.2886f;

	public const float innerRadius = outerRadius * 0.8660254f;

	#region World size metrics
	public static readonly int ChunkWidth = 16;
	public static readonly int ChunkHeight = 128;

	public static readonly int ViewDistanceinChunks = 16;
	public static readonly int MaxRenderChunks = 24;

	public static readonly int WorldSizeInChunks = 10;
	public static int WorldSizeInBlocks
	{

		get { return WorldSizeInChunks * ChunkWidth; }

	}
    #endregion

    #region TextureAtlas metrics
    public static readonly int TextureAtlasSizeInBlocks = 16;
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
    public static readonly Vector3 p00 = new Vector3(0.00f, 1.00f, 1.00f);
	public static readonly Vector3 p01 = new Vector3(Mathf.Sqrt(3.00f) / 2.00f, 1.00f, (1.00f / 2.00f));
	public static readonly Vector3 p02 = new Vector3(Mathf.Sqrt(3.00f) / 2.00f, 1.00f, -(1.00f / 2.00f));
	public static readonly Vector3 p03 = new Vector3(0.00f, 1.00f, -1.00f);
	public static readonly Vector3 p04 = new Vector3(-Mathf.Sqrt(3.00f) / 2.00f, 1.00f, -(1.00f / 2.00f));
	public static readonly Vector3 p05 = new Vector3(-Mathf.Sqrt(3.00f) / 2.00f, 1.00f, (1.00f / 2.00f));
	public static readonly Vector3 p06 = new Vector3(0.00f, 0.00f, 1.00f);
	public static readonly Vector3 p07 = new Vector3(Mathf.Sqrt(3.00f) / 2.00f, 0.00f, (1.00f / 2.00f));
	public static readonly Vector3 p08 = new Vector3(Mathf.Sqrt(3.00f) / 2.00f, 0.00f, -(1.00f / 2.00f));
	public static readonly Vector3 p09 = new Vector3(0.00f, 0.00f, -1.00f);
	public static readonly Vector3 p10 = new Vector3(-Mathf.Sqrt(3.00f) / 2.00f, 0.00f, -(1.00f / 2.00f));
	public static readonly Vector3 p11 = new Vector3(-Mathf.Sqrt(3.00f) / 2.00f, 0.00f, (1.00f / 2.00f));

	
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

	public static readonly Vector3[][] hexVertices = new Vector3[][]
	{
		topVertices,
		bottomVertices,
		rightVertices,
		leftVertices,
		frontRightVertices,
		frontLeftVertices,
		backRightVertices,
		backLeftVertices,
	};
	#endregion

	#region Face values as a compass 
	public static readonly Vector3Int fu = new Vector3Int(0, 1, 0);
	public static readonly Vector3Int fd = new Vector3Int(0, -1, 0);
	public static readonly Vector3Int fe = new Vector3Int(1, 0, 0);
	public static readonly Vector3Int fw = new Vector3Int(-1, 0, 0);

	public static readonly Vector3Int fne = new Vector3Int(1, 0, 1);
	public static readonly Vector3Int fsw = new Vector3Int(-1, 0, -1);

	public static readonly Vector3Int fnw = new Vector3Int(-1, 0, 1);
	public static readonly Vector3Int fse = new Vector3Int(1, 0, -1);

	public static readonly Vector3Int[] faces = new Vector3Int[]
	{
		fu,
		fd,
		fe,
		fw,
		fse,
		fsw,
		fne,
		fnw
	};
	#endregion

	#region Normals
	public static readonly Vector3 nup = Vector3.up;
	public static readonly Vector3 ndown = Vector3.down;
	public static readonly Vector3 nright = Vector3.right;
	public static readonly Vector3 nleft = Vector3.left;
	public static readonly Vector3 nfrontRight = new Vector3(0.50f, 0, -((Mathf.Sqrt(3.00f)) / 2.00f));
	public static readonly Vector3 nfrontLeft = new Vector3(-0.50f, 0, -((Mathf.Sqrt(3.00f)) / 2.00f));
	public static readonly Vector3 nbackRight = new Vector3(0.50f, 0, ((Mathf.Sqrt(3.00f)) / 2.00f));
	public static readonly Vector3 nbackLeft = new Vector3(-0.50f, 0, ((Mathf.Sqrt(3.00f)) / 2.00f));

	public static readonly Vector3[] normalsTop = new Vector3[]
	{
		// Top
		up, up, up, up, up, up,
	};
	public static readonly Vector3[] normalsBottom = new Vector3[]
	{
		// Bottom
		down, down, down, down, down, down,
		
	}; 
	public static readonly Vector3[] normalsRight = new Vector3[]
	{
		// Right
		right, right, right, right
	};

	public static readonly Vector3[] normalsLeft = new Vector3[]
	{
		// left
		left, left, left, left,
	}; 
	public static readonly Vector3[] normalsFrontRight = new Vector3[]
	{
		// Front Right
		frontRight, frontRight, frontRight, frontRight,
	}; 
	public static readonly Vector3[] normalsFrontLeft = new Vector3[]
	{
		// Front Left
		frontLeft, frontLeft, frontLeft, frontLeft,
	};
	public static readonly Vector3[] normalsBackRight = new Vector3[]
	{
		// Back Right
		backRight, backRight, backRight, backRight,
	}; 
	public static readonly Vector3[] normalsBackLeft = new Vector3[]
	{
		// Back Left
		backLeft, backLeft, backLeft, backLeft,
	};
	public static readonly Vector3[][] normals = new Vector3[][]
	{
		normalsTop,
		normalsBottom,
		normalsRight,
		normalsLeft,
		normalsFrontRight,
		normalsFrontLeft,
		normalsBackRight,
		normalsBackLeft
	};
	#endregion

	#region UVs clockwise top to bottom

	public static float epsilon = 0.0001f;

	public static readonly Vector2 _00 = new Vector2(0.50f, 1.00f);
	public static readonly Vector2 _01 = new Vector2(1.00f, 0.50f + radiusVal);
	public static readonly Vector2 _02 = new Vector2(1.00f, radiusVal);
	public static readonly Vector2 _03 = new Vector2(0.50f, 0.00f);
	public static readonly Vector2 _04 = new Vector2(0.00f, radiusVal);
	public static readonly Vector2 _05 = new Vector2(0.00f, 0.50f + radiusVal);

	public static readonly Vector2 _06 = new Vector2(0.00f, 0.00f);
	public static readonly Vector2 _07 = new Vector2(1.00f, 0.00f);
	public static readonly Vector2 _08 = new Vector2(0.00f, 1.00f);
	public static readonly Vector2 _09 = new Vector2(1.00f, 1.00f);

	public static readonly Vector2[] topUvs = new Vector2[]
	{
		// Top
		_00, _01, _02, _03, _04, _05,
	};

	public static readonly Vector2[] bottomUvs = new Vector2[]
	{
		// Bottom
		_05, _04, _03, _02, _01, _00,
	};

	public static readonly Vector2[] rightUvs = new Vector2[]
	{	
		// Right
		_09, _08, _07, _06,
	};

	public static readonly Vector2[] leftUvs = new Vector2[]
	{	
		// Left
		_09, _08, _07, _06,
	};

	public static readonly Vector2[] frontRightUvs = new Vector2[]
	{		
		// Front Right
		_09, _08, _07, _06,
	};

	public static readonly Vector2[] frontLeftUvs = new Vector2[]
	{
		// Front Left
		_09, _08, _07, _06,
	};

	public static readonly Vector2[] backRightUvs = new Vector2[]
	{
		// Back Right
		_09, _08, _07, _06,
	};

	public static readonly Vector2[] backLeftUvs = new Vector2[]
	{	
		// Back Left
		_09, _08, _07, _06,
	};

	public static readonly Vector2[][] hexUvs = new Vector2[][]
	{
		topUvs,
		bottomUvs,
		rightUvs,
		leftUvs,
		frontRightUvs,
		frontLeftUvs,
		backRightUvs,
		backLeftUvs,
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
		3, 1, 0,
		3, 2, 1,
		4, 3, 0,
		5, 4, 0,
	};

	public static readonly int[] rightTriangles = new int[]
	{	
		// Right
		2, 1, 0,
		2, 3, 1,
	};

	public static readonly int[] leftTriangles = new int[]
	{
		// Left
		2, 1, 0,
		2, 3, 1,
	};

	public static readonly int[] frontRightTriangles = new int[]
	{
		// Front Right
		2, 1, 0,
		2, 3, 1,
	};

	public static readonly int[] frontLeftTriangles = new int[]
	{
		// Front Left
		2, 1, 0,
		2, 3, 1,
	};

	public static readonly int[] backRightTriangles = new int[]
	{
		// Back Right
		2, 1, 0,
		2, 3, 1,
	};

	public static readonly int[] backLeftTriangles = new int[]
	{
		// Back Left
		2, 1, 0,
		2, 3, 1,
	};

	public static readonly int[][] hexTriangles = new int[][]
	{
		topTriangles,
		bottomTriangles,
		rightTriangles,
		leftTriangles,
		frontRightTriangles,
		frontLeftTriangles,
		backRightTriangles,
		backLeftTriangles,
	};

	/*public static readonly int[] bottomTriangles = new int[]
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
	};*/
	#endregion
}
