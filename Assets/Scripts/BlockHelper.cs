using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BlockHelper
{

    public static byte GetBlock(BlockTypes blockType)
    {
        byte id;
        switch (blockType)
        {
            case BlockTypes.E_BLOCK_AIR:
                id = 0;
                break;
            case BlockTypes.E_BLOCK_STONE:
                id = 1;
                break;
            case BlockTypes.E_BLOCK_GRASS_DIRT:
                id = 2;
                break;
            case BlockTypes.E_BLOCK_OAK_WOOD:
                id = 3;
                break;
            case BlockTypes.E_BLOCK_DIRT:
                id = 4;
                break;
            case BlockTypes.E_BLOCK_OAK_LEAF:
                id = 5;
                break;
            case BlockTypes.E_BLOCK_SAND:
                id = 6;
                break;
            case BlockTypes.E_BLOCK_WATER:
                id = 7;
                break;
            case BlockTypes.E_BLOCK_GLASS:
                id = 8;
                break;
            case BlockTypes.E_BLOCK_ICE:
                id = 9;
                break;
            case BlockTypes.E_BLOCK_SAVANNAH_GRASS_DIRT:
                id = 10;
                break;
            case BlockTypes.E_BLOCK_BADLANDS_SAND:
                id = 11;
                break;
            case BlockTypes.E_BLOCK_SPRUCE_WOOD:
                id = 12;
                break;
            case BlockTypes.E_BLOCK_SPRUCE_LEAF:
                id = 13;
                break;
            case BlockTypes.E_BLOCK_SNOW:
                id = 14;
                break;
            case BlockTypes.E_BLOCK_TAIGA_GRASS_DIRT:
                id = 15;
                break;
            case BlockTypes.E_BLOCK_TAIGA_DIRT:
                id = 16;
                break;
            case BlockTypes.E_BLOCK_GRASS_BLADE:
                id = 17;
                break;
            case BlockTypes.E_BLOCK_TULLIP:
                id = 18;
                break;
            case BlockTypes.E_BLOCK_DANDELION:
                id = 19;
                break;
            case BlockTypes.E_BLOCK_DAISY:
                id = 20;
                break;
            case BlockTypes.E_BLOCK_CACTUS:
                id = 21;
                break;
            case BlockTypes.E_BLOCK_COBBLESTONE:
                id = 22;
                break;
            case BlockTypes.E_BLOCK_COAL_ORE:
                id = 23;
                break;
            case BlockTypes.E_BLOCK_IRON_ORE:
                id = 24;
                break;
            case BlockTypes.E_BLOCK_VINY_STONE:
                id = 25;
                break;
            case BlockTypes.E_BLOCK_MOSSY_COBBLESTONE:
                id = 26;
                break;
            case BlockTypes.E_BLOCK_FLOWER_LEAF:
                id = 27;
                break;
            case BlockTypes.E_BLOCK_RED_BRICK:
                id = 28;
                break;
            default:
                id = 0;
                break;
        }

        return id;
    }

}
