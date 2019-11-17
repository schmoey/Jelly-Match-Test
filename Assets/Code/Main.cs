using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

public class Main : MonoBehaviour
{
    // Scene references
    public Board board;
    public Level currentLevel;

    // Quick gamestate
    public enum GameState
    {
        LevelInit,
        Playing,
        LevelFinished
    }

    GameState gameState;

    [Button("Load Level")]
    void LoadStartLevel()
    {
        board.LoadLevel(currentLevel);
    }

    // Used by UI buttons
    public void LoadNextLevel()
    {
        currentLevel = LevelManager.Instance.GetNextLevel(currentLevel);
        board.LoadLevel(currentLevel);
    }

    public void LoadPreviousLevel()
    {
        currentLevel = LevelManager.Instance.GetPreviousLevel(currentLevel);
        board.LoadLevel(currentLevel);
    }

    // Quick game loop - not doing much obviously
    public void Update()
    {
        switch(gameState)
        {
            case GameState.LevelInit:
                {
                    LoadStartLevel();
                    gameState = GameState.Playing;
                } break;
            case GameState.Playing:
                {
                } break;
            case GameState.LevelFinished:
                {
                } break;
        }
    }
}
