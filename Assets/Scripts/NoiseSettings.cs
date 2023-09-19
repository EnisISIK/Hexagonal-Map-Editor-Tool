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
    public Vector2 offset=new Vector2(0,0);

    public float noiseZoom=1;
    public float redistrubitionModifier=0;
    public float exponent=0;

}
