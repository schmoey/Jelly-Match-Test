using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "MagTest/Managers/Level Manager")]
public class LevelManager : ScriptableObject
{
    static LevelManager _instance = null;

    public static LevelManager Instance
    {
        get
        {
            if (!_instance)
            {
                string[] guids = AssetDatabase.FindAssets("t:LevelManager");

                if (guids.Length > 1)
                {
                    Debug.LogError(string.Format("LevelManager: multiple instances of manager found"));
                }
                else
                {
                    _instance = AssetDatabase.LoadAssetAtPath<LevelManager>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }

            return _instance;
        }
    }

    public List<Level> levels = new List<Level>();

    [Button("Populate Levels")]
    public void PopulateLevels()
    {
        levels.Clear();

        foreach (string guid in AssetDatabase.FindAssets("t:Level"))
        {
            levels.Add(AssetDatabase.LoadAssetAtPath<Level>(AssetDatabase.GUIDToAssetPath(guid)));
        }

        SanityCheck();
    }

    public Level GetNextLevel(Level level)
    {
        return (levels.IndexOf(level) == levels.Count - 1) ? levels[0] : levels[levels.IndexOf(level) + 1];
    }

    public Level GetPreviousLevel(Level level)
    {
        return (levels.IndexOf(level) == 0) ? levels[levels.Count - 1] : levels[levels.IndexOf(level) - 1];
    }

    [Button("Sanity Check")]
    public void SanityCheck()
    {
        // Check for duplicate level numbers
        Dictionary<int, Level> uniqueLevels = new Dictionary<int, Level>();

        foreach (Level level in levels)
        {
            if (!uniqueLevels.ContainsKey(level.level))
            {
                uniqueLevels.Add(level.level, level);
            }
            else
            {
                Debug.Log(string.Format("{0} ({1}) level already found in {2}", level.level, level, uniqueLevels[level.level]));
            }
        }
    }

    public int GetNextAvailableLevelNumber()
    {
        int highestLevel = 0;

        foreach (Level level in levels)
        {
            if (level.level > highestLevel)
            {
                highestLevel = level.level;
            }
        }

        return highestLevel + 1;
    }
}