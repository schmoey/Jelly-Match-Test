using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using DG.Tweening;
using Random = UnityEngine.Random;

public class Board : SerializedMonoBehaviour
{
    // Tile spawn parent
    public Transform tileParent;
    // Block spawn parent
    public Transform blockParent;
    // Scale to apply to blocks if grid size gets reduced
    float blockScale = 1f;
    // Grid layout group for that timeless grid look
    public GridLayoutGroup tileParentGridLayoutGroup;
    // Block grid
    //BoardBlock[,] blockGrid;
    public BoardPosition[,] blockGrid;
    // List of potentially acceptable links when a tile is selected
    public List<BoardPosition> potentialLinks = new List<BoardPosition>();
    // Simple state of what the blocks are doing
    public BlockState blockState = BlockState.Unselected;
    // Keeps track of the chain of selected blocks
    public List<BoardPosition> selectedBlocks = new List<BoardPosition>();
    // Amount of blocks to match in a row
    const int BLOCKS_MATCH_AMOUNT = 3;
    
    public enum BlockState
    {
        Unselected,
        Selecting,
        Selected,
        Falling
    }

    public enum BoardDirection
    {
        TopLeft,
        Top,
        TopRight,
        Left,
        Right,
        BottomLeft,
        Bottom,
        BottomRight
    }

    public BoardPosition GetBoardPosition(BoardBlock boardBlock)
    {
        BoardPosition foundBoardPos = null;

        foreach (BoardPosition boardPos in blockGrid)
        {
            if (boardPos.boardBlock == boardBlock)
            {
                foundBoardPos = boardPos;
                break;
            }
        }

        return foundBoardPos;
    }

    // Reduced tileSize if the required area is bigger than the playfield, but leaves max at prefab size
    void CalculateSmallestPossibleTileSize(Level level, ref Rect tileSize)
    {
        float totalWidth = 0f;
        int gridRows = level.grid.GetLength(1);
        int gridColumns = level.grid.GetLength(0);
        int reductionIteration = 1;
        float reductionAmountStep = 4f;
        float playAreaWidth = tileParent.GetComponent<RectTransform>().rect.width;
        float originalTileSize = tileSize.width;

        // Calculating needed width as the prefab width with 1/16th padding (both sides) in between tiles
        totalWidth = (gridColumns * tileSize.width) + ((gridColumns - 1) * (tileSize.width / 16f) * 2);

        // Gridlayoutgroup padding to put board in the middle -- not necessary anymore
        //if (totalWidth < playAreaWidth)
        //{
        //    int halfPadding = (int)(playAreaWidth - totalWidth) / 2;
        //    tileParentGridLayoutGroup.padding.Add(new Rect(halfPadding, halfPadding, 0, 0));
        //}

        // Make grid component match level grid layout
        tileParentGridLayoutGroup.constraintCount = gridColumns;

        // If the total width is greater than the allocated size we need to reduce the tiles evenly to fit
        while (totalWidth > playAreaWidth)
        {
            // ...yes I could just calculate the amount needed but this is already here
            tileSize.width = tileSize.height = (tileSize.width - (reductionIteration * reductionAmountStep));
            reductionIteration++;

            totalWidth = (gridColumns * tileSize.width) + ((gridColumns - 1) * (tileSize.width / 16f) * 2);
        }

        // Set block scale accordingly
        blockScale = (reductionIteration > 1) ? tileSize.width / originalTileSize : 1f;
    }

