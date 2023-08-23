using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure
{
    public static void MakeTree(Vector3 position, Queue<HexMod> queue,int minTrunkHeight, int maxTrunkHeight)
    {
        int height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(position.x, position.z), 250f, 3f));

        if (height < minTrunkHeight)
            height = minTrunkHeight;

        for(int i = 1; i < height; i++)
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
        /*queue.Enqueue(new HexMod(new Vector3(position.x-1, position.y + height, position.z), 6));
        queue.Enqueue(new HexMod(new Vector3(position.x , position.y + height, position.z), 6));
        queue.Enqueue(new HexMod(new Vector3(position.x + 1, position.y + height, position.z), 6));

        queue.Enqueue(new HexMod(new Vector3(position.x , position.y + height, position.z-1), 6));
        queue.Enqueue(new HexMod(new Vector3(position.x , position.y + height, position.z+1), 6));

        queue.Enqueue(new HexMod(new Vector3(position.x - 1, position.y + height, position.z-1), 6));
        queue.Enqueue(new HexMod(new Vector3(position.x +1, position.y + height, position.z-1), 6));
        queue.Enqueue(new HexMod(new Vector3(position.x + 1, position.y + height, position.z+1), 6));
        queue.Enqueue(new HexMod(new Vector3(position.x - 1, position.y + height, position.z + 1), 6));
        */

    }
}
