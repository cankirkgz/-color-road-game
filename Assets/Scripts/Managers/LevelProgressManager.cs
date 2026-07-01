using UnityEngine;

namespace RenkYolu.Managers
{
    public static class LevelProgressManager
    {
        private const string UnlockedLevelKey = "UnlockedLevel";
        private const int DefaultUnlockedLevel = 1;

        public static int GetUnlockedLevel()
        {
            return PlayerPrefs.GetInt(UnlockedLevelKey, DefaultUnlockedLevel);
        }

        public static void UnlockNextLevel(int completedLevelNumber, int maxLevelCount)
        {
            int currentUnlockedLevel = GetUnlockedLevel();
            int nextLevel = completedLevelNumber + 1;

            if (nextLevel > maxLevelCount)
            {
                Debug.Log(
                    $"Unlock skipped. Completed last level. " +
                    $"Completed Level: {completedLevelNumber} | Max Level Count: {maxLevelCount}"
                );

                return;
            }

            if (nextLevel <= currentUnlockedLevel)
            {
                Debug.Log(
                    $"Unlock skipped. Level already unlocked. " +
                    $"Next Level: {nextLevel} | Current Unlocked Level: {currentUnlockedLevel}"
                );

                return;
            }

            PlayerPrefs.SetInt(UnlockedLevelKey, nextLevel);
            PlayerPrefs.Save();

            Debug.Log(
                $"New Level Unlocked | " +
                $"Completed Level: {completedLevelNumber} | " +
                $"Unlocked Level: {nextLevel}"
            );
        }

        public static bool IsLevelUnlocked(int levelNumber)
        {
            return levelNumber <= GetUnlockedLevel();
        }

        public static void ResetProgress()
        {
            PlayerPrefs.DeleteKey(UnlockedLevelKey);
            PlayerPrefs.Save();

            Debug.Log("Level progress reset. Unlocked Level returned to 1.");
        }
    }
}