using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

    public static Dictionary<Vector3Int, Vector3> CalculateBiomeCentersDictionary(int pixelsPerCell, int chunkRange, ChunkCoord coord)
    {
        Dictionary<Vector3Int, Vector3> biomeCentersDict = new Dictionary<Vector3Int, Vector3>();
        int cellSize = pixelsPerCell;
        int seed = 45;

        System.Random randomGenerator = new System.Random(seed);

        for (int x = coord.x - chunkRange - 3; x < coord.x + chunkRange + 4; x++)
        {
            for (int z = coord.z - chunkRange - 3; z < coord.z + chunkRange + 4; z++)
            {

                int randomNumberX = randomGenerator.Next(0, cellSize);
                int randomNumberZ = randomGenerator.Next(0, cellSize);

                if (biomeCentersDict.ContainsKey(new Vector3Int(x, 0, z))) continue;
                
                biomeCentersDict.Add(new Vector3Int(x, 0, z), new Vector3(x * cellSize + randomNumberX, 0, z * cellSize + randomNumberZ));
            }
        }
        //İşe Yaradı Gibi??
        return biomeCentersDict;
    }

    public static Dictionary<Vector3Int, VoronoiSeed> CalculateBiomeCentersVoronoi(int pixelsPerCell, int chunkRange, ChunkCoord coord,World world)
    {
        Dictionary<Vector3Int, VoronoiSeed> biomeCentersDict = new Dictionary<Vector3Int, VoronoiSeed>();
        int cellSize = pixelsPerCell;
        int seed = 45;

        System.Random randomGenerator = new System.Random(seed);

        for (int x = coord.x - chunkRange - 3; x < coord.x + chunkRange + 4; x++)
        {
            for (int z = coord.z - chunkRange - 3; z < coord.z + chunkRange + 4; z++)
            {

                int randomNumberX = randomGenerator.Next(0, cellSize);
                int randomNumberZ = randomGenerator.Next(0, cellSize);

                if (biomeCentersDict.ContainsKey(new Vector3Int(x, 0, z))) continue;

                float land = Mathf.PerlinNoise((x + 100f) * 0.1f, (z + 100f) * 0.1f);
                if (land < 0.40f) biomeCentersDict.Add(new Vector3Int(x, 0, z),new VoronoiSeed(world.OceanBiome, new Vector3(x * cellSize + randomNumberX, 0, z * cellSize + randomNumberZ)));  //0.40f 0.30f falan ayarla işte
                else
                {
                    float temperature = Mathf.PerlinNoise(x * 0.2f, z * 0.2f);//Noise.OctavePerlin(new Vector2(x, z), biomeNoiseSettings);// simplexNoise.coherentNoise(x, 0, z);//+ z *0.05f);
                    float humidity = Mathf.PerlinNoise((x + 160f) * 0.2f, (z + 160f) * 0.2f);//Noise.OctavePerlin(new Vector2(x+160f, z + 160f), biomeNoiseSettings);//+ (z + 160f) *0.05f);// simplexNoise.coherentNoise(x+160f, 0, z+160f);//çöl yanında kar gibi ufak bir sıkıntı yaşandı
                    biomeCentersDict.Add(new Vector3Int(x, 0, z), new VoronoiSeed(world.SelectBiomes(temperature, humidity), new Vector3(x * cellSize + randomNumberX, 0, z * cellSize + randomNumberZ)));
                    //voronoiBiomeAttributesDict.Add(seedCoord, biomeAttributesData[randomGenerator.Next(0,biomeAttributesData.Count)].Biome);
                }

            }
        }

        return biomeCentersDict;
    }
}
