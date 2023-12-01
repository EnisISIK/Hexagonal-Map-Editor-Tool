using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{
    World _world;
    Text text;

    float frameRate;
    float timer;

    int halfWorldSizeInHex;
    int halfWorldSizeInChunks;

    void Start()
    {
        _world = GameObject.Find("World").GetComponent<World>();
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
        debugText += "XYZ: " + (Mathf.FloorToInt(_world.player.position.x)-halfWorldSizeInHex) +" / "+ (Mathf.FloorToInt(_world.player.position.y) - halfWorldSizeInHex) + " / " + (Mathf.FloorToInt(_world.player.position.z) - halfWorldSizeInHex);
        debugText += "\n";
        debugText += "Chunk: " + (_world.playerCurrentChunkCoord.x - halfWorldSizeInChunks) + " / " + (_world.playerCurrentChunkCoord.z - halfWorldSizeInChunks);

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
