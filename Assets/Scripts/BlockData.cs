using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Blocks", menuName = "Blocks/BlockData")]
public class BlockData : ScriptableObject
{
    [Header("Vertices")]
    public Vector3[] vertices;

    public Vector3[] topVertices;

}
