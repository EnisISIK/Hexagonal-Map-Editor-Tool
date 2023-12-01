﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;

public class Chunk 
{
	private readonly World _world;
	private ChunkMeshRenderer _chunkMeshRenderer;

	private GameObject chunkObject;

	[SerializeField] private Material[] materials= new Material[3];
	private MeshRenderer meshRenderer;
	private MeshFilter meshFilter;

	private ChunkCoord chunkCoordinates;
	public Vector3Int position;

	private bool _isActive;

	public bool isActive
	{
		get { return _isActive; }
		set {
			_isActive = value;
			if(chunkObject != null){
				chunkObject.SetActive(value);
            }
		}
	}

	public Chunk(ChunkCoord _chunkCoordinates ,World world)
    {

		chunkCoordinates = _chunkCoordinates;
		_world = world;
		isActive = true;

	}

	public void Init()
    {
		chunkObject = new GameObject();
		meshFilter = chunkObject.AddComponent<MeshFilter>();
		meshRenderer = chunkObject.AddComponent<MeshRenderer>();

		materials[0] = _world.material;
		materials[1] = _world.transparentMaterial;
		materials[2] = _world.waterMaterial;
		meshRenderer.materials = materials;

		chunkObject.name = "Chunk " + chunkCoordinates.x + ", " + chunkCoordinates.z;
		chunkObject.transform.position = new Vector3(chunkCoordinates.x * HexData.ChunkWidth, 0f, chunkCoordinates.z * HexData.ChunkWidth);
		//Debug.Log(PositionHelper.HexToPixel(new Vector3(chunkCoordinates.x * HexData.ChunkWidth, 0f, chunkCoordinates.z * HexData.ChunkWidth)));
		chunkObject.transform.SetParent(_world.transform);
		position = Vector3Int.FloorToInt(chunkObject.transform.position);

		_chunkMeshRenderer = new ChunkMeshRenderer(_world, position);


		_world.chunksToUpdate.Enqueue(this);
		//_world.StartCoroutine(PopulateHexMap());

	}

	private bool CheckHex(float _y, float _x, float _z, HexState[,,] hexMap)
    {
		int x = Mathf.FloorToInt(_x);
		int y = Mathf.FloorToInt(_y);
		int z = Mathf.FloorToInt(_z);

		if (!PositionHelper.IsHexInChunk(x, y, z))
        {
			return _world.CheckForHex(new Vector3(x, y, z)+position);
        }

		return _world.blocktypes[hexMap[x, y, z].id].isSolid;
	}
	private bool CheckWaterHex(float _y, float _x, float _z, HexState[,,] hexMap)
	{
		int x = Mathf.FloorToInt(_x);
		int y = Mathf.FloorToInt(_y);
		int z = Mathf.FloorToInt(_z);

		if (!PositionHelper.IsHexInChunk(x, y, z))
		{
			return _world.CheckForWaterHex(new Vector3(x, y, z) + position);
		}

		return _world.blocktypes[hexMap[x, y, z].id].isWater;
	}

	public IEnumerator UpdateSurroundingHex(int x, int y, int z,int blockID)
	{
		Vector3Int thisHex = new Vector3Int(x, y, z);

		Task t = Task.Factory.StartNew(delegate
		{
			if (blockID != 0)
			{
				_world.chunksToUpdate.Enqueue(this);
			}
			for (int p = 0; p < 8; p++)
			{
				Vector3Int currentHex = thisHex + HexData.faces[p];

				if (!PositionHelper.IsHexInChunk(currentHex.x, currentHex.y,currentHex.z))
				{
					if(!_world.chunksToUpdate.Contains(_world.GetChunkFromChunkVector3(currentHex + position)))
							_world.chunksToUpdate.Enqueue(_world.GetChunkFromChunkVector3(currentHex + position));
				}
			}
			if (blockID == 0)
			{
				_world.chunksToUpdate.Enqueue(this);
			}
		});
		yield return new WaitUntil(() =>
		{
			return t.IsCompleted;
		});

        if (t.Exception != null)
        {
			Debug.LogError(t.Exception);
        }

	}

