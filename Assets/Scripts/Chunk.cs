using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Chunk 
{
	[SerializeField] private Material material;
	[SerializeField] private float radius = 5;
	[SerializeField] private int numVertices = 6;
	[SerializeField] private int VoxelVertices = 4;

	public ChunkCoord chunkCoordinates;

	GameObject chunkObject;
	int chunkCountX;
	int chunkCountZ;

	MeshRenderer meshRenderer;
	MeshFilter meshFilter;

	List<Vector3> vertices = new List<Vector3>();
	List<int> triangles = new List<int>();
	List<Vector2> uvs = new List<Vector2>();
	List<Vector3> normals = new List<Vector3>();

	byte[,,] voxelMap = new byte[5, 10, 5];

	World world;

	
	

	
	public Chunk(ChunkCoord _chunkCoordinates ,World _world)
    {
		chunkCoordinates = _chunkCoordinates;
		world = _world;
		chunkObject = new GameObject();
		meshFilter = chunkObject.AddComponent<MeshFilter>();
		meshRenderer = chunkObject.AddComponent<MeshRenderer>();
		chunkCountX = chunkCoordinates.x;
		chunkCountZ = chunkCoordinates.z;

		Vector3 chunkPosition;
		chunkPosition.x = (((chunkCoordinates.x)*5f) + ((chunkCoordinates.z )* 5f) * 0.5f - ((chunkCoordinates.z) * 5f) / 2) * (VoxelData.innerRadius * 2f);
		chunkPosition.z = ((chunkCoordinates.z) * 5f) * (VoxelData.outerRadius * 1.5f);

		meshRenderer.material = world.material;

		chunkObject.transform.position = new Vector3(chunkCoordinates.x*5, 0f, chunkCoordinates.z*5);
		chunkObject.transform.SetParent(world.transform);
		//bu position sus geldi bir bak yarın sabah
		//chunkObject.transform.position = new Vector3(chunkCoordinates.x * 5f*2*innerRadius, 0f, chunkCoordinates.z * 5f * 1.5f * 1f);
		//chunkObject.transform.position = new Vector3(chunkPosition.x , 0f, chunkPosition.z);
		//chunkObject.transform.position = new Vector3(0f, 0f, 0f);
		chunkObject.name = "Chunk " + chunkCoordinates.x + ", " + chunkCoordinates.z;
		PopulateVoxelMap();
		CreateChunk();
		CreateMesh();

	}
	void Start()
	{

		/*MeshFilter mf = GetComponent<MeshFilter>();
		Mesh mesh = new Mesh();
		mf.mesh = mesh;
		mesh.Clear();*/


		//PopulateVoxelMap();
		/*List<Vector3> tempVertices = new List<Vector3>();
		for (float y = 0; y < 10; y++)
		{
			for (int z = 0; z < 5; z++)
			{
				for (int x = 0; x < 5; x++)
				{
					if (!CheckVoxel(y, x, z))
					{
						byte blockID = voxelMap[x,(int) y, z];
						Vector3 position;
						position.x = (x + z * 0.5f - z / 2) * (innerRadius * 2f);
						position.y = -1f * y;
						position.z = z * (1f * 1.5f);

						int triangleOffset = vertices.Count;
						vertices.AddRange(hexVertices.Select(v => v + position));
						triangles.AddRange(hexTriangles.Select(t => t + triangleOffset));

						//normals.AddRange(hexNormals);
						//uvs.AddRange(hexUvs);
						//AddTexture(3, hexUvs);
						//AddTexture(world.blocktypes[blockID].GetTextureID(0), hexUvs);
						AddText(blockID, hexUvs);
					}
				}
			}
		}*/

		/*mesh.vertices = vertices.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.triangles = triangles.ToArray();

		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mesh.Optimize();*/
		//CreateMesh();


	}


	void PopulateVoxelMap()
    {
		for (int y = 0; y < VoxelData.ChunkHeight; y++)
		{
			for (int z = 0; z < VoxelData.ChunkWidth; z++)
			{
				for (int x = 0; x < VoxelData.ChunkWidth; x++)
				{
					//böyle hepsine true deyince olmaz tabi
					voxelMap[x, (int) y, z] = world.GetHex(new Vector3(x,y,z)+_position);
				}
			}
		}
	}

	public bool IsActive
    {
        get { return chunkObject.activeSelf; }
		set { chunkObject.SetActive(value); }
    }
	public Vector3 _position
    {
		get { return chunkObject.transform.position; }
    }
	bool IsHexInChunk(float _y, float _x, float _z)
    {
		if (_x < 0 || _x > 4 || _y < 0 || _y > 9 || _z < 0 || _z > 4)
		{
			return false;
		}
		else if (_x == 0 || _y == 0 || _z == 0 || _x == 4 || _y == 9 || _z == 4)
		{
			return false;
		}
        else
        {
			return true;
        }
		
	}

	bool CheckVoxel(float _y, float _x, float _z)
    {
        if (!IsHexInChunk(_y, _x, _z))
        {
			return world.blocktypes[world.GetHex(new Vector3(_x, _y, _z)+_position)].isSolid;
        }


		return world.blocktypes[voxelMap[(int)_x, (int)_y, (int)_z]].isSolid;
    }

	void AddTexture(int textureId, Vector2[] hexUVs)
    {   //1 için
		float y = textureId / VoxelData.TextureAtlasSizeInBlocks; //0
		float x = textureId - (y * VoxelData.TextureAtlasSizeInBlocks);  //1

		x *= VoxelData.NormalizedBlockTextureSize; //0.25
		y *= VoxelData.NormalizedBlockTextureSize; //0

		y = 1f - y - VoxelData.NormalizedBlockTextureSize;  //0.75
												  //float y = Mathf.Floor(textureId / VoxelData.TextureAtlasSizeInBlocks) / VoxelData.TextureAtlasSizeInBlocks;
												  //float x = (textureId % VoxelData.TextureAtlasSizeInBlocks) / VoxelData.TextureAtlasSizeInBlocks;

		//Vector2 textureUV = new Vector2(x, y);
		//uvs.AddRange(hexUVs.Select(u => u + textureUV));

		for (int i = 0; i < hexUVs.Length; i++)
		{
			Vector2 textureUV = hexUVs[i];
			textureUV.x = textureUV.x * VoxelData.NormalizedBlockTextureSize + x;
			textureUV.y = textureUV.y * VoxelData.NormalizedBlockTextureSize + y;

			uvs.Add(textureUV);
		}

	}
	void CreateChunk()
    {
		List<Vector3> tempVertices = new List<Vector3>();
		for (float y = 0; y < VoxelData.ChunkHeight; y++)
		{
			for (int z = 0; z < VoxelData.ChunkWidth; z++)
			{
				for (int x = 0; x < VoxelData.ChunkWidth; x++)
				{
					if (!CheckVoxel(y, x, z))
					{
						byte blockID = voxelMap[x, (int)y, z];
						//x = (chunkCountX * 5 + x);
						//z = (chunkCountZ * 5 + z);
						Vector3 position;
						position.x = (((chunkCountX + x) + (chunkCountZ  + z) * 0.5f - (chunkCountZ  + z) / 2) * (VoxelData.innerRadius * 2f))+chunkCountX*2f* VoxelData.innerRadius;// - chunkCountX; bu niye 2f hiçbir fikrim yok. Sanırım bu inner radius
						position.y = -1f * y;
						position.z = ((chunkCountZ  + z) * (VoxelData.outerRadius * 1.5f)) + chunkCountZ * VoxelData.outerRadius;//-chunkCountZ; bu da neden 1f bilmiyorum onu sormak lazım chatgptye bu da outerradius


						int triangleOffset = vertices.Count;
						vertices.AddRange(VoxelData.hexVertices.Select(v => v + position));
						triangles.AddRange(VoxelData.hexTriangles.Select(t => t + triangleOffset));

						//normals.AddRange(hexNormals);
						//uvs.AddRange(hexUvs);
						//AddTexture(3, hexUvs);
						//AddTexture(world.blocktypes[blockID].GetTextureID(0), hexUvs);
						AddText(blockID, VoxelData.hexUvs);
					}
				}
			}
		}
	}
	//buraya facecheck ekle
	void CreateChunkWithFaces()
	{
		List<Vector3> tempVertices = new List<Vector3>();
		for (float y = 0; y < VoxelData.ChunkHeight; y++)
		{
			for (int z = 0; z < VoxelData.ChunkWidth; z++)
			{
				for (int x = 0; x < VoxelData.ChunkWidth; x++)
				{
					int hexOffset = 0;
					int triangleOffset = 0;
					for(int i = 0; i < 2; i++)
                    {
						float _y = y + VoxelData.hexFaces[i].y;
						float _z = z + VoxelData.hexFaces[i].z;
						float _x = x + VoxelData.hexFaces[i].x;

						if (!CheckVoxel(_y,_x, _z))
                        {
							byte blockID = voxelMap[x, (int)y, z];

							Vector3 position;
							position.x = (((chunkCountX + x) + (chunkCountZ + z) * 0.5f - (chunkCountZ + z) / 2) * (VoxelData.innerRadius * 2f)) + chunkCountX * 2f * VoxelData.innerRadius;// - chunkCountX; bu niye 2f hiçbir fikrim yok. Sanırım bu inner radius
							position.y = -1f * y;
							position.z = ((chunkCountZ + z) * (VoxelData.outerRadius * 1.5f)) + chunkCountZ * VoxelData.outerRadius;

							vertices.Add(VoxelData.hexVertices[hexOffset] + position);
							vertices.Add(VoxelData.hexVertices[hexOffset+1] + position);
							vertices.Add(VoxelData.hexVertices[hexOffset+2] + position);
							vertices.Add(VoxelData.hexVertices[hexOffset+3] + position);
							vertices.Add(VoxelData.hexVertices[hexOffset+4] + position);
							vertices.Add(VoxelData.hexVertices[hexOffset+5] + position);
							hexOffset += 6;

							triangles.Add(VoxelData.hexTriangles[triangleOffset]);
							triangles.Add(VoxelData.hexTriangles[triangleOffset + 1]);
							triangles.Add(VoxelData.hexTriangles[triangleOffset + 2]);
							triangles.Add(VoxelData.hexTriangles[triangleOffset + 3]);
							triangles.Add(VoxelData.hexTriangles[triangleOffset + 4]);
							triangles.Add(VoxelData.hexTriangles[triangleOffset + 5]);
							triangles.Add(VoxelData.hexTriangles[triangleOffset + 6]);
							triangles.Add(VoxelData.hexTriangles[triangleOffset + 7]);
							triangles.Add(VoxelData.hexTriangles[triangleOffset + 8]);
							triangles.Add(VoxelData.hexTriangles[triangleOffset + 9]);
							triangles.Add(VoxelData.hexTriangles[triangleOffset + 10]);
							triangles.Add(VoxelData.hexTriangles[triangleOffset + 11]);
							triangleOffset += 12;

							//AddText(blockID, hexUvs);
						}
                    }
					for(int i = 2; i < VoxelData.hexFaces.Length; i++)
                    {
						float _y = y + VoxelData.hexFaces[i].y;
						float _z = z + VoxelData.hexFaces[i].z;
						float _x = x + VoxelData.hexFaces[i].x;
						if (!CheckVoxel(_y, _x, _z))
						{
							byte blockID = voxelMap[x, (int)y, z];

							Vector3 position;
							position.x = (((chunkCountX + x) + (chunkCountZ + z) * 0.5f - (chunkCountZ + z) / 2) * (VoxelData.innerRadius * 2f)) + chunkCountX * 2f * VoxelData.innerRadius;// - chunkCountX; bu niye 2f hiçbir fikrim yok. Sanırım bu inner radius
							position.y = -1f * y;
							position.z = ((chunkCountZ + z) * (1f * 1.5f)) + chunkCountZ * 1f;

							vertices.Add(VoxelData.hexVertices[hexOffset]+ position);
							vertices.Add(VoxelData.hexVertices[hexOffset + 1] + position);
							vertices.Add(VoxelData.hexVertices[hexOffset + 2] + position);
							vertices.Add(VoxelData.hexVertices[hexOffset + 3] + position);
							hexOffset += 4;

							triangles.Add(VoxelData.hexTriangles[triangleOffset]);
							triangles.Add(VoxelData.hexTriangles[triangleOffset + 1]);
							triangles.Add(VoxelData.hexTriangles[triangleOffset + 2]);
							triangles.Add(VoxelData.hexTriangles[triangleOffset + 3]);
							triangles.Add(VoxelData.hexTriangles[triangleOffset + 4]);
							triangles.Add(VoxelData.hexTriangles[triangleOffset + 5]);
							triangleOffset += 6;

							//AddText(blockID, hexUvs);
						}
					}
				}
			}
		}
	}
	void AddText(int blockID, Vector2[] hexUVs)
    {

		//Vector2 textureUV = new Vector2(x, y);
		//uvs.AddRange(hexUVs.Select(u => u + textureUV));

		int textureIterator = 0;

		for (int i = 0; i < hexUVs.Length; i++)
		{
            if (i == 6 || i == 12)
            {
				textureIterator++;
            }
			else if(i>12 && i%4 == 0)
            {
				textureIterator++;
            }
			int textureId = world.blocktypes[blockID].GetTextureID(textureIterator);
			float y = textureId / VoxelData.TextureAtlasSizeInBlocks; //0
			float x = textureId - (y * VoxelData.TextureAtlasSizeInBlocks);  //0

			x *= VoxelData.NormalizedBlockTextureSize; //0
			y *= VoxelData.NormalizedBlockTextureSize; //0

			y = 1f - y - VoxelData.NormalizedBlockTextureSize;  // 0.75f
													  //x veya y 0 ise çarpma gibi bişeyler eklenebilir.
			Vector2 textureUV = hexUVs[i];
			textureUV.x = textureUV.x * VoxelData.NormalizedBlockTextureSize + x;
			textureUV.y = textureUV.y * VoxelData.NormalizedBlockTextureSize + y;

			uvs.Add(textureUV);
		}
	}
	void AddTextFaces(int blockID, Vector2[] hexUVs,int faceID)
	{

		for (int i = 0; i < hexUVs.Length; i++)
		{
			int textureId = world.blocktypes[blockID].GetTextureID(faceID);
			float y = textureId / VoxelData.TextureAtlasSizeInBlocks; //0
			float x = textureId - (y * VoxelData.TextureAtlasSizeInBlocks);  //0

			x *= VoxelData.NormalizedBlockTextureSize; //0
			y *= VoxelData.NormalizedBlockTextureSize; //0

			y = 1f - y - VoxelData.NormalizedBlockTextureSize;  // 0.75f
													  //x veya y 0 ise çarpma gibi bişeyler eklenebilir.
			Vector2 textureUV = hexUVs[i];
			textureUV.x = textureUV.x * VoxelData.NormalizedBlockTextureSize + x;
			textureUV.y = textureUV.y * VoxelData.NormalizedBlockTextureSize + y;

			uvs.Add(textureUV);
		}
	}

	void CreateMesh()
    {
		Mesh mesh = new Mesh();

		mesh.vertices = vertices.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.triangles = triangles.ToArray();

		mesh.RecalculateNormals();
		//mesh.RecalculateBounds();
		//mesh.Optimize();
		meshFilter.mesh = mesh;
	}
}

