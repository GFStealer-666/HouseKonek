using System;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

[ExecuteAlways]
// A data-driven XR layer. Positions are design-space coordinates, then fitted to the imported village bounds.
public sealed class VillageXRExperience : MonoBehaviour
{
    Vector3 centre; float unit = 1f;
    Material ink, aqua, amber, leaf, snow;
    [SerializeField] bool hierarchyBuilt;

    readonly string[][] bus = {
        new[]{"Wooden Bench / เก้าอี้ไม้","rise"}, new[]{"Arrival Display / ตู้ดูรถ","arrived"},
        new[]{"Advertisement Board / ป้ายโฆษณา","ad"}, new[]{"Solar Panel / แผงโซล่าเซล","slide"},
        new[]{"Ticket Machine / ตู้กดบัตร","ticket"}, new[]{"Announcement Speaker / ลำโพง","sound"},
        new[]{"Bin / ถังขยะ","open"}, new[]{"Security Booth / ที่เวรยาม","none"} };
    readonly string[][] park = {
        new[]{"Campfire / กองไฟ","fire"}, new[]{"Large Bin / ถังขยะใหญ่","rise"},
        new[]{"Park Speaker / ลำโพง","sound"}, new[]{"Brown Camera / กล้องสีน้ำตาล","coin"},
        new[]{"Fountain / น้ำพุ","water"}, new[]{"Lamp / โคมไฟ","light"}, new[]{"Two-leg Sign / ป้ายสองขา","openText"} };
    readonly string[][] restroom = {
        new[]{"Toilet Door / ประตูห้องน้ำ","door"}, new[]{"Faucet / ก๊อกน้ำ","water"},
        new[]{"Soap Bottle / ขวดสบู่","bubbles"}, new[]{"Hand Dryer / เครื่องเป่ามือ","sound"},
        new[]{"Light Switch / สวิตช์ไฟ","roomLight"}, new[]{"Toilet / โถชักโครก","flush"},
        new[]{"Urinal / โถฉี่","water"} };
    readonly string[][] house = {
        new[]{"House Door / ประตู","door"}, new[]{"Stairs / บันได","right"},
        new[]{"Spotlight","light"}, new[]{"Telescope / กล้องส่องทางไกล","rise"},
        new[]{"Vending Machine / ตู้กดน้ำ","bottle"}, new[]{"Table / โต๊ะ","rise"},
        new[]{"Table Lamp / โคมไฟ","light"}, new[]{"Locker / ตู้ locker","open"} };

    void OnEnable()
    {
        if (!Application.isPlaying && !hierarchyBuilt)
            BuildSavedHierarchy();
    }

    void Awake()
    {
        if (!Application.isPlaying)
            return;
        EnsureTeleportation();
    }

    void BuildSavedHierarchy()
    {
        MeasureVillage(); EnsureTeleportation();
        ink = Mat(new Color(.06f,.09f,.13f)); aqua = Mat(new Color(.04f,.74f,.92f), true);
        amber = Mat(new Color(1f,.42f,.06f), true); leaf = Mat(new Color(.1f,.42f,.2f)); snow = Mat(new Color(.85f,.92f,.98f));
        BuildHub(); BuildZone("BUS STOP / ป้ายรถเมย์", new Vector3(-34,0,14), bus, aqua);
        BuildZone("PARK / สวน", new Vector3(34,0,14), park, leaf);
        BuildZone("RESTROOM / ห้องน้ำ", new Vector3(-34,0,-27), restroom, snow);
        BuildZone("TWIN HOUSE / บ้านคู่", new Vector3(34,0,-27), house, amber);
        hierarchyBuilt = true;
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
    }

