using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Maze.Generators
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
            DateTime before = DateTime.Now;
            VisitedCells = new bool[width, height]; // Initialize the visitedCells array
            CellStack = new Stack<Vector3Int>(); // Initialize the cell stack
            bool nextVisitIsVisited = false;

            SetupMaze();

            var startPosition = FindAndSetStartPositions();
            CellStack.Push(startPosition[0]);
            // UpdateTileColor(startPosition);
        
            // Update the starting tile color before entering the main loop
            yield return new WaitForSeconds(waitTime);
            UpdateTileColor(startPosition[0]);

            // Loop until all cells have been in the CellStack
            while (CellStack.Count > 0)
            {
                if (isSlowly && !nextVisitIsVisited) 
                {
                    yield return new WaitForSeconds(waitTime); // Pause execution and resume
                }

                var currentCell = CellStack.Peek();
                VisitedCells[currentCell.x, currentCell.y] = true;

                var unvisitedNeighbors = GetUnvisitedNeighbors(currentCell);

                if (unvisitedNeighbors.Count > 0)
                {
                    var randomNeighbor = unvisitedNeighbors[Random.Range(0, unvisitedNeighbors.Count)];
                    UpdateTileColor(currentCell, randomNeighbor);
                    CellStack.Push(randomNeighbor);
                    nextVisitIsVisited = false;
                }
                else
                {
                    // Mark the current cell as visited after backtracking
                    UpdateTileColor(currentCell);
                    CellStack.Pop();
                    nextVisitIsVisited = true;
                }

                // Stop the maze generation process
                if (AllTilesVisited()) break;
            }

            CreateEntrances();
            DateTime after = DateTime.Now;
            TimeSpan duration = after.Subtract(before);
            SetTime(duration);
        }
    
        #endregion
    }
}
