using System;
using RenkYolu.Campaign;
using UnityEngine;
using UnityEngine.UI;

namespace RenkYolu.Mobile
{
    public sealed partial class RoadApp
    {
        private string CubeName(int style) => style switch
        {
            1 => T("Mischief", "Проказник"),
            2 => T("Starlight", "Звёздный"),
            3 => T("Fluffy", "Пушистик"),
            _ => T("Classic", "Классика")
        };
        private static readonly Color[] CubeColors =
        {
            RoadTheme.Amber, RoadTheme.Mint, RoadTheme.Hex(0xCDB2FD), RoadTheme.Pink
        };

        private void ShowHome()
        {
            NewPage("Ana menü");
            phase = Phase.Home;
            Label(page, Application.productName.ToUpperInvariant(), 33, 32, 260, 24, 18, RoadTheme.Cream, theme.Bold);
            theme.Icon(page, "spark", 481, 32, 27, RoadTheme.Amber);
            Image hero = theme.Box(page, "Ana menü kartı", 32, 80, 476, 450, RoadTheme.Hex(0x352751), 32);
            RectTransform card = hero.rectTransform;
            theme.Box(card, "İç zemin", 2, 2, 472, 446, RoadTheme.Hex(0x261E3F), 30);
            DecorativeTile(card, 90, 155, 72, -13, RoadTheme.Hex(0x574678), .87f);
            DecorativeTile(card, 324, 84, 70, 16, RoadTheme.Hex(0x4C3D71), .95f);
            DecorativeTile(card, 167, 169, 79, 5, RoadTheme.Hex(0x514073), 1);
            DecorativeTile(card, 274, 168, 71, 10, RoadTheme.Hex(0x4C655E), .90f);
            theme.Icon(card, "spark", 103, 78, 31, RoadTheme.Hex(0xD9ACEC));
            theme.Icon(card, "spark", 375, 178, 28, RoadTheme.Amber);
            theme.Icon(card, "diamond", 162, 54, 23, RoadTheme.Mint);
            theme.Icon(card, "spark", 312, 48, 14, RoadTheme.Cream);
            theme.Icon(card, "spark", 79, 122, 11, RoadTheme.Muted);
            menuCube = new RoadCube(theme, card, 112, progress.cube);
            menuCube.Draw(new Vector2(238, 143), 10, 0, 0, CubeColors[progress.cube]);
            Label(card, Application.productName.Replace(' ', '\n'), 35, 241, 405, 131, 66, RoadTheme.Cream, theme.Display,
                TextAnchor.MiddleCenter).lineSpacing = .78f;
            Label(card, T("Remember the color. Draw the route.", "Запомни цвет. Нарисуй маршрут."), 24, 387, 428, 27, 22, RoadTheme.Muted,
                theme.Body, TextAnchor.MiddleCenter);

            bool first = ContinueAt == 1;
            theme.Button(page, first ? T("Play", "Играть") : T("Continue to Level ", "Продолжить с уровня ") + Format(ContinueAt),
                32, 553, 476, 69, RoadTheme.Mint, RoadTheme.Ink, () => StartLevel(ContinueAt), "right");
            theme.Button(page, T("Levels", "Уровни"), 32, 639, 231, 55, RoadTheme.Raised, RoadTheme.Cream, () => ShowLevels(0));
            theme.Button(page, T("Cubes", "Кубы"), 277, 639, 231, 55, RoadTheme.Raised, RoadTheme.Cream, ShowCubes);
            Image stats = theme.Box(page, "İlerleme", 32, 715, 476, 94, RoadTheme.Panel, 20);
            HomeStat(stats.rectTransform, 17, "dot", Format(progress.TotalScore), T("Total score", "Всего очков"), RoadTheme.Cream);
            HomeStat(stats.rectTransform, 174, "star", Format(progress.TotalStars), T("Stars", "Звёзды"), RoadTheme.Amber);
            HomeStat(stats.rectTransform, 323, "diamond", Format(progress.TotalShards), T("Crystals", "Кристаллы"), RoadTheme.Mint);
            theme.Button(page, T("Settings", "Настройки"), 277, 833, 231, 48, RoadTheme.Panel, RoadTheme.Muted, ShowSettings);
            theme.Button(page, T("How to Play", "Как играть"), 32, 833, 231, 48, RoadTheme.Panel, RoadTheme.Muted, ShowTutorial);
        }

