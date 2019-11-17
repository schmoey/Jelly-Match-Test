using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

public class LevelEditorWindow : OdinMenuEditorWindow
{
    [MenuItem("MagTest/Level Editor %#m")]
    static void OpenWindow()
    {
        GetWindow<LevelEditorWindow>().Show();        
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        return new OdinMenuTree(false)
        {
            {"Levels", new VisualLevelEditorWindow(), EditorIcons.Char1 }
        };
    } 
}

[ShowOdinSerializedPropertiesInInspector]
public class VisualLevelEditorWindow
{
    public VisualLevelEditorWindow()
    {
        tileSelector.SetSelection(Tile.TileType.Empty);
    }

    void LoadCurrentLevel()
    {
        rows = currentLevel.grid.GetLength(1);
        columns = currentLevel.grid.GetLength(0);
        
        grid = currentLevel.grid;
        levelNumber = currentLevel.level;
    }

    void SaveCurrentLevel()
    {
        currentLevel.grid = grid;
        currentLevel.level = levelNumber;
    }

    [InfoBox("Use the arrows to select a level, or \"New Level\" create a new level")]
    [BoxGroup("Asset", ShowLabel = false, Order = 0), AssetList(Path = "/Resources/Levels"), OnValueChanged("LoadCurrentLevel")]
    public Level currentLevel;
    [BoxGroup("Asset"), OnValueChanged("SaveCurrentLevel")]
    public int levelNumber;
    [BoxGroup("Asset/Buttons", ShowLabel = false), Button(ButtonSizes.Medium)]
    void NewLevel()
    {
        // Refresh all the levels
        LevelManager.Instance.PopulateLevels();
        // Get the next level
        int newLevelNumber = LevelManager.Instance.GetNextAvailableLevelNumber();

        // Create and save a new level asset
        Level newLevel = ScriptableObject.CreateInstance<Level>();
        AssetDatabase.CreateAsset(newLevel, string.Format("Assets/Resources/Levels/{0}.asset", newLevelNumber.ToString()));
        AssetDatabase.SaveAssets();

        newLevel.name = newLevelNumber.ToString();
        newLevel.level = newLevelNumber;
        EditorUtility.SetDirty(newLevel);
    }

    [BoxGroup("Settings", Order = 1)]
    public int columns = 10;
    [BoxGroup("Settings")]
    public int rows = 10;
    [BoxGroup("Settings"), Button("Resize Level Grid", ButtonSizes.Medium)]
    public void ResizeLevelGrid()
    {
        grid = new Tile.TileType[columns, rows];

        // Initialise with empty tiles
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                grid[i, j] = Tile.TileType.Empty;
            }
        }

        if (currentLevel)
        {
            SaveCurrentLevel();
        }
    }
    
    [InfoBox("Select a tile type and then paint over the grid to create a level layout")]
    // Annoyingly this gets squished when the level grid expands... thank you all-father
    [BoxGroup("Editor", Order = 2), ShowInInspector]
    static EnumSelector<Tile.TileType> tileSelector = new EnumSelector<Tile.TileType>();

    // And where is the cell size?? :| 
    [BoxGroup("Editor"), TableMatrix(HorizontalTitle = "Level Grid", DrawElementMethod = "DrawColoredEnumElement", ResizableColumns = false, SquareCells = true, Transpose = false)]
    public Tile.TileType[,] grid;

    // Draws the selected tile on to the grid
    static Tile.TileType DrawColoredEnumElement(Rect rect, Tile.TileType tileType)
    {
        if ((Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown) && rect.Contains(Event.current.mousePosition))
        {
            GUI.changed = true;
            Event.current.Use();
            tileType = tileSelector.GetCurrentSelection().First();
        }

        EditorGUI.DrawTextureTransparent(rect, BlockManager.Instance.GetTile(tileType).sprite.texture);

        return tileType;
    }
}