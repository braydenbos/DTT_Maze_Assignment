using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Generators
{
    /// <summary>
    /// Prim's Algorithm. Inheritance from MazeGenerator.
    /// </summary>
    public sealed class PrimGenerator : MazeGenerator
    {
        protected override void GenerateMaze() => StartCoroutine(GenerateMazeCoroutine(false));

        protected override IEnumerator GenerateMazeCoroutine(bool isSlowly = true)
        {
            // Initialize the visitedCells array and the cell stack
            VisitedCells = new bool[width, height];
            CellStack = new Stack<Vector3Int>();
        
            SetupMaze();

            // Choose a random starting cell and mark it as visited
            var startPosition = FindAndSetStartPositions();
            VisitedCells[startPosition[0].x, startPosition[0].y] = true;

            // Add the starting cell to the cell stack
            CellStack.Push(startPosition[0]);

            // Loop until all cells have been visited
            while (!AllTilesVisited())
            {
                if (isSlowly)
                {
                    yield return new WaitForSeconds(waitTime); // Pause execution and resume
                }
            
                // Get the current cell from the top of the stack
                var currentCell = CellStack.Peek();

                // Get the unvisited neighbors of the current cell
                var unvisitedNeighbors = GetUnvisitedNeighbors(currentCell);

                if (unvisitedNeighbors.Count > 0)
                {
                    var randomNeighbor = unvisitedNeighbors[Random.Range(0, unvisitedNeighbors.Count)];
                    VisitedCells[randomNeighbor.x, randomNeighbor.y] = true;

                    // Add the neighbor to the maze by updating the tile colors
                    UpdateTileColor(currentCell, randomNeighbor);

                    // Add the neighbor to the cell stack
                    CellStack.Push(randomNeighbor);
                }
                else CellStack.Pop(); // If all neighbors have been visited, remove the current cell from the stack
            }

            CreateEntrances();
        }
    }
}
