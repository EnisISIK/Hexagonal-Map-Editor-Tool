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