    // Fit the 96 x 72 design map inside the imported village. This is the scale reference.
    void MeasureVillage()
    {
        var all = FindObjectsByType<Renderer>(FindObjectsSortMode.None); var hasBounds = false; var b = new Bounds();
        foreach (var r in all)
        {
            var n = r.name.ToLowerInvariant(); if (n.Contains("hand") || n.Contains("controller")) continue;
            if (!hasBounds) { b = r.bounds; hasBounds = true; } else b.Encapsulate(r.bounds);
        }
        if (hasBounds && b.size.x > 4 && b.size.z > 4) { centre = b.center; unit = Mathf.Clamp(Mathf.Min(b.size.x/96f, b.size.z/72f), .2f, 8f); }
    }
    Vector3 W(Vector3 p) => centre + p * unit;
    Vector3 S(Vector3 s) => s * unit;
    Material Mat(Color c, bool glow=false) { var m = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")); m.color=c; if(glow){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",c*1.5f);} return m; }

    void EnsureTeleportation()
    {
        if (FindFirstObjectByType<TeleportationProvider>() != null) return;
        var origin = FindFirstObjectByType<XROrigin>(); if (origin == null) return;
        var mediator = origin.GetComponent<LocomotionMediator>() ?? origin.gameObject.AddComponent<LocomotionMediator>();
        mediator.xrOrigin = origin; var provider = origin.gameObject.AddComponent<TeleportationProvider>(); provider.mediator = mediator;
    }
    void BuildHub()
    {
        var root = new GameObject("TELEPORT HUB / ศูนย์วาร์ป"); root.transform.SetParent(transform);
        Cube(root.transform,"Hub Platform",Vector3.zero,new Vector3(12,.15f,9),ink);
        Text(root.transform,"TELEPORT HUB\nเลือกพื้นที่ด้วย Teleport Ray",new Vector3(0,.25f,3.4f),.35f,Color.white);
        Anchor(root.transform,"Hub",new Vector3(0,.1f,-2.5f));
        Text(root.transform,"BUS STOP ◀    PARK ▶\nRESTROOM ◀    TWIN HOUSE ▶",new Vector3(0,.25f,0),.22f,aqua.color);
    }
    void BuildZone(string title, Vector3 p, string[][] items, Material floor)
    {
        var root = new GameObject(title); root.transform.SetParent(transform); Cube(root.transform,"Territory Base",p,new Vector3(20,.12f,14),floor);
        Text(root.transform,title,p+new Vector3(0,.2f,5.7f),.34f,Color.white); Anchor(root.transform,title,p+new Vector3(0,.12f,-4.7f));
        for(var i=0;i<items.Length;i++)
        {
            var x = -7.2f + (i%4)*4.8f; var z = i<4 ? 1.8f : -1.7f;
            var h = items[i][1] == "door" ? 2.6f : (items[i][1] == "light" ? 2.8f : 1.25f);
            Prop(root.transform,items[i][0],items[i][1],p+new Vector3(x,h*.5f,z),new Vector3(1.65f,h,.85f));
        }
    }
    void Anchor(Transform parent,string label,Vector3 p)
    {
        var pad=Cube(parent,"Teleport Anchor - "+label,p,new Vector3(2,.14f,2),aqua); pad.AddComponent<TeleportationAnchor>();
        Text(pad.transform,"TELEPORT\n"+label,p+new Vector3(0,.18f,0),.18f,Color.white);
    }
    void Prop(Transform parent,string title,string effect,Vector3 p,Vector3 size)
    {
        var material = effect=="fire"||effect=="light" ? amber : effect=="water"||effect=="slide" ? aqua : ink;
        var go=Cube(parent,title,p,size,material); var action=go.AddComponent<VillageXRAction>(); action.effect=effect; action.unit=unit; action.accent=material;
        Text(go.transform,title,p+new Vector3(0,size.y*.65f+.25f,0),.15f,Color.white);
    }
    GameObject Cube(Transform parent,string name,Vector3 p,Vector3 size,Material m)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube); go.name=name; go.transform.position=W(p); go.transform.localScale=S(size); go.transform.SetParent(parent,true); go.GetComponent<Renderer>().material=m; return go;
    }
    void Text(Transform parent,string value,Vector3 p,float size,Color c)
    {
        var go=new GameObject("Label - "+value); go.transform.SetParent(parent); go.transform.position=W(p); go.transform.rotation=Quaternion.Euler(0,180,0);
        var t=go.AddComponent<TextMesh>(); t.text=value; t.anchor=TextAnchor.MiddleCenter; t.alignment=TextAlignment.Center; t.characterSize=size*unit; t.fontSize=64; t.color=c;
    }
}

