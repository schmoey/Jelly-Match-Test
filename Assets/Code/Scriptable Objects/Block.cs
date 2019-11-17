using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MagTest/Block")]
public class Block : ScriptableObject
{
    public Sprite sprite;
    public BlockType blockType;

    // Types of block (rows and column block functionality unimplemented)
    public enum BlockType
    {
        Normal,
        Row,
        Column,
        None
    }

    // Types of block colors
    public enum BlockColor
    {
        Pink,
        Orange,
        Yellow,
        Green,
        Brown
    }
}