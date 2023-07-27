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
		float y = textureId / VoxelData.TextureAtlasSizeInBlocks; 
		float x = textureId - (y * VoxelData.TextureAtlasSizeInBlocks);  

		x *= VoxelData.NormalizedBlockTextureSize; 
		y *= VoxelData.NormalizedBlockTextureSize; 

		y = 1f - y - VoxelData.NormalizedBlockTextureSize;  

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

						Vector3 position;
						position.x = (((chunkCountX + x) + (chunkCountZ  + z) * 0.5f - (chunkCountZ  + z) / 2) * (VoxelData.innerRadius * 2f))+chunkCountX*2f* VoxelData.innerRadius;
						position.y = -1f * y;
						position.z = ((chunkCountZ  + z) * (VoxelData.outerRadius * 1.5f)) + chunkCountZ * VoxelData.outerRadius;


						int triangleOffset = vertices.Count;
						vertices.AddRange(VoxelData.hexVertices.Select(v => v + position));
						triangles.AddRange(VoxelData.hexTriangles.Select(t => t + triangleOffset));

						AddText(blockID, VoxelData.hexUvs);
					}
				}
			}
		}
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

					int triangleOffset = vertices.Count;
					RenderUp(new Vector3(x, y, z),blockID,triangleOffset);
					RenderDown(new Vector3(x, y, z), blockID,triangleOffset);
					RenderEast(new Vector3(x, y, z), blockID,triangleOffset);
					RenderWest(new Vector3(x, y, z), blockID,triangleOffset);
					RenderSouthEast(new Vector3(x, y, z), blockID,triangleOffset);
					RenderSouthWest(new Vector3(x, y, z), blockID,triangleOffset);
					RenderNorthEast(new Vector3(x, y, z), blockID,triangleOffset);
					RenderNorthWest(new Vector3(x, y, z), blockID,triangleOffset);
				}
			}
		}
	}

	void AddHexagon(Vector3 pos,int blockID,Vector3[] hexVert , int[] hexTri, Vector2[] hexUV,int triangleOffset)
    {
		Vector3 position;
		position.x = (((chunkCountX + pos.x) + (chunkCountZ + pos.z) * 0.5f - (chunkCountZ + pos.z) / 2) * (VoxelData.innerRadius * 2f)) + chunkCountX * 2f * VoxelData.innerRadius;
		position.y = -1f * pos.y;
		position.z = ((chunkCountZ + pos.z) * (VoxelData.outerRadius * 1.5f)) + chunkCountZ * VoxelData.outerRadius;

		vertices.AddRange(hexVert.Select(v => v + position));
		triangles.AddRange(hexTri.Select(t => t + triangleOffset));
		//uvs.AddRange(VoxelData.topUvs.Select(u => u));
		for(int i = 0; i < hexUV.Length; i++)
        {
			int textureId = world.blocktypes[blockID].GetTextureID(0);
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
	
	void AddQuad(Vector3 pos, int blockID, Vector3[] hexVert, int[] hexTri, Vector2[] hexUV)
    {
		Vector3 position;
		position.x = (((chunkCountX + pos.x) + (chunkCountZ + pos.z) * 0.5f - (chunkCountZ + pos.z) / 2) * (VoxelData.innerRadius * 2f)) + chunkCountX * 2f * VoxelData.innerRadius;
		position.y = -1f * pos.y;
		position.z = ((chunkCountZ + pos.z) * (VoxelData.outerRadius * 1.5f)) + chunkCountZ * VoxelData.outerRadius;

		int triangleOffset = vertices.Count;
		vertices.AddRange(hexVert.Select(v => v + position));
		triangles.AddRange(hexTri.Select(t => t + triangleOffset));
		//uvs.AddRange(VoxelData.topUvs.Select(u => u));
		for (int i = 0; i < hexUV.Length; i++)
		{
			int textureId = world.blocktypes[blockID].GetTextureID(0);
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

	private void RenderUp(Vector3 neighboor,byte blockID,int triangleOffset)
    {
        if (CheckVoxel(neighboor.y, neighboor.x, neighboor.z))//if works add facechecks here
        {
			return;
        }

		AddHexagon(neighboor,blockID,VoxelData.topVertices,VoxelData.topTriangles,VoxelData.topUvs, triangleOffset);
    }
	private void RenderDown(Vector3 neighboor, byte blockID,int triangleOffset)
	{
		if (CheckVoxel(neighboor.y, neighboor.x, neighboor.z))//if works add facechecks here
		{
			return;
		}

		AddHexagon(neighboor, blockID, VoxelData.bottomVertices, VoxelData.bottomTriangles, VoxelData.bottomUvs,triangleOffset);
	}
	private void RenderEast(Vector3 neighboor, byte blockID,int triangleOffset)
	{
		if (CheckVoxel(neighboor.y, neighboor.x, neighboor.z))//if works add facechecks here
		{
			return;
		}

		AddHexagon(neighboor, blockID, VoxelData.rightVertices, VoxelData.rightTriangles, VoxelData.rightUvs,triangleOffset);
	}
	private void RenderWest(Vector3 neighboor, byte blockID,int triangleOffset)
	{
		if (CheckVoxel(neighboor.y, neighboor.x, neighboor.z))//if works add facechecks here
		{
			return;
		}

		AddHexagon(neighboor, blockID, VoxelData.leftVertices, VoxelData.leftTriangles, VoxelData.leftUvs,triangleOffset);
	}
	private void RenderSouthEast(Vector3 neighboor, byte blockID,int triangleOffset)
	{
		if (CheckVoxel(neighboor.y, neighboor.x, neighboor.z))//if works add facechecks here
		{
			return;
		}

		AddHexagon(neighboor, blockID, VoxelData.frontRightVertices, VoxelData.frontRightTriangles, VoxelData.frontRightUvs,triangleOffset);
	}
	private void RenderSouthWest(Vector3 neighboor, byte blockID,int triangleOffset)
	{
		if (CheckVoxel(neighboor.y, neighboor.x, neighboor.z))//if works add facechecks here
		{
			return;
		}

		AddHexagon(neighboor, blockID, VoxelData.frontLeftVertices, VoxelData.frontLeftTriangles, VoxelData.frontLeftUvs, triangleOffset);
	}
	private void RenderNorthEast(Vector3 neighboor, byte blockID,int triangleOffset)
	{
		if (CheckVoxel(neighboor.y, neighboor.x, neighboor.z))//if works add facechecks here
		{
			return;
		}

		AddHexagon(neighboor, blockID, VoxelData.backRightVertices, VoxelData.backRightTriangles, VoxelData.backRightUvs, triangleOffset);
	}
	private void RenderNorthWest(Vector3 neighboor, byte blockID,int triangleOffset)
	{
		if (CheckVoxel(neighboor.y, neighboor.x, neighboor.z))//if works add facechecks here
		{
			return;
		}

		AddHexagon(neighboor, blockID, VoxelData.backLeftVertices, VoxelData.backLeftTriangles, VoxelData.backLeftUvs,triangleOffset);
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
	void AddTextFaces(int blockID, Vector2[] hexUVs,int faceID)
	{

		for (int i = 0; i < hexUVs.Length; i++)
		{
			int textureId = world.blocktypes[blockID].GetTextureID(faceID);
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