//Chunk's position in all of the Chunks
public class ChunkCoord{
	public int x;
	public int z;

	public ChunkCoord(int _x, int _z)
    {
		x = _x;
		z = _z;
    }
}

/* version 1.0.6
 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Chunk : MonoBehaviour
{
	[SerializeField] private Material material;
	[SerializeField] private float radius = 5;
	[SerializeField] private int numVertices = 6;
	[SerializeField] private int VoxelVertices = 4;

	public static readonly int TextureAtlasSizeInBlocks = 4;
	public static float NormalizedBlockTextureSize {
		get { return 1f / (float)TextureAtlasSizeInBlocks; }
	}


	const float innerRadius = 1f * 0.866025404f;
	Vector3 p00 = new Vector3(0.00f, 0.50f, 1.00f);
	Vector3 p01 = new Vector3(Mathf.Sqrt(3.00f) / 2.00f, 0.50f, (1.00f / 2.00f));
	Vector3 p02 = new Vector3(Mathf.Sqrt(3.00f) / 2.00f, 0.50f, -(1.00f / 2.00f));
	Vector3 p03 = new Vector3(0.00f, 0.50f, -1.00f);
	Vector3 p04 = new Vector3(-Mathf.Sqrt(3.00f) / 2.00f, 0.50f, -(1.00f / 2.00f));
	Vector3 p05 = new Vector3(-Mathf.Sqrt(3.00f) / 2.00f, 0.50f, (1.00f / 2.00f));
	Vector3 p06 = new Vector3(0.00f, -0.50f, 1.00f);
	Vector3 p07 = new Vector3(Mathf.Sqrt(3.00f) / 2.00f, -0.50f, (1.00f / 2.00f));
	Vector3 p08 = new Vector3(Mathf.Sqrt(3.00f) / 2.00f, -0.50f, -(1.00f / 2.00f));
	Vector3 p09 = new Vector3(0.00f, -0.50f, -1.00f);
	Vector3 p10 = new Vector3(-Mathf.Sqrt(3.00f) / 2.00f, -0.50f, -(1.00f / 2.00f));
	Vector3 p11 = new Vector3(-Mathf.Sqrt(3.00f) / 2.00f, -0.50f, (1.00f / 2.00f));

	Vector3 up = Vector3.up;
	Vector3 down = Vector3.down;
	Vector3 right = Vector3.right;
	Vector3 left = Vector3.left;
	Vector3 frontRight = new Vector3(0.50f, 0, -((Mathf.Sqrt(3.00f)) / 2.00f));
	Vector3 frontLeft = new Vector3(-0.50f, 0, -((Mathf.Sqrt(3.00f)) / 2.00f));
	Vector3 backRight = new Vector3(0.50f, 0, ((Mathf.Sqrt(3.00f)) / 2.00f));
	Vector3 backLeft = new Vector3(-0.50f, 0, ((Mathf.Sqrt(3.00f)) / 2.00f));

	Vector2 _00 = new Vector2(0.50f, 1.00f); // 0.12 , 1f  //0.12, 1 olmalı
	Vector2 _01 = new Vector2(1.00f, 0.50f + (1.00f / (2.00f * Mathf.Sqrt(3.00f))));  // 0.25, 0.78
	Vector2 _02 = new Vector2(1.00f, (1.00f / (2.00f * Mathf.Sqrt(3.00f))));  //0.25, 0.28
	Vector2 _03 = new Vector2(0.50f, 0.00f);  //0.12, 0.75
	Vector2 _04 = new Vector2(0.00f, (1.00f / (2.00f * Mathf.Sqrt(3.00f)))); //0, 0.28
	Vector2 _05 = new Vector2(0.00f, 0.50f + (1.00f / (2.00f * Mathf.Sqrt(3.00f)))); //0, 0.78

	Vector2 _06 = new Vector2(0f, 0f);
	Vector2 _07 = new Vector2(1f, 0f);
	Vector2 _08 = new Vector2(0f, 1f);
	Vector2 _09 = new Vector2(1f, 1f);

	List<Vector3> vertices = new List<Vector3>();
	List<int> triangles = new List<int>();
	List<Vector2> uvs = new List<Vector2>();
	List<Vector3> normals = new List<Vector3>();

	byte[,,] voxelMap = new byte[5, 10, 5];

	World world;

	void Start()
	{

		world = GameObject.Find("World").GetComponent<World>();

		MeshFilter mf = GetComponent<MeshFilter>();
		Mesh mesh = new Mesh();
		mf.mesh = mesh;
		mesh.Clear();



		#region Vertices clockwise top to bottom
		Vector3[] hexVertices = new Vector3[]
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


		#region Normales
		Vector3[] hexNormals = new Vector3[]
		{
		// Top
		up, up, up, up, up, up,
		
		// Bottom
		down, down, down, down, down, down,
		
		// Right
		right, right, right, right,
		
		// left
		left, left, left, left,
		
		// Front Right
		frontRight, frontRight, frontRight, frontRight,
		
		// Front Left
		frontLeft, frontLeft, frontLeft, frontLeft,
		
		// Back Right
		backRight, backRight, backRight, backRight,
		
		// Back Left
		backLeft, backLeft, backLeft, backLeft,
		};
		#endregion

		#region UVs
		Vector2[] hexUvs = new Vector2[]
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
		int[] hexTriangles = new int[]
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


		PopulateVoxelMap();
		List<Vector3> tempVertices = new List<Vector3>();
		for (float y = 0; y < 10; y++)
		{
			for (int z = 0; z < 5; z++)
			{
				for (int x = 0; x < 5; x++)
				{
					if (!CheckVoxel(y, x, z))
					{
						byte blockID = voxelMap[x,(int) y, z];
						Vector3 position;
						position.x = (x + z * 0.5f - z / 2) * (innerRadius * 2f);
						position.y = -1f * y;
						position.z = z * (1f * 1.5f);

						int triangleOffset = vertices.Count;
						vertices.AddRange(hexVertices.Select(v => v + position));
						triangles.AddRange(hexTriangles.Select(t => t + triangleOffset));

						normals.AddRange(hexNormals);
						//uvs.AddRange(hexUvs);
						//AddTexture(3, hexUvs);
						//AddTexture(world.blocktypes[blockID].GetTextureID(0), hexUvs);
						AddText(blockID, hexUvs);
					}
				}
			}
		}

		mesh.vertices = vertices.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.triangles = triangles.ToArray();

		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mesh.Optimize();


	}


	void PopulateVoxelMap()
    {
		for (float y = 0; y < 10; y++)
		{
			for (int z = 0; z < 5; z++)
			{
				for (int x = 0; x < 5; x++)
				{
					//böyle hepsine true deyince olmaz tabi
					voxelMap[x, (int) y, z] = 2;
				}
			}
		}
	}

	bool CheckVoxel(float _y, int _x, int _z)
    {

		if(_x< 0 || _x>4|| (int)_y < 0 || (int)_y > 9|| _z < 0 || _z > 4)
        {
			return false;
        }
		if(_x==0|| _y == 0 || _z == 0 || _x == 4 || _y == 9 || _z == 4)
        {
			return false;
        }


		return world.blocktypes[voxelMap[_x, (int)_y, _z]].isSolid;
    }

	void AddTexture(int textureId, Vector2[] hexUVs)
    {   //1 için
		float y = textureId / TextureAtlasSizeInBlocks; //0
		float x = textureId - (y * TextureAtlasSizeInBlocks);  //1

		x *= NormalizedBlockTextureSize; //0.25
		y *= NormalizedBlockTextureSize; //0

		y = 1f - y - NormalizedBlockTextureSize;  //0.75
												  //float y = Mathf.Floor(textureId / VoxelData.TextureAtlasSizeInBlocks) / VoxelData.TextureAtlasSizeInBlocks;
												  //float x = (textureId % VoxelData.TextureAtlasSizeInBlocks) / VoxelData.TextureAtlasSizeInBlocks;

		//Vector2 textureUV = new Vector2(x, y);
		//uvs.AddRange(hexUVs.Select(u => u + textureUV));

		for (int i = 0; i < hexUVs.Length; i++)
		{
			Vector2 textureUV = hexUVs[i];
			textureUV.x = textureUV.x * NormalizedBlockTextureSize + x;
			textureUV.y = textureUV.y * NormalizedBlockTextureSize + y;

			uvs.Add(textureUV);
		}

	}

	void AddText(int blockID, Vector2[] hexUVs)
    {

		//Vector2 textureUV = new Vector2(x, y);
		//uvs.AddRange(hexUVs.Select(u => u + textureUV));

		int textureIterator = 0;

		for (int i = 0; i < hexUVs.Length; i++)
		{
            if (i == 6 || i == 12)
            {
				textureIterator++;
            }
			else if(i>12 && i%4 == 0)
            {
				textureIterator++;
            }
			int textureId = world.blocktypes[blockID].GetTextureID(textureIterator);
			float y = textureId / TextureAtlasSizeInBlocks; //0
			float x = textureId - (y * TextureAtlasSizeInBlocks);  //0

			x *= NormalizedBlockTextureSize; //0
			y *= NormalizedBlockTextureSize; //0

			y = 1f - y - NormalizedBlockTextureSize;  // 0.75f
													  //x veya y 0 ise çarpma gibi bişeyler eklenebilir.
			Vector2 textureUV = hexUVs[i];
			textureUV.x = textureUV.x * NormalizedBlockTextureSize + x;
			textureUV.y = textureUV.y * NormalizedBlockTextureSize + y;

			uvs.Add(textureUV);
		}
	}
}

 */

