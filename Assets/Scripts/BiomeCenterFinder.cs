using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BiomeCenterFinder 
{
    public static List<Vector2Int> neighbours8Directions = new List<Vector2Int>
    {
        new Vector2Int(0,1),
        new Vector2Int(1,1),
        new Vector2Int(1,0),
        new Vector2Int(1,-1),
        new Vector2Int(0,-1),
        new Vector2Int(-1,-1),
        new Vector2Int(-1,0),
        new Vector2Int(-1,1)
    };

    public static List<Vector3Int> CalculateBiomeCenters(Vector3 playerPosition,int drawRange, int mapSize)
    {
        int biomeLength = drawRange * mapSize;

        Vector3Int origin = new Vector3Int(Mathf.RoundToInt(playerPosition.x/biomeLength)*biomeLength,0, Mathf.RoundToInt(playerPosition.z / biomeLength) * biomeLength);

        HashSet<Vector3Int> biomeCentersTemp = new HashSet<Vector3Int>();

        biomeCentersTemp.Add(origin);

        foreach(Vector2Int offsetXY in neighbours8Directions)
        {
            Vector3Int newBiomePoint_1 = new Vector3Int(origin.x + offsetXY.x * biomeLength, 0, origin.z + offsetXY.y * biomeLength);
            Vector3Int newBiomePoint_2 = new Vector3Int(origin.x + offsetXY.x * biomeLength, 0, origin.z + offsetXY.y * 2 * biomeLength);
            Vector3Int newBiomePoint_3 = new Vector3Int(origin.x + offsetXY.x * 2 * biomeLength, 0, origin.z + offsetXY.y * biomeLength);
            Vector3Int newBiomePoint_4 = new Vector3Int(origin.x + offsetXY.x * 2 * biomeLength, 0, origin.z + offsetXY.y * 2 * biomeLength);
            biomeCentersTemp.Add(newBiomePoint_1);
            biomeCentersTemp.Add(newBiomePoint_2);
            biomeCentersTemp.Add(newBiomePoint_3);
            biomeCentersTemp.Add(newBiomePoint_4);
        }

        return new List<Vector3Int>(biomeCentersTemp);
    }

    public static List<Vector3> CalculateBiomeCentersBetter(int pixelsPerCell,int chunkRange,ChunkCoord coord)
    {
        HashSet<Vector3> biomeCentersTemp = new HashSet<Vector3>();

        for(int x = coord.x - chunkRange-1;x< coord.x + chunkRange+1;x++)
        {
            for (int z = coord.z - chunkRange-1; z < coord.z + chunkRange+1; z++)
            {
                biomeCentersTemp.Add(new Vector3(x * pixelsPerCell + UnityEngine.Random.Range(0, pixelsPerCell), 0, z * pixelsPerCell  + UnityEngine.Random.Range(0, pixelsPerCell)));
            }
        }
        //İşe Yaradı Gibi??
        return new List<Vector3>(biomeCentersTemp);
    }
}
