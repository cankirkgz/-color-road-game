using System;
using System.Collections.Generic;
using RenkYolu.Grid;

namespace RenkYolu.Campaign
{
    public readonly struct RoadTile
    {
        public readonly TileColorType Color;
        public readonly TileOperationType Operation;
        public readonly TileSpecialType Kind;
        public readonly GridDirection Direction;
        public readonly bool HasShard;

        public RoadTile(
            TileColorType color, TileOperationType operation, TileSpecialType kind = TileSpecialType.None,
            GridDirection direction = GridDirection.None, bool hasShard = false)
        {
            Color = color;
            Operation = operation;
            Kind = kind;
            Direction = direction;
            HasShard = hasShard;
        }

        public bool Walkable => Kind != TileSpecialType.Wall;
        public bool IsRegular => Kind == TileSpecialType.None;
    }

    public sealed class RoadBoard
    {
        private static readonly GridDirection[] Directions =
        {
            GridDirection.Up, GridDirection.Right, GridDirection.Down, GridDirection.Left
        };

        private readonly RoadTile[] tiles;
        private readonly IReadOnlyList<int>[] neighbours;
        private readonly int[] landing;

        public RoadBoard(int size, RoadTile[] cells)
        {
            if (size < 2 || size > 7)
                throw new ArgumentOutOfRangeException(nameof(size));
            if (cells == null || cells.Length != size * size || !cells[0].IsRegular)
                throw new ArgumentException("The board requires a regular start tile and a full square grid.", nameof(cells));

            Size = size;
            tiles = (RoadTile[])cells.Clone();
            neighbours = new IReadOnlyList<int>[tiles.Length];
            landing = new int[tiles.Length];

            for (int cell = 0; cell < tiles.Length; cell++)
            {
                var adjacent = new List<int>(4);
                foreach (GridDirection direction in Directions)
                {
                    int next = NeighbourInDirection(cell, direction);
                    if (next >= 0 && tiles[next].Walkable)
                        adjacent.Add(next);
                }

                neighbours[cell] = Array.AsReadOnly(adjacent.ToArray());
                landing[cell] = NeighbourInDirection(cell, tiles[cell].Direction);
            }
        }

        public const int Start = 0;
        public int Size { get; }
        public int Count => tiles.Length;
        public TileColorType StartColor => tiles[Start].Color;
        public RoadTile this[int cell] => tiles[cell];

        public IReadOnlyList<int> Neighbours(int cell) => neighbours[cell];
        public bool Contains(int cell) => cell >= 0 && cell < tiles.Length;
        public bool Adjacent(int from, int to) =>
            Contains(from) && Contains(to) &&
            Math.Abs(from % Size - to % Size) + Math.Abs(from / Size - to / Size) == 1;

        public int LauncherLanding(int cell) => landing[cell];

        public bool CanFinish(int cell) =>
            cell != Start && tiles[cell].IsRegular && tiles[cell].Color == StartColor;

        private int NeighbourInDirection(int cell, GridDirection direction)
        {
            int x = cell % Size;
            int y = cell / Size;
            switch (direction)
            {
                case GridDirection.Up: y++; break;
                case GridDirection.Down: y--; break;
                case GridDirection.Right: x++; break;
                case GridDirection.Left: x--; break;
                default: return -1;
            }

            return x >= 0 && y >= 0 && x < Size && y < Size ? y * Size + x : -1;
        }
    }
}
