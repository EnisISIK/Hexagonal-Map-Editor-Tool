using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Chunk 
{
	private readonly World world;

	public Queue<HexMod> modifications = new Queue<HexMod>();

	[SerializeField] private Material material;

	public byte[,,] hexMap = new byte[HexData.ChunkWidth, HexData.ChunkHeight, HexData.ChunkWidth];

	public ChunkCoord chunkCoordinates;

	private GameObject chunkObject;
	private readonly int chunkCountX;
	private readonly int chunkCountZ;

	private MeshRenderer meshRenderer;
	private MeshFilter meshFilter;

	private int triangleOffsetValue = 0;
	private readonly List<Vector3> vertices = new List<Vector3>();
	private readonly List<int> triangles = new List<int>();
	private readonly List<Vector2> uvs = new List<Vector2>();
	private readonly List<Vector3> normals = new List<Vector3>();
	
	private bool _isActive;
	public bool isHexMapPopulated = false;

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

	public Vector3 position
	{
		get { return chunkObject.transform.position; }
	}

	public Chunk(ChunkCoord _chunkCoordinates ,World _world,bool generateOnLoad)
    {
		chunkCoordinates = _chunkCoordinates;
		world = _world;
		isActive = true;

		chunkCountX = chunkCoordinates.x;
		chunkCountZ = chunkCoordinates.z;

		if (generateOnLoad)
			Init();
	}

	public void Init()
    {
		chunkObject = new GameObject();
		meshFilter = chunkObject.AddComponent<MeshFilter>();
		meshRenderer = chunkObject.AddComponent<MeshRenderer>();

		meshRenderer.material = world.material;

		chunkObject.transform.position = new Vector3(chunkCoordinates.x * HexData.ChunkWidth, 0f, chunkCoordinates.z * HexData.ChunkWidth);
		chunkObject.transform.SetParent(world.transform);

		chunkObject.name = "Chunk " + chunkCoordinates.x + ", " + chunkCoordinates.z;
		PopulateHexMap();
		UpdateChunk();
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

		isHexMapPopulated = true;
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
			return world.CheckForHex(new Vector3(x, y, z)+position);
        }

		return world.blocktypes[hexMap[x, y, z]].isSolid;
	}

	public void EditHex(Vector3 pos, byte newID)
	{	
		int xCheck = Mathf.FloorToInt(pos.x);
		int yCheck = Mathf.FloorToInt(pos.y);
		int zCheck = Mathf.FloorToInt(pos.z);

		xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
		zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

		hexMap[xCheck, yCheck, zCheck] = newID;

		UpdateSurroundingHex(xCheck, yCheck, zCheck);
		UpdateChunk();

	}
	public void UpdateSurroundingHex(int x, int y, int z)
	{
		Vector3 thisHex = new Vector3(x, y, z);

		for (int p = 0; p < 8; p++){
			Vector3 currentHex = thisHex + HexData.faces[p];

			if (!IsHexInChunk((int)currentHex.x, (int)currentHex.y, (int)currentHex.z))
				world.GetChunkFromChunkVector3(currentHex + position).UpdateChunk();
		}
	}

	public byte GetHexFromGlobalVector3(Vector3 pos)
	{
		int xCheck = Mathf.FloorToInt(pos.x);
		int yCheck = Mathf.FloorToInt(pos.y);
		int zCheck = Mathf.FloorToInt(pos.z);

		xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
		zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);
		try
		{
			return hexMap[xCheck, yCheck, zCheck];
		}
        catch
        {
			return world.GetHex(pos);
        }

	}

	public void UpdateChunk()
	{
		while (modifications.Count > 0)
		{
			HexMod structureHex = modifications.Dequeue();
			Vector3 structureHexPos = structureHex.position -= position;
			hexMap[(int)structureHexPos.x, (int)structureHexPos.y, (int)structureHexPos.z] = structureHex.id;

		}
		ClearMesh();
		for (float y = 0; y < HexData.ChunkHeight; y++){
			for (int z = 0; z < HexData.ChunkWidth; z++){
				for (int x = 0; x < HexData.ChunkWidth; x++){
					if (world.blocktypes[hexMap[x,(int) y, z]].isSolid)
						AddHexCell(x, y, z);
				}
			}
		}
		CreateMesh();
	}

	private void AddHexCell(int x, float y, int z)
    {
		byte blockID = hexMap[x, (int)y, z];

		triangleOffsetValue = vertices.Count;
		RenderUp(new Vector3(x, y, z), blockID);
		RenderDown(new Vector3(x, y, z), blockID);
		RenderEast(new Vector3(x, y, z), blockID);
		RenderWest(new Vector3(x, y, z), blockID);
		RenderSouthEast(new Vector3(x, y, z), blockID);
		RenderSouthWest(new Vector3(x, y, z), blockID);
		RenderNorthEast(new Vector3(x, y, z), blockID);
		RenderNorthWest(new Vector3(x, y, z), blockID);
	}

	private void AddHex(Vector3 pos,Vector3[] hexVert , int[] hexTri,int triangleOffset)
    {
		Vector3 _position = HexPrism.HexToPixel(pos);
		_position.x += (chunkCountX * (HexData.ChunkWidth * 2 ) * HexData.innerRadius - position.x);
		_position.z += (chunkCountZ * (HexData.ChunkWidth * 1.5f) * HexData.outerRadius-position.z);
		
		vertices.AddRange(hexVert.Select(v => v + _position));
		triangles.AddRange(hexTri.Select(t => t + triangleOffset));

	}

	private void AddUvs(int blockID, Vector2[] hexUV, int textureIDNum)
    {
		for (int i = 0; i < hexUV.Length; i++)
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

	private void RenderUp(Vector3 neighboor,byte blockID)
    {
        if (CheckHexagon(neighboor.y+HexData.fu.y, neighboor.x + HexData.fu.x, neighboor.z + HexData.fu.z))
		{
			triangleOffsetValue -= 6;
			return;
        }

		AddHex(neighboor,HexData.topVertices,HexData.topTriangles, triangleOffsetValue);
		AddUvs(blockID, HexData.topUvs, 0);
    }

	private void RenderDown(Vector3 neighboor, byte blockID)
	{
		if (CheckHexagon(neighboor.y + HexData.fd.y, neighboor.x + HexData.fd.x, neighboor.z + HexData.fd.z))
		{
			triangleOffsetValue -= 6;
			return;
		}

		AddHex(neighboor, HexData.bottomVertices, HexData.bottomTriangles,  triangleOffsetValue);
		AddUvs(blockID, HexData.bottomUvs, 1);
	}

	private void RenderEast(Vector3 neighboor, byte blockID)
	{
		if (CheckHexagon(neighboor.y + HexData.fe.y, neighboor.x + HexData.fe.x, neighboor.z + HexData.fe.z))
		{
			triangleOffsetValue -= 4;
			return;
		}

		AddHex(neighboor, HexData.rightVertices, HexData.rightTriangles, triangleOffsetValue);
		AddUvs(blockID, HexData.rightUvs, 2);
	}

	private void RenderWest(Vector3 neighboor, byte blockID)
	{
		if (CheckHexagon(neighboor.y + HexData.fw.y, neighboor.x + HexData.fw.x, neighboor.z + HexData.fw.z))
		{
			triangleOffsetValue -= 4;
			return;
		}

		AddHex(neighboor, HexData.leftVertices, HexData.leftTriangles, triangleOffsetValue);
		AddUvs(blockID, HexData.leftUvs, 3);
	}

	private void RenderSouthEast(Vector3 neighboor, byte blockID)
	{
		float newNeighboorX = neighboor.x;
		if (neighboor.z % 2 == 1 ) newNeighboorX = neighboor.x + HexData.fse.x;

		if (CheckHexagon(neighboor.y + HexData.fse.y, newNeighboorX, neighboor.z + HexData.fse.z))
		{
			triangleOffsetValue -= 4;
			return;
		}

		AddHex(neighboor, HexData.frontRightVertices, HexData.frontRightTriangles, triangleOffsetValue);
		AddUvs(blockID, HexData.frontRightUvs, 4);
	}

	private void RenderSouthWest(Vector3 neighboor, byte blockID)
	{
		float newNeighboorX = neighboor.x;
		if (neighboor.z % 2 == 0) newNeighboorX = neighboor.x + HexData.fsw.x;

		if (CheckHexagon(neighboor.y + HexData.fsw.y, newNeighboorX, neighboor.z + HexData.fsw.z))
		{
			triangleOffsetValue -= 4;
			return;
		}

		AddHex(neighboor, HexData.frontLeftVertices, HexData.frontLeftTriangles, triangleOffsetValue);
		AddUvs(blockID, HexData.frontLeftUvs, 5);
	}

	private void RenderNorthEast(Vector3 neighboor, byte blockID)
	{
		float newNeighboorX = neighboor.x;
		if (neighboor.z % 2 == 1) newNeighboorX = neighboor.x + HexData.fne.x;

		if (CheckHexagon(neighboor.y + HexData.fne.y, newNeighboorX, neighboor.z + HexData.fne.z))
		{
			triangleOffsetValue -= 4;
			return;
		}

		AddHex(neighboor, HexData.backRightVertices, HexData.backRightTriangles, triangleOffsetValue);
		AddUvs(blockID, HexData.backRightUvs, 6);
	}

	private void RenderNorthWest(Vector3 neighboor, byte blockID)
	{
		float newNeighboorX = neighboor.x;
		if (neighboor.z % 2 == 0) newNeighboorX = neighboor.x + HexData.fnw.x;

		if (CheckHexagon(neighboor.y + HexData.fnw.y, newNeighboorX, neighboor.z + HexData.fnw.z))
		{
			triangleOffsetValue -= 4;
			return;
		}

		AddHex(neighboor, HexData.backLeftVertices, HexData.backLeftTriangles, triangleOffsetValue);
		AddUvs(blockID, HexData.backLeftUvs, 7);
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

	void ClearMesh()
	{
		triangleOffsetValue = 0;
		vertices.Clear();
		uvs.Clear();
		triangles.Clear();

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