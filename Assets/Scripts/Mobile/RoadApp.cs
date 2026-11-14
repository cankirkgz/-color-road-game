using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using RenkYolu.Campaign;
using RenkYolu.Grid;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace RenkYolu.Mobile
{
    [DisallowMultipleComponent]
    public sealed partial class RoadApp : MonoBehaviour
    {
        private enum Phase { Home, Memorizing, Drawing, Walking, Result }

        private const string SaveKey = "RenkYolu.Campaign.1";
        private const float Width = 540;
        private const float Height = 900;
        private const float BoardExtent = 476;
        private const int NoPointer = int.MinValue;

        private readonly List<int> route = new List<int>();
        private RoadTheme theme;
        private RoadLanguage language;
        private CultureInfo numberCulture;
        private RoadAudio sounds;
        private RoadProgress progress;
        private RectTransform safeRoot;
        private Canvas canvas;
        private RectTransform surface;
        private RectTransform page;
        private RectTransform modal;
        private Rect lastSafeArea;
        private Vector2Int lastResolution;
        private RoadLevel level;
        private RoadBoardView boardView;
        private Phase phase;
        private float clock;
        private int displaySecond = -1;
        private int pointer = NoPointer;
        private bool startedOnEnd;
        private bool arrivalChangedRoute;
        private Vector2 lastPointer;
        private int currentCell;
        private Color cubePaint;
        private Text phaseTitle;
        private Text phaseNote;
        private Text countdown;
        private Text drawNote;
        private Text scoreValue;
        private Text shardValue;
        private Image timerFill;
        private Button confirmButton;
        private Button undoButton;
        private Coroutine walking;
        private RoadCube menuCube;
        private bool suspended;

        private RoadBoard Board => level.Board;
        private RoadDifficulty Difficulty => level.Difficulty;
        private int ContinueAt => progress.unlocked;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Application.runInBackground = false;
            UseLanguage(RoadLocale.Load());
            theme = new RoadTheme();
            progress = LoadProgress();
            sounds = new RoadAudio(gameObject) { Muted = progress.muted };
            var input = new GameObject("Giriş", typeof(EventSystem), typeof(InputSystemUIInputModule));
            input.transform.SetParent(transform, false);
            BuildCanvas();
            ShowHome();
        }

        private void Update()
        {
            FitCanvas();
            if (Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            {
                if (phase == Phase.Result)
                    ShowHome();
                else if (modal != null)
                    CloseModal();
                else if (phase != Phase.Home)
                    ShowPause();
            }

            if (suspended || modal != null)
                return;

            if (phase == Phase.Home && menuCube != null)
            {
                float wave = Mathf.Sin(Time.unscaledTime * 2.65f);
                menuCube.Draw(new Vector2(238, 143), 8 + Mathf.Abs(wave) * 13, -.035f * wave,
                    wave * 5, CubeColors[progress.cube]);
            }

            if (phase != Phase.Memorizing && phase != Phase.Drawing)
                return;

            clock = Mathf.Max(0, clock - Time.unscaledDeltaTime);
            UpdateTimer();
            if (clock > 0)
                return;

            if (phase == Phase.Memorizing)
                StartDrawing();
            else if (route.Count >= 2)
                ConfirmRoute();
            else
                ShowResult(RoadFailure.InvalidPath, 0);
        }

        private void OnApplicationPause(bool isPaused)
        {
            suspended = isPaused;
            pointer = NoPointer;
        }

        private void BuildCanvas()
        {
            var obj = new GameObject("Renk Yolu Canvas", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            obj.transform.SetParent(transform, false);
            canvas = obj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            RectTransform canvasRect = (RectTransform)obj.transform;
            Image fullBleed = theme.Box(canvasRect, "Arka plan", 0, 0, 0, 0, RoadTheme.Background, 0);
            Stretch(fullBleed.rectTransform);
            safeRoot = RoadTheme.Place(canvasRect, "Güvenli alan", 0, 0, 0, 0);
            surface = RoadTheme.Place(safeRoot, "Oyun ekranı", 0, 0, Width, Height);
            surface.anchorMin = surface.anchorMax = surface.pivot = new Vector2(.5f, .5f);
            surface.anchoredPosition = Vector2.zero;
            FitCanvas();
        }

        private void FitCanvas()
        {
            var resolution = new Vector2Int(Screen.width, Screen.height);
            Rect area = Screen.safeArea;
            if (area.width <= 1 || area.height <= 1)
                area = new Rect(0, 0, resolution.x, resolution.y);
            if ((area == lastSafeArea && resolution == lastResolution) || resolution.x < 2 || resolution.y < 2)
                return;

            lastSafeArea = area;
            lastResolution = resolution;
            safeRoot.anchorMin = new Vector2(area.xMin / resolution.x, area.yMin / resolution.y);
            safeRoot.anchorMax = new Vector2(area.xMax / resolution.x, area.yMax / resolution.y);
            safeRoot.offsetMin = safeRoot.offsetMax = Vector2.zero;
            float scale = Mathf.Min(area.width / Width, area.height / Height);
            canvas.scaleFactor = scale;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private void NewPage(string name)
        {
            CloseModal();
            if (walking != null)
            {
                StopCoroutine(walking);
                walking = null;
            }

            if (page != null)
            {
                page.gameObject.SetActive(false);
                Destroy(page.gameObject);
            }

            pointer = NoPointer;
            menuCube = null;
            page = RoadTheme.Place(surface, name, 0, 0, Width, Height);
        }

        private void StartLevel(int number)
        {
            if (number < 1 || number > progress.unlocked)
                return;

            if (level == null || level.Difficulty.Level != number)
                level = RoadGenerator.Generate(progress.seed, number);

            NewPage("Bölüm " + number);
            route.Clear();
            currentCell = RoadBoard.Start;
            cubePaint = RoadTheme.ColorOf(Board.StartColor);
            phase = Phase.Memorizing;
            CreateStage();
            if (number == 1 && progress.Find(1) == null)
                ShowTutorial();
        }

        private void CreateStage()
        {
            Label(page, Application.productName.ToUpperInvariant(), 32, 28, 220, 22, 18, RoadTheme.Muted, theme.Bold);
            Label(page, LevelTitle(Difficulty.Level), 32, 50, 280, 49, 36, RoadTheme.Cream, theme.Display);
            Image size = theme.Box(page, "Tahta boyutu", 342, 52, 86, 44, RoadTheme.Panel, 14);
            Label(size.rectTransform, Board.Size + " × " + Board.Size, 0, 0, 86, 44, 21,
                RoadTheme.Muted, theme.Bold, TextAnchor.MiddleCenter);
            theme.Button(page, "", 452, 48, 56, 50, RoadTheme.Panel, RoadTheme.Cream, ShowPause, "pause");

            Image dark = theme.Box(page, "Puan kartı", 32, 115, 476, 69, RoadTheme.Panel, 20);
            Label(dark.rectTransform, T("SCORE", "ОЧКИ"), 17, 11, 140, 17, 14, RoadTheme.Muted, theme.Bold);
            scoreValue = Label(dark.rectTransform, "0", 17, 27, 147, 32, 27, RoadTheme.Cream, theme.Display);
            FitNumber(scoreValue, 16);
            theme.Box(dark.rectTransform, "Ayraç", 169, 16, 1, 36, RoadTheme.Rim, 1);
            Label(dark.rectTransform, T("MAXIMUM", "МАКСИМУМ"), 185, 11, 170, 17, 14, RoadTheme.Muted, theme.Bold);
            Text maximum = Label(dark.rectTransform, Format(level.Solution.Maximum), 185, 27, 154, 32, 27,
                RoadTheme.Cream, theme.Display);
            FitNumber(maximum, 16);
            theme.Box(dark.rectTransform, "Ayraç", 349, 16, 1, 36, RoadTheme.Rim, 1);
            theme.Icon(dark.rectTransform, "diamond", 367, 24, 25, RoadTheme.Mint);
            shardValue = Label(dark.rectTransform, "0", 401, 14, 63, 40, 24, RoadTheme.Mint, theme.Display);

            theme.Box(page, "Aşama", 32, 201, 476, 82, RoadTheme.Raised, 18);
            phaseTitle = Label(page, "", 49, 208, 368, 31, 27, RoadTheme.Cream, theme.Display);
            phaseNote = Label(page, "", 49, 238, 369, 28, 16, RoadTheme.Muted, theme.Body);
            countdown = Label(page, "", 444, 211, 48, 48, 30, RoadTheme.Amber, theme.Display, TextAnchor.MiddleCenter);
            theme.Box(page, "Sayaç çizgisi", 49, 269, 442, 4, RoadTheme.Rim, 2);
            timerFill = theme.Box(page, "Kalan süre", 49, 269, 442, 4, RoadTheme.Amber, 2);
            timerFill.type = Image.Type.Filled;
            timerFill.fillMethod = Image.FillMethod.Horizontal;

            boardView = new RoadBoardView(theme, Board, page, 32, 299, BoardExtent, progress.cube, T("START", "СТАРТ"), HandleGesture);
            drawNote = Label(page, "", 32, 779, 476, 34, 18, RoadTheme.Muted, theme.Body, TextAnchor.MiddleCenter);
            undoButton = theme.Button(page, "", 32, 820, 74, 60, RoadTheme.Raised, RoadTheme.Cream, Undo, "undo");
            confirmButton = theme.Button(page, T("Confirm Route", "Подтвердить маршрут"), 120, 820, 388, 60, RoadTheme.Mint, RoadTheme.Ink, ConfirmRoute, "right");
            SetPhase(Phase.Memorizing, Difficulty.MemorySeconds, T("Memorize the board", "Запомни поле"),
                T("Remember your starting color and the tile values.", "Запомни цвет старта и значения клеток."),
                T("Colors and values will disappear soon.", "Скоро цвета и значения исчезнут."), RoadTheme.Amber);
        }

        private void StartDrawing()
        {
            boardView.Hide();
            string note = Difficulty.Level switch
            {
                1 => T("Drag from Start to a tile of the same color.", "Проведи путь от старта до клетки того же цвета."),
                6 => T("Striped walls block your route.", "Полосатые стены перекрывают путь."),
                8 => T("Arrows launch the cube one tile in their direction.", "Стрелка перебрасывает куб на соседнюю клетку."),
                12 => T("Spikes cost 50 points and break your combo.", "Шипы отнимают 50 очков и сбрасывают комбо."),
                _ => T("Start at the bottom left and finish on your color.", "Начни снизу слева и финишируй на своём цвете.")
            };
            SetPhase(Phase.Drawing, Difficulty.DrawingSeconds, T("Draw your route", "Нарисуй маршрут"),
                T("Connect tiles horizontally or vertically.", "Соединяй клетки по горизонтали или вертикали."), note, RoadTheme.Mint);
        }

        private void SetPhase(Phase next, float time, string title, string note, string tip, Color accent)
        {
            phase = next;
            clock = time;
            displaySecond = -1;
            pointer = NoPointer;
            phaseTitle.text = title;
            phaseNote.text = note;
            drawNote.text = tip;
            countdown.color = accent;
            timerFill.color = accent;
            UpdateTimer();
            UpdateButtons();
        }

        private void UpdateTimer()
        {
            bool timed = phase == Phase.Memorizing || phase == Phase.Drawing;
            timerFill.gameObject.SetActive(timed);
            if (!timed)
            {
                countdown.text = "";
                return;
            }

            int seconds = Mathf.CeilToInt(clock);
            if (seconds != displaySecond)
            {
                displaySecond = seconds;
                countdown.text = Format(seconds);
                if (phase == Phase.Drawing)
                    countdown.color = seconds <= 5 ? (Color)RoadTheme.Pink : RoadTheme.Mint;
            }

            int duration = phase == Phase.Memorizing ? Difficulty.MemorySeconds : Difficulty.DrawingSeconds;
            timerFill.fillAmount = clock / duration;
        }

        private void UpdateButtons()
        {
            confirmButton.interactable = phase == Phase.Drawing && route.Count >= 2;
            undoButton.interactable = phase == Phase.Drawing && route.Count > 0;
        }

        private void HandleGesture(int id, Vector2 screen, GesturePhase gesture)
        {
            if (phase != Phase.Drawing || modal != null)
                return;

            Vector2 point = boardView.LocalPoint(screen);
            if (gesture == GesturePhase.Start)
            {
                if (pointer != NoPointer)
                    return;

                int cell = boardView.CellAt(point);
                if (cell < 0)
                    return;
                if (!Board[cell].Walkable)
                {
                    SpawnFeedback(cell, T("WALL", "СТЕНА"), RoadTheme.Pink, false);
                    return;
                }
                if (route.Count == 0 && cell != RoadBoard.Start)
                    return;

                pointer = id;
                lastPointer = point;
                arrivalChangedRoute = false;
                startedOnEnd = route.Count > 0 && route[route.Count - 1] == cell;
                VisitCell(cell);
                return;
            }

            if (pointer != id)
                return;

            // Sample the drag segment so a quick swipe still visits the squares it crossed.
            int samples = Mathf.Clamp(Mathf.CeilToInt(Vector2.Distance(lastPointer, point) / (boardView.TileSize * .32f)), 1, 120);
            for (int i = 1; i <= samples; i++)
                VisitCell(boardView.CellAt(Vector2.Lerp(lastPointer, point, i / (float)samples)), true);
            lastPointer = point;
            if (gesture == GesturePhase.End)
            {
                if (startedOnEnd && !arrivalChangedRoute && route.Count > 0 &&
                    boardView.CellAt(point) == route[route.Count - 1])
                    Undo();

                pointer = NoPointer;
            }
        }

        private void VisitCell(int cell, bool dragging = false)
        {
            if (cell < 0 || !Board[cell].Walkable)
                return;

            int last = route.Count > 0 ? route[route.Count - 1] : -1;
            if (cell == last)
                return;

            int seen = route.IndexOf(cell);
            if (dragging && last >= 0 && !Board.Adjacent(last, cell))
                return;

            if (seen >= 0)
                route.RemoveRange(seen + 1, route.Count - seen - 1);
            else if (RoadRules.CanExtend(Board, last, cell, 0))
                route.Add(cell);
            else
                return;

            arrivalChangedRoute = true;
            PaintRoute();
            sounds.Select();
        }

        private void Undo()
        {
            if (phase != Phase.Drawing || route.Count == 0)
                return;

            route.RemoveAt(route.Count - 1);
            PaintRoute();
            sounds.Select();
        }

        private void PaintRoute()
        {
            boardView.DrawPath(route);
            UpdateButtons();
        }

        private void ConfirmRoute()
        {
            if (phase != Phase.Drawing || route.Count < 2 || modal != null)
                return;

            RoadRun result = RoadRules.Play(Board, route);
            SetPhase(Phase.Walking, 0, T("Cube on the move", "Куб в пути"),
                T("Your score changes as tiles are revealed.", "Очки меняются по мере открытия клеток."),
                T("Tile values apply in route order.", "Значения клеток применяются по порядку."), RoadTheme.Mint);
            walking = StartCoroutine(AnimateWalk(result));
        }

        private IEnumerator AnimateWalk(RoadRun run)
        {
            int walked = 0;
            int shards = 0;
            long lastScore = 0;
            foreach (RoadStep step in run.Steps)
            {
                if (step.Kind == RoadStepKind.Fall)
                {
                    Vector2 from = boardView.Center(currentCell);
                    Vector2 to = step.Cell >= 0 ? boardView.Center(step.Cell) : from + FallDirection();
                    yield return Jump(from, to, cubePaint, RoadTheme.Pink, true, true);
                    break;
                }

                if (step.Kind == RoadStepKind.ShortcutBonus)
                {
                    scoreValue.text = Format(step.Score);
                    lastScore = step.Score;
                    SpawnFeedback(step.Cell, T("+50 SHORTCUT", "+50 ЗА ПРЫЖОК"), RoadTheme.Mint, true);
                    continue;
                }

                RoadTile tile = Board[step.Cell];
                bool skipped = step.Kind == RoadStepKind.Skipped;
                if (!skipped && (walked > 0 || step.Cell != RoadBoard.Start))
                {
                    Color next = tile.IsRegular ? RoadTheme.ColorOf(tile.Color) : tile.Kind == TileSpecialType.Spike
                        ? RoadTheme.Pink : RoadTheme.Hex(0xDDBAFF);
                    bool launch = step.Kind == RoadStepKind.Launch;
                    sounds.Hop();
                    yield return Jump(boardView.Center(currentCell), boardView.Center(step.Cell), cubePaint,
                        next, launch, false);
                    currentCell = step.Cell;
                    cubePaint = next;
                }

                boardView.Reveal(step.Cell, step.PickedShard);
                if (step.PickedShard)
                {
                    shardValue.text = Format(++shards);
                    sounds.Shard();
                }

                string message = step.ColorBonus ? T("COLOR ×2", "ЦВЕТ ×2") :
                    tile.Kind == TileSpecialType.Spike ? "−50" :
                    step.Combo >= 2 && !skipped ? T("COMBO ×", "КОМБО ×") + Format(step.Combo) :
                    step.Score != lastScore && step.Cell != RoadBoard.Start ? RoadBoardView.OperationText(tile.Operation) : "";
                if (message.Length > 0 || step.PickedShard)
                {
                    if (step.PickedShard && message.Length == 0)
                        message = T("+1 CRYSTAL", "+1 КРИСТАЛЛ");
                    SpawnFeedback(step.Cell, message,
                        tile.Kind == TileSpecialType.Spike ? RoadTheme.Pink : RoadTheme.Mint,
                        step.PickedShard || step.ColorBonus || tile.Kind == TileSpecialType.Spike);
                }

                scoreValue.text = Format(step.Score);
                lastScore = step.Score;
                if (walked < route.Count && step.Cell == route[walked])
                    walked++;
                boardView.DrawPath(route, Mathf.Max(0, walked - 1));
            }

            boardView.DrawPath(Array.Empty<int>());
            boardView.RevealAll();
            yield return PauseableDelay(.42f);
            walking = null;
            ShowResult(run.Failure, run.Score, run.ShardMask);
        }

        private Vector2 FallDirection()
        {
            Vector2 direction = Board[currentCell].Direction switch
            {
                GridDirection.Up => Vector2.down,
                GridDirection.Down => Vector2.up,
                GridDirection.Left => Vector2.left,
                _ => Vector2.right
            };
            return direction * boardView.TileSize * .7f;
        }

        private IEnumerator Jump(Vector2 from, Vector2 to, Color start, Color end, bool launch, bool fall)
        {
            float duration = launch ? .28f : .185f;
            float time = 0;
            float direction = Mathf.Sign(to.x - from.x);
            while (time < duration)
            {
                if (!suspended && modal == null)
                    time += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(time / duration);
                float arc = Mathf.Sin(t * Mathf.PI);
                float rise = arc * boardView.TileSize * (launch ? .58f : .31f);
                if (fall)
                    rise -= Mathf.Pow(t, 4) * boardView.TileSize * .40f;
                float squash = t > .76f ? Mathf.Sin((t - .76f) / .24f * Mathf.PI) * .18f : -arc * .16f;
                Color color = Color.Lerp(start, end, Mathf.SmoothStep(0, 1, (t - .24f) / .65f));
                boardView.ShowCube(Vector2.Lerp(from, to, t), rise, squash, -direction * arc * (launch ? 28 : 13),
                    color, launch || fall);
                yield return null;
            }

            boardView.ShowCube(to, fall ? -boardView.TileSize * .40f : 0, 0, 0, end, fall);
        }

        private IEnumerator PauseableDelay(float duration)
        {
            while (duration > 0)
            {
                if (!suspended && modal == null)
                    duration -= Time.unscaledDeltaTime;
                yield return null;
            }
        }

        private void SpawnFeedback(int cell, string message, Color color, bool showSparks)
        {
            Vector2 center = boardView.Center(cell);
            Text popup = boardView.Floating(center, message, color);
            StartCoroutine(Dissolve(popup, new Vector2(0, 28), .46f));
            if (!showSparks)
                return;

            for (int i = 0; i < 4; i++)
            {
                float angle = (i * 90 + 45) * Mathf.Deg2Rad;
                var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Image spark = boardView.Spark(center + direction * 17, 11 + i * 2, color);
                StartCoroutine(Dissolve(spark, direction * boardView.TileSize * .36f, .38f));
            }
        }

        private IEnumerator Dissolve(Graphic graphic, Vector2 offset, float duration)
        {
            Vector2 origin = graphic.rectTransform.anchoredPosition;
            Color color = graphic.color;
            float timer = 0;
            while (graphic != null && timer < duration)
            {
                if (modal == null && !suspended)
                    timer += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(timer / duration);
                graphic.rectTransform.anchoredPosition = origin + offset * t;
                graphic.color = new Color(color.r, color.g, color.b, 1 - t * t);
                yield return null;
            }

            if (graphic != null)
                Destroy(graphic.gameObject);
        }

        private static RoadProgress LoadProgress()
        {
            var fresh = new RoadProgress(Guid.NewGuid().GetHashCode());
            if (PlayerPrefs.HasKey(SaveKey))
            {
                try
                {
                    RoadProgress save = JsonUtility.FromJson<RoadProgress>(PlayerPrefs.GetString(SaveKey));
                    if (save != null && save.levels != null && save.unlocked > 0 &&
                        save.cube >= 0 && save.cube < save.AvailableCubes)
                    {
                        if (save.RefreshEarlyStars())
                            WriteProgress(save);
                        return save;
                    }
                }
                catch (ArgumentException)
                {
                    // A damaged save starts a new campaign using the existing install's legacy progress, if any.
                }
            }

            fresh.unlocked = Mathf.Clamp(PlayerPrefs.GetInt("UnlockedLevel", 1), 1, 20);
            fresh.legacyScore = Math.Max(0, PlayerPrefs.GetInt("TotalScore", 0));
            WriteProgress(fresh);
            return fresh;
        }

        private void SaveProgress() => WriteProgress(progress);

        private static void WriteProgress(RoadProgress save)
        {
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(save));
            PlayerPrefs.Save();
        }

        private Text Label(RectTransform parent, string value, float x, float y, float w, float h,
            int size, Color color, Font font, TextAnchor anchor = TextAnchor.MiddleLeft) =>
            theme.Text(parent, value, x, y, w, h, size, color, font, anchor);

        private void UseLanguage(RoadLanguage selected)
        {
            language = selected;
            numberCulture = CultureInfo.GetCultureInfo(T("en-US", "ru-RU"));
        }

        private string T(string english, string russian) => language == RoadLanguage.Russian ? russian : english;
        private string Format(long number) => number.ToString("#,0", numberCulture);
        private string LevelTitle(int number) => T("Level ", "Уровень ") + Format(number);

        private static void FitNumber(Text label, int smallest)
        {
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = smallest;
            label.resizeTextMaxSize = label.fontSize;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
        }
    }
}
