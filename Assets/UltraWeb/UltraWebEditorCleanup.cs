#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
public class UltraWebEditorCleanup
{
    static UltraWebEditorCleanup()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            // Extra aggressive cleanup for editor
            if (UltraWeb.Instance != null)
            {
                UltraWeb.Instance.Dispose();
            }
            UltraWeb.ResetStaticState();

            // Editor-specific GC push
            EditorUtility.UnloadUnusedAssetsImmediate();
            System.GC.Collect();
        }
    }
}
#endif