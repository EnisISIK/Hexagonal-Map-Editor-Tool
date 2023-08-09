using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{
    World world;
    Text text;

    float frameRate;
    float timer;

    int halfWorldSizeInHex;
    int halfWorldSizeInChunks;

    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        text = GetComponent<Text>();

        halfWorldSizeInHex = HexData.WorldSizeInBlocks / 2;
        halfWorldSizeInChunks = HexData.WorldSizeInChunks / 2;

    }

    void Update()
    {
        string debugText = " Debug Screen";
        debugText += "\n";
        debugText += frameRate + " fps";
        debugText += "\n";
        debugText += "XYZ: " + (Mathf.FloorToInt(world.player.position.x)-halfWorldSizeInHex) +" / "+ (Mathf.FloorToInt(world.player.position.y) - halfWorldSizeInHex) + " / " + (Mathf.FloorToInt(world.player.position.z) - halfWorldSizeInHex);
        debugText += "\n";
        debugText += "Chunk: " + (world.playerCurrentChunkCoord.x - halfWorldSizeInChunks) + " / " + (world.playerCurrentChunkCoord.z - halfWorldSizeInChunks);

        text.text = debugText;

        if (timer > 1f)
        {
            frameRate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;

        }
        else
        {
            timer += Time.deltaTime;
        }
    }
}
