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
            noiseValue += (v + 1) * 0.5f*amplitude;

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

    public static float Map01(float newmin, float newmax, float noiseValue)
    {
        return Mathf.Lerp(newmin, newmax, Mathf.InverseLerp(0, 1, noiseValue));
    }

    public static int Map01Int(float newmin, float newmax, float noiseValue)
    {
        return (int) Map01(newmin,newmax,noiseValue);
    }
    
    public static float Redistribution(float noise,NoiseSettings settings)
    {
        return Mathf.Pow(noise * settings.redistrubitionModifier, settings.exponent); 
    }
    public static float OctavePerlin(Vector2 position,NoiseSettings settings)
    {
        float x = position.x;
        float z = position.y;
        x *= settings.noiseZoom;
        z *= settings.noiseZoom;
        x += settings.noiseZoom;
        z += settings.noiseZoom;
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float amplitudeSum = 0;
        for(int i = 0; i < settings.numLayers; i++)
        {
            total += Mathf.PerlinNoise((settings.offset.x + x) * frequency, (settings.offset.y + z) * frequency) * amplitude;

            amplitudeSum += amplitude;

            amplitude *= settings.persistence;
            frequency *= 2;
        }

        return total / amplitudeSum;
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
