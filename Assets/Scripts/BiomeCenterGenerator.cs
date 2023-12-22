using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class BiomeCenterGenerator 
{
    private const float landScale = 0.1f;
    private const float landOffset = 100f;

    private const float temperatureScale = 0.2f;
    private const float temperatureOffset = 0f;

    private const float humidityScale = 0.2f;
    private const float humidityOffset = 160f;

    private const float landThreshold = 0.40f;


    //Generates Distorted Voronoi Seeds to the chunkRange around the player
    public static Dictionary<Vector3Int, VoronoiSeed> GenerateBiomeCenters(World world, int pixelsPerCell, int chunkRange, ChunkCoord coord, int biomeScale)
    {
        Dictionary<Vector3Int, VoronoiSeed> biomeCentersDict = new Dictionary<Vector3Int, VoronoiSeed>();

        int cellSize = pixelsPerCell * biomeScale;

        int halfRange = chunkRange / 2;
        for (int x = (coord.x / biomeScale) - halfRange - 1; x < (coord.x / biomeScale) + halfRange + 2; x++)
        {
            for (int z = (coord.z / biomeScale) - halfRange - 1; z < (coord.z / biomeScale) + halfRange + 2; z++)
            {
                //Non-Distorted Voronoi position of the biome
                Vector3Int biomeCenterHexPosition = new Vector3Int(x, 0, z);
                if (biomeCentersDict.ContainsKey(biomeCenterHexPosition)) continue;

                //Distorted Voronoi position of the biome
                int distortedX = Noise.Map01Int(0, 64, Noise.Get2DPerlin(new Vector2(x , z), 0f, 50f));
                int distortedZ = Noise.Map01Int(0, 64, Noise.Get2DPerlin(new Vector2(x , z), 0f, 50f));
                Vector3Int biomeCenterDistortedHexPosition = new Vector3Int(x * cellSize + distortedX, 0, z * cellSize + distortedZ);

                //Add OceanBiome if the value is below Threshold, else use BiomeTable to pick biome
                float land = Mathf.PerlinNoise((x + landOffset) * landScale, (z + landOffset) * landScale);
                if (land < landThreshold) biomeCentersDict.Add(biomeCenterHexPosition, new VoronoiSeed(world.OceanBiome, biomeCenterDistortedHexPosition));
                else
                {
                    float temperature = Mathf.PerlinNoise(x * temperatureScale, z * temperatureScale);
                    float humidity = Mathf.PerlinNoise((x + humidityOffset) * humidityScale, (z + humidityOffset) * humidityScale);
                    biomeCentersDict.Add(biomeCenterHexPosition, new VoronoiSeed(world.SelectBiomes(temperature, humidity), biomeCenterDistortedHexPosition));
                }
            }
        }

        return biomeCentersDict;
    }
}
