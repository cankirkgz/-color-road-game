using RenkYolu.Grid;
using TMPro;
using UnityEditor;
using UnityEngine;

public static class SpikeLauncherMigration
{
    private const string TilePrefabPath = "Assets/Prefabs/Tile.prefab";

    [InitializeOnLoadMethod]
    private static void ScheduleIfNeeded()
    {
        EditorApplication.delayCall += RunIfNeeded;
    }

    private static void RunIfNeeded()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TilePrefabPath);

        if (prefab == null || prefab.transform.Find("SpikeTrapVisual") != null)
        {
            return;
        }

        Run();
    }

    public static void Run()
    {
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(TilePrefabPath);

        try
        {
            Tile tile = prefabRoot.GetComponent<Tile>();

            if (tile == null)
            {
                throw new MissingComponentException("Tile component is missing on Tile.prefab.");
            }

            Transform launcherTransform = prefabRoot.transform.Find("LauncherIconText") ??
                                          prefabRoot.transform.Find("SpikeIconText");

            if (launcherTransform == null)
            {
                throw new MissingReferenceException("Existing launcher direction icon could not be found.");
            }

            launcherTransform.name = "LauncherIconText";
            TMP_Text launcherText = launcherTransform.GetComponent<TMP_Text>();

            if (launcherText == null)
            {
                throw new MissingComponentException("LauncherIconText has no TMP_Text component.");
            }

            Transform spikeTransform = prefabRoot.transform.Find("SpikeTrapVisual");
            GameObject spikeVisual;
            TextMeshPro spikeText;

            if (spikeTransform == null)
            {
                spikeVisual = new GameObject("SpikeTrapVisual", typeof(RectTransform), typeof(TextMeshPro));
                spikeVisual.transform.SetParent(prefabRoot.transform, false);
                spikeText = spikeVisual.GetComponent<TextMeshPro>();
            }
            else
            {
                spikeVisual = spikeTransform.gameObject;
                spikeText = spikeVisual.GetComponent<TextMeshPro>();

                if (spikeText == null)
                {
                    spikeText = spikeVisual.AddComponent<TextMeshPro>();
                }
            }

            RectTransform spikeRect = spikeVisual.GetComponent<RectTransform>();
            spikeRect.anchoredPosition = new Vector2(0f, 0.05f);
            spikeRect.sizeDelta = new Vector2(2f, 2f);
            spikeRect.localPosition = new Vector3(0f, 0.05f, -0.2f);
            spikeRect.localRotation = Quaternion.identity;
            spikeRect.localScale = Vector3.one;

            spikeText.text = "▲▲▲";
            spikeText.font = launcherText.font;
            spikeText.fontSize = 5f;
            spikeText.alignment = TextAlignmentOptions.Center;
            spikeText.color = new Color(1f, 0.12f, 0.12f, 1f);
            spikeText.raycastTarget = false;

            if (spikeText.renderer != null)
            {
                spikeText.renderer.sortingOrder = 10;
            }

            SerializedObject serializedTile = new SerializedObject(tile);
            serializedTile.FindProperty("launcherIconText").objectReferenceValue = launcherText;
            serializedTile.FindProperty("spikeTrapVisual").objectReferenceValue = spikeVisual;
            serializedTile.ApplyModifiedPropertiesWithoutUndo();

            spikeVisual.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, TilePrefabPath);
            AssetDatabase.SaveAssets();

            Debug.Log("Spike/Launcher Tile prefab migration completed.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }
}
