using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A position on the block grid
/// Contains grid position, block type, color and reference to block gameobject
/// </summary>
public class BoardPosition
{
    public int x;
    public int y;
    public Block.BlockType blockType;
    public Block.BlockColor blockColor;
    public Vector3 position;
    public BoardBlock boardBlock;
    public GameObject go;
    public bool needsABlock = false;

    // Setup block from block scriptable object
    public void SetupBlock(Block.BlockType blockType)
    {
        // Get tile scriptable object
        Block blockSettings = BlockManager.Instance.GetBlock(blockType);
        this.blockType = blockType;

        if (blockType == Block.BlockType.Normal)
        {
            go.GetComponent<BoardBlock>().blockImage.sprite = BlockManager.Instance.SetRandomColor(this);
        }
        else if (blockType == Block.BlockType.None)
        {
            go.GetComponent<Image>().enabled = false;
        }
    }
}