/* version 1.0.5
 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Chunk : MonoBehaviour
{
	[SerializeField] private Material material;
	[SerializeField] private float radius = 5;
	[SerializeField] private int numVertices = 6;
	[SerializeField] private int VoxelVertices = 4;

	public static readonly int TextureAtlasSizeInBlocks = 4;
	public static float NormalizedBlockTextureSize {
		get { return 1f / (float)TextureAtlasSizeInBlocks; }
	}


	const float innerRadius = 1f * 0.866025404f;
	Vector3 p00 = new Vector3(0.00f, 0.50f, 1.00f);
	Vector3 p01 = new Vector3(Mathf.Sqrt(3.00f) / 2.00f, 0.50f, (1.00f / 2.00f));
	Vector3 p02 = new Vector3(Mathf.Sqrt(3.00f) / 2.00f, 0.50f, -(1.00f / 2.00f));
	Vector3 p03 = new Vector3(0.00f, 0.50f, -1.00f);
	Vector3 p04 = new Vector3(-Mathf.Sqrt(3.00f) / 2.00f, 0.50f, -(1.00f / 2.00f));
	Vector3 p05 = new Vector3(-Mathf.Sqrt(3.00f) / 2.00f, 0.50f, (1.00f / 2.00f));
	Vector3 p06 = new Vector3(0.00f, -0.50f, 1.00f);
	Vector3 p07 = new Vector3(Mathf.Sqrt(3.00f) / 2.00f, -0.50f, (1.00f / 2.00f));
	Vector3 p08 = new Vector3(Mathf.Sqrt(3.00f) / 2.00f, -0.50f, -(1.00f / 2.00f));
	Vector3 p09 = new Vector3(0.00f, -0.50f, -1.00f);
	Vector3 p10 = new Vector3(-Mathf.Sqrt(3.00f) / 2.00f, -0.50f, -(1.00f / 2.00f));
	Vector3 p11 = new Vector3(-Mathf.Sqrt(3.00f) / 2.00f, -0.50f, (1.00f / 2.00f));

	Vector3 up = Vector3.up;
	Vector3 down = Vector3.down;
	Vector3 right = Vector3.right;
	Vector3 left = Vector3.left;
	Vector3 frontRight = new Vector3(0.50f, 0, -((Mathf.Sqrt(3.00f)) / 2.00f));
	Vector3 frontLeft = new Vector3(-0.50f, 0, -((Mathf.Sqrt(3.00f)) / 2.00f));
	Vector3 backRight = new Vector3(0.50f, 0, ((Mathf.Sqrt(3.00f)) / 2.00f));
	Vector3 backLeft = new Vector3(-0.50f, 0, ((Mathf.Sqrt(3.00f)) / 2.00f));

	Vector2 _00 = new Vector2(0.50f, 1.00f); // 0.12 , 1f  //0.12, 1 olmalı
	Vector2 _01 = new Vector2(1.00f, 0.50f + (1.00f / (2.00f * Mathf.Sqrt(3.00f))));  // 0.25, 0.78
	Vector2 _02 = new Vector2(1.00f, (1.00f / (2.00f * Mathf.Sqrt(3.00f))));  //0.25, 0.28
	Vector2 _03 = new Vector2(0.50f, 0.00f);  //0.12, 0.75
	Vector2 _04 = new Vector2(0.00f, (1.00f / (2.00f * Mathf.Sqrt(3.00f)))); //0, 0.28
	Vector2 _05 = new Vector2(0.00f, 0.50f + (1.00f / (2.00f * Mathf.Sqrt(3.00f)))); //0, 0.78

	Vector2 _06 = new Vector2(0f, 0f);
	Vector2 _07 = new Vector2(1f, 0f);
	Vector2 _08 = new Vector2(0f, 1f);
	Vector2 _09 = new Vector2(1f, 1f);

	List<Vector3> vertices = new List<Vector3>();
	List<int> triangles = new List<int>();
	List<Vector2> uvs = new List<Vector2>();
	List<Vector3> normals = new List<Vector3>();

	void Start()
	{

		MeshFilter mf = GetComponent<MeshFilter>();
		Mesh mesh = new Mesh();
		mf.mesh = mesh;
		mesh.Clear();



		#region Vertices clockwise top to bottom
		Vector3[] hexVertices = new Vector3[]
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


		#region Normales
		Vector3[] hexNormals = new Vector3[]
		{
		// Top
		up, up, up, up, up, up,
		
		// Bottom
		down, down, down, down, down, down,
		
		// Right
		right, right, right, right,
		
		// left
		left, left, left, left,
		
		// Front Right
		frontRight, frontRight, frontRight, frontRight,
		
		// Front Left
		frontLeft, frontLeft, frontLeft, frontLeft,
		
		// Back Right
		backRight, backRight, backRight, backRight,
		
		// Back Left
		backLeft, backLeft, backLeft, backLeft,
		};
		#endregion

		#region UVs
		Vector2[] hexUvs = new Vector2[]
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
		int[] hexTriangles = new int[]
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


		List<Vector3> tempVertices = new List<Vector3>();
		for (float y = 0; y < 10; y++)
		{
			for (int z = 0; z < 5; z++)
			{
				for (int x = 0; x < 5; x++)
				{
					Vector3 position;
					position.x = (x + z * 0.5f - z / 2) * (innerRadius * 2f);
					position.y = -1f * y;
					position.z = z * (1f * 1.5f);

					int triangleOffset = vertices.Count;
					vertices.AddRange(hexVertices.Select(v => v + position));
					triangles.AddRange(hexTriangles.Select(t => t + triangleOffset));

					normals.AddRange(hexNormals);
					//uvs.AddRange(hexUvs);
					AddTexture(0, hexUvs);
				}
			}
		}

		mesh.vertices = vertices.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.triangles = triangles.ToArray();

		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mesh.Optimize();


	}

	void AddTexture(int textureId, Vector2[] hexUVs)
    {
		float y = textureId / TextureAtlasSizeInBlocks; //0
		float x = textureId - (y * TextureAtlasSizeInBlocks);  //0

		x *= NormalizedBlockTextureSize; //0
		y *= NormalizedBlockTextureSize; //0

		y = 1f - y - NormalizedBlockTextureSize;  // 0.75f

		//Vector2 textureUV = new Vector2(x, y);
		//uvs.AddRange(hexUVs.Select(u => u + textureUV));

		for(int i = 0; i < hexUVs.Length; i++)
        {
			//x veya y 0 ise çarpma gibi bişeyler eklenebilir.
			Vector2 textureUV = hexUVs[i];
            if (textureUV.x != 0.00f)
            {
				textureUV.x = textureUV.x * (x + NormalizedBlockTextureSize);

			}
            else if (textureUV.x==0.00f)
            {
				textureUV.x = x;
            }

			if (textureUV.y == 1.00f || textureUV.y == 0.50f)
			{
				textureUV.y = textureUV.y * (y + NormalizedBlockTextureSize);

			}
			else if (textureUV.y == 0.00f)
            {
				textureUV.y = y;
            }
            else
            {
				textureUV.y = (textureUV.y / TextureAtlasSizeInBlocks) + y;
			}
			//uvs.Add(new Vector2(textureUV.x*(x+NormalizedBlockTextureSize), textureUV.y * (y + NormalizedBlockTextureSize)));
			uvs.Add(new Vector2(textureUV.x, textureUV.y));
		}

    }
}
 
 */

/* version 1.0.4 //produces a chunk but side edges are a bit bleak
 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Chunk : MonoBehaviour
{
    [SerializeField] private Material material;
    [SerializeField] private float radius = 5;
    [SerializeField] private int numVertices = 6;
    [SerializeField] private int VoxelVertices = 4;


	float innerRadius = Mathf.Sqrt(3.00f) / 2.00f;
	Vector3 p00 = new Vector3(0.00f, 0.50f, 1.00f);
	Vector3 p01 = new Vector3(Mathf.Sqrt(3.00f) / 2.00f, 0.50f, (1.00f / 2.00f));
	Vector3 p02 = new Vector3(Mathf.Sqrt(3.00f) / 2.00f, 0.50f, -(1.00f / 2.00f));
	Vector3 p03 = new Vector3(0.00f, 0.50f, -1.00f);
	Vector3 p04 = new Vector3(-Mathf.Sqrt(3.00f) / 2.00f, 0.50f, -(1.00f / 2.00f));
	Vector3 p05 = new Vector3(-Mathf.Sqrt(3.00f) / 2.00f, 0.50f, (1.00f / 2.00f));
	Vector3 p06 = new Vector3(0.00f, -0.50f, 1.00f);
	Vector3 p07 = new Vector3(Mathf.Sqrt(3.00f) / 2.00f, -0.50f, (1.00f / 2.00f));
	Vector3 p08 = new Vector3(Mathf.Sqrt(3.00f) / 2.00f, -0.50f, -(1.00f / 2.00f));
	Vector3 p09 = new Vector3(0.00f, -0.50f, -1.00f);
	Vector3 p10 = new Vector3(-Mathf.Sqrt(3.00f) / 2.00f, -0.50f, -(1.00f / 2.00f));
	Vector3 p11 = new Vector3(-Mathf.Sqrt(3.00f) / 2.00f, -0.50f, (1.00f / 2.00f));

	Vector3 up = Vector3.up;
	Vector3 down = Vector3.down;
	Vector3 right = Vector3.right;
	Vector3 left = Vector3.left;
	Vector3 frontRight = new Vector3(0.50f, 0, -((Mathf.Sqrt(3.00f)) / 2.00f));
	Vector3 frontLeft = new Vector3(-0.50f, 0, -((Mathf.Sqrt(3.00f)) / 2.00f));
	Vector3 backRight = new Vector3(0.50f, 0, ((Mathf.Sqrt(3.00f)) / 2.00f));
	Vector3 backLeft = new Vector3(-0.50f, 0, ((Mathf.Sqrt(3.00f)) / 2.00f));

	Vector2 _00 = new Vector2(0.50f, 1.00f);
	Vector2 _01 = new Vector2(1.00f, 0.50f + (1.00f / (2.00f * Mathf.Sqrt(3.00f))));
	Vector2 _02 = new Vector2(1.00f, (1.00f / (2.00f * Mathf.Sqrt(3.00f))));
	Vector2 _03 = new Vector2(0.50f, 0.00f);
	Vector2 _04 = new Vector2(0.00f, (1.00f / (2.00f * Mathf.Sqrt(3.00f))));
	Vector2 _05 = new Vector2(0.00f, 0.50f + (1.00f / (2.00f * Mathf.Sqrt(3.00f))));

	Vector2 _06 = new Vector2(0f, 0f);
	Vector2 _07 = new Vector2(1f, 0f);
	Vector2 _08 = new Vector2(0f, 1f);
	Vector2 _09 = new Vector2(1f, 1f);

	List<Vector3> vertices = new List<Vector3>();
	List<int> triangles = new List<int>();
	List<Vector2> uvs = new List<Vector2>();
	List<Vector3> normals = new List<Vector3>();

	void Start()
    {

		MeshFilter mf = GetComponent<MeshFilter>();
		Mesh mesh = new Mesh();
		mf.mesh = mesh;
		mesh.Clear();



		#region Vertices clockwise top to bottom
		Vector3[] hexVertices = new Vector3[]
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


		#region Normales
		Vector3[] hexNormals = new Vector3[]
		{
		// Top
		up, up, up, up, up, up,
		
		// Bottom
		down, down, down, down, down, down,
		
		// Right
		right, right, right, right,
		
		// left
		left, left, left, left,
		
		// Front Right
		frontRight, frontRight, frontRight, frontRight,
		
		// Front Left
		frontLeft, frontLeft, frontLeft, frontLeft,
		
		// Back Right
		backRight, backRight, backRight, backRight,
		
		// Back Left
		backLeft, backLeft, backLeft, backLeft,
		};
		#endregion

		#region UVs
		Vector2[] hexUvs = new Vector2[]
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
		int[] hexTriangles = new int[]
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


		List<Vector3> tempVertices = new List<Vector3>();
		for (float y = 0; y < 10; y++)
		{
			for (int z = 0; z < 5; z++)
			{
				for (int x = 0; x < 5; x++)
				{
					Vector3 position;
					position.x = (x + z * 0.5f - z / 2) * (innerRadius * 2f);
					position.y = y / 2;
					position.z = z * (1f * 1.5f);

					int triangleOffset = vertices.Count;
					vertices.AddRange(hexVertices.Select(v => v + position));
					triangles.AddRange(hexTriangles.Select(t => t + triangleOffset));

					triangles.AddRange(hexTriangles);
					normals.AddRange(hexNormals);
					uvs.AddRange(hexUvs);

				}
			}
		}
		vertices.AddRange(tempVertices);

		mesh.vertices = vertices.ToArray();
		//mesh.normals = normals.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.triangles = triangles.ToArray();

		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mesh.Optimize();


	}
}
 */

/* best version 1.0.3
 *using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    [SerializeField] private Material material;
    [SerializeField] private float radius = 5;
    [SerializeField] private int numVertices = 6;
    [SerializeField] private int VoxelVertices = 4;

    void Start()
    {

		MeshFilter mf = GetComponent<MeshFilter>();
		Mesh mesh = new Mesh();
		mf.mesh = mesh;
		mesh.Clear();

		#region Vertices clockwise top to bottom
		Vector3 p00 = new Vector3(0.00f, 0.50f, 1.00f);
		Vector3 p01 = new Vector3(Mathf.Sqrt(3.00f) / 2.00f, 0.50f, (1.00f / 2.00f));
		Vector3 p02 = new Vector3(Mathf.Sqrt(3.00f) / 2.00f, 0.50f, -(1.00f / 2.00f));
		Vector3 p03 = new Vector3(0.00f, 0.50f, -1.00f);
		Vector3 p04 = new Vector3(-Mathf.Sqrt(3.00f) / 2.00f, 0.50f, -(1.00f / 2.00f));
		Vector3 p05 = new Vector3(-Mathf.Sqrt(3.00f) / 2.00f, 0.50f, (1.00f / 2.00f));
		Vector3 p06 = new Vector3(0.00f, -0.50f, 1.00f);
		Vector3 p07 = new Vector3(Mathf.Sqrt(3.00f) / 2.00f, -0.50f, (1.00f / 2.00f));
		Vector3 p08 = new Vector3(Mathf.Sqrt(3.00f) / 2.00f, -0.50f, -(1.00f / 2.00f));
		Vector3 p09 = new Vector3(0.00f, -0.50f, -1.00f);
		Vector3 p10 = new Vector3(-Mathf.Sqrt(3.00f) / 2.00f, -0.50f, -(1.00f / 2.00f));
		Vector3 p11 = new Vector3(-Mathf.Sqrt(3.00f) / 2.00f, -0.50f, (1.00f / 2.00f));

		Vector3[] vertices = new Vector3[]
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

		#region Normales
		Vector3 up = Vector3.up;
		Vector3 down = Vector3.down;
		Vector3 right = Vector3.right;
		Vector3 left = Vector3.left;
		Vector3 frontRight = new Vector3(0.50f, 0, -((Mathf.Sqrt(3.00f)) / 2.00f));
		Vector3 frontLeft = new Vector3(-0.50f, 0, -((Mathf.Sqrt(3.00f)) / 2.00f));
		Vector3 backRight = new Vector3(0.50f, 0, ((Mathf.Sqrt(3.00f)) / 2.00f));
		Vector3 backLeft = new Vector3(-0.50f, 0, ((Mathf.Sqrt(3.00f)) / 2.00f));

		Vector3[] normals = new Vector3[]
		{
		// Top
		up, up, up, up, up, up,
		
		// Bottom
		down, down, down, down, down, down,
		
		// Right
		right, right, right, right,
		
		// left
		left, left, left, left,
		
		// Front Right
		frontRight, frontRight, frontRight, frontRight,
		
		// Front Left
		frontLeft, frontLeft, frontLeft, frontLeft,
		
		// Back Right
		backRight, backRight, backRight, backRight,
		
		// Back Left
		backLeft, backLeft, backLeft, backLeft,
		};
		#endregion

		#region UVs
		Vector2 _00 = new Vector2(0.50f, 1.00f);
		Vector2 _01 = new Vector2(1.00f, 0.50f + (1.00f / (2.00f * Mathf.Sqrt(3.00f))));
		Vector2 _02 = new Vector2(1.00f, (1.00f / (2.00f * Mathf.Sqrt(3.00f))));
		Vector2 _03 = new Vector2(0.50f, 0.00f);
		Vector2 _04 = new Vector2(0.00f, (1.00f / (2.00f * Mathf.Sqrt(3.00f))));
		Vector2 _05 = new Vector2(0.00f, 0.50f + (1.00f / (2.00f * Mathf.Sqrt(3.00f))));

		Vector2 _06 = new Vector2(0f, 0f);
		Vector2 _07 = new Vector2(1f, 0f);
		Vector2 _08 = new Vector2(0f, 1f);
		Vector2 _09 = new Vector2(1f, 1f);


		Vector2[] uvs = new Vector2[]
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
		int[] triangles = new int[]
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

		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.uv = uvs;
		mesh.triangles = triangles;

		mesh.RecalculateBounds();
		mesh.Optimize();
	}
}

 
 */

