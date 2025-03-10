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

	private MeshRenderer meshRenderer;
	private MeshFilter meshFilter;

	private ChunkCoord chunkCoordinates;
	public Vector3Int position;

	private bool _isActive;
	bool chunkUpdatingFlag=false;

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

	//Initialization of chunk
	public void Init()
    {
		chunkObject = new GameObject();
		meshFilter = chunkObject.AddComponent<MeshFilter>();
		meshRenderer = chunkObject.AddComponent<MeshRenderer>();

		Material[] materials = new Material[3];
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

	}


	//Checks if the given block is solid
	private bool CheckHex(float _y, float _x, float _z, byte[,,] hexMap)
    {
		int x = Mathf.FloorToInt(_x);
		int y = Mathf.FloorToInt(_y);
		int z = Mathf.FloorToInt(_z);

		if (!PositionHelper.IsHexInChunk(x, y, z))
        {
			return _world.CheckForHex(new Vector3(x, y, z)+position);
        }

		return _world.blocktypes[hexMap[x, y, z]].isSolid;
	}


	//Checks if the given block is water
	private bool CheckWaterHex(float _y, float _x, float _z, byte[,,] hexMap)
	{
		int x = Mathf.FloorToInt(_x);
		int y = Mathf.FloorToInt(_y);
		int z = Mathf.FloorToInt(_z);

		if (!PositionHelper.IsHexInChunk(x, y, z))
		{
			return _world.CheckForWaterHex(new Vector3(x, y, z) + position);
		}

		return _world.blocktypes[hexMap[x, y, z]].isWater;
	}


	//Checks the block and neighbouring blocks and if needed adds to the update queue
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
					if(!_world.chunksToUpdate.Contains(_world.GetChunkFromHexPosition(currentHex + position)))
							_world.chunksToUpdate.Enqueue(_world.GetChunkFromHexPosition(currentHex + position));
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


	//Does the update chunk mesh logic
	public IEnumerator UpdateChunk(byte[,,] hexMap,System.Action onComplete )
	{
		if (!chunkUpdatingFlag)
		{
			chunkUpdatingFlag = true;
			ClearMesh();
			Task t = Task.Factory.StartNew(delegate
			{
				for (int y = 0; y < HexData.ChunkHeight; y++)
				{
					for (int z = 0; z < HexData.ChunkWidth; z++)
					{
						for (int x = 0; x < HexData.ChunkWidth; x++)
						{
							if (_world.blocktypes[hexMap[x, y, z]].isSolid || _world.blocktypes[hexMap[x, y, z]].isTransparent)
							{
								AddHexCell(x, y, z, hexMap);
							}
							else if (!_world.blocktypes[hexMap[x, y, z]].isSolid && _world.blocktypes[hexMap[x, y, z]].isWater)
							{
								AddHexCell(x, y, z, hexMap);
							}
						}
					}
				}
				_chunkMeshRenderer.ConvertListsToArrays();
			});
			yield return new WaitUntil(() => {
				return t.IsCompleted;
			});
			if (t.Exception != null)
			{
				Debug.LogError(t.Exception);
			}
			CreateMesh();
			chunkUpdatingFlag = false;
			onComplete?.Invoke();
		}
	}


	//Adds or Updates block mesh data
	private void AddHexCell(int x, int y, int z, byte[,,] hexMap)
    {
		byte blockID = hexMap[x, y, z]; 
		BlockData block = GetBlockType(_world.blocktypes[blockID].blockDataType);

		bool isTransparent = _world.blocktypes[blockID].isTransparent;
		bool isWater = _world.blocktypes[blockID].isWater;
		for (int i = 0; i < block.facesCount; i++)
        {
			float faceX = x + HexData.faces[i].x;
			if (i >= 4 && i % 2 == z % 2) faceX = x;
			if (isWater && CheckWaterHex(y + HexData.faces[i].y, faceX, z + HexData.faces[i].z, hexMap)) continue; //if the neighbour IS water, dont render the face
			if (CheckHex(y + HexData.faces[i].y, faceX, z + HexData.faces[i].z, hexMap)&&!isTransparent)  //if the neighbour IS a block(and not transparent but its not implemented yet), dont render the face 
			{
				if (_world.blocktypes[blockID].isSolid && !_world.CheckForTransparentHex(new Vector3(faceX, y + HexData.faces[i].y, z + HexData.faces[i].z) + position))
				{
					continue;
				}
			}

			float lightLevel;
			bool inShade = false;
			if (inShade) lightLevel = 0.4f;
			else lightLevel = 0f;
			
			_chunkMeshRenderer.AddHex(new Vector3Int(x, y, z), block.hexVertices[i], block.hexTriangles[i], block.normals[i], isWater, isTransparent, lightLevel);
			_chunkMeshRenderer.AddUvs(blockID, block.hexUvs[i], i, isWater);
		}
	}


	//Gets Block Type
	public BlockData GetBlockType(BlockDataTypes blockDataType)
    {
		BlockData block;
        switch (blockDataType)
        {
			case BlockDataTypes.Full:
				block = new BlockData(FullBlockData.hexVertices, FullBlockData.hexUvs, FullBlockData.hexTriangles, HexData.normals, 8);
				break;
			case BlockDataTypes.Half:
				block = new BlockData(HalfBlockData.hexVertices, HalfBlockData.hexUvs, HalfBlockData.hexTriangles, HexData.normals, 8);
				break;
			case BlockDataTypes.Grass:
				block = new BlockData(GrassBlockData.hexVertices, GrassBlockData.hexUvs, GrassBlockData.hexTriangles,GrassBlockData.normals, 6);
				break;
			default:
				block = new BlockData();
				break;
		}

		return block;
    }


	//Creates or Updates the block mesh (Can only be called on the main thread)
	private void CreateMesh()
    {
		Mesh mesh = new Mesh();

		mesh.vertices = _chunkMeshRenderer.arrayVertices;
		mesh.uv = _chunkMeshRenderer.arrayUvs;
		mesh.normals = _chunkMeshRenderer.arrayNormals;
		mesh.colors = _chunkMeshRenderer.arrayColors;
		mesh.subMeshCount = 3;

		mesh.SetTriangles(_chunkMeshRenderer.arrayTriangles, 0);
		mesh.SetTriangles(_chunkMeshRenderer.arrayTransparentTriangles, 1);
		mesh.SetTriangles(_chunkMeshRenderer.arrayWaterTriangles, 2);

		meshFilter.mesh = mesh;
	}


	//Clears Mesh
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