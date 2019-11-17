using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;


[ExecuteInEditMode]
public class BoardTile : MonoBehaviour
{
    Image tileImage;
    public Tile.TileType tileType;

    void Awake()
    {
        tileImage = GetComponent<Image>();
    }

    // Setup tile from tile scriptable object
    public void SetupTile(Tile.TileType tileType)
    {
        // Get tile scriptable object
        Tile tileSettings = BlockManager.Instance.GetTile(tileType);
        
        // Use the right sprite
        tileImage.sprite = tileSettings.sprite;

        // Set type
        this.tileType = tileSettings.tileType;
    }
}
