using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class BiomeCenterFinder 
{

    public static Dictionary<Vector3Int, VoronoiSeed> CalculateBiomeCentersCopy(int pixelsPerCell, int chunkRange, ChunkCoord coord, World world,int biomeScale)
    {
        Dictionary<Vector3Int, VoronoiSeed> biomeCentersDict = new Dictionary<Vector3Int, VoronoiSeed>();
        int cellSize = pixelsPerCell * biomeScale;
        int seed = 45;

        System.Random randomGenerator = new System.Random(seed);

        for (int x = (coord.x/ biomeScale) -2; x < (coord.x/ biomeScale) + 3; x++)
        {
            for (int z = (coord.z / biomeScale) -2; z < (coord.z/ biomeScale) + 3; z++)
            {

                int randomNumberX = randomGenerator.Next(0, cellSize);
                int randomNumberZ = randomGenerator.Next(0, cellSize);

                if (biomeCentersDict.ContainsKey(new Vector3Int(x, 0, z))) continue;

                float land = Mathf.PerlinNoise((x + 100f) * 0.1f, (z + 100f) * 0.1f);
                if (land < 0.40f) biomeCentersDict.Add(new Vector3Int(x, 0, z), new VoronoiSeed(world.OceanBiome, new Vector3(x * cellSize + randomNumberX, 0, z * cellSize + randomNumberZ)));
                else
                {
                    float temperature = Mathf.PerlinNoise(x * 0.2f, z * 0.2f);
                    float humidity = Mathf.PerlinNoise((x + 160f) * 0.2f, (z + 160f) * 0.2f);
                    biomeCentersDict.Add(new Vector3Int(x, 0, z), new VoronoiSeed(world.SelectBiomes(temperature, humidity), new Vector3(x * cellSize + randomNumberX, 0, z * cellSize + randomNumberZ)));
                    if (x == 6 && z == 0) Debug.Log("center: " +temperature +"wow"+humidity+ world.SelectBiomes(temperature, humidity));
                }
                //if (x == 6 && z == 0) Debug.Log("center: " + biomeCentersDict[new Vector3Int(x, 0, z)].voronoiBiome);
            }
        }

        return biomeCentersDict;
    }
}
