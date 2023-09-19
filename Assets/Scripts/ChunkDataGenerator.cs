using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class ChunkDataGenerator
{
	public World _world;

	public ChunkDataGenerator(World world)
    {
		_world = world;
    }

	public IEnumerator GenerateData(Vector3 chunkPos, System.Action<byte[,,]> callback)
	{
		byte[,,] tempData = new byte[HexData.ChunkWidth, HexData.ChunkHeight, HexData.ChunkWidth];

		Task t = Task.Factory.StartNew(delegate
		{
			for (int y = 0; y < HexData.ChunkHeight; y++)
			{
				for (int z = 0; z < HexData.ChunkWidth; z++)
				{
					for (int x = 0; x < HexData.ChunkWidth; x++)
					{
						tempData[x, y, z] = _world.GetHex(new Vector3(x, y, z) + chunkPos);
					}
				}
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

		callback(tempData);
	}
}
