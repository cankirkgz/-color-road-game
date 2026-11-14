using System;
using RenkYolu.Campaign;
using UnityEngine;
using UnityEngine.UI;

namespace RenkYolu.Mobile
{
    public sealed partial class RoadApp
    {
        private static readonly string[] CubeNames = { "Klasik", "Yaramaz", "Yıldız", "Pofuduk" };
        private static readonly Color[] CubeColors =
        {
            RoadTheme.Amber, RoadTheme.Mint, RoadTheme.Hex(0xCDB2FD), RoadTheme.Pink
        };

        private void ShowHome()
        {
            NewPage("Ana menü");
            phase = Phase.Home;
            Label(page, "RENK YOLU", 33, 32, 200, 24, 18, RoadTheme.Cream, theme.Bold);
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
            Label(card, "Renk\nYolu", 35, 241, 405, 131, 66, RoadTheme.Cream, theme.Display,
                TextAnchor.MiddleCenter).lineSpacing = .78f;
            Label(card, "Rengi hatırla, yolunu çiz.", 24, 387, 428, 27, 22, RoadTheme.Muted,
                theme.Body, TextAnchor.MiddleCenter);

            bool first = ContinueAt == 1;
            theme.Button(page, first ? "Oyna" : Format(ContinueAt) + ". Bölüme Devam Et",
                32, 553, 476, 69, RoadTheme.Mint, RoadTheme.Ink, () => StartLevel(ContinueAt), "right");
            theme.Button(page, "Bölümler", 32, 639, 231, 55, RoadTheme.Raised, RoadTheme.Cream, () => ShowLevels(0));
            theme.Button(page, "Küpler", 277, 639, 231, 55, RoadTheme.Raised, RoadTheme.Cream, ShowCubes);
            Image stats = theme.Box(page, "İlerleme", 32, 715, 476, 94, RoadTheme.Panel, 20);
            HomeStat(stats.rectTransform, 17, "dot", Format(progress.TotalScore), "Toplam puan", RoadTheme.Cream);
            HomeStat(stats.rectTransform, 174, "star", Format(progress.TotalStars), "Yıldız", RoadTheme.Amber);
            HomeStat(stats.rectTransform, 323, "diamond", Format(progress.TotalShards), "Kristal", RoadTheme.Mint);
            UpdateSoundButton(page, 295, 833, 213);
            theme.Button(page, "Nasıl Oynanır?", 32, 833, 231, 48, RoadTheme.Panel, RoadTheme.Muted, ShowTutorial);
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
            RectTransform card = OpenModal("Nasıl Oynanır", 585);
            Label(card, "Nasıl oynanır?", 27, 23, 370, 48, 39, RoadTheme.Cream, theme.Display);
            TutorialRow(card, 90, "01", "Ezberle", "Renkleri ve puan işlemlerini incele.", RoadTheme.Amber);
            TutorialRow(card, 181, "02", "Yolunu çiz", "Sol alttan başla. Parmağını yatay veya dikey komşu karelere sürükle.", RoadTheme.Mint);
            TutorialRow(card, 288, "03", "Renginde bitir", "Küp aynı renkli karede durursa bölümü geçersin. Yol üzerindeki aynı renkler puanı ikiye katlar.",
                RoadTheme.Pink);
            theme.Icon(card, "diamond", 32, 436, 28, RoadTheme.Mint);
            Label(card, "İşaretli karelerden kristal topla ve yeni küpler aç.",
                76, 416, 367, 62, 20, RoadTheme.Muted, theme.Body);
            theme.Button(card, phase == Phase.Memorizing ? "Hazırım" : "Anladım", 27, 496, 414, 60,
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

            RectTransform card = OpenModal("Duraklatıldı", 437);
            Label(card, "Duraklatıldı", 28, 25, 410, 51, 41, RoadTheme.Cream, theme.Display);
            Label(card, "Bölüm " + Format(Difficulty.Level), 30, 79, 410, 32, 20, RoadTheme.Muted, theme.Body);
            theme.Button(card, "Devam Et", 28, 126, 412, 62, RoadTheme.Mint, RoadTheme.Ink, CloseModal);
            theme.Button(card, "Bölümü Baştan Al", 28, 202, 412, 54, RoadTheme.Raised,
                RoadTheme.Cream, () => StartLevel(Difficulty.Level));
            theme.Button(card, "Ana Menü", 28, 268, 412, 54, RoadTheme.Raised, RoadTheme.Cream, ShowHome);
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

        private string SoundLabel() => progress.muted ? "Ses Kapalı" : "Ses Açık";

        private void ShowLevels(int batch)
        {
            int lastBatch = (progress.unlocked - 1) / 12;
            if (batch < 0)
                batch = lastBatch;
            int visibleLastBatch = Mathf.Min(lastBatch + 1, (int.MaxValue - 1) / 12);
            batch = Mathf.Clamp(batch, 0, visibleLastBatch);
            RectTransform card = OpenModal("Bölümler", 682);
            Label(card, "Bölümler", 25, 27, 360, 53, 43, RoadTheme.Cream, theme.Display);
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
                    Label(box, active ? "Sıradaki" : "Kilitli", 0, 49, 131, 30, 15,
                        current ? RoadTheme.Ink : RoadTheme.Muted, theme.Body, TextAnchor.MiddleCenter);
            }

            Button previous = theme.Button(card, "Önceki", 25, 581, 196, 59, RoadTheme.Raised,
                RoadTheme.Cream, () => ShowLevels(batch - 1), "left");
            Button next = theme.Button(card, "Sonraki", 247, 581, 196, 59, RoadTheme.Raised,
                RoadTheme.Cream, () => ShowLevels(batch + 1), "right");
            previous.interactable = batch > 0;
            next.interactable = batch < visibleLastBatch;
        }

        private void ShowCubes()
        {
            RectTransform card = OpenModal("Küpler", 673);
            Label(card, "Küpler", 25, 27, 360, 52, 43, RoadTheme.Cream, theme.Display);
            theme.Icon(card, "diamond", 28, 91, 22, RoadTheme.Mint);
            Label(card, Format(progress.TotalShards) + " kristal", 59, 85, 330, 32, 22, RoadTheme.Mint, theme.Bold);
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
                Label(box, CubeNames[style], 0, 115, 204, 30, 24, RoadTheme.Cream, theme.Display, TextAnchor.MiddleCenter);
                string note = selected ? "Seçili" : owned ? "Seç" : Format(style * RoadProgress.ShardsPerCube) + " kristal";
                Label(box, note, 0, 150, 204, 29, 18, selected ? RoadTheme.Mint : RoadTheme.Muted,
                    theme.Body, TextAnchor.MiddleCenter);
            }

            Label(card, "Kristalleri toplamak için bölümü başarıyla bitir.",
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
            Label(card, won ? "Bölüm tamam!" : "Tekrar dene", 22, 105, 424, 48, 43,
                RoadTheme.Cream, theme.Display, TextAnchor.MiddleCenter);
            if (won)
                DrawStars(card, 131, 168, 64, stars);
            else
                Label(card, FailureText(failure), 31, 163, 406, 65, 22, RoadTheme.Muted, theme.Body, TextAnchor.MiddleCenter);

            Image dark = theme.Box(card, "Sonuç puanları", 28, 246, 412, 119, RoadTheme.Background, 18);
            RectTransform info = dark.rectTransform;
            ResultStat(info, "PUAN", Format(score), 12, 182, RoadTheme.Cream);
            theme.Box(info, "Ayraç", 206, 22, 1, 75, RoadTheme.Rim, 1);
            ResultStat(info, "MAKSİMUM", Format(level.Solution.Maximum), 218, 182, RoadTheme.Amber);

            string note = won && stars < 3 ? "3 yıldız için " + Format(level.ThreeStarScore) + " puan" :
                won && earned > 0 ? "+" + earned + " yeni kristal" :
                won ? "Toplam puan  " + Format(progress.TotalScore) :
                failure == RoadFailure.InvalidPath ? "Süre dolmadan en az iki kare çiz." : "Bir sonraki denemede başka bir yol çiz.";
            Label(card, note, 26, 383, 416, 41, 19, won ? RoadTheme.Mint : RoadTheme.Muted,
                theme.Bold, TextAnchor.MiddleCenter);

            bool canAdvance = won && number < int.MaxValue;
            theme.Button(card, canAdvance ? "Sonraki Bölüm" : "Yeniden Dene", 29, 439, 410, 65,
                RoadTheme.Mint, RoadTheme.Ink,
                () => StartLevel(canAdvance ? number + 1 : number), canAdvance ? "right" : "undo");
            if (canAdvance)
                theme.Button(card, "Tekrar Oyna", 29, 520, 198, 50, RoadTheme.Raised,
                    RoadTheme.Cream, () => StartLevel(number));
            theme.Button(card, "Ana Menü", canAdvance ? 241 : 29, 520, canAdvance ? 198 : 410, 50,
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

        private static string FailureText(RoadFailure failure) => failure switch
        {
            RoadFailure.InvalidPath => "Süre doldu.",
            RoadFailure.WrongColor => "Küp başlangıç renginde bitirmedi.",
            RoadFailure.ObstacleFinish => "Küp bir engelin üzerinde durdu.",
            RoadFailure.LauncherOffBoard => "Fırlatıcı küpü tahta dışına attı.",
            RoadFailure.LauncherBlocked => "Fırlatıcının önü kapalıydı.",
            RoadFailure.LauncherMissedPath => "Fırlatıcının iniş karesi rotanda yoktu.",
            RoadFailure.LauncherRevisit => "Fırlatıcı seni geçtiğin bir kareye attı.",
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