    public void LoadLevel(Level level)
    {
        blockGrid = new BoardPosition[level.grid.GetLength(0), level.grid.GetLength(1)];

        ClearBoard();

        Rect tileSize = BlockManager.Instance.tilePrefab.GetComponent<RectTransform>().rect;

        // Make sure tiles are optimally sized
        CalculateSmallestPossibleTileSize(level, ref tileSize);

        // Gridlayoutgroup hates changing the rect size, but all is not lost let's use the new rect size for the cellsize instead
        tileParentGridLayoutGroup.cellSize = new Vector2(tileSize.width, tileSize.height);

        for (int j = 0; j < level.grid.GetLength(1); j++)
        {
            for (int i = 0; i < level.grid.GetLength(0); i++)
            {
                // Load in tiles
                GameObject boardTileObject = Instantiate(BlockManager.Instance.tilePrefab, Vector3.zero, Quaternion.identity, tileParent);
                boardTileObject.name = string.Format("{0} - {1}", i, j);
                BoardTile boardTile = boardTileObject.GetComponent<BoardTile>();

                // Style tile according to level data
                boardTile.SetupTile(level.grid[i, j]);

                // Load in blocks
                GameObject boardBlockObject = Instantiate(BlockManager.Instance.blockPrefab, boardTile.transform.position, Quaternion.identity, transform);
                boardBlockObject.name = string.Format("BLOCK {0} - {1}", i, j);
                boardBlockObject.transform.localScale = BlockManager.Instance.blockPrefab.transform.localScale * blockScale;

                // Ugh, this is so ridiculous
                // At least in the editor we need to wait a couple frames for the gridlayoutgroup component to put the tiles in their right place so we know their correct positions for the blocks
                // Could be mitigated by running the whole loop first for tiles, hidden by animation / loading, rendering it offscreen and calculating it all in advance
                // Here's some terrible thing where we wait a tiny bit for the gridlayoutgroup to do it's thing first
                // UI = fun
                StartCoroutine(WaitThenDo(0.1f, () => { boardBlockObject.GetComponent<RectTransform>().anchoredPosition = boardTileObject.GetComponent<RectTransform>().anchoredPosition; }));

                // Set grid data
                blockGrid[i, j] = new BoardPosition();
                BoardPosition boardPos = blockGrid[i, j];
                boardPos.go = boardBlockObject;
                boardPos.boardBlock = boardBlockObject.GetComponent<BoardBlock>();
                boardPos.y = j;
                boardPos.x = i;

                // Spawn blocks on empty tiles
                if (boardTile.tileType == Tile.TileType.Empty)
                {
                    // Style block
                    boardPos.SetupBlock(Block.BlockType.Normal);

                    // Setup block input events
                    boardPos.boardBlock.OnBlockPointerEnter += OnBlockPointerEnter;
                    boardPos.boardBlock.OnBlockPointerDown += OnBlockPointerDown;
                    boardPos.boardBlock.OnBlockPointerUp += OnBlockPointerUp;
                }
                else if (boardTile.tileType == Tile.TileType.Solid)
                {
                    boardPos.SetupBlock(Block.BlockType.None);
                }
            }
        }

        // UI hacks are the worst - fade in grid while waiting to get the recttransform positions
        Sequence fadeInSequence = DOTween.Sequence();
        fadeInSequence.Append(tileParent.GetComponent<CanvasGroup>().DOFade(1f, 1f));
        fadeInSequence.Insert(0f, blockParent.GetComponent<CanvasGroup>().DOFade(1f, 1f));

        fadeInSequence.OnComplete(() =>
        {
            foreach (BoardPosition bp in blockGrid)
            {
                bp.position = bp.go.GetComponent<RectTransform>().anchoredPosition;
            }
        });

        fadeInSequence.Play();
    }

    IEnumerator WaitThenDo(float waitTime, Action action)
    {
        yield return new WaitForSeconds(waitTime);
        action();
    }

    public void Update()
    {
        switch (blockState)
        {
            // We're now collecting a group of the same color
            case BlockState.Selecting:
                {
                    // We're selecting a second+ block and potentiallinks contains the options
                    // Fade every other block slightly for visual effect
                    foreach (BoardPosition boardPos in blockGrid)
                    {
                        if (!potentialLinks.Contains(boardPos))
                        {
                            boardPos.boardBlock?.Fade(true);
                        }
                        else
                        {
                            boardPos.boardBlock?.Fade(false);
                        }
                    }
                } break;
            // When we scoop up a block
            case BlockState.Selected:
                {
                    foreach (BoardPosition boardPos in blockGrid)
                    {
                        boardPos.boardBlock?.Fade(false);
                    }
                }  break;
        }
    }

    void ResetBlockSelection()
    {
        foreach (BoardPosition boardPos in blockGrid)
        {
            boardPos.boardBlock?.Fade(false);
        }
    }

