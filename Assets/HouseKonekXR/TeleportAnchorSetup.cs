using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

/// <summary>Creates four saved destination pads beneath the organized main scene roots.</summary>
[ExecuteAlways]
public sealed class TeleportAnchorSetup : MonoBehaviour
{
    [SerializeField] bool built;

    void OnEnable()
    {
        if (!Application.isPlaying && !built)
            Build();
        if (!Application.isPlaying && built)
            ApplySceneMaterial();
    }

    void Build()
    {
        EnsureTeleportationProvider();
        CreateAnchor("Garden", "GARDEN");
        CreateAnchor("Village", "VILLAGE");
        CreateAnchor("Toilet", "TOILET");
        CreateAnchor("BusStop", "BUS STOP");
        built = true;
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
    }

    void CreateAnchor(string instanceName, string label)
    {
        var instance = GameObject.Find(instanceName);
        if (instance == null || transform.Find("Teleport - " + label) != null)
            return;

        var renderers = instance.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return;

        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);

        var pad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pad.name = "Teleport - " + label;
        pad.transform.SetParent(transform, true);
        pad.transform.position = new Vector3(bounds.center.x, bounds.min.y + .05f, bounds.min.z - Mathf.Max(1.2f, bounds.extents.z * .35f));
        var radius = Mathf.Clamp(Mathf.Max(bounds.size.x, bounds.size.z) * .08f, .65f, 1.4f);
        pad.transform.localScale = new Vector3(radius, .06f, radius);

        pad.GetComponent<Renderer>().sharedMaterial = PadMaterial();
        pad.AddComponent<TeleportationAnchor>();

        var text = new GameObject("Label").AddComponent<TextMesh>();
        text.transform.SetParent(pad.transform, false);
        text.transform.localPosition = new Vector3(0, 1.2f, 0);
        text.transform.localRotation = Quaternion.Euler(65, 180, 0);
        text.text = "TELEPORT\n" + label;
        text.anchor = TextAnchor.MiddleCenter; text.alignment = TextAlignment.Center;
        text.characterSize = .18f; text.fontSize = 64; text.color = Color.white;
    }

    Material PadMaterial()
    {
 #if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/New Material.mat");
 #else
        return null;
 #endif
    }

    void ApplySceneMaterial()
    {
        var material = PadMaterial();
        if (material == null) return;
        foreach (Transform child in transform)
        {
            var renderer = child.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;
        }
    }

    void EnsureTeleportationProvider()
    {
        if (FindFirstObjectByType<TeleportationProvider>() != null) return;
        var origin = FindFirstObjectByType<XROrigin>();
        if (origin == null) return;
        var mediator = origin.GetComponent<LocomotionMediator>() ?? origin.gameObject.AddComponent<LocomotionMediator>();
        mediator.xrOrigin = origin;
        var provider = origin.gameObject.AddComponent<TeleportationProvider>();
        provider.mediator = mediator;
    }
}
