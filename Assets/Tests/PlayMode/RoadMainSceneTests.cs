using System.Collections;
using System.Linq;
using NUnit.Framework;
using RenkYolu.Campaign;
using RenkYolu.Mobile;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RenkYolu.Tests
{
    public sealed class RoadMainSceneTests
    {
        private const string Key = "RenkYolu.Campaign.1";
        private const int Seed = 92871;
        private string oldSave;
        private bool hadSave;
        private string oldLanguage;
        private bool hadLanguage;

        [UnitySetUp]
        public IEnumerator OpenGame()
        {
            hadSave = PlayerPrefs.HasKey(Key);
            oldSave = PlayerPrefs.GetString(Key);
            hadLanguage = PlayerPrefs.HasKey(RoadLocale.PreferenceKey);
            oldLanguage = PlayerPrefs.GetString(RoadLocale.PreferenceKey);
            RoadLocale.Save(RoadLanguage.English);
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(new RoadProgress(Seed) { muted = true }));
            yield return SceneManager.LoadSceneAsync("MainScene");
            yield return null;
        }

        [TearDown]
        public void RestoreSave()
        {
            if (hadSave)
                PlayerPrefs.SetString(Key, oldSave);
            else
                PlayerPrefs.DeleteKey(Key);
            if (hadLanguage)
                PlayerPrefs.SetString(RoadLocale.PreferenceKey, oldLanguage);
            else
                PlayerPrefs.DeleteKey(RoadLocale.PreferenceKey);
            PlayerPrefs.Save();
        }

        [UnityTest]
        public IEnumerator ADragWinsUnlocksNextLevelAndPersistsAfterReload()
        {
            Assert.That(Exists("Play"), Is.True);
            Click("Settings");
            Click("Русский");
            Assert.That(Exists("Язык интерфейса"), Is.True);
            CloseModal();
            Assert.That(Exists("Play"), Is.False);
            Click("Играть");
            Assert.That(Exists("Как играть"), Is.True);
            Click("Начать");
            Assert.That(Exists("Запомни поле"), Is.True);
            Assert.That(Exists("СТАРТ"), Is.True);
            yield return new WaitForSecondsRealtime(10.2f);
            Assert.That(Exists("Нарисуй маршрут"), Is.True);

            RoadLevel level = RoadGenerator.Generate(Seed, 1);
            int target = level.Solution.ShortRoute[1];
            Drag(0, target);
            Assert.That(ButtonNamed("Подтвердить маршрут").interactable, Is.True);
            Click("Подтвердить маршрут");
            yield return new WaitForSecondsRealtime(1.1f);

            Assert.That(Exists("Уровень пройден!"), Is.True);
            var save = JsonUtility.FromJson<RoadProgress>(PlayerPrefs.GetString(Key));
            Assert.That(save.unlocked, Is.EqualTo(2));
            Assert.That(save.Find(1).stars, Is.EqualTo(1));
            Assert.That(save.TotalShards, Is.EqualTo(1));
            Click("Следующий уровень");
            Assert.That(Exists("Уровень 2"), Is.True);

            yield return SceneManager.LoadSceneAsync("MainScene");
            yield return null;
            Assert.That(Exists("Продолжить с уровня 2"), Is.True);
            Click("Уровни");
            Assert.That(Exists("Назад"), Is.True);
            Assert.That(Exists("Далее"), Is.True);
            CloseModal();
            Click("Настройки");
            Click("English");
            Assert.That(Exists("Interface language"), Is.True);
            Assert.That(Exists("Continue to Level 2"), Is.True);
        }

        [UnityTest]
        public IEnumerator PauseHoldsTimerAndAnEmptyRouteTimesOutWithoutSaving()
        {
            Click("Play");
            Click("Ready");
            yield return null;
            Button pause = Object.FindObjectsByType<Button>()
                .Single(button => button.transform.Find("pause") != null);
            pause.onClick.Invoke();
            yield return new WaitForSecondsRealtime(1.2f);
            Assert.That(Exists("Paused"), Is.True);
            Assert.That(Exists("10"), Is.True);
            Click("Resume");

            yield return new WaitForSecondsRealtime(10.2f);
            Assert.That(ButtonNamed("Confirm Route").interactable, Is.False);
            Drag(0, RoadGenerator.Generate(Seed, 1).Solution.ShortRoute[1]);
            Assert.That(ButtonNamed("Confirm Route").interactable, Is.True);
            Button undo = Object.FindObjectsByType<Button>()
                .Single(button => button.transform.Find("undo") != null);
            undo.onClick.Invoke();
            Assert.That(ButtonNamed("Confirm Route").interactable, Is.False);
            yield return new WaitForSecondsRealtime(25.5f);
            Assert.That(Exists("Time's up."), Is.True);
            Assert.That(JsonUtility.FromJson<RoadProgress>(PlayerPrefs.GetString(Key)).unlocked, Is.EqualTo(1));
            Click("Try Again");
            Assert.That(Exists("Memorize the board"), Is.True);
        }

        [TestCase("ru", "US", true)]
        [TestCase("DE", "RU", false)]
        [TestCase("", "RU", true)]
        [TestCase(null, "KZ", false)]
        [TestCase(" ", null, false)]
        public void RegionSelectionUsesTheCurrentNetworkFirst(string network, string region, bool expectRussian)
        {
            Assert.That(RoadLocale.ForCountries(network, region),
                Is.EqualTo(expectRussian ? RoadLanguage.Russian : RoadLanguage.English));
        }

        [Test]
        public void InterfaceFontsIncludeCyrillicAndLatin()
        {
            var theme = new RoadTheme();
            foreach (Font font in new[] { theme.Display, theme.Bold, theme.Body })
            {
                Assert.That(font.name, Does.StartWith("Inter"));
                foreach (char letter in "AtlasЯёлка×÷−")
                    Assert.That(font.HasCharacter(letter), Is.True, font.name + " " + letter);
            }
        }

        private static void Drag(int start, int finish)
        {
            Canvas.ForceUpdateCanvases();
            GameObject board = GameObject.Find("Tahta");
            const float pitch = (476 - 26) / 4f;
            PointerEventData data = new PointerEventData(EventSystem.current)
            {
                pointerId = 1,
                button = PointerEventData.InputButton.Left,
                position = Point(start)
            };
            ExecuteEvents.Execute(board, data, ExecuteEvents.pointerDownHandler);
            data.position = Point(finish);
            ExecuteEvents.Execute(board, data, ExecuteEvents.dragHandler);
            ExecuteEvents.Execute(board, data, ExecuteEvents.pointerUpHandler);

            Vector2 Point(int cell) => RectTransformUtility.WorldToScreenPoint(null,
                board.transform.TransformPoint(new Vector3(13 + (cell % 4 + .5f) * pitch,
                    -13 - (4 - cell / 4 - .5f) * pitch)));
        }

        private static bool Exists(string text) =>
            Object.FindObjectsByType<Text>().Any(label => label.text == text);

        private static Button ButtonNamed(string name) =>
            Object.FindObjectsByType<Button>().Single(button => button.name == name);

        private static void Click(string name)
        {
            Button button = ButtonNamed(name);
            Assert.That(button.interactable, Is.True, name);
            button.onClick.Invoke();
        }

        private static void CloseModal() =>
            Object.FindObjectsByType<Button>().Single(button => button.transform.Find("close") != null).onClick.Invoke();
    }
}
