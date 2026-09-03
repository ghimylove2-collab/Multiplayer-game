using UnityEngine;
using System.Collections.Generic;

public class AiroxBootstrap : MonoBehaviour
{
    public static AiroxBootstrap Instance;
    public AiroxPlayer player;
    public LayanNpc layan;
    public int batteries;
    public int parts;
    public int medicine;
    public float energy = 78f;
    public string mission = "اقترب من ليان قرب المنطقة الآمنة";
    public string toast = "استكشف القطاع واجمع الموارد";
    public bool dialogueOpen;
    readonly List<ResourcePickup> pickups = new();
    float toastTimer;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateGame()
    {
        if (FindObjectOfType<AiroxBootstrap>() != null) return;
        var root = new GameObject("Airox_NovaCity");
        root.AddComponent<AiroxBootstrap>();
    }

    void Awake()
    {
        Instance = this;
        BuildWorld();
    }

    void BuildWorld()
    {
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.18f, 0.13f, 0.10f);
        RenderSettings.fogDensity = 0.012f;
        var sun = new GameObject("Copper Sun").AddComponent<Light>();
        sun.type = LightType.Directional; sun.intensity = 1.15f; sun.color = new Color(1f, 0.72f, 0.5f);
        sun.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
        MakeCube("Dusty Ground", new Vector3(0, -1.25f, 0), new Vector3(46, 1, 38), new Color(0.25f, 0.14f, 0.09f));
        MakeCube("Safe Zone", new Vector3(-10, -0.65f, 4), new Vector3(7, 0.18f, 6), new Color(0.12f, 0.34f, 0.30f));
        MakeCube("Road", new Vector3(3, -0.65f, 0), new Vector3(4, 0.18f, 38), new Color(0.05f, 0.045f, 0.04f));
        for (int i = -16; i <= 16; i += 4) MakeCube("RoadMark", new Vector3(3, -0.53f, i), new Vector3(0.22f, 0.04f, 1.5f), new Color(0.82f, 0.48f, 0.16f));
        for (int i = 0; i < 11; i++)
        {
            float x = -17 + (i * 3.7f % 31f);
            float z = -14 + (i * 7.9f % 28f);
            MakeCube("Rubble", new Vector3(x, -0.1f, z), new Vector3(1.3f, 1.4f, 1.2f), new Color(0.25f, 0.20f, 0.17f));
        }
        var playerObject = new GameObject("Airox_Player"); playerObject.transform.position = new Vector3(-3, 0, -7);
        player = playerObject.AddComponent<AiroxPlayer>();
        playerObject.AddComponent<CharacterController>();
        CreateAvatar(playerObject);
        var cam = new GameObject("Third Person Camera");
        var camera = cam.AddComponent<Camera>(); camera.fieldOfView = 62f; camera.tag = "MainCamera";
        var follow = cam.AddComponent<FollowCamera>(); follow.target = playerObject.transform;
        var layanObject = new GameObject("Layan_NPC"); layanObject.transform.position = new Vector3(-7, 0, 2);
        layan = layanObject.AddComponent<LayanNpc>(); CreateNpcVisual(layanObject);
        MakeCube("Energy Node", new Vector3(8, 0.35f, 7), new Vector3(1.4f, 2.2f, 1.4f), new Color(0.18f, 0.65f, 0.63f)).AddComponent<EnergyNode>();
        AddPickup("بطارية", new Vector3(-1, 0, -1), 1); AddPickup("قطع", new Vector3(12, 0, -5), 2); AddPickup("دواء", new Vector3(-13, 0, -8), 3);
    }

    void CreateAvatar(GameObject root)
    {
        MakePart(root, "Torso", new Vector3(0, 1.25f, 0), new Vector3(.75f, 1.2f, .45f), new Color(.08f, .12f, .12f));
        MakePart(root, "Head", new Vector3(0, 2.2f, 0), new Vector3(.48f, .48f, .48f), new Color(.42f, .23f, .14f));
        MakePart(root, "ToolPack", new Vector3(0, 1.2f, -.34f), new Vector3(.5f, .65f, .18f), new Color(.55f, .25f, .1f));
        MakePart(root, "LeftArm", new Vector3(-.52f, 1.3f, 0), new Vector3(.18f, .9f, .18f), new Color(.72f, .22f, .12f));
        MakePart(root, "RightArm", new Vector3(.52f, 1.3f, 0), new Vector3(.18f, .9f, .18f), new Color(.72f, .22f, .12f));
    }
    void CreateNpcVisual(GameObject root)
    {
        MakePart(root, "LayanTorso", new Vector3(0, 1.15f, 0), new Vector3(.7f, 1.25f, .4f), new Color(.62f, .25f, .10f));
        MakePart(root, "LayanHead", new Vector3(0, 2.15f, 0), new Vector3(.45f, .45f, .45f), new Color(.48f, .28f, .18f));
        MakePart(root, "LayanScarf", new Vector3(0, 1.7f, .24f), new Vector3(.52f, .25f, .16f), new Color(.86f, .55f, .22f));
    }
    GameObject MakePart(GameObject parent, string name, Vector3 local, Vector3 scale, Color color)
    { var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name; go.transform.SetParent(parent.transform); go.transform.localPosition = local; go.transform.localScale = scale; go.GetComponent<Renderer>().material.color = color; return go; }
    GameObject MakeCube(string name, Vector3 position, Vector3 scale, Color color)
    { var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name; go.transform.position = position; go.transform.localScale = scale; go.GetComponent<Renderer>().material.color = color; return go; }
    void AddPickup(string label, Vector3 position, int kind) { var go = MakeCube(label, position + Vector3.up * .45f, Vector3.one * .65f, kind == 1 ? Color.yellow : kind == 2 ? new Color(.75f,.45f,.2f) : Color.red); var p = go.AddComponent<ResourcePickup>(); p.label = label; p.kind = kind; pickups.Add(p); }
    public void ShowToast(string value) { toast = value; toastTimer = 4f; }
    public void Collect(ResourcePickup pickup) { if (pickup.kind == 1) batteries++; else if (pickup.kind == 2) parts++; else medicine++; Destroy(pickup.gameObject); ShowToast("تم جمع " + pickup.label); }
    void Update() { if (toastTimer > 0) toastTimer -= Time.deltaTime; if (energy < 100 && !dialogueOpen) energy += Time.deltaTime * .35f; }
    void OnGUI()
    {
        GUI.color = Color.white; GUI.Box(new Rect(18, 18, 285, 106), "AIROX // غبار النحاس\n\nالطاقة  " + Mathf.RoundToInt(energy) + "%\nالبطاريات " + batteries + "   القطع " + parts + "   الدواء " + medicine);
        GUI.Box(new Rect(Screen.width - 350, 18, 330, 76), "المهمة الحالية\n" + mission + "\nWASD / اللمس للحركة • E للتفاعل");
        if (toastTimer > 0) GUI.Box(new Rect(Screen.width / 2 - 180, 26, 360, 45), toast);
        if (dialogueOpen) { GUI.Box(new Rect(25, Screen.height - 175, Screen.width - 50, 130), "ليان\n" + layan.Dialogue() + "\n\nاضغط E أو المس لإغلاق الحوار"); }
    }
}