        private void ShowSettings()
        {
            string title = T("Settings", "Настройки");
            RectTransform card = OpenModal(title, 418);
            Label(card, title, 27, 24, 350, 53, 42, RoadTheme.Cream, theme.Display);
            Label(card, T("Interface language", "Язык интерфейса"), 28, 99, 411, 35, 22, RoadTheme.Muted, theme.Bold);
            LanguageOption(RoadLanguage.English, "English", 147);
            LanguageOption(RoadLanguage.Russian, "Русский", 214);
            UpdateSoundButton(card, 28, 330, 412);

            void LanguageOption(RoadLanguage option, string name, float y)
            {
                bool selected = option == language;
                Button button = theme.Button(card, name, 28, y, 412, 56,
                    selected ? RoadTheme.Mint : RoadTheme.Raised, selected ? RoadTheme.Ink : RoadTheme.Cream, () =>
                    {
                        if (option == language)
                            return;

                        RoadLocale.Save(option);
                        UseLanguage(option);
                        ShowHome();
                        ShowSettings();
                    });
                if (selected)
                    theme.Icon((RectTransform)button.transform, "diamond", 378, 18, 20, RoadTheme.Ink);
            }
        }

        private void DecorativeTile(RectTransform parent, float x, float y, float size, float angle, Color color, float opacity)
        {
            color.a = opacity;
            Image tile = theme.Box(parent, "Kare", x, y, size, size, color, 17);
            tile.rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
            theme.Box(tile.rectTransform, "Kenar", 5, 5, size - 10, size - 10,
                new Color(1, 1, 1, .065f), 13);
        }

        private void HomeStat(RectTransform root, int x, string icon, string value, string name, Color color)
        {
            theme.Icon(root, icon, x, 17, 23, color);
            Text number = Label(root, value, x + 29, 13, 107, 30, 25, color, theme.Display);
            FitNumber(number, 15);
            Label(root, name, x, 49, 131, 25, 16, RoadTheme.Muted, theme.Body);
        }

        private void ShowTutorial()
        {
            string title = T("How to Play", "Как играть");
            RectTransform card = OpenModal(title, 585);
            Label(card, title, 27, 23, 350, 48, 39, RoadTheme.Cream, theme.Display);
            TutorialRow(card, 90, "01", T("Memorize", "Запомни"), T("Study the colors and tile values.", "Запомни цвета и значения клеток."), RoadTheme.Amber);
            TutorialRow(card, 181, "02", T("Draw a route", "Нарисуй маршрут"),
                T("Start at the bottom left. Drag across neighboring tiles, horizontally or vertically.",
                    "Начни снизу слева. Веди пальцем по соседним клеткам по горизонтали или вертикали."), RoadTheme.Mint);
            TutorialRow(card, 288, "03", T("Match your color", "Финишируй на своём цвете"),
                T("Finish on your starting color to win. Each tile of that color doubles your score.",
                    "Закончи на клетке цвета старта. Каждая клетка этого цвета удваивает счёт."),
                RoadTheme.Pink);
            theme.Icon(card, "diamond", 32, 436, 28, RoadTheme.Mint);
            Label(card, T("Collect crystals from marked tiles to unlock cubes.", "Собирай кристаллы на отмеченных клетках, чтобы открыть новые кубы."),
                76, 416, 367, 62, 20, RoadTheme.Muted, theme.Body);
            theme.Button(card, phase == Phase.Memorizing ? T("Ready", "Начать") : T("Got It", "Понятно"), 27, 496, 414, 60,
                RoadTheme.Mint, RoadTheme.Ink, CloseModal);
        }

        private void TutorialRow(RectTransform parent, float y, string number, string title, string description, Color accent)
        {
            Image badge = theme.Box(parent, title, 27, y + 5, 49, 48, accent, 13);
            Label(badge.rectTransform, number, 0, 0, 49, 48, 19, RoadTheme.Ink, theme.Display, TextAnchor.MiddleCenter);
            Label(parent, title, 89, y, 348, 31, 23, RoadTheme.Cream, theme.Display);
            Label(parent, description, 89, y + 31, 348, 68, 19, RoadTheme.Muted, theme.Body, TextAnchor.UpperLeft);
        }

