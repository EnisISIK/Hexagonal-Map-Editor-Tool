using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;

public static class Structure
{
    public static ConcurrentQueue<HexMod> GenerateMajorFlora(int index, Vector3 position, int minTrunkHeight, int maxTrunkHeight)
    {
        switch (index)
        {
            case 0:
                return MakeTree(position, minTrunkHeight, maxTrunkHeight);
            case 1:
                return MakeCactus(position, minTrunkHeight, maxTrunkHeight);
            case 2:
                return MakeSpruceTree(position, minTrunkHeight, maxTrunkHeight);
            case 3:
                return MakeIceSpike(position, minTrunkHeight, maxTrunkHeight);
            case 4:
                return MakeSpruceFoliage(position);
        }

        return new ConcurrentQueue<HexMod>();
    }

    public static ConcurrentQueue<HexMod> MakeTree(Vector3 position, int minTrunkHeight, int maxTrunkHeight)
    {
        ConcurrentQueue<HexMod> queue = new ConcurrentQueue<HexMod>();

        int height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(position.x, position.z), 4005f, 2f));

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
    public static ConcurrentQueue<HexMod> MakeCactus(Vector3 position, int minTrunkHeight, int maxTrunkHeight)
    {
        ConcurrentQueue<HexMod> queue = new ConcurrentQueue<HexMod>();

        int height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(position.x, position.z), 250f, 3f));

        if (height < minTrunkHeight)
            height = minTrunkHeight;

        for (int i = 1; i <= height; i++)
        {
            queue.Enqueue(new HexMod(new Vector3(position.x, position.y + i, position.z), 6));
        }

        return queue;
    }

    public static ConcurrentQueue<HexMod> MakeSpruceTree(Vector3 position, int minTrunkHeight, int maxTrunkHeight)
    {
        ConcurrentQueue<HexMod> queue = new ConcurrentQueue<HexMod>();

        //queue = MakeSpruceFoliage(position);

        int height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(position.x, position.z), 4005f, 2f));

        if (height < minTrunkHeight)
            height = minTrunkHeight;

        for (int a = 3; a < height; a += 3)
        {
            for (int i = 0; i < 8; i++)
            {
                float faceX = position.x + HexData.faces[i].x;
                if (i >= 4 && i % 2 == 0 && Mathf.FloorToInt(position.z) % 2 == 0) faceX = position.x;
                if (i >= 4 && i % 2 == 1 && Mathf.FloorToInt(position.z) % 2 == 1) faceX = position.x;

                queue.Enqueue(new HexMod(new Vector3(faceX, position.y + a, position.z + HexData.faces[i].z), 14));
            }
        }

        for (int i = 1; i < height; i++)
        {
            queue.Enqueue(new HexMod(new Vector3(position.x, position.y + i, position.z), 13));
        }

        queue.Enqueue(new HexMod(new Vector3(position.x, position.y + height, position.z), 14));

        return queue;
        //ConcurrentQueue<HexMod> queue = new ConcurrentQueue<HexMod>();

        //int height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(position.x, position.z), 4005f, 2f));

        //if (height < minTrunkHeight)
        //    height = minTrunkHeight;

        //for (int i = 1; i < height; i++)
        //{
        //    queue.Enqueue(new HexMod(new Vector3(position.x, position.y + i, position.z), 13));
        //}

        //for (int x = -2; x < 3; x++)
        //{
        //    for (int y = 0; y < 3; y++)
        //    {
        //        for (int z = -2; z < 3; z++)
        //        {
        //            queue.Enqueue(new HexMod(new Vector3(position.x + x, position.y + height + y, position.z + z), 14));
        //        }
        //    }
        //}

        //return queue;
    }
    public static ConcurrentQueue<HexMod> MakeSpruceFoliage(Vector3 position)
    {
        ConcurrentQueue<HexMod> queue = new ConcurrentQueue<HexMod>();

        queue.Enqueue(new HexMod(new Vector3(position.x, position.y + 1, position.z), 18));

        return queue;
    }

    public static ConcurrentQueue<HexMod> MakeIceSpike(Vector3 position, int minTrunkHeight, int maxTrunkHeight)
    {
        ConcurrentQueue<HexMod> queue = new ConcurrentQueue<HexMod>();

        int height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(position.x, position.z), 4005f, 2f));

        if (height < minTrunkHeight)
            height = minTrunkHeight;

        for (int a = 1; a < height/2; a++)
        {
            for (int i = 0; i < 8; i++)
            {
                float faceX = position.x + HexData.faces[i].x;
                if (i >= 4 && i % 2 == 0 && Mathf.FloorToInt(position.z) % 2 == 0) faceX = position.x;
                if (i >= 4 && i % 2 == 1 && Mathf.FloorToInt(position.z) % 2 == 1) faceX = position.x;

                queue.Enqueue(new HexMod(new Vector3(faceX, position.y + a, position.z + HexData.faces[i].z), 10));
            }
        }

        for (int i = height/2; i < height; i++)
        {
            queue.Enqueue(new HexMod(new Vector3(position.x, position.y + i, position.z), 10));
        }

        return queue;
    }
}
