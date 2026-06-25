using UnityEngine;

namespace RenkYolu.Grid
{
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int width = 4;
        [SerializeField] private int height = 4;
        [SerializeField] private float maxTileSize = 1f;
        [SerializeField] private float screenPadding = 0.8f;

        [Header("Tile Settings")]
        [SerializeField] private Tile tilePrefab;
        [Header("Start Tile Settings")]
        [SerializeField] private int startTileX = 0;
        [SerializeField] private int startTileY = 0;

        [Header("Random Operation Chances")]
        [SerializeField] private int positiveChance = 65;
        [SerializeField] private int negativeChance = 25;
        [SerializeField] private int multiplierChance = 10;

        private Tile[,] tiles;
        private float calculatedTileSize;
        private Tile startTile;

        public int Width => width;
        public int Height => height;
        public float TileSize => calculatedTileSize;
        public bool HasGenerated => tiles != null;
        public Tile StartTile => startTile;

        public Tile GetStartTile()
        {
            if (startTile != null)
            {
                return startTile;
            }

            return GetTileAt(startTileX, startTileY);
        }

        private void Start()
        {
            GenerateGrid();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                HideAllTiles();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                RevealAllTiles();
            }
        }

        public Tile GetTileAt(int x, int y)
        {
            if (tiles == null)
            {
                return null;
            }

            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return null;
            }

            return tiles[x, y];
        }

        public void HideAllTiles()
        {
            if (tiles == null)
            {
                return;
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (tiles[x, y] != null)
                    {
                        tiles[x, y].Hide();
                    }
                }
            }

            Debug.Log("All Tiles Hidden");
        }

        public void RevealAllTiles()
        {
            if (tiles == null)
            {
                return;
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (tiles[x, y] != null)
                    {
                        tiles[x, y].Reveal();
                    }
                }
            }

            Debug.Log("All Tiles Revealed");
        }

        private void GenerateGrid()
        {
            if (tilePrefab == null)
            {
                Debug.LogError("Tile Prefab is missing!");
                return;
            }

            tiles = new Tile[width, height];
            calculatedTileSize = CalculateTileSize();

            float gridWidth = (width - 1) * calculatedTileSize;
            float gridHeight = (height - 1) * calculatedTileSize;

            Vector3 gridOffset = new Vector3(
                -gridWidth / 2f,
                -gridHeight / 2f,
                0f
            );

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 spawnPosition = new Vector3(
                        x * calculatedTileSize,
                        y * calculatedTileSize,
                        0f
                    ) + gridOffset;

                    TileColorType randomColor = GetRandomColor();
                    TileOperationType randomOperation = GetControlledRandomOperation();

                    Tile tile = Instantiate(tilePrefab, spawnPosition, Quaternion.identity, transform);
                    tile.transform.localScale = Vector3.one * calculatedTileSize;

                    int tileId = y * width + x;

                    tile.Initialize(tileId, x, y, randomColor, randomOperation);

                    tiles[x, y] = tile;
                }
            }

            SetTileNeighbours();
            SetStartTile();

            Debug.Log($"Grid Generated | Width: {width}, Height: {height}, Tile Size: {calculatedTileSize}, Total Tiles: {width * height}");
        }

        private float CalculateTileSize()
        {
            Camera mainCamera = Camera.main;

            if (mainCamera == null || !mainCamera.orthographic)
            {
                return maxTileSize;
            }

            float screenHeight = mainCamera.orthographicSize * 2f;
            float screenWidth = screenHeight * mainCamera.aspect;

            float availableWidth = screenWidth - screenPadding;
            float availableHeight = screenHeight - screenPadding;

            float tileSizeByWidth = availableWidth / width;
            float tileSizeByHeight = availableHeight / height;

            return Mathf.Min(maxTileSize, tileSizeByWidth, tileSizeByHeight);
        }

        private TileColorType GetRandomColor()
        {
            int colorCount = System.Enum.GetValues(typeof(TileColorType)).Length;
            int randomIndex = Random.Range(0, colorCount);

            return (TileColorType)randomIndex;
        }

        private TileOperationType GetControlledRandomOperation()
        {
            int randomValue = Random.Range(0, 100);

            if (randomValue < positiveChance)
            {
                return GetRandomPositiveOperation();
            }

            if (randomValue < positiveChance + negativeChance)
            {
                return GetRandomNegativeOperation();
            }

            return GetRandomMultiplierOperation();
        }

        private TileOperationType GetRandomPositiveOperation()
        {
            TileOperationType[] positiveOperations =
            {
                TileOperationType.PlusOne,
                TileOperationType.PlusFive,
                TileOperationType.PlusTen
            };

            int randomIndex = Random.Range(0, positiveOperations.Length);

            return positiveOperations[randomIndex];
        }

        private TileOperationType GetRandomNegativeOperation()
        {
            TileOperationType[] negativeOperations =
            {
                TileOperationType.MinusFive,
                TileOperationType.MinusTen,
                TileOperationType.MinusFifteen
            };

            int randomIndex = Random.Range(0, negativeOperations.Length);

            return negativeOperations[randomIndex];
        }

        private TileOperationType GetRandomMultiplierOperation()
        {
            TileOperationType[] multiplierOperations =
            {
                TileOperationType.MultiplyTwo,
                TileOperationType.MultiplyThree,
                TileOperationType.DivideTwo
            };

            int randomIndex = Random.Range(0, multiplierOperations.Length);

            return multiplierOperations[randomIndex];
        }

        private void SetTileNeighbours()
        {
            if (tiles == null)
            {
                return;
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Tile currentTile = tiles[x, y];

                    if (currentTile == null)
                    {
                        continue;
                    }

                    Tile up = GetTileAt(x, y + 1);
                    Tile down = GetTileAt(x, y - 1);
                    Tile left = GetTileAt(x - 1, y);
                    Tile right = GetTileAt(x + 1, y);

                    currentTile.SetNeighbours(up, down, left, right);
                }
            }

            Debug.Log("Tile Neighbours Set");
        }

        private void SetStartTile()
        {
            startTile = GetTileAt(startTileX, startTileY);

            if (startTile == null)
            {
                Debug.LogError($"Start Tile could not be set! X: {startTileX}, Y: {startTileY}");
                return;
            }

            Debug.Log(
                $"Start Tile Set | " +
                $"Tile ID: {startTile.TileId} | " +
                $"X: {startTile.X}, Y: {startTile.Y} | " +
                $"Color: {startTile.ColorType}"
            );
        }
    }
}