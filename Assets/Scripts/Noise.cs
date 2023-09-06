using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static float Get2DPerlin(Vector2 position, float offset, float scale)
    {
        return Mathf.PerlinNoise((position.x+0.1f)/HexData.ChunkWidth*scale+offset, (position.y + 0.1f) / HexData.ChunkWidth * scale + offset);
    }

    public static float EvaluateNoise(Vector2 position, float roughness, float strength, float centre,float baseRoughness,int numLayers,float persistance,float minValue,float offset)
    {
        float noiseValue = 0;
        float frequency= baseRoughness;
        float amplitude = 1;
        float maxValue=0;

        for(int i = 0; i < numLayers; i++)
        {
            float v = Mathf.PerlinNoise((position.x*0.01f) * frequency + centre+ offset, (position.y * 0.01f) * frequency + centre+ offset);
            noiseValue +=  (v + 1) * 0.5f*amplitude;

            maxValue += amplitude;

            frequency *= roughness;
            amplitude *= persistance;
        }
        noiseValue = Mathf.Max(0, noiseValue - minValue);
        return (noiseValue*strength)/maxValue;
    }

    public static float Map(float newmin, float newmax, float noiseMin, float noiseMax, float noiseValue)
    {
        return Mathf.Lerp(newmin, newmax, Mathf.InverseLerp(noiseMin, noiseMax, noiseValue));
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
