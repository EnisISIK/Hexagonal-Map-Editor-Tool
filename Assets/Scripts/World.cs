﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class World : MonoBehaviour
{
    public int seed;
    public BiomeAttributes[] biomes;

    public Transform player;
    public Vector3 spawnPosition;

    public Material material;
    public BlockType[] blocktypes;

    public static Dictionary<Vector3Int, Chunk> chunksDictionary;

    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    public static Dictionary<Vector2Int, ChunkCoord> activeChunksDictionary;

    public ConcurrentQueue<ConcurrentQueue<HexMod>> modifications = new ConcurrentQueue<ConcurrentQueue<HexMod>>();

    public ChunkCoord playerCurrentChunkCoord;
    ChunkCoord playerLastChunkCoord;
    List<ChunkCoord> chunksToCreate= new List<ChunkCoord>();

    public ConcurrentQueue<Chunk> chunksToDraw = new ConcurrentQueue<Chunk>();
    public ConcurrentQueue<Chunk> chunksToUpdate = new ConcurrentQueue<Chunk>();

    public GameObject debugScreen;

    private bool isCreatingChunks;
    private bool isDrawingChunks;
    public bool isUpdatingChunks;

    // TODO: may implement a system for chunk that holds real calculated position and position relative to other chunks separate(done, just need a bit cleaning)
    // TODO: make blocktypes enum or a scriptable object
    // TODO: add creation stack
    // TODO: work on random generated seeds
    // FIX: some of the chunks do not reload after getting into view distance when you go beyond that chunk disappears and never reloads
    // FIX: perlin noise is same for values below zero
    // TODO: clean code and turn repeating codes to functions


    private void Start()
    {

        chunksDictionary = new Dictionary<Vector3Int, Chunk>();
        activeChunksDictionary = new Dictionary<Vector2Int, ChunkCoord>();

        Random.InitState(seed);
        int centerChunk = (HexData.WorldSizeInChunks * HexData.ChunkWidth) / 2;
        spawnPosition.x = (centerChunk + centerChunk * 0.5f - centerChunk / 2) * (HexData.innerRadius * 2f);
        spawnPosition.y = HexData.ChunkHeight -50f;
        spawnPosition.z = centerChunk * (HexData.outerRadius * 1.5f);

        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVector3(HexPrism.PixelToHex(player.position));
    }
    private void Update()
    {
        playerCurrentChunkCoord = GetChunkCoordFromVector3(HexPrism.PixelToHex(player.position));
        if (!playerCurrentChunkCoord.Equals(playerLastChunkCoord))
        { 
            CheckViewDistance();
            playerLastChunkCoord = playerCurrentChunkCoord; 
        }
        if (chunksToCreate.Count > 0 && !isCreatingChunks)
        {
            StartCoroutine(CreateChunks());
        }
        if (chunksToDraw.Count > 0 && !isDrawingChunks)
        {
            //StartCoroutine(DrawChunks());
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            debugScreen.SetActive(!debugScreen.activeSelf);
        }
    }
    private void FixedUpdate()
    {
        if (chunksToUpdate.Count > 0 && !isUpdatingChunks)
        {
            StartCoroutine(UpdateChunks());
        }
    }

    IEnumerator DrawChunks()
    {
        isDrawingChunks = true;
        while (chunksToDraw.Count > 0) { 
            if (chunksToDraw.TryPeek(out Chunk var))
                if (var.isHexMapPopulated)
                    if (chunksToDraw.TryDequeue(out Chunk var1))
                        var1.CreateMesh();
            yield return null;
        }
        isDrawingChunks = false;
    }
    IEnumerator UpdateChunks()
    {
        isUpdatingChunks = true;
        while (chunksToUpdate.Count > 0)
        {
            if (chunksToUpdate.TryPeek(out Chunk var))
                if (var.isHexMapPopulated)
                    if (chunksToUpdate.TryDequeue(out Chunk var1)) { 
                        StartCoroutine(var1.UpdateChunk());
                        yield return null;  //bu yield iki yerdede çalışıyor.  eskiden yield return var1.UpdateChunk(); yapıyordun o daha yavaştı
                        //var1.CreateMesh();  
                    }
            //yield return null;
        }
        isUpdatingChunks = false;
    }
    public IEnumerator ApplyModifications()
    {
        while (modifications.Count > 0)
        {

            ConcurrentQueue<HexMod> queue;
            modifications.TryDequeue(out queue);

            while (queue.Count > 0)
            {

                HexMod v;
                queue.TryDequeue(out v);

                ChunkCoord c = GetChunkCoordFromVector3(v.position);

                if (chunksDictionary.ContainsKey(new Vector3Int(c.x,0,c.z)))
                {
                    chunksDictionary[new Vector3Int(c.x, 0, c.z)].modifications.Enqueue(v);
                }



            }
            yield return null;
        }
    }

    void GenerateWorld()
    {
        for(int x = (HexData.WorldSizeInChunks / 2)-HexData.ViewDistanceinChunks; x < (HexData.WorldSizeInChunks / 2) + HexData.ViewDistanceinChunks; x++)
        {
            for (int z = (HexData.WorldSizeInChunks / 2) - HexData.ViewDistanceinChunks; z < (HexData.WorldSizeInChunks / 2) + HexData.ViewDistanceinChunks; z++)
            {
                chunksDictionary.Add(new Vector3Int(x, 0, z), new Chunk(new ChunkCoord(x, z), this, true));
                activeChunks.Add(new ChunkCoord(x, z));
            }
        }
       
        player.position = spawnPosition;
    }
    IEnumerator CreateChunks()
    {
        isCreatingChunks = true;

        while (chunksToCreate.Count > 0)
        {
            chunksDictionary[new Vector3Int(chunksToCreate[0].x, 0, chunksToCreate[0].z)].Init();
            chunksToCreate.RemoveAt(0);
            yield return null;
        }
        isCreatingChunks = false;
    }

    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {

        int x = Mathf.FloorToInt(pos.x / HexData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / HexData.ChunkWidth);
        return new ChunkCoord(x, z);

    }

    public Chunk GetChunkFromChunkVector3(Vector3 pos)
    {

        int x = Mathf.FloorToInt(pos.x / HexData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / HexData.ChunkWidth);

        return chunksDictionary[new Vector3Int(x, 0, z)];

    }

    void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkCoordFromVector3(HexPrism.PixelToHex(player.position)); 

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        activeChunks.Clear();

        for (int x = coord.x - HexData.ViewDistanceinChunks; x < coord.x + HexData.ViewDistanceinChunks; x++)
        {
            for (int z = coord.z - HexData.ViewDistanceinChunks; z < coord.z + HexData.ViewDistanceinChunks; z++)
            {
                if (!chunksDictionary.ContainsKey(new Vector3Int(x, 0, z))){
                    chunksDictionary.Add(new Vector3Int(x, 0, z), new Chunk(new ChunkCoord(x, z), this, false));
                    chunksToCreate.Add(new ChunkCoord(x, z));
                }
                else if (!chunksDictionary[new Vector3Int(x, 0, z)].isActive){
                    chunksDictionary[new Vector3Int(x, 0, z)].isActive = true;
                }
                activeChunks.Add(new ChunkCoord(x, z));

                for(int i= 0; i < previouslyActiveChunks.Count; i++){
                    if (previouslyActiveChunks[i].Equals(new ChunkCoord(x,z)))
                        previouslyActiveChunks.RemoveAt(i);
                }
            }
        }
        foreach(ChunkCoord _chunk in previouslyActiveChunks)
        {
            chunksDictionary[new Vector3Int(_chunk.x, 0, _chunk.z)].isActive = false;
            activeChunks.Remove(new ChunkCoord(_chunk.x, _chunk.z));
        }
    }
    public bool CheckForHex(Vector3 pos)
    {
        ChunkCoord thisChunk = GetChunkCoordFromVector3(pos);

        if (!IsHexInWorld(pos))
        {
            return false;
        }
        if(chunksDictionary.ContainsKey(new Vector3Int(thisChunk.x, 0, thisChunk.z))&& chunksDictionary[new Vector3Int(thisChunk.x, 0, thisChunk.z)].isHexMapPopulated) 
        {
            return blocktypes[chunksDictionary[new Vector3Int(thisChunk.x, 0, thisChunk.z)].GetHexFromGlobalVector3(pos)].isSolid; 
        }
        return blocktypes[GetHex(pos)].isSolid;
    }

    public bool CheckTheHex(Vector3 pos,System.Action<bool> callback)
    {
        bool doesHexExist;
        ChunkCoord thisChunk = GetChunkCoordFromVector3(pos);

        if (!IsHexInWorld(pos))
        {
            doesHexExist = false;
        }
        if (chunksDictionary.ContainsKey(new Vector3Int(thisChunk.x, 0, thisChunk.z)) && chunksDictionary[new Vector3Int(thisChunk.x, 0, thisChunk.z)].isHexMapPopulated)
        {
            doesHexExist = blocktypes[chunksDictionary[new Vector3Int(thisChunk.x, 0, thisChunk.z)].GetHexFromGlobalVector3(pos)].isSolid;
        }
        else 
        { 
            doesHexExist = blocktypes[GetHex(pos)].isSolid; 
        }
        callback(doesHexExist);
        return doesHexExist;
    }
    public byte GetHex(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);
        if (!IsHexInWorld(pos))
        {
            return 0;
        }

        /* BIOME SELECTION Pass*/

        float sumOfHeights = 0f;
        int count = 0;
        float strongestWeight = 0f;
        int strongestBiomeIndex = 0;

        /*for(int i = 0; i < biomes.Length; i++)
        {
            float weight = Noise.Get2DPerlin(new Vector2(pos.x, pos.z), biomes[i].offset, biomes[i].scale);

            if(weight>strongestWeight)
            {
                strongestWeight = weight;
                strongestBiomeIndex = i;
            }

            //float height = biomes[i].terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biomes[i].terrainScale)*weight;
            float height = biomes[i].terrainHeight * Noise.EvaluateNoise(new Vector2(pos.x, pos.z), biomes[i].roughness, biomes[i].strength, 0);// * weight;

            if (height > 0)
            {
                sumOfHeights += height;
                count++;
            }

        }*/

        BiomeAttributes biome = biomes[0];

        //sumOfHeights /= count;
        //float myHeight = noiseMap[Mathf.FloorToInt(pos.x-HexData.ChunkWidth), Mathf.FloorToInt(pos.z - HexData.ChunkWidth)];
        int firstLayerValue = 0;
        int terrainHeight =42;

        if (biome.noiseSettings.Length > 0)
        {
            firstLayerValue = Mathf.RoundToInt(biome.terrainHeight * Noise.EvaluateNoise(new Vector2(pos.x, pos.z), biome.noiseSettings[0].roughness, biome.noiseSettings[0].strength, 0, biome.noiseSettings[0].baseRoughness, biome.noiseSettings[0].numLayers, biome.noiseSettings[0].persistence, biome.noiseSettings[0].minValue, 123145));
            if (biome.noiseSettings[0].enabled)
            {
                terrainHeight += Mathf.RoundToInt(Noise.Map(0, 86, 0, 1, Noise.EvaluateNoise(new Vector2(pos.x, pos.z), biome.noiseSettings[0].roughness, biome.noiseSettings[0].strength, 0, biome.noiseSettings[0].baseRoughness, biome.noiseSettings[0].numLayers, biome.noiseSettings[0].persistence, biome.noiseSettings[0].minValue, 123145)));
                //terrainHeight += firstLayerValue;
            }
        }

        for (int i = 1; i < biome.noiseSettings.Length; i++)
        {
            if (biome.noiseSettings[i].enabled) {
                int mask = (biome.noiseSettings[i].useFirstLayerAsMask) ? firstLayerValue : 1;
                terrainHeight += Mathf.FloorToInt(biome.terrainHeight * Noise.EvaluateNoise(new Vector2(pos.x, pos.z), biome.noiseSettings[i].roughness, biome.noiseSettings[i].strength, 0, biome.noiseSettings[i].baseRoughness, biome.noiseSettings[i].numLayers, biome.noiseSettings[i].persistence, biome.noiseSettings[i].minValue, 123145)) * mask;
            } 
        }

        terrainHeight = Mathf.Min(terrainHeight, HexData.ChunkHeight-1);

        /* Basic Terrain Pass*/

        //int terrainHeight = Mathf.FloorToInt(biome.terrainHeight*Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0 , biome.terrainScale))+biome.solidGroundHeight;
        //int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * Noise.Generate2DPerlin(new Vector2(pos.x, pos.z), 0.25f, 2, 0.5f, 2) + biome.solidGroundHeight);
        //alttakini kullanınca yırtılma oluyor neden bir bak
        byte voxelValue = 0;

        if (yPos == terrainHeight)
        {
            voxelValue = biome.surfaceBlock;
        }
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
        {
            voxelValue = biome.subSurfaceBlock;
        }
        else if (yPos > terrainHeight)
        {
            return 0;
        }
        else
        {
            voxelValue = 1;
        }

        /*Second Pass*/

        if(voxelValue == 1)
        {
            foreach(Lode lode in biome.lodes)
            {
                if(yPos > lode.minHeight && yPos < lode.maxHeight)
                {
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                    {
                        voxelValue = lode.blockID;
                    }
                }
            }
        }

        /* TREE PASS */

        if (yPos == terrainHeight && biome.placeFlora)
        {

            if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.floraZoneScale) > biome.floraZoneThreshold)
            {

                if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.floraPlacementScale) > biome.floraPlacementThreshold)
                {
                    ConcurrentQueue<HexMod> structure = Structure.GenerateMajorFlora(biome.floraIndex,pos, biome.minFloraHeight, biome.maxFloraHeight);
                    modifications.Enqueue(structure);
                }
            }

        }
        return voxelValue;
    }

    bool IsChunkInWorld(ChunkCoord coord)
    {
        return coord.x >= 0 && coord.x < HexData.WorldSizeInChunks && coord.z >= 0 && coord.z < HexData.WorldSizeInChunks; 

    }

    bool IsHexInWorld(Vector3 pos)
    {
        return pos.y >= 0 && pos.y < HexData.ChunkHeight;

    }
}

[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;

    [Header("Texture Values")]
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int rightFaceTexture;
    public int leftFaceTexture;
    public int frontrightFaceTexture;
    public int frontleftFaceTexture;
    public int backrightFaceTexture;
    public int backleftFaceTexture;

    //Top, Bottom, Right, Left, Front Right, Front Left, Back Right, Back Left
    public int GetTextureID(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
                return topFaceTexture;
            case 1:
                return bottomFaceTexture;
            case 2:
                return rightFaceTexture;
            case 3:
                return leftFaceTexture;
            case 4:
                return frontrightFaceTexture;
            case 5:
                return frontleftFaceTexture;
            case 6:
                return backrightFaceTexture;
            case 7:
                return backleftFaceTexture;
            default:
                Debug.Log("Error in GetTextureID: invalid face index");
                return 0;

        } 
    }
}
public class HexMod
{
    public Vector3 position;
    public byte id;

    public HexMod()
    {
        position = new Vector3();
        id = 0;
    }

    public HexMod(Vector3 _position, byte _id)
    {
        position = _position;
        id = _id;
    }
}