	public IEnumerator UpdateChunk(HexState[,,] hexMap)
	{

		ClearMesh();
		Task t = Task.Factory.StartNew(delegate
		{
			for (int y = 0; y < HexData.ChunkHeight; y++)
			{
				for (int z = 0; z < HexData.ChunkWidth; z++)
				{
					for (int x = 0; x < HexData.ChunkWidth; x++)
					{
						if (_world.blocktypes[hexMap[x, y, z].id].isSolid) { 
						AddHexCell(x, y, z, hexMap);
						}
						else if (!_world.blocktypes[hexMap[x, y, z].id].isSolid && _world.blocktypes[hexMap[x, y, z].id].isWater) { 
						AddHexCell(x, y, z, hexMap); 
						}
					}
				}
			}
		});
		yield return new WaitUntil(() => {
			return t.IsCompleted;
		});
        if (t.Exception != null)
        {
			Debug.LogError(t.Exception);
        }
		CreateMesh();
	}

	private void AddHexCell(int x, int y, int z,HexState[,,] hexMap)
    {
		byte blockID = hexMap[x, y, z].id;
		bool isTransparent = _world.blocktypes[hexMap[x, y, z].id].isTransparent;
		bool isWater = _world.blocktypes[hexMap[x, y, z].id].isWater;
		for (int i = 0; i < 8; i++)
        {
			float faceX = x + HexData.faces[i].x;
			if (i >= 4 && i % 2 == z % 2) faceX = x;
			if (isWater && CheckWaterHex(y + HexData.faces[i].y, faceX, z + HexData.faces[i].z, hexMap)) continue; //if the neighbour IS water, dont render the face
			if (CheckHex(y + HexData.faces[i].y, faceX, z + HexData.faces[i].z, hexMap))  //if the neighbour IS a block(and not transparent but its not implemented yet), dont render the face 
			{
				continue;
			}

			float lightLevel;
			int yPos = y + 2;
			bool inShade = false;
			while (yPos < HexData.ChunkHeight)
			{
				if(hexMap[x,yPos,z].id!=0)
				{ 
					inShade = true;
					break;
				}

				yPos++;
			}

			if (inShade) lightLevel = 0.4f;
			else lightLevel = 0f;

			_chunkMeshRenderer.AddHex(new Vector3Int(x, y, z), HexData.hexVertices[i], HexData.hexTriangles[i],HexData.normals[i], isWater, isTransparent, lightLevel);
			_chunkMeshRenderer.AddUvs(blockID, HexData.hexUvs[i], i, isWater);
		}
	}

	private void CreateMesh()
    {
		Mesh mesh = new Mesh();

		mesh.vertices = _chunkMeshRenderer.vertices.ToArray();
		mesh.uv = _chunkMeshRenderer.uvs.ToArray();
		mesh.normals = _chunkMeshRenderer.normals.ToArray();
		mesh.colors = _chunkMeshRenderer.colors.ToArray();
		mesh.subMeshCount = 3;

		mesh.SetTriangles(_chunkMeshRenderer.triangles.ToArray(), 0);
		mesh.SetTriangles(_chunkMeshRenderer.transparentTriangles.ToArray(), 1);
		mesh.SetTriangles(_chunkMeshRenderer.waterTriangles.ToArray(), 2);

		//mesh.RecalculateNormals();

		meshFilter.mesh = mesh;
	}

	private void ClearMesh()
	{
		_chunkMeshRenderer.vertices.Clear();
		_chunkMeshRenderer.uvs.Clear();
		_chunkMeshRenderer.normals.Clear();
		_chunkMeshRenderer.triangles.Clear();
		_chunkMeshRenderer.transparentTriangles.Clear();
		_chunkMeshRenderer.waterTriangles.Clear();
		_chunkMeshRenderer.colors.Clear();
	}
}

//Chunk's position in all of the Chunks
public class ChunkCoord{
	public int x;
	public int z;

	public ChunkCoord(int _x=0, int _z=0)
    {
		x = _x;
		z = _z;
    }

	public ChunkCoord(Vector3 pos)
    {
		int xCheck = Mathf.FloorToInt(pos.x);
		int zCheck = Mathf.FloorToInt(pos.z);

		x = xCheck / HexData.ChunkWidth;
		z = zCheck / HexData.ChunkWidth;
	}

	public bool Equals(ChunkCoord other)
    {
		if (other == null)
			return false;
		else if (other.x == x && other.z == z)
			return true;
		else
			return false;
    }
}

[System.Serializable]
public class HexState
{
	public byte id;
	public BiomeAttributes biome;

	public HexState()
    {
		id = 0;

    }
	public HexState(byte _id)
	{
		id = _id;
	}

	public HexState(byte _id, BiomeAttributes _biome)
	{
		id = _id;
		biome = _biome;
	}
}