        private void ShowPause()
        {
            if (phase == Phase.Home || phase == Phase.Result)
                return;

            string title = T("Paused", "Пауза");
            RectTransform card = OpenModal(title, 437);
            Label(card, title, 28, 25, 350, 51, 41, RoadTheme.Cream, theme.Display);
            Label(card, LevelTitle(Difficulty.Level), 30, 79, 410, 32, 20, RoadTheme.Muted, theme.Body);
            theme.Button(card, T("Resume", "Продолжить"), 28, 126, 412, 62, RoadTheme.Mint, RoadTheme.Ink, CloseModal);
            theme.Button(card, T("Restart Level", "Начать заново"), 28, 202, 412, 54, RoadTheme.Raised,
                RoadTheme.Cream, () => StartLevel(Difficulty.Level));
            theme.Button(card, T("Main Menu", "Главное меню"), 28, 268, 412, 54, RoadTheme.Raised, RoadTheme.Cream, ShowHome);
            UpdateSoundButton(card, 28, 344, 412);
        }

        private void UpdateSoundButton(RectTransform parent, float x, float y, float width)
        {
            Button button = theme.Button(parent, SoundLabel(), x, y, width, 48, RoadTheme.Panel, RoadTheme.Muted, null);
            Text label = button.GetComponentInChildren<Text>();
            button.onClick.AddListener(() =>
            {
                progress.muted = !progress.muted;
                sounds.Muted = progress.muted;
                label.text = SoundLabel();
                SaveProgress();
                sounds.Select();
            });
        }

        private string SoundLabel() => progress.muted ? T("Sound Off", "Звук выключен") : T("Sound On", "Звук включён");

        private void ShowLevels(int batch)
        {
            int lastBatch = (progress.unlocked - 1) / 12;
            if (batch < 0)
                batch = lastBatch;
            int visibleLastBatch = Mathf.Min(lastBatch + 1, (int.MaxValue - 1) / 12);
            batch = Mathf.Clamp(batch, 0, visibleLastBatch);
            string title = T("Levels", "Уровни");
            RectTransform card = OpenModal(title, 682);
            Label(card, title, 25, 27, 360, 53, 43, RoadTheme.Cream, theme.Display);
            long first = (long)batch * 12 + 1;
            Label(card, Format(first) + "–" + Format(first + 11), 28, 84, 380, 28, 19, RoadTheme.Muted, theme.Body);

            for (int index = 0; index < 12; index++)
            {
                long position = first + index;
                int number = (int)Math.Min(position, int.MaxValue);
                bool active = position <= progress.unlocked;
                bool current = position == progress.unlocked;
                RoadLevelRecord record = active ? progress.Find(number) : null;
                float x = 25 + index % 3 * 143;
                float y = 128 + index / 3 * 105;
                Button tile = theme.Button(card, "", x, y, 131, 92,
                    current ? RoadTheme.Mint : RoadTheme.Raised, RoadTheme.Ink, () => StartLevel(number));
                tile.interactable = active;
                RectTransform box = (RectTransform)tile.transform;
                Label(box, Format(position), 3, 9, 125, 39, 29,
                    current ? RoadTheme.Ink : active ? RoadTheme.Cream : RoadTheme.Muted, theme.Display, TextAnchor.MiddleCenter);
                if (record != null)
                    DrawStars(box, 30, 53, 23, record.stars);
                else
                    Label(box, active ? T("Up next", "Следующий") : T("Locked", "Закрыто"), 0, 49, 131, 30, 15,
                        current ? RoadTheme.Ink : RoadTheme.Muted, theme.Body, TextAnchor.MiddleCenter);
            }

            Button previous = theme.Button(card, T("Previous", "Назад"), 25, 581, 196, 59, RoadTheme.Raised,
                RoadTheme.Cream, () => ShowLevels(batch - 1), "left");
            Button next = theme.Button(card, T("Next", "Далее"), 247, 581, 196, 59, RoadTheme.Raised,
                RoadTheme.Cream, () => ShowLevels(batch + 1), "right");
            previous.interactable = batch > 0;
            next.interactable = batch < visibleLastBatch;
        }

