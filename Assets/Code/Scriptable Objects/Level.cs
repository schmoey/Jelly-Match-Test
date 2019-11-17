using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "MagTest/Level")]
public class Level : SerializedScriptableObject
{
    public int level = 0;

    [ReadOnly, InfoBox("Edit level with editor - MagTest/Level Editor", InfoMessageType.Info)]
    public Tile.TileType[,] grid = new Tile.TileType[5,5];

    public Level()
    {
        // Initialise with empty tiles
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                grid[i, j] = Tile.TileType.Empty;
            }
        }
    }
}
