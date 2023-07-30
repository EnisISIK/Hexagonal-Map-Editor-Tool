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

	byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth ];

	World world;

	int triangleOffsetValue = 0;
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

		chunkObject.transform.position = new Vector3(chunkCoordinates.x*5f, 0f, chunkCoordinates.z*5f);
		chunkObject.transform.SetParent(world.transform);

		chunkObject.name = "Chunk " + chunkCoordinates.x + ", " + chunkCoordinates.z;
		PopulateVoxelMap();
		//CreateChunk();
		CreateChunkRendered();
		CreateMesh();

	}

	void PopulateVoxelMap()
    {
		for (int y = 0; y < VoxelData.ChunkHeight; y++)
		{
			for (int z = 0; z < VoxelData.ChunkWidth; z++)
			{
				for (int x = 0; x < VoxelData.ChunkWidth; x++)
				{
					voxelMap[x, (int)y, z] = world.GetHex(new Vector3(x,y,z)+position);
				}
			}
		}
	}

	public bool IsActive
    {
        get { return chunkObject.activeSelf; }
		set { chunkObject.SetActive(value); }
    }
	public Vector3 position
    {
		get { return chunkObject.transform.position; }
    }
	bool IsHexInChunk(float _y, float _x, float _z)
    {
		if (_x < 0 || _x > VoxelData.ChunkWidth-1 || _y < 0 || _y > VoxelData.ChunkHeight - 1 || _z < 0 || _z > VoxelData.ChunkWidth - 1)
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
		int x = Mathf.FloorToInt(_x);
		int y = Mathf.FloorToInt(_y);
		int z = Mathf.FloorToInt(_z);

		if (!IsHexInChunk(y, x, z))
        {
			return world.blocktypes[world.GetHex(new Vector3(x, y, z)+position)].isSolid;
        }


		return world.blocktypes[voxelMap[x, y, z]].isSolid;
	}

	void CreateChunkRendered()
    {
		List<Vector3> tempVertices = new List<Vector3>();
		for (float y = 0; y < VoxelData.ChunkHeight; y++)
		{
			for (int z = 0; z < VoxelData.ChunkWidth; z++)
			{
				for (int x = 0; x < VoxelData.ChunkWidth; x++)
				{
					byte blockID = voxelMap[x, (int)y, z];

					triangleOffsetValue = vertices.Count;
					RenderUp(new Vector3(x, y, z),blockID, triangleOffsetValue);
					RenderDown(new Vector3(x, y, z), blockID, triangleOffsetValue);
					RenderEast(new Vector3(x, y, z), blockID, triangleOffsetValue);
					RenderWest(new Vector3(x, y, z), blockID, triangleOffsetValue);
					RenderSouthEast(new Vector3(x, y, z), blockID, triangleOffsetValue);
					RenderSouthWest(new Vector3(x, y, z), blockID, triangleOffsetValue);
					RenderNorthEast(new Vector3(x, y, z), blockID, triangleOffsetValue);
					RenderNorthWest(new Vector3(x, y, z), blockID, triangleOffsetValue);
				}
			}
		}
	}

	void AddHexagon(Vector3 pos,int blockID,Vector3[] hexVert , int[] hexTri, Vector2[] hexUV,int triangleOffset,int textureIDNum)
    {
		int _x = (int) pos.x;
		int _y = (int)pos.y;
		int _z = (int)pos.z;

		Vector3 _position;
		_position.x = (((chunkCountX + _x) + (chunkCountZ + _z) * 0.5f - (chunkCountZ + _z) / 2) * (VoxelData.innerRadius * 2f)) + (chunkCountX*8f*VoxelData.innerRadius-position.x);
		_position.y = 1f * _y;
		_position.z = ((chunkCountZ + _z) * (VoxelData.outerRadius * 1.5f)) + chunkCountZ * VoxelData.outerRadius;

		vertices.AddRange(hexVert.Select(v => v + _position));
		triangles.AddRange(hexTri.Select(t => t + triangleOffset));
		for(int i = 0; i < hexUV.Length; i++)
        {
			int textureId = world.blocktypes[blockID].GetTextureID(textureIDNum);
			float y = textureId / VoxelData.TextureAtlasSizeInBlocks;
			float x = textureId - (y * VoxelData.TextureAtlasSizeInBlocks);

			x *= VoxelData.NormalizedBlockTextureSize;
			y *= VoxelData.NormalizedBlockTextureSize;

			y = 1f - y - VoxelData.NormalizedBlockTextureSize;

			Vector2 textureUV = hexUV[i];
			textureUV.x = textureUV.x * VoxelData.NormalizedBlockTextureSize + x;
			textureUV.y = textureUV.y * VoxelData.NormalizedBlockTextureSize + y;

			uvs.Add(textureUV);
		}
	}

	private void RenderUp(Vector3 neighboor,byte blockID,int triangleOffset)
    {
        if (CheckVoxel(neighboor.y+VoxelData.f00.y, neighboor.x + VoxelData.f00.x, neighboor.z + VoxelData.f00.z))
		{
			triangleOffsetValue -= 6;
			return;
        }

		AddHexagon(neighboor,blockID,VoxelData.topVertices,VoxelData.topTriangles,VoxelData.topUvs, triangleOffsetValue, 0);
    }
	private void RenderDown(Vector3 neighboor, byte blockID,int triangleOffset)
	{
		if (CheckVoxel(neighboor.y + VoxelData.f01.y, neighboor.x + VoxelData.f01.x, neighboor.z + VoxelData.f01.z))
		{
			triangleOffsetValue -= 6;
			return;
		}

		AddHexagon(neighboor, blockID, VoxelData.bottomVertices, VoxelData.bottomTriangles, VoxelData.bottomUvs, triangleOffsetValue, 1);
	}
	private void RenderEast(Vector3 neighboor, byte blockID,int triangleOffset)
	{
		if (CheckVoxel(neighboor.y + VoxelData.f02.y, neighboor.x + VoxelData.f02.x, neighboor.z + VoxelData.f02.z))
		{
			triangleOffsetValue -= 4;
			return;
		}

		AddHexagon(neighboor, blockID, VoxelData.rightVertices, VoxelData.rightTriangles, VoxelData.rightUvs, triangleOffsetValue, 2);
	}
	private void RenderWest(Vector3 neighboor, byte blockID,int triangleOffset)
	{
		if (CheckVoxel(neighboor.y + VoxelData.f03.y, neighboor.x + VoxelData.f03.x, neighboor.z + VoxelData.f03.z))
		{
			triangleOffsetValue -= 4;
			return;
		}

		AddHexagon(neighboor, blockID, VoxelData.leftVertices, VoxelData.leftTriangles, VoxelData.leftUvs, triangleOffsetValue, 3);
	}
	private void RenderSouthEast(Vector3 neighboor, byte blockID,int triangleOffset)
	{
		if (CheckVoxel(neighboor.y + VoxelData.f11.y, neighboor.x + VoxelData.f11.x, neighboor.z + VoxelData.f11.z))
		{
			triangleOffsetValue -= 4;
			return;
		}

		AddHexagon(neighboor, blockID, VoxelData.frontRightVertices, VoxelData.frontRightTriangles, VoxelData.frontRightUvs, triangleOffsetValue, 4);
	}
	private void RenderSouthWest(Vector3 neighboor, byte blockID,int triangleOffset)
	{
		if (CheckVoxel(neighboor.y + VoxelData.f09.y, neighboor.x + VoxelData.f09.x, neighboor.z + VoxelData.f09.z))
		{
			triangleOffsetValue -= 4;
			return;
		}

		AddHexagon(neighboor, blockID, VoxelData.frontLeftVertices, VoxelData.frontLeftTriangles, VoxelData.frontLeftUvs, triangleOffsetValue, 5);
	}
	private void RenderNorthEast(Vector3 neighboor, byte blockID,int triangleOffset)
	{
		if (CheckVoxel(neighboor.y + VoxelData.f08.y, neighboor.x + VoxelData.f08.x, neighboor.z + VoxelData.f08.z))
		{
			triangleOffsetValue -= 4;
			return;
		}

		AddHexagon(neighboor, blockID, VoxelData.backRightVertices, VoxelData.backRightTriangles, VoxelData.backRightUvs, triangleOffsetValue, 6);
	}
	private void RenderNorthWest(Vector3 neighboor, byte blockID,int triangleOffset)
	{
		if (CheckVoxel(neighboor.y + VoxelData.f10.y, neighboor.x + VoxelData.f10.x, neighboor.z + VoxelData.f10.z))
		{
			triangleOffsetValue -= 4;
			return;
		}

		AddHexagon(neighboor, blockID, VoxelData.backLeftVertices, VoxelData.backLeftTriangles, VoxelData.backLeftUvs, triangleOffsetValue, 7);
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
			float y = textureId / VoxelData.TextureAtlasSizeInBlocks; 
			float x = textureId - (y * VoxelData.TextureAtlasSizeInBlocks);  

			x *= VoxelData.NormalizedBlockTextureSize; 
			y *= VoxelData.NormalizedBlockTextureSize; 

			y = 1f - y - VoxelData.NormalizedBlockTextureSize;  
													  
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