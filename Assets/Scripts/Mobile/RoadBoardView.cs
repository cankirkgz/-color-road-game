using System;
using System.Collections.Generic;
using RenkYolu.Campaign;
using RenkYolu.Grid;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RenkYolu.Mobile
{
    internal sealed class RoadBoardView
    {
        private sealed class CellView
        {
            public RoadTile Tile;
            public Image Bed;
            public Image Surface;
            public Image Marker;
            public Image Shard;
            public Text Label;
            public bool Revealed;
            public bool Collected;
        }

        private readonly RoadTheme theme;
        private readonly RoadBoard board;
        private readonly RectTransform root;
        private readonly RectTransform pathLayer;
        private readonly RectTransform effects;
        private readonly RoadCube cube;
        private readonly CellView[] cells;
        private readonly List<RectTransform> lines = new List<RectTransform>();
        private readonly float padding = 13;
        private readonly float pitch;
        private readonly float tileSize;
        private readonly float extent;

        internal RoadBoardView(RoadTheme theme, RoadBoard board, RectTransform parent, float x, float y,
            float extent, int skin, string startLabel, RoadGesture.OnGesture callback)
        {
            this.theme = theme;
            this.board = board;
            this.extent = extent;
            Image background = theme.Box(parent, "Tahta", x, y, extent, extent, RoadTheme.Hex(0x17142B), 24);
            background.raycastTarget = true;
            root = background.rectTransform;
            Image rim = theme.Box(root, "Çerçeve", 1, 1, extent - 2, extent - 2, RoadTheme.Rim, 23);
            theme.Box(rim.rectTransform, "Zemin", 1, 1, extent - 4, extent - 4, RoadTheme.Hex(0x211B37), 22);
            RoadGesture gesture = root.gameObject.AddComponent<RoadGesture>();
            gesture.Callback = callback;

            pitch = (extent - padding * 2) / board.Size;
            tileSize = pitch - (board.Size < 6 ? 8 : 6);
            cells = new CellView[board.Count];
            for (int cell = 0; cell < cells.Length; cell++)
                cells[cell] = AddCell(cell, startLabel);

            pathLayer = RoadTheme.Place(root, "Çizilen rota", 0, 0, extent, extent);
            cube = new RoadCube(theme, root, tileSize * .70f, skin);
            effects = RoadTheme.Place(root, "Efektler", 0, 0, extent, extent);
            for (int cell = 0; cell < cells.Length; cell++)
                Paint(cell, false);
            ShowCube(Center(RoadBoard.Start), 0, 0, 0, RoadTheme.ColorOf(board.StartColor));
        }

        internal float TileSize => tileSize;

        internal Vector2 Center(int cell) =>
            new Vector2(padding + (cell % board.Size + .5f) * pitch, padding + (board.Size - cell / board.Size - .5f) * pitch);

        internal Vector2 LocalPoint(Vector2 screen)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screen, null, out Vector2 point);
            return new Vector2(point.x, -point.y);
        }

        internal int CellAt(Vector2 point)
        {
            if (point.x < padding || point.y < padding || point.x >= extent - padding || point.y >= extent - padding)
                return -1;
            int x = (int)((point.x - padding) / pitch);
            int y = board.Size - 1 - (int)((point.y - padding) / pitch);
            int cell = y * board.Size + x;
            Vector2 delta = point - Center(cell);
            float half = tileSize * .46f;
            return Mathf.Abs(delta.x) <= half && Mathf.Abs(delta.y) <= half ? cell : -1;
        }

        internal void Hide()
        {
            for (int cell = 1; cell < cells.Length; cell++)
            {
                cells[cell].Revealed = false;
                Paint(cell, false);
            }
        }

        internal void Reveal(int cell, bool collected)
        {
            if (!board.Contains(cell))
                return;

            cells[cell].Revealed = true;
            cells[cell].Collected |= collected;
            Paint(cell, false);
        }

        internal void RevealAll()
        {
            for (int cell = 1; cell < cells.Length; cell++)
                Reveal(cell, false);
        }

        internal void DrawPath(IReadOnlyList<int> path, int offset = 0)
        {
            int lineCount = Mathf.Max(0, path.Count - offset - 1);
            for (int i = 0; i < cells.Length; i++)
                Paint(i, false);
            for (int i = offset; i < path.Count; i++)
                Paint(path[i], true);

            while (lines.Count < lineCount)
            {
                Image segment = theme.Box(pathLayer, "Adım", 0, 0, 0, 0, RoadTheme.Mint, 4);
                segment.rectTransform.pivot = new Vector2(.5f, .5f);
                lines.Add(segment.rectTransform);
            }

            for (int i = 0; i < lines.Count; i++)
            {
                RectTransform line = lines[i];
                line.gameObject.SetActive(i < lineCount);
                if (i >= lineCount)
                    continue;

                Vector2 from = Center(path[offset + i]);
                Vector2 to = Center(path[offset + i + 1]);
                Vector2 center = (from + to) / 2;
                float thickness = Mathf.Max(4, tileSize * .065f);
                float width = (to - from).magnitude + thickness;
                RoadTheme.Move(line, center.x, center.y, width, thickness);
                line.localRotation = Quaternion.Euler(0, 0, from.x == to.x ? 90 : 0);
            }
        }

        internal void ShowCube(Vector2 center, float jump, float squash, float tilt, Color color, bool surprised = false) =>
            cube.Draw(center, jump, squash, tilt, color, surprised);

        internal Image Spark(Vector2 center, float size, Color color) =>
            theme.Icon(effects, "spark", center.x - size / 2, center.y - size / 2, size, color);

        internal Text Floating(Vector2 center, string value, Color color)
        {
            Text text = theme.Text(effects, value, center.x - 90, center.y - tileSize * .7f, 180, 30,
                Mathf.RoundToInt(Mathf.Clamp(tileSize * .28f, 17, 26)), color, theme.Display, TextAnchor.MiddleCenter);
            Outline outline = text.gameObject.AddComponent<Outline>();
            outline.effectDistance = new Vector2(2, -2);
            outline.effectColor = new Color(.09f, .07f, .16f, .85f);
            return text;
        }

        internal static string OperationText(TileOperationType operation)
        {
            int value = TileOperationCalculator.DefaultValue(operation);
            return operation switch
            {
                TileOperationType.MultiplyTwo or TileOperationType.MultiplyThree => "×" + value,
                TileOperationType.DivideTwo => "÷" + value,
                _ => value < 0 ? "−" + Mathf.Abs(value) : "+" + value
            };
        }

        private CellView AddCell(int cell, string startLabel)
        {
            Vector2 center = Center(cell);
            float x = center.x - tileSize / 2;
            float y = center.y - tileSize / 2;
            int radius = board.Size < 6 ? 14 : 10;
            var view = new CellView
            {
                Tile = board[cell],
                Revealed = true,
                Bed = theme.Box(root, "Kare " + cell, x, y + 3, tileSize, tileSize, RoadTheme.Rim, radius),
                Surface = theme.Box(root, "Yüzey " + cell, x + 1, y, tileSize - 2, tileSize - 3, RoadTheme.Raised, radius)
            };
            RectTransform face = view.Surface.rectTransform;
            float mark = tileSize * .43f;
            string icon = view.Tile.Kind switch
            {
                TileSpecialType.Wall => "wall",
                TileSpecialType.Spike => "spike",
                TileSpecialType.Launcher => view.Tile.Direction.ToString().ToLowerInvariant(),
                _ => view.Tile.HasShard ? "diamond" : "dot"
            };
            view.Marker = theme.Icon(face, icon, (tileSize - mark) * .5f - 1, (tileSize - mark) * .5f - 1,
                mark, RoadTheme.Cream);
            float shard = tileSize * .25f;
            view.Shard = theme.Icon(face, "diamond", tileSize - shard - 6, 5, shard, RoadTheme.Cream);
            view.Label = theme.Text(face, cell == RoadBoard.Start ? startLabel : OperationText(view.Tile.Operation),
                0, cell == RoadBoard.Start ? tileSize * .76f : 0, tileSize - 2, cell == RoadBoard.Start ? tileSize * .18f : tileSize - 2,
                Mathf.RoundToInt(tileSize * (cell == RoadBoard.Start ? .13f : .34f)),
                RoadTheme.Ink, theme.Display, TextAnchor.MiddleCenter);
            return view;
        }

        private void Paint(int cell, bool selected)
        {
            CellView view = cells[cell];
            bool regular = view.Tile.IsRegular;
            bool hidden = regular && !view.Revealed;
            view.Label.gameObject.SetActive(regular && !hidden);
            view.Marker.gameObject.SetActive(!regular || hidden);
            view.Shard.gameObject.SetActive(view.Tile.HasShard && !hidden && !view.Collected);
            view.Shard.color = new Color(.15f, .08f, .26f, .70f);

            if (regular && !hidden)
                view.Surface.color = RoadTheme.ColorOf(view.Tile.Color);
            else if (hidden)
                view.Surface.color = selected ? RoadTheme.Hex(0x42675F) : RoadTheme.Hex(0x352D51);
            else
                view.Surface.color = view.Tile.Kind switch
                {
                    TileSpecialType.Spike => RoadTheme.Hex(0x563449),
                    TileSpecialType.Launcher => RoadTheme.Hex(0x4B3C6E),
                    _ => RoadTheme.Hex(0x29223E)
                };

            view.Bed.color = selected ? RoadTheme.Mint : Color.Lerp(view.Surface.color, RoadTheme.Background, .38f);
            view.Marker.color = view.Tile.Kind switch
            {
                TileSpecialType.Spike => RoadTheme.Pink,
                TileSpecialType.Launcher => RoadTheme.Hex(0xDDBAFF),
                TileSpecialType.Wall => RoadTheme.Hex(0x716388),
                _ => selected ? RoadTheme.Mint : view.Tile.HasShard ? RoadTheme.Hex(0x869D95) : RoadTheme.Hex(0x62557E)
            };
        }
    }

    internal sealed class RoadGesture : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler,
        IInitializePotentialDragHandler
    {
        internal delegate void OnGesture(int id, Vector2 position, GesturePhase phase);
        internal OnGesture Callback { private get; set; }

        public void OnPointerDown(PointerEventData eventData) => Dispatch(eventData, GesturePhase.Start);
        public void OnDrag(PointerEventData eventData) => Dispatch(eventData, GesturePhase.Move);
        public void OnPointerUp(PointerEventData eventData) => Dispatch(eventData, GesturePhase.End);
        public void OnInitializePotentialDrag(PointerEventData eventData) => eventData.useDragThreshold = false;

        private void Dispatch(PointerEventData eventData, GesturePhase phase)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                Callback?.Invoke(eventData.pointerId, eventData.position, phase);
        }
    }

    internal enum GesturePhase { Start, Move, End }
}
