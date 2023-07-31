using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Chunk 
{
	[SerializeField] private Material material;

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

	public bool IsActive
	{
		get { return chunkObject.activeSelf; }
		set { chunkObject.SetActive(value); }
	}

	public Vector3 position
	{
		get { return chunkObject.transform.position; }
	}

	byte[,,] hexMap = new byte[HexData.ChunkWidth, HexData.ChunkHeight, HexData.ChunkWidth ];

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
		chunkPosition.x = (((chunkCoordinates.x)*5f) + ((chunkCoordinates.z )* 5f) * 0.5f - ((chunkCoordinates.z) * 5f) / 2) * (HexData.innerRadius * 2f);
		chunkPosition.z = ((chunkCoordinates.z) * 5f) * (HexData.outerRadius * 1.5f);

		meshRenderer.material = world.material;

		chunkObject.transform.position = new Vector3(chunkCoordinates.x*5f, 0f, chunkCoordinates.z*5f);
		chunkObject.transform.SetParent(world.transform);

		chunkObject.name = "Chunk " + chunkCoordinates.x + ", " + chunkCoordinates.z;
		PopulateHexMap();
		CreateChunkRendered();
		CreateMesh();

	}

	void PopulateHexMap()
    {
		for (int y = 0; y < HexData.ChunkHeight; y++)
		{
			for (int z = 0; z < HexData.ChunkWidth; z++)
			{
				for (int x = 0; x < HexData.ChunkWidth; x++)
				{
					hexMap[x, (int)y, z] = world.GetHex(new Vector3(x,y,z)+position);
				}
			}
		}
	}

	bool IsHexInChunk(float _y, float _x, float _z)
    {
		if (_x < 0 || _x > HexData.ChunkWidth-1 || _y < 0 || _y > HexData.ChunkHeight - 1 || _z < 0 || _z > HexData.ChunkWidth - 1)
		{
			return false;
		}
        else
        {
			return true;
        }
		
	}

	bool CheckHexagon(float _y, float _x, float _z)
    {
		int x = Mathf.FloorToInt(_x);
		int y = Mathf.FloorToInt(_y);
		int z = Mathf.FloorToInt(_z);

		if (!IsHexInChunk(y, x, z))
        {
			return world.blocktypes[world.GetHex(new Vector3(x, y, z)+position)].isSolid;
        }


		return world.blocktypes[hexMap[x, y, z]].isSolid;
	}

	void CreateChunkRendered()
    {
		for (float y = 0; y < HexData.ChunkHeight; y++)
		{
			for (int z = 0; z < HexData.ChunkWidth; z++)
			{
				for (int x = 0; x < HexData.ChunkWidth; x++)
				{
					byte blockID = hexMap[x, (int)y, z];

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
		_position.x = (((chunkCountX + _x) + (chunkCountZ + _z) * 0.5f - (chunkCountZ + _z) / 2) * (HexData.innerRadius * 2f)) + (chunkCountX*8f*HexData.innerRadius-position.x);
		_position.y = 1f * _y;
		_position.z = ((chunkCountZ + _z) * (HexData.outerRadius * 1.5f)) + chunkCountZ * HexData.outerRadius;

		vertices.AddRange(hexVert.Select(v => v + _position));
		triangles.AddRange(hexTri.Select(t => t + triangleOffset));
		for(int i = 0; i < hexUV.Length; i++)
        {
			int textureId = world.blocktypes[blockID].GetTextureID(textureIDNum);
			float y = textureId / HexData.TextureAtlasSizeInBlocks;
			float x = textureId - (y * HexData.TextureAtlasSizeInBlocks);

			x *= HexData.NormalizedBlockTextureSize;
			y *= HexData.NormalizedBlockTextureSize;

			y = 1f - y - HexData.NormalizedBlockTextureSize;

			Vector2 textureUV = hexUV[i];
			textureUV.x = textureUV.x * HexData.NormalizedBlockTextureSize + x;
			textureUV.y = textureUV.y * HexData.NormalizedBlockTextureSize + y;

			uvs.Add(textureUV);
		}
	}

	private void RenderUp(Vector3 neighboor,byte blockID,int triangleOffset)
    {
        if (CheckHexagon(neighboor.y+HexData.fu.y, neighboor.x + HexData.fu.x, neighboor.z + HexData.fu.z))
		{
			triangleOffsetValue -= 6;
			return;
        }

		AddHexagon(neighboor,blockID,HexData.topVertices,HexData.topTriangles,HexData.topUvs, triangleOffsetValue, 0);
    }

	private void RenderDown(Vector3 neighboor, byte blockID,int triangleOffset)
	{
		if (CheckHexagon(neighboor.y + HexData.fd.y, neighboor.x + HexData.fd.x, neighboor.z + HexData.fd.z))
		{
			triangleOffsetValue -= 6;
			return;
		}

		AddHexagon(neighboor, blockID, HexData.bottomVertices, HexData.bottomTriangles, HexData.bottomUvs, triangleOffsetValue, 1);
	}

	private void RenderEast(Vector3 neighboor, byte blockID,int triangleOffset)
	{
		if (CheckHexagon(neighboor.y + HexData.fe.y, neighboor.x + HexData.fe.x, neighboor.z + HexData.fe.z))
		{
			triangleOffsetValue -= 4;
			return;
		}

		AddHexagon(neighboor, blockID, HexData.rightVertices, HexData.rightTriangles, HexData.rightUvs, triangleOffsetValue, 2);
	}

	private void RenderWest(Vector3 neighboor, byte blockID,int triangleOffset)
	{
		if (CheckHexagon(neighboor.y + HexData.fw.y, neighboor.x + HexData.fw.x, neighboor.z + HexData.fw.z))
		{
			triangleOffsetValue -= 4;
			return;
		}

		AddHexagon(neighboor, blockID, HexData.leftVertices, HexData.leftTriangles, HexData.leftUvs, triangleOffsetValue, 3);
	}

	private void RenderSouthEast(Vector3 neighboor, byte blockID,int triangleOffset)
	{
		if (CheckHexagon(neighboor.y + HexData.fse.y, neighboor.x + HexData.fse.x, neighboor.z + HexData.fse.z))
		{
			triangleOffsetValue -= 4;
			return;
		}

		AddHexagon(neighboor, blockID, HexData.frontRightVertices, HexData.frontRightTriangles, HexData.frontRightUvs, triangleOffsetValue, 4);
	}

	private void RenderSouthWest(Vector3 neighboor, byte blockID,int triangleOffset)
	{
		if (CheckHexagon(neighboor.y + HexData.fsw.y, neighboor.x + HexData.fsw.x, neighboor.z + HexData.fsw.z))
		{
			triangleOffsetValue -= 4;
			return;
		}

		AddHexagon(neighboor, blockID, HexData.frontLeftVertices, HexData.frontLeftTriangles, HexData.frontLeftUvs, triangleOffsetValue, 5);
	}

	private void RenderNorthEast(Vector3 neighboor, byte blockID,int triangleOffset)
	{
		if (CheckHexagon(neighboor.y + HexData.fne.y, neighboor.x + HexData.fne.x, neighboor.z + HexData.fne.z))
		{
			triangleOffsetValue -= 4;
			return;
		}

		AddHexagon(neighboor, blockID, HexData.backRightVertices, HexData.backRightTriangles, HexData.backRightUvs, triangleOffsetValue, 6);
	}

	private void RenderNorthWest(Vector3 neighboor, byte blockID,int triangleOffset)
	{
		if (CheckHexagon(neighboor.y + HexData.fnw.y, neighboor.x + HexData.fnw.x, neighboor.z + HexData.fnw.z))
		{
			triangleOffsetValue -= 4;
			return;
		}

		AddHexagon(neighboor, blockID, HexData.backLeftVertices, HexData.backLeftTriangles, HexData.backLeftUvs, triangleOffsetValue, 7);
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