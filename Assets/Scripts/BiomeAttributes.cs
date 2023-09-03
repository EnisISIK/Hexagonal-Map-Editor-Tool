using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="BiomeAttributes",menuName ="HexMapTool/Biome Attribute")]
public class BiomeAttributes : ScriptableObject
{
    [Header("Biome")]
    public string biomeName;
    public int offset;
    public float scale;

    public int solidGroundHeight;
    public int terrainHeight;
    public float terrainScale;

    public byte surfaceBlock;
    public byte subSurfaceBlock;

    [Header("Major Flora")]
    public int floraIndex;
    public float floraZoneScale = 1.3f;
    [Range(0.1f, 1f)]
    public float floraZoneThreshold = 0.6f;
    public float floraPlacementScale = 15f;
    [Range(0.1f, 1f)]
    public float floraPlacementThreshold = 0.8f;
    public bool placeFlora = true;


    public int maxFloraHeight = 12;
    public int minFloraHeight = 5;

    public Lode[] lodes;
}

[System.Serializable]
public class Lode
{
    public string nodeName;
    public byte blockID;
    public int minHeight;
    public int maxHeight;
    public float scale;
    public float threshold;
    public float noiseOffset;
}
