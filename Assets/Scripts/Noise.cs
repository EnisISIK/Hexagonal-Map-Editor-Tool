using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static float [,] GenerateNoiseMap(int mapWidth, int mapHeight, float offset,float scale, float octaves, float persistance, float lacunarity)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        if(scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        for (int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x + 0.1f) / scale * frequency + offset;
                    float sampleY = (y + 0.1f) / scale * frequency + offset;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if(noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if(noiseHeight< minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight;
            }
        }

        for(int y =0; y<mapHeight;y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }
    public static float Get2DPerlin(Vector2 position, float offset, float scale)
    {
        return Mathf.PerlinNoise((position.x+0.1f)/HexData.ChunkWidth*scale+offset, (position.y + 0.1f) / HexData.ChunkWidth * scale + offset);
    }
    public static float Generate2DPerlin(Vector2 position,float offset, float scale, float octaves, float persistance, float lacunarity)
    {
        float perlinValue;
        float amplitude = 1;
        float frequency = 1;
        float noiseValue = 0;

        for(int i = 0; i < octaves; i++)
        {
            float sampleX = (position.x+0.1f) / HexData.ChunkWidth * scale * frequency + offset;
            float sampleY = (position.y+0.1f) / HexData.ChunkWidth * scale * frequency + offset;

            perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
            noiseValue += perlinValue * amplitude;

            amplitude *= persistance;
            frequency *= lacunarity;
        }

        return noiseValue;
    }

    public static bool Get3DPerlin(Vector3 position, float offset,float scale, float threshold)
    {
        float x = (position.x + offset + 0.1f) * scale;
        float y = (position.y + offset + 0.1f) * scale;
        float z = (position.z + offset + 0.1f) * scale;

        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);

        float BA = Mathf.PerlinNoise(y, x);
        float CB  = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);

        return (AB + BC + AC + BA + CB + CA) / 6f > threshold;

    }
}
