using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DomainWarping : MonoBehaviour
{
    public NoiseSettings noiseDomainX, noiseDomainY;
    public int amplitudex = 20, amplitudey = 20;

    public float GenerateDomainNoise(Vector2 pos, NoiseSettings defaultNoiseSettings)
    {
        int x = Mathf.RoundToInt(pos.x);
        int z = Mathf.RoundToInt(pos.y);

        Vector2 domainOffset = GenerateDomainOffset(x, z);
        return Noise.OctavePerlin(new Vector2(x + domainOffset.x, z + domainOffset.y), defaultNoiseSettings);
    }

    public Vector2 GenerateDomainOffset(int x, int z)
    {
        var noiseX = Noise.OctavePerlin(new Vector2(x, z), noiseDomainX) * amplitudex;
        var noiseY = Noise.OctavePerlin(new Vector2(x, z), noiseDomainY) * amplitudey;

        return new Vector2(noiseX, noiseY);
    }

    public Vector2Int GenerateDomainOffsetInt(int x, int z)
    {
        return Vector2Int.RoundToInt(GenerateDomainOffset(x, z));
    }
}