    // Gets a block neighbour
    BoardPosition GetNeighbour(BoardPosition boardPos, BoardDirection direction)
    {
        // Grid boundaries
        int minX = 0;
        int minY = 0;
        int maxX = blockGrid.GetLength(0) - 1;
        int maxY = blockGrid.GetLength(1) - 1;

        BoardPosition neighbour = null;

        switch (direction)
        {            
            case BoardDirection.TopLeft:
                {
                    if (boardPos.x - 1 >= minX && boardPos.y - 1 >= minY)
                    {
                        neighbour = blockGrid[boardPos.x - 1, boardPos.y - 1];
                    }
                } break;
            case BoardDirection.Top:
                {
                    if (boardPos.y - 1 >= minY)
                    {
                        neighbour = blockGrid[boardPos.x, boardPos.y - 1];
                    }
                } break;
            case BoardDirection.TopRight:
                {
                    if (boardPos.x + 1 <= maxX && boardPos.y - 1 >= minY)
                    {
                        neighbour = blockGrid[boardPos.x + 1, boardPos.y - 1];
                    }
                } break;
            case BoardDirection.Left:
                {
                    if (boardPos.x - 1 >= minX)
                    {
                        neighbour = blockGrid[boardPos.x - 1, boardPos.y];
                    }
                } break;
            case BoardDirection.Right:
                {
                    if (boardPos.x + 1 <= maxX)
                    {
                        neighbour = blockGrid[boardPos.x + 1, boardPos.y];
                    }
                }
                break;
            case BoardDirection.BottomLeft:
                {
                    if (boardPos.x - 1 >= minX && boardPos.y + 1 <= maxY)
                    {
                        neighbour = blockGrid[boardPos.x - 1, boardPos.y + 1];
                    }
                } break;
            case BoardDirection.Bottom:
                {
                    if (boardPos.y + 1 <= maxY)
                    {
                        neighbour = blockGrid[boardPos.x, boardPos.y + 1];
                    }
                } break;
            case BoardDirection.BottomRight:
                {
                    if (boardPos.x + 1 <= maxX && boardPos.y + 1 <= maxY)
                    {
                        neighbour = blockGrid[boardPos.x + 1, boardPos.y + 1];
                    }
                } break;
        }
        
        return neighbour;
    }

    // Get neighbours for multiple directions
    List<BoardPosition> GetNeighbours(BoardPosition boardPos, BoardDirection[] directions)
    {
        List<BoardPosition> boardBlocks = new List<BoardPosition>();

        foreach (BoardDirection direction in directions)
        {
            if (boardPos != null)
            {
                BoardPosition neighbour = GetNeighbour(boardPos, direction);

                if (neighbour != null && neighbour.blockType != Block.BlockType.None)
                {
                    boardBlocks.Add(GetNeighbour(boardPos, direction));
                }
            }
        }

        return boardBlocks;
    }

    // Checking neighbours to see if they are valid next moves
    List<BoardPosition> NextPotentialPositions(BoardPosition boardPos)
    {
        List<BoardPosition> blockChoices = new List<BoardPosition>();
       
        // Check every direction
        foreach (BoardDirection boardDirection in Enum.GetValues(typeof(BoardDirection)))
        {
            // Get that direction neighbour
            BoardPosition neighbour = GetNeighbour(boardPos, boardDirection);

            // Does is match the color and is can it go there
            if (neighbour != null && neighbour.blockColor == boardPos.blockColor && neighbour.blockType != Block.BlockType.None)
            {
                blockChoices.Add(neighbour);
            }
        }

        return blockChoices;
    }


    // Block input events

    // Selecting first tile
    void OnBlockPointerDown(BoardBlock boardBlock, PointerEventData.InputButton inputButton)
    {
        blockState = BlockState.Selecting;

        BoardPosition boardPos = GetBoardPosition(boardBlock);
        // Add it to the list of tiles chosen
        selectedBlocks.Add(boardPos);
        // Find the next tiles of the same color
        potentialLinks = NextPotentialPositions(boardPos);
    }

    // Selecting other blocks after an initial block is selected, continues the matching chain
    void OnBlockPointerEnter(BoardBlock boardBlock, PointerEventData.InputButton inputButton)
    {
        BoardPosition boardPos = GetBoardPosition(boardBlock);

        // Only care about going over other tiles if one is selected already
        // Also needs to match the same color
        if (blockState == BlockState.Selecting && potentialLinks.Contains(boardPos))
        {
            // If we're entering the previously recorded tile...
            if (selectedBlocks.Count > 1 && selectedBlocks.IndexOf(boardPos) == selectedBlocks.Count - 2)
            {
                // ...remove it, the player has gone back a step in their line drawing
                selectedBlocks.RemoveAt(selectedBlocks.Count - 1);

                // Calculate next set of potential block links
                potentialLinks = NextPotentialPositions(boardPos);
            }
            // Otherwise add the tile if it isn't already captured
            else if (!selectedBlocks.Contains(boardPos))
            {
                selectedBlocks.Add(boardPos);

                // Calculate next set of potential block links
                potentialLinks = NextPotentialPositions(boardPos);
            }
        }
    }

