using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MagTest/Tile")]
public class Tile : ScriptableObject
{
    public Sprite sprite;
    public GameObject tilePrefab;
    public bool canHoldBlock = true;
    public TileType tileType;

    public enum TileType
    {
        Solid,
        Empty
    }
}
