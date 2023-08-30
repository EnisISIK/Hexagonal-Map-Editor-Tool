using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;

public static class Structure
{
    public static ConcurrentQueue<HexMod> MakeTree(Vector3 position, int minTrunkHeight, int maxTrunkHeight)
    {
        ConcurrentQueue<HexMod> queue = new ConcurrentQueue<HexMod>();

        int height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(position.x, position.z), 250f, 3f));

        if (height < minTrunkHeight)
            height = minTrunkHeight;

            for (int i = 1; i < height; i++)
            {
                queue.Enqueue(new HexMod(new Vector3(position.x, position.y + i, position.z), 3));
            }

            for (int x = -2; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int z = -2; z < 3; z++)
                    {
                        queue.Enqueue(new HexMod(new Vector3(position.x + x, position.y + height + y, position.z + z), 6));
                    }
                }
            }

        return queue;
    }

}
