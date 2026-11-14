using System.Collections;
using System.Linq;
using NUnit.Framework;
using RenkYolu.Campaign;
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

        [UnitySetUp]
        public IEnumerator OpenGame()
        {
            hadSave = PlayerPrefs.HasKey(Key);
            oldSave = PlayerPrefs.GetString(Key);
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
            PlayerPrefs.Save();
        }

        [UnityTest]
        public IEnumerator ADragWinsUnlocksNextLevelAndPersistsAfterReload()
        {
            Click("Oyna");
            Assert.That(Exists("Hazırım"), Is.True);
            Click("Hazırım");
            Assert.That(Exists("Haritayı ezberle"), Is.True);
            yield return new WaitForSecondsRealtime(10.2f);
            Assert.That(Exists("Rotayı çiz"), Is.True);

            RoadLevel level = RoadGenerator.Generate(Seed, 1);
            int target = level.Solution.ShortRoute[1];
            Drag(0, target);
            Assert.That(ButtonNamed("Yolu Onayla").interactable, Is.True);
            Click("Yolu Onayla");
            yield return new WaitForSecondsRealtime(1.1f);

            Assert.That(Exists("Bölüm tamam!"), Is.True);
            var save = JsonUtility.FromJson<RoadProgress>(PlayerPrefs.GetString(Key));
            Assert.That(save.unlocked, Is.EqualTo(2));
            Assert.That(save.Find(1).stars, Is.EqualTo(1));
            Assert.That(save.TotalShards, Is.EqualTo(1));
            Click("Sonraki Bölüm");
            Assert.That(Exists("Bölüm 2"), Is.True);

            yield return SceneManager.LoadSceneAsync("MainScene");
            yield return null;
            Assert.That(Exists("2. Bölüme Devam Et"), Is.True);
            Click("Bölümler");
            Assert.That(Exists("Önceki"), Is.True);
            Assert.That(Exists("Sonraki"), Is.True);
        }

        [UnityTest]
        public IEnumerator PauseHoldsTimerAndAnEmptyRouteTimesOutWithoutSaving()
        {
            Click("Oyna");
            Click("Hazırım");
            yield return null;
            Button pause = Object.FindObjectsByType<Button>()
                .Single(button => button.transform.Find("pause") != null);
            pause.onClick.Invoke();
            yield return new WaitForSecondsRealtime(1.2f);
            Assert.That(Exists("Duraklatıldı"), Is.True);
            Assert.That(Exists("10"), Is.True);
            Click("Devam Et");

            yield return new WaitForSecondsRealtime(10.2f);
            Assert.That(ButtonNamed("Yolu Onayla").interactable, Is.False);
            Drag(0, RoadGenerator.Generate(Seed, 1).Solution.ShortRoute[1]);
            Assert.That(ButtonNamed("Yolu Onayla").interactable, Is.True);
            Button undo = Object.FindObjectsByType<Button>()
                .Single(button => button.transform.Find("undo") != null);
            undo.onClick.Invoke();
            Assert.That(ButtonNamed("Yolu Onayla").interactable, Is.False);
            yield return new WaitForSecondsRealtime(25.5f);
            Assert.That(Exists("Süre doldu."), Is.True);
            Assert.That(JsonUtility.FromJson<RoadProgress>(PlayerPrefs.GetString(Key)).unlocked, Is.EqualTo(1));
            Click("Yeniden Dene");
            Assert.That(Exists("Haritayı ezberle"), Is.True);
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
    }
}