        private void ShowCubes()
        {
            string title = T("Cubes", "Кубы");
            RectTransform card = OpenModal(title, 673);
            Label(card, title, 25, 27, 360, 52, 43, RoadTheme.Cream, theme.Display);
            theme.Icon(card, "diamond", 28, 91, 22, RoadTheme.Mint);
            Label(card, CrystalCount(progress.TotalShards), 59, 85, 330, 32, 22, RoadTheme.Mint, theme.Bold);
            for (int index = 0; index < RoadProgress.CubeCount; index++)
            {
                int style = index;
                bool owned = style < progress.AvailableCubes;
                bool selected = style == progress.cube;
                float x = 25 + style % 2 * 214;
                float y = 136 + style / 2 * 211;
                Button picker = theme.Button(card, "", x, y, 204, 196,
                    selected ? RoadTheme.Hex(0x405864) : RoadTheme.Raised, RoadTheme.Cream, () =>
                    {
                        progress.cube = style;
                        SaveProgress();
                        ShowHome();
                        ShowCubes();
                    });
                picker.interactable = owned;
                RectTransform box = (RectTransform)picker.transform;
                new RoadCube(theme, box, 70, style).Draw(new Vector2(102, 76), 0, 0, 0, CubeColors[style]);
                Label(box, CubeName(style), 0, 115, 204, 30, 24, RoadTheme.Cream, theme.Display, TextAnchor.MiddleCenter);
                string note = selected ? T("Selected", "Выбран") : owned ? T("Select", "Выбрать") :
                    CrystalCount(style * RoadProgress.ShardsPerCube);
                Label(box, note, 0, 150, 204, 29, 18, selected ? RoadTheme.Mint : RoadTheme.Muted,
                    theme.Body, TextAnchor.MiddleCenter);
            }

            Label(card, T("Finish a level to keep the crystals you collect.", "Заверши уровень, чтобы сохранить собранные кристаллы."),
                24, 571, 420, 55, 18, RoadTheme.Muted, theme.Body, TextAnchor.MiddleCenter);
        }

        private void ShowResult(RoadFailure failure, long score, long shardMask = 0)
        {
            phase = Phase.Result;
            pointer = NoPointer;
            UpdateButtons();
            bool won = failure == RoadFailure.None;
            int stars = won ? level.Stars(score) : 0;
            int earned = 0;
            int number = Difficulty.Level;
            boardView.RevealAll();
            boardView.DrawPath(Array.Empty<int>());
            if (won)
            {
                int old = progress.TotalShards;
                progress.RecordWin(number, score, stars, shardMask);
                earned = progress.TotalShards - old;
                SaveProgress();
            }

            sounds.Result(won);
            RectTransform card = OpenModal("Bölüm sonucu", 600, false);
            Color accent = won ? RoadTheme.Mint : RoadTheme.Pink;
            theme.Icon(card, won ? "spark" : "undo", 204, 32, 60, accent);
            Label(card, won ? T("Level complete!", "Уровень пройден!") : T("Try again", "Попробуй снова"), 22, 105, 424, 48, 41,
                RoadTheme.Cream, theme.Display, TextAnchor.MiddleCenter);
            if (won)
                DrawStars(card, 131, 168, 64, stars);
            else
                Label(card, FailureText(failure), 31, 163, 406, 65, 22, RoadTheme.Muted, theme.Body, TextAnchor.MiddleCenter);

            Image dark = theme.Box(card, "Sonuç puanları", 28, 246, 412, 119, RoadTheme.Background, 18);
            RectTransform info = dark.rectTransform;
            ResultStat(info, T("SCORE", "ОЧКИ"), Format(score), 12, 182, RoadTheme.Cream);
            theme.Box(info, "Ayraç", 206, 22, 1, 75, RoadTheme.Rim, 1);
            ResultStat(info, T("MAXIMUM", "МАКСИМУМ"), Format(level.Solution.Maximum), 218, 182, RoadTheme.Amber);

            string note = won && stars < 3 ? string.Format(T("{0} points for 3 stars", "До трёх звёзд нужно набрать {0}"), Format(level.ThreeStarScore)) :
                won && earned > 0 ? "+" + CrystalCount(earned) :
                won ? T("Total score  ", "Всего очков  ") + Format(progress.TotalScore) :
                failure == RoadFailure.InvalidPath ? T("Connect at least two tiles before time runs out.", "Соедини хотя бы две клетки до конца отсчёта.") :
                T("Try a different route next time.", "Попробуй проложить другой маршрут.");
            Label(card, note, 26, 383, 416, 41, 19, won ? RoadTheme.Mint : RoadTheme.Muted,
                theme.Bold, TextAnchor.MiddleCenter);

            bool canAdvance = won && number < int.MaxValue;
            theme.Button(card, canAdvance ? T("Next Level", "Следующий уровень") : T("Try Again", "Попробовать снова"), 29, 439, 410, 65,
                RoadTheme.Mint, RoadTheme.Ink,
                () => StartLevel(canAdvance ? number + 1 : number), canAdvance ? "right" : "undo");
            if (canAdvance)
                theme.Button(card, T("Replay", "Переиграть"), 29, 520, 198, 50, RoadTheme.Raised,
                    RoadTheme.Cream, () => StartLevel(number));
            theme.Button(card, T("Main Menu", "Главное меню"), canAdvance ? 241 : 29, 520, canAdvance ? 198 : 410, 50,
                RoadTheme.Raised, RoadTheme.Cream, ShowHome);
        }

