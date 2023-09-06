using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseSettings 
{
    public bool enabled;
    public bool useFirstLayerAsMask;
    public float strength = 1;
    public int numLayers = 1;
    public float baseRoughness = 1;
    public float roughness = 1;
    public float persistence = 0.5f;
    public float minValue;
    public float centre = 0;
}
