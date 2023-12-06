using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassBlockData //: HexData
{
	//A Single Hexagon's metric values
	public const float radiusVal = 0.2886f;

	#region Normals
	public static readonly Vector3 up = Vector3.up;
	public static readonly Vector3 down = Vector3.down;
	public static readonly Vector3 right = Vector3.right;
	public static readonly Vector3 left = Vector3.left;
	public static readonly Vector3 frontRight = new Vector3(0.50f, 0, -((Mathf.Sqrt(3.00f)) / 2.00f));
	public static readonly Vector3 frontLeft = new Vector3(-0.50f, 0, -((Mathf.Sqrt(3.00f)) / 2.00f));
	public static readonly Vector3 backRight = new Vector3(0.50f, 0, ((Mathf.Sqrt(3.00f)) / 2.00f));
	public static readonly Vector3 backLeft = new Vector3(-0.50f, 0, ((Mathf.Sqrt(3.00f)) / 2.00f));

	public static readonly Vector3[] normalsRight = new Vector3[]
	{
		// Right
		right,right,right,right,
	};
	public static readonly Vector3[] normalsLeft = new Vector3[]
	{
		// Left
		left,left,left,left,
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
		normalsRight,
		normalsLeft,
		normalsFrontRight,
		normalsFrontLeft,
		normalsBackRight,
		normalsBackLeft
	};
	#endregion

	#region Vertices clockwise top to bottom
	public static readonly Vector3 p00 = new Vector3(0.00f, 1.00f, 0.80f);
	public static readonly Vector3 p01 = new Vector3((Mathf.Sqrt(3.00f) / 2.00f) - 0.20f, 1.00f, (1.00f / 2.00f) - 0.20f); //
	public static readonly Vector3 p02 = new Vector3((Mathf.Sqrt(3.00f) / 2.00f) - 0.20f, 1.00f, -(1.00f / 2.00f) + 0.20f); //\
	public static readonly Vector3 p03 = new Vector3(0.00f, 1.00f, -0.80f);
	public static readonly Vector3 p04 = new Vector3((-Mathf.Sqrt(3.00f) / 2.00f) + 0.20f, 1.00f, -(1.00f / 2.00f) + 0.20f); //
	public static readonly Vector3 p05 = new Vector3((-Mathf.Sqrt(3.00f) / 2.00f) + 0.20f, 1.00f, (1.00f / 2.00f) - 0.20f); //\
	public static readonly Vector3 p06 = new Vector3(0.00f, 0.00f, 0.80f);
	public static readonly Vector3 p07 = new Vector3((Mathf.Sqrt(3.00f) / 2.00f) - 0.20f, 0.00f, (1.00f / 2.00f) - 0.20f); //
	public static readonly Vector3 p08 = new Vector3((Mathf.Sqrt(3.00f) / 2.00f) - 0.20f, 0.00f, -(1.00f / 2.00f) + 0.20f); //\
	public static readonly Vector3 p09 = new Vector3(0.00f, 0.00f, -0.80f);
	public static readonly Vector3 p10 = new Vector3((-Mathf.Sqrt(3.00f) / 2.00f) + 0.20f, 0.00f, -(1.00f / 2.00f) + 0.20f); //
	public static readonly Vector3 p11 = new Vector3((-Mathf.Sqrt(3.00f) / 2.00f) + 0.20f, 0.00f, (1.00f / 2.00f) - 0.20f); //\

	public static readonly Vector3[] rightFrontVertices = new Vector3[]
	{
		// Right
		p00, p03, p06, p09,
	};

	public static readonly Vector3[] leftFrontVertices = new Vector3[]
	{
		// Left
		p03, p00, p09, p06,
	};

	public static readonly Vector3[] diagonalRightFrontVertices = new Vector3[]
	{
		// Right
		p01, p04, p07, p10,
	};

	public static readonly Vector3[] diagonalLeftFrontVertices = new Vector3[]
	{
		// Left
		p05, p02, p11, p08,
	};

	public static readonly Vector3[] diagonalRightBackVertices = new Vector3[]
	{	
		// Front Right
		p04, p01, p10, p07,
	};

	public static readonly Vector3[] diagonalLeftBackVertices = new Vector3[]
	{
		// Front Left
		p02, p05, p08, p11,
	};

	public static readonly Vector3[][] hexVertices = new Vector3[][]
	{
		rightFrontVertices,
		leftFrontVertices,
		diagonalRightFrontVertices,
		diagonalLeftFrontVertices,
		diagonalRightBackVertices,
		diagonalLeftBackVertices,
	};
	#endregion

	#region UVs clockwise top to bottom

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
		rightUvs,
		leftUvs,
		frontRightUvs,
		frontLeftUvs,
		backRightUvs,
		backLeftUvs,
	};
	#endregion

	#region Triangles clockwise top to bottom

	public static readonly int[] rightTriangles = new int[]
{
		// Front Right
		2, 1, 0,
		2, 3, 1,
};

	public static readonly int[] leftTriangles = new int[]
	{
		// Front Left
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
		rightTriangles,
		leftTriangles,
		frontRightTriangles,
		frontLeftTriangles,
		backRightTriangles,
		backLeftTriangles,
	};


	#endregion
}