public class AiroxPlayer : MonoBehaviour
{
    CharacterController controller; float speed = 4.2f; float gravity = -18f; Vector3 velocity;
    void Start() { controller = GetComponent<CharacterController>(); }
    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal"), z = Input.GetAxisRaw("Vertical");
        Vector3 move = new Vector3(x, 0, z).normalized;
        bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (sprint && move.sqrMagnitude > .1f && AiroxBootstrap.Instance.energy > 3) { speed = 6.4f; AiroxBootstrap.Instance.energy -= Time.deltaTime * 5f; } else speed = 4.2f;
        controller.Move(move * speed * Time.deltaTime); if (move.sqrMagnitude > .05f) transform.forward = Vector3.Slerp(transform.forward, move, Time.deltaTime * 10f);
        if (controller.isGrounded && velocity.y < 0) velocity.y = -2; velocity.y += gravity * Time.deltaTime; controller.Move(velocity * Time.deltaTime);
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space)) Interact();
    }
    void Interact()
    { if (AiroxBootstrap.Instance.layan != null && Vector3.Distance(transform.position, AiroxBootstrap.Instance.layan.transform.position) < 3.2f) { AiroxBootstrap.Instance.dialogueOpen = !AiroxBootstrap.Instance.dialogueOpen; if (AiroxBootstrap.Instance.dialogueOpen) AiroxBootstrap.Instance.mission = "استمع إلى ليان ثم واصل نحو عقدة الطاقة"; } }
}

public class FollowCamera : MonoBehaviour
{
    public Transform target; void LateUpdate() { if (!target) return; Vector3 desired = target.position + new Vector3(0, 5.4f, -7.4f); transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * 6f); transform.LookAt(target.position + Vector3.up * 1.1f); }
}

public class LayanNpc : MonoBehaviour
{
    public string Dialogue() { return "لا تبتعد عن المنطقة الآمنة. الإشارة ضعيفة، لكن الطريق إلى عقدة الطاقة ما زال مفتوحًا."; }
    void Update() { transform.localScale = Vector3.one * (1f + Mathf.Sin(Time.time * 2f) * .015f); }
}

public class ResourcePickup : MonoBehaviour
{
    public string label; public int kind; void Update() { transform.Rotate(Vector3.up, 75f * Time.deltaTime); if (AiroxBootstrap.Instance.player && Vector3.Distance(transform.position, AiroxBootstrap.Instance.player.transform.position) < 1.4f) AiroxBootstrap.Instance.Collect(this); }
}

public class EnergyNode : MonoBehaviour
{
    void Update() { transform.Rotate(Vector3.up, 22f * Time.deltaTime); }
}
