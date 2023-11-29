using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Generators
{
    /// <summary>
    /// An abstract maze generator class. Inherit this class and make a own GenerateMaze() & GenerateMazeCoroutine().
    /// </summary>
    public abstract class MazeGenerator : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] protected int width;
        [SerializeField] protected int height;
        [SerializeField] private bool generateInstantly = true;
        [SerializeField] protected float waitTime = 0.1f;
    
        [Header("Tilemap")]
        [SerializeField] protected Tilemap tilemap;
        [SerializeField] protected TileBase wallTile;
        [SerializeField] protected TileBase unvisitedCellTile;
        [SerializeField] protected TileBase visitedCellTile;

        [Header("UI")]
        [SerializeField] private Slider inputWidth;
        [SerializeField] private Slider inputHeight;
        [SerializeField] private TextMeshProUGUI textWidth;
        [SerializeField] private TextMeshProUGUI textHeight;
        
        [SerializeField] private TMP_InputField inputWaitTime;

        [Header("Other generators")]
        [SerializeField] private CurrentGenerator currentGenerator;
        [SerializeField] private MazeGenerator otherGenerator;
    
        [Space]
        [SerializeField] private UnityEvent onGenerate = new UnityEvent();
    
        protected bool[,] VisitedCells; // To track visited cells
        protected Stack<Vector3Int> CellStack; // Stack to track visited cells

        private readonly Vector3Int[] _directions = 
        {
            new (0, 2, 0), // Up
            new (2, 0, 0), // Right
            new (0, -2, 0), // Down
            new (-2, 0, 0) // Left
        };

        #region Lifetime functions

        private void Start()
        {
            inputWidth.onValueChanged.AddListener(SetWidth);
            inputHeight.onValueChanged.AddListener(SetHeight);
            inputWaitTime.onValueChanged.AddListener(SetWaitTime);
        }

        #endregion

        #region public functions

        /// <summary>
        /// Start generating or regenerate the maze.
        /// </summary>
        public void StartGenerating()
        {
            // When regenerating, it clears the previous maze
            tilemap.ClearAllTiles();

            // Stop the current maze generation if it is in progress
            if (currentGenerator.CurrentGeneration != null)
            {
                otherGenerator.StopAllCoroutines();
                StopAllCoroutines();
                currentGenerator.CurrentGeneration = null;
            }

            if (generateInstantly) GenerateMaze();
            else currentGenerator.CurrentGeneration = StartCoroutine(GenerateMazeCoroutine());

            onGenerate?.Invoke();
        }

        #endregion

        #region protected functions

        /// <summary>
        /// Generates the maze.
        /// </summary>
        protected abstract void GenerateMaze();

        /// <summary>
        /// Generates the maze with some delay in between visiting the tiles.
        /// </summary>
        protected abstract IEnumerator GenerateMazeCoroutine(bool isSlowly = true);
    
        /// <summary>
        /// Creates a blank maze for the generation.
        /// </summary>
        protected void SetupMaze()
        {
            width = width % 2 == 0 ? width + 1 : width;
            height = height % 2 == 0 ? height + 1 : height;

            VisitedCells = new bool[width, height]; // Initialize the VisitedCells array
        
            for (int x = -1; x < width + 1; x++)
            {
                for (int y = -1; y < height + 1; y++)
                {
                    var position = new Vector3Int(x, y, 0);

                    if (x == -1 || y == -1 || x == width || y == height)
                    {
                        // Set walls for the border cells
                        tilemap.SetTile(position, wallTile);
                    }
                    else if (x % 2 == 0 || y % 2 == 0)
                    {
                        // Set walls for even-indexed cells
                        tilemap.SetTile(position, wallTile);
                    }
                    else
                    {
                        // Set empty space for odd-indexed cells
                        tilemap.SetTile(position, unvisitedCellTile);
                        VisitedCells[x, y] = false;
                    }
                }
            }
        }

        /// <summary>
        /// Creates the entrance and exit for the maze.
        /// </summary>
        protected void CreateEntrances()
        {
            var numberOfEntrances = Random.Range(2, 5); // Choose the number of entrances (adjust as needed)

            for (int i = 0; i < numberOfEntrances; i++)
            {
                var entrancePosition = FindRandomEmptyTile();

                // Ensure entrance position is valid
                while (!IsValidEntranceExitPosition(entrancePosition))
                    entrancePosition = FindRandomEmptyTile();

                // Adjust entrance position to be at the left edge
                entrancePosition.x = -1;

                // Set tiles for entrance
                tilemap.SetTile(entrancePosition, visitedCellTile);

                // Make the second tile for entrance, because the outer wall is 2 tiles thick
                var entrancePosition2 = new Vector3Int(0, entrancePosition.y, 0);
                tilemap.SetTile(entrancePosition2, visitedCellTile);
            }
        }
        /// <summary>
        /// Finds a starting position for the generation. And sets its self.
        /// </summary>
        /// <returns>The chosen starting position.</returns>
        protected List<Vector3Int> FindAndSetStartPositions()
        {
            var numberOfStartPositions = Random.Range(2, 5); // Choose the number of starting positions (adjust as needed)
            var startPositions = new List<Vector3Int>();

            for (int i = 0; i < numberOfStartPositions; i++)
            {
                var availableCells = new List<Vector3Int>();

                // Adds every green tile
                for (int x = 1; x < width; x += 2)
                {
                    for (int y = 1; y < height; y += 2)
                    {
                        availableCells.Add(new Vector3Int(x, y, 0));
                    }
                }

                if (availableCells.Count != 0)
                {
                    var startPosition = GetRandomTile(availableCells);
                    startPositions.Add(startPosition);
                }
                else
                {
                    Debug.LogError("No available cells for starting position.");
                }
            }

            return startPositions;
        }    
        /// <summary>
        /// Checks if every tile is visited with the generation.
        /// </summary>
        /// <returns>If every tile is visited.</returns>
        protected bool AllTilesVisited()
        {
            for (int x = 1; x < width; x += 2)
            {
                for (int y = 1; y < height; y += 2)
                {
                    if (!VisitedCells[x, y]) return false; // At least one tile is unvisited
                }
            }

            return true; // All tiles are visited
        }

        /// <summary>
        /// Changes the tile in between currentTile and neighborTile to a color. And the currentTile gets changed.
        /// </summary>
        /// <param name="currentTile">The current tile, that gets also changed</param>
        /// <param name="neighborTile">The neighbor to look for the tile in between.</param>
        protected void UpdateTileColor(Vector3Int currentTile, Vector3Int neighborTile = default)
        {
            // Making the wallTile green
            if (neighborTile != default)
            {
                var wallTilePosition = currentTile + (neighborTile - currentTile) / 2;
                tilemap.SetTile(wallTilePosition, visitedCellTile);
            }

            // Update the tiles for the path
            tilemap.SetTile(currentTile, visitedCellTile);
            tilemap.SetTile(neighborTile, visitedCellTile);
        }
    
        protected List<Vector3Int> GetUnvisitedNeighbors(Vector3Int cell) => _directions.Select(direction => cell + direction).Where(neighbor => IsInsideMaze(neighbor) && !VisitedCells[neighbor.x, neighbor.y]).ToList();
    
        #endregion

        #region private functions
    
        /// <summary>
        /// Checks if the tile is inside the maze.
        /// </summary>
        /// <param name="tile">The tile to check for.</param>
        /// <returns>If it's in the maze or not.</returns>
        private bool IsInsideMaze(Vector3Int tile) => tile.x >= 0 && tile.x < width && tile.y >= 0 && tile.y < height;
    
        /// <summary>
        /// Looking at the neighbors of the entrance and exit if it's a valid position. Prevents spawning next to a wall.
        /// </summary>
        /// <param name="position">The preferred position.</param>
        /// <returns>If it's valid position to spawn.</returns>
        private bool IsValidEntranceExitPosition(Vector3Int position) => _directions.Select(direction => position + direction).Any(adjacentPosition => IsInsideMaze(adjacentPosition) && tilemap.GetTile<TileBase>(adjacentPosition) == visitedCellTile);
    
        /// <summary>
        /// Find a random tile that is empty in the tile map.
        /// </summary>
        /// <returns>The chosen random tile.</returns>
        private Vector3Int FindRandomEmptyTile()
        {
            var emptyCells = new List<Vector3Int>();

            // Adds every green tile (empty tile)
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (tilemap.GetTile<TileBase>(new Vector3Int(x, y, 0)) == visitedCellTile) emptyCells.Add(new Vector3Int(x, y, 0));
                }
            }

            if (emptyCells.Count != 0) return GetRandomTile(emptyCells);
        
            Debug.LogError("No empty cells found.");
            return Vector3Int.zero;
        }

        /// <summary>
        /// Get a random tile in the tilemap.
        /// </summary>
        /// <param name="tiles">The list of given tile to select a random tile form.</param>
        /// <returns>The chosen tile.</returns>
        private static Vector3Int GetRandomTile(IReadOnlyList<Vector3Int> tiles)
        {
            var randomIndex = Random.Range(0, tiles.Count);
            var startPosition = tiles[randomIndex];

            return startPosition;
        }

        #endregion
    
        #region UI functions

        public void ToggleInstantlyGeneration() => generateInstantly = !generateInstantly;
    
        private void UpdateUI()
        {
            textWidth.text = $"Set width: {width}";
            textHeight.text = $"Set height: {height}";
        }

        private void SetHeight(float value)
        {
            height = (int)value;
            UpdateUI();
        }

        private void SetWidth(float value)
        {
            width = (int)value;
            UpdateUI();
        }
        private void SetWaitTime(string input)
        {
            if (float.TryParse(input, out float parsedWaitTime))
            {
                // Check if the parsed wait time is within the allowed range
                waitTime = parsedWaitTime;
            }
        }
        #endregion
    }
}