[RequireComponent(typeof(Collider))]
public sealed class VillageXRAction : MonoBehaviour
{
    public string effect; public float unit=1; public Material accent; Vector3 home; bool on; Light lamp;
    void Awake(){ home=transform.position; var x=gameObject.AddComponent<XRSimpleInteractable>(); x.selectEntered.AddListener(_=>Use()); }
    void Use()
    {
        on=!on;
        if(effect=="rise") Move(Vector3.up*1.4f); else if(effect=="slide") Move(Vector3.forward*2f); else if(effect=="right") Move(Vector3.right*4f);
        else if(effect=="door"||effect=="open") { transform.localRotation=Quaternion.Euler(0,on?90:0,0); Say(effect=="open"?"OPEN":"OPEN / CLOSE"); }
        else if(effect=="arrived") Say("ARRIVED"); else if(effect=="ad") Say("LOCAL CAFE\nBUY 1 GET 1"); else if(effect=="openText") Say("OPEN");
        else if(effect=="ticket") Spawn("TICKET",new Vector3(1.1f,.07f,.65f)); else if(effect=="bottle") Spawn("BOTTLE",new Vector3(.35f,.8f,.35f));
        else if(effect=="light"||effect=="roomLight") LightUp(effect=="roomLight"); else if(effect=="fire") Particles(new Color(1,.2f,.02f),130,.9f);
        else if(effect=="water") Particles(new Color(.1f,.65f,1),90,1.1f); else if(effect=="bubbles") Particles(Color.white,55,.7f);
        else if(effect=="flush") { Tone(210,.5f); Particles(new Color(.2f,.7f,1),60,.8f); } else if(effect=="coin") { Tone(880,.15f); Say("COIN!"); }
        else if(effect=="sound") { Tone(440,.35f); Say("PLAYING"); }
    }
    void Move(Vector3 d){transform.position=on?home+d*unit:home;}
    void Say(string text){var t=GetComponentInChildren<TextMesh>();if(t)t.text=text;}
    void Spawn(string name,Vector3 s){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.position=transform.position+Vector3.down*unit;g.transform.localScale=s*unit;g.GetComponent<Renderer>().material=accent;Destroy(g,6);}
    void LightUp(bool room){if(!lamp)lamp=gameObject.AddComponent<Light>();lamp.type=LightType.Point;lamp.range=(room?12:6)*unit;lamp.intensity=on?(room?4:2):0;lamp.color=new Color(1,.65f,.25f);if(room)Say(on?"LIGHTS ON":"LIGHTS OFF");}
    void Particles(Color c,int count,float life){var g=new GameObject("Effect");g.transform.position=transform.position+Vector3.up*.4f*unit;var p=g.AddComponent<ParticleSystem>();var m=p.main;m.startColor=c;m.startLifetime=life;m.startSpeed=1.5f*unit;m.startSize=.14f*unit;m.maxParticles=count;var e=p.emission;e.rateOverTime=count;Destroy(g,life+.5f);}
    void Tone(float hz,float seconds){var a=gameObject.AddComponent<AudioSource>();int rate=44100,n=Mathf.CeilToInt(rate*seconds);var d=new float[n];for(int i=0;i<n;i++)d[i]=Mathf.Sin(2*Mathf.PI*hz*i/rate)*.18f;var clip=AudioClip.Create("XR feedback",n,1,rate,false);clip.SetData(d,0);a.clip=clip;a.Play();Destroy(a,seconds+.1f);}
}
