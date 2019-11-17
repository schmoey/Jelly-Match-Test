using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "MagTest/Managers/Block Manager")]
public class BlockManager : SerializedScriptableObject
{
    // Singleton pattern
    static BlockManager _instance = null;

    public static BlockManager Instance
    {
        get
        {
            if (!_instance)
            {
                string[] guids = AssetDatabase.FindAssets("t:BlockManager");

                if (guids.Length > 1)
                {
                    Debug.LogError(string.Format("BlockManager: multiple instances of manager found"));
                }
                else
                {
                    _instance = AssetDatabase.LoadAssetAtPath<BlockManager>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }

            return _instance;
        }
    }

    public List<Tile> tiles = new List<Tile>();
    public List<Block> blocks = new List<Block>();
    public GameObject tilePrefab;
    public GameObject blockPrefab;
    // Quick store of block sprites
    public Dictionary<Block.BlockType, Dictionary<Block.BlockColor, Sprite>> blockSprites = new Dictionary<Block.BlockType, Dictionary<Block.BlockColor, Sprite>>();
    // You can edit the easing on the blocks falling!
    public AnimationCurve blockFallEase;

    [Button("Populate Block & Tiles Data")]
    public void PopulateBlockTileTypes()
    {
        blocks.Clear();
        tiles.Clear();

        // string ugh could use reflection
        foreach (string guid in AssetDatabase.FindAssets("t:Block"))
        {
            blocks.Add(AssetDatabase.LoadAssetAtPath<Block>(AssetDatabase.GUIDToAssetPath(guid)));
        }

        foreach (string guid in AssetDatabase.FindAssets("t:Tile"))
        {
            tiles.Add(AssetDatabase.LoadAssetAtPath<Tile>(AssetDatabase.GUIDToAssetPath(guid)));
        }
    }

    // Get block from enum
    public Block GetBlock(Block.BlockType blockType)
    {
        Block foundBlock = null;

        foreach (Block block in blocks)
        {
            if (block.blockType == blockType)
            {
                foundBlock = block;
            }
        }

        return foundBlock;
    }

    // Get block sprite with specific color and type
    public Sprite GetBlockSprite(Block.BlockType blockType, Block.BlockColor blockColor)
    {
        return blockSprites[blockType][blockColor];
    }

    public Sprite SetRandomColor(BoardPosition boardPosition)
    {
        Block.BlockColor randomColor = (Block.BlockColor)Random.Range(0, Enum.GetValues(typeof(Block.BlockColor)).Length);
        boardPosition.blockColor = randomColor;

        return blockSprites[boardPosition.blockType][randomColor];
    }

    // Get tile from enum
    public Tile GetTile(Tile.TileType tileType)
    {
        Tile foundTile = null;

        foreach (Tile tile in tiles)
        {
            if (tile.tileType == tileType)
            {
                foundTile = tile;
                break;
            }
        }

        return foundTile;
    }
}
