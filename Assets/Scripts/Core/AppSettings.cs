using UnityEngine;

namespace RenkYolu.Core
{
    public class AppSettings : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            Debug.Log("App Settings Initialized");
        }
    }
}