        private void ResultStat(RectTransform parent, string name, string number, int x, int width, Color accent)
        {
            Label(parent, name, x, 18, width, 24, 15, RoadTheme.Muted, theme.Bold, TextAnchor.MiddleCenter);
            Text text = Label(parent, number, x, 47, width, 49, 32, accent, theme.Display, TextAnchor.MiddleCenter);
            FitNumber(text, 16);
        }

        private void DrawStars(RectTransform parent, float x, float y, float size, int filled)
        {
            for (int i = 0; i < 3; i++)
                theme.Icon(parent, "star", x + i * (size + 5), y, size, i < filled ? RoadTheme.Amber : RoadTheme.Rim);
        }

        private string CrystalCount(int count)
        {
            string noun = "crystals";
            if (language == RoadLanguage.Russian)
            {
                int ones = count % 10;
                int lastTwo = count % 100;
                noun = lastTwo is >= 11 and <= 14 ? "кристаллов" :
                    ones == 1 ? "кристалл" : ones is >= 2 and <= 4 ? "кристалла" : "кристаллов";
            }
            else if (count == 1)
                noun = "crystal";

            return Format(count) + " " + noun;
        }

        private string FailureText(RoadFailure failure) => failure switch
        {
            RoadFailure.InvalidPath => T("Time's up.", "Время вышло."),
            RoadFailure.WrongColor => T("The cube finished on the wrong color.", "Куб финишировал на клетке другого цвета."),
            RoadFailure.ObstacleFinish => T("The cube stopped on an obstacle.", "Куб остановился на препятствии."),
            RoadFailure.LauncherOffBoard => T("The launcher sent the cube off the board.", "Стрелка перебросила куб за пределы поля."),
            RoadFailure.LauncherBlocked => T("The launcher's landing tile was blocked.", "Клетка приземления была заблокирована."),
            RoadFailure.LauncherMissedPath => T("The launcher's landing tile was outside your route.", "Клетка приземления не входила в маршрут."),
            RoadFailure.LauncherRevisit => T("The launcher sent the cube to a tile it had already visited.", "Стрелка вернула куб на уже пройденную клетку."),
            _ => ""
        };

        private RectTransform OpenModal(string title, float height, bool closeButton = true)
        {
            CloseModal();
            pointer = NoPointer;
            modal = RoadTheme.Place(surface, title, 0, 0, Width, Height);
            Image shade = theme.Box(modal, "Karartma", 0, 0, Width, Height, new Color(.045f, .035f, .075f, .85f), 0);
            shade.raycastTarget = true;
            Image outer = theme.Box(modal, title, 36, (Height - height) / 2, 468, height, RoadTheme.Rim, 31);
            theme.Box(outer.rectTransform, "İç yüz", 2, 2, 464, height - 4, RoadTheme.Panel, 29);
            if (closeButton)
                theme.Button(outer.rectTransform, "", 388, 25, 51, 49, RoadTheme.Raised, RoadTheme.Cream, CloseModal, "close");
            return outer.rectTransform;
        }

        private void CloseModal()
        {
            if (modal == null)
                return;

            modal.gameObject.SetActive(false);
            Destroy(modal.gameObject);
            modal = null;
        }
    }
}
