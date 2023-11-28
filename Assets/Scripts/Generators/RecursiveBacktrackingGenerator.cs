using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Generators
{
    /// <summary>
    /// Recursive Backtracking Algorithm (Depth-First Search). Inheritance from MazeGenerator.
    /// </summary>
    public sealed class RecursiveBacktrackingGenerator : MazeGenerator
    {
        #region protected functions
    
        protected override void GenerateMaze() => StartCoroutine(GenerateMazeCoroutine(false));

        protected override IEnumerator GenerateMazeCoroutine(bool isSlowly = true)
        {
            visitedCells = new bool[width, height]; // Initialize the visitedCells array
            cellStack = new Stack<Vector3Int>(); // Initialize the cell stack

            SetupMaze();

            var startPosition = FindAndSetStartPosition();
            cellStack.Push(startPosition);
            // UpdateTileColor(startPosition);
        
            // Update the starting tile color before entering the main loop
            yield return new WaitForSeconds(waitTime);
            UpdateTileColor(startPosition);

            // Loop until all cells have been in the CellStack
            while (cellStack.Count > 0)
            {
                if (isSlowly) 
                {
                    yield return new WaitForSeconds(waitTime); // Pause execution and resume
                }

                var currentCell = cellStack.Peek();
                visitedCells[currentCell.x, currentCell.y] = true;

                var unvisitedNeighbors = GetUnvisitedNeighbors(currentCell);

                if (unvisitedNeighbors.Count > 0)
                {
                    var randomNeighbor = unvisitedNeighbors[Random.Range(0, unvisitedNeighbors.Count)];
                    UpdateTileColor(currentCell, randomNeighbor);
                    cellStack.Push(randomNeighbor);
                }
                else
                {
                    // Mark the current cell as visited after backtracking
                    UpdateTileColor(currentCell);
                    cellStack.Pop();
                }

                // Stop the maze generation process
                if (AllTilesVisited()) break;
            }

            CreateEntranceAndExit();
        }
    
        #endregion
    }
}