    // Input released, check for valid block string
    void OnBlockPointerUp(GameObject gameObject, PointerEventData.InputButton inputButton)
    {
        // Line of collected tiles
        // Gotta match the right amount (three!)
        if (selectedBlocks.Count >= BLOCKS_MATCH_AMOUNT)
        {
            foreach (BoardPosition bp in selectedBlocks)
            {
                bp.needsABlock = true;
                bp.boardBlock = null;
                Destroy(bp.go);
            }
        }

        blockState = BlockState.Selected;

        RefillBlockGrid();
    }


    void RefillBlockGrid()
    {
        blockState = BlockState.Falling;

        // Three ways a block can fall down into an empty tile
        BoardDirection[] upwardsDirections = new BoardDirection[] { BoardDirection.Top, BoardDirection.TopLeft, BoardDirection.TopRight };

        // Loop checking to keep blocks falling until they can't fall anymore
        bool areBlocksNeeded = true;
       
        while (areBlocksNeeded)
        {
            bool isFalling = false;

            foreach (BoardPosition pos in blockGrid)
            {
                if (pos.needsABlock && pos.y > 0)
                {
                    List<BoardPosition> availableBlocksToFall = GetNeighbours(pos, upwardsDirections);

                    BoardPosition aps = null;

                    if (availableBlocksToFall.Count > 0)
                    {
                        aps = availableBlocksToFall[0];
                    }
                    
                    if (aps != null && !aps.needsABlock && aps.blockType != Block.BlockType.None)
                    {
                        isFalling = true;

                        aps.go.GetComponent<RectTransform>().DOAnchorPos(pos.position, .5f).SetEase(BlockManager.Instance.blockFallEase);

                        // Block swapping
                        pos.boardBlock = null;
                        pos.boardBlock = aps.boardBlock;
                        pos.needsABlock = false;
                        pos.go = aps.go;
                        pos.blockColor = aps.blockColor;

                        aps.boardBlock = null;
                        aps.needsABlock = true;
                        aps.go = null;
                    }
                }
                // Block is in top row, need to bring a new one from above the playfield
                else if (pos.needsABlock && pos.y == 0)
                {
                    isFalling = true;

                    // Bring in new block & apply block settings
                    GameObject boardBlockObject = Instantiate(BlockManager.Instance.blockPrefab, pos.position, Quaternion.identity, transform);
                    boardBlockObject.transform.localScale = BlockManager.Instance.blockPrefab.transform.localScale * blockScale;

                    BoardBlock boardBlock = boardBlockObject.GetComponent<BoardBlock>();
                    RectTransform blockRectTransform = boardBlock.GetComponent<RectTransform>();

                    blockRectTransform.anchoredPosition = pos.position + new Vector3(0, 64, 0);
                    blockRectTransform.DOAnchorPos(pos.position, 0.5f).SetEase(BlockManager.Instance.blockFallEase);

                    pos.boardBlock = boardBlock;

                    boardBlock.OnBlockPointerDown += OnBlockPointerDown;
                    boardBlock.OnBlockPointerUp += OnBlockPointerUp;
                    boardBlock.OnBlockPointerEnter += OnBlockPointerEnter;
                    pos.go = boardBlockObject;
                    pos.SetupBlock(Block.BlockType.Normal);

                    pos.needsABlock = false;
                }
            }

            // Leave the loop if no more blocks are falling
            if (!isFalling)
            {
                areBlocksNeeded = false;
            }
        }


        blockState = BlockState.Unselected;
        selectedBlocks.Clear();

        // Visual state reset, no block fade
        ResetBlockSelection();
    }

    public void ClearBoard()
    {
        foreach (Transform children in tileParent)
        {
            GameObject.Destroy(children.gameObject);
        }

        foreach (Transform children in transform)
        {
            GameObject.Destroy(children.gameObject);
        }
    }
}
