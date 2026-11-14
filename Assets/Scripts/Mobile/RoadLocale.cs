using System;
using System.Globalization;
using UnityEngine;

namespace RenkYolu.Mobile
{
    internal enum RoadLanguage { English, Russian }

    internal static class RoadLocale
    {
        internal const string PreferenceKey = "RenkYolu.Language";

        internal static RoadLanguage Load()
        {
            string saved = PlayerPrefs.GetString(PreferenceKey);
            if (saved == "en")
                return RoadLanguage.English;
            if (saved == "ru")
                return RoadLanguage.Russian;

#if UNITY_ANDROID && !UNITY_EDITOR
            string networkCountry = ReadAndroidNetworkCountry();
            string deviceCountry = string.IsNullOrWhiteSpace(networkCountry) ? ReadAndroidDeviceCountry() : "";
            RoadLanguage chosen = ForCountries(networkCountry, deviceCountry);
#else
            RoadLanguage chosen = ForCountries(null, RegionInfo.CurrentRegion.TwoLetterISORegionName);
#endif
            Save(chosen);
            return chosen;
        }

        internal static RoadLanguage ForCountries(string networkCountry, string deviceCountry)
        {
            string country = string.IsNullOrWhiteSpace(networkCountry) ? deviceCountry : networkCountry;
            return string.Equals(country?.Trim(), "RU", StringComparison.OrdinalIgnoreCase)
                ? RoadLanguage.Russian : RoadLanguage.English;
        }

        internal static void Save(RoadLanguage language)
        {
            PlayerPrefs.SetString(PreferenceKey, language == RoadLanguage.Russian ? "ru" : "en");
            PlayerPrefs.Save();
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static string ReadAndroidNetworkCountry()
        {
            try
            {
                using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using AndroidJavaObject activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                using AndroidJavaObject phone = activity?.Call<AndroidJavaObject>("getSystemService", "phone");
                return phone?.Call<string>("getNetworkCountryIso");
            }
            catch (AndroidJavaException)
            {
                return null;
            }
        }

        private static string ReadAndroidDeviceCountry()
        {
            try
            {
                using var locale = new AndroidJavaClass("java.util.Locale");
                using AndroidJavaObject current = locale.CallStatic<AndroidJavaObject>("getDefault");
                return current?.Call<string>("getCountry");
            }
            catch (AndroidJavaException)
            {
                return null;
            }
        }
#endif
    }
}
