#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
static class ReimportBlendModels
{
    const string SessionKey = "HouseKonek.BlendReimportAttempted";

    static ReimportBlendModels()
    {
        if (!SessionState.GetBool(SessionKey, false))
            EditorApplication.delayCall += Reimport;
    }

    [MenuItem("Tools/HouseKonek/Reimport Blender Models")]
    static void Reimport()
    {
        SessionState.SetBool(SessionKey, true);
        foreach (var path in new[]
        {
            "Assets/5StructuresKonek.blend", "Assets/model split.blend", "Assets/model split 1.blend",
            "Assets/model split 2.blend", "Assets/model split 3.blend", "Assets/model split 4.blend"
        })
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
    }
}
#endif