/* another working version 1.0.2
 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    [SerializeField] private Material material;
    [SerializeField] private float radius = 5;
    [SerializeField] private int numVertices = 6;
    [SerializeField] private int VoxelVertices = 4;

    public static readonly int[,] voxelTrisSpare = new int[12, 3] {
        {0,  6, 1}, //0
        {6,  7, 1}, //1

        {1,  7, 2}, //2
        {7,  8, 2}, //3

        {2,  8, 3}, //4
        {8,  9, 3}, //5

        {3,  9, 4},//6
        {9,  10, 4},//7

        {4,  10, 5},//8
        {10,  11, 5},//9

        {5, 11, 0},//10
        {11, 6, 0}//11
        };

    void Start()
    {

        // Add required components to display a mesh.
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh();

        meshRenderer.material = material;
        meshFilter.mesh = mesh;

        // Angle of each segment in radians.
        float angle = 2 * Mathf.PI / numVertices;
        float VoxelAngle = 2 * Mathf.PI / 4;

        // Create the vertices around the polygon.
        Vector3[] vertices = new Vector3[numVertices * 2 + 24];
        Vector2[] uvs = new Vector2[numVertices * 2 + 24];
        for (int i = 0; i < numVertices; i++)
        {
            vertices[i] = new Vector3(Mathf.Sin(i * angle), 0, Mathf.Cos(i * angle)) * radius;
            uvs[i] = new Vector2(1 + Mathf.Sin(i * angle), 1 + Mathf.Cos(i * angle)) * 0.5f;

            vertices[i + 6] = new Vector3(Mathf.Sin(i * angle), -1, Mathf.Cos(i * angle)) * radius;
            uvs[i + 6] = new Vector2(1 + Mathf.Sin(i * angle), 1 + Mathf.Cos(i * angle)) * 0.5f;


            //vertices[i+6] = new Vector3(Mathf.Cos(i * angle), -2, Mathf.Sin(i * angle)) * radius;
            //uvs[i+6] = new Vector2(1 + Mathf.Cos(i * angle), 1 + Mathf.Sin(i * angle)) * 0.5f;

        }
        
        mesh.vertices = vertices;
        mesh.uv = uvs;

        // The triangle vertices must be done in clockwise order.
        int[] triangles = new int[((3 * (numVertices - 2)) * 2) + (3 * (VoxelVertices - 2)) * 6];
        for (int i = 0; i < numVertices - 2; i++)
        {
            triangles[3 * i] = 0;
            triangles[(3 * i) + 1] = i + 1;
            triangles[(3 * i) + 2] = i + 2;
            triangles[(3 * i) + 12] = 6;
            triangles[(3 * i) + 13] = i + 8;
            triangles[(3 * i) + 14] = i + 7;
        }
       

        int offset = ((3 * (numVertices - 2)) * 2);
        for (int i = 0; i < numVertices; i++)
        {
            int current = i;
            int next = (i + 1) % numVertices;

            triangles[offset + i * 6] = current;
            triangles[offset + i * 6 + 1] = current + numVertices;
            triangles[offset + i * 6 + 2] = next;
            triangles[offset + i * 6 + 3] = current + numVertices;
            triangles[offset + i * 6 + 4] = next + numVertices;
            triangles[offset + i * 6 + 5] = next;
        }
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }
}
 
 */

/* a working version  1.0.1
 * 
 * float radius;
    float width ;
     
    void Start()
    {
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh();

        meshFilter.mesh = mesh;

        Vector3[] vertices = new Vector3[12];

        // Top face
        for (int i = 0; i < 6; i++)
        {
            vertices[i] = new Vector3(radius * Mathf.Sin(i * Mathf.PI / 3), width / 2, radius * Mathf.Cos(i * Mathf.PI / 3)); 
        }

        // Bottom face
        for (int i = 6; i < 12; i++)
        {
            vertices[i] = new Vector3(radius * Mathf.Sin(i * Mathf.PI / 3), -width / 2, radius * Mathf.Cos(i * Mathf.PI / 3));
        }

        mesh.vertices = vertices;

        int[] tri  = new int[60] {
        // Top face:
        0, 1, 2,
        0, 2, 3,
        0, 3, 4,
        0, 4, 5,
        // Bottom face:
        6, 8, 7,
        6, 9, 8,
        6, 10, 9,
        6, 11, 10,
        // Sides:
        0, 6, 1,
        6, 7, 1,
        1, 7, 2,
        7, 8, 2,
        2, 8, 3,
        8, 9, 3,
        3, 9, 4,
        9, 10, 4,
        4, 10, 5,
        10, 11, 5,
        5, 11, 0,
        11, 6, 0
        };

        mesh.triangles = tri;

        mesh.RecalculateNormals();
    }*/
/*
 
    
        // Note: We'll do UVs in the next part.
    }*/
