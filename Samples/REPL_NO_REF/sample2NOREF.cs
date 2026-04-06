// ═══════════════════════════════════════════════════════════
//  GARAGE OS  v1.0  —  CMS2026 UITK Framework showcase
//  Styl: ciemny amber/gold, live stats z gry, quick actions
// ═══════════════════════════════════════════════════════════

var p = CMS2026UITKFramework.FrameworkAPI.CreatePanel("GARAGE OS", 30, 30, 300, 560, sortOrder: 9999);

// ── Pro API: styl panelu — ciemne złoto ──────────────────────
var panelVE = CMS2026UITKFramework.UIRuntime.WrapVE(p.GetPanelRawPtr());
var ps = CMS2026UITKFramework.UIRuntime.GetStyle(panelVE);
CMS2026UITKFramework.S.BorderRadius(ps, 16f);
CMS2026UITKFramework.S.BorderColor(ps, new UnityEngine.Color(0.85f, 0.65f, 0.1f, 1f));
CMS2026UITKFramework.S.BorderWidth(ps, 2f);
CMS2026UITKFramework.S.BgColor(ps, new UnityEngine.Color(0.07f, 0.06f, 0.04f, 0.97f));

// ── STATUS BAR — pulsujący dot + wersja ──────────────────────
p.AddHeader("● SYSTEM STATUS");
var lblStatus = p.AddLabel("ONLINE  |  Scene: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
lblStatus.SetColor(new UnityEngine.Color(0.4f, 0.9f, 0.4f, 1f));
lblStatus.SetFontSize(11);

var lblFps = p.AddLabel("FPS: --");
lblFps.SetColor(new UnityEngine.Color(0.6f, 0.6f, 0.6f, 1f));
lblFps.SetFontSize(11);

p.AddSeparator();

// ── LIVE STATS ────────────────────────────────────────────────
p.AddHeader("PLAYER STATS");

var lblMoney = p.AddLabel("$ --");
lblMoney.SetColor(new UnityEngine.Color(0.95f, 0.78f, 0.2f, 1f));
lblMoney.SetFontSize(15);

var lblLevel = p.AddLabel("LVL  --");
lblLevel.SetColor(new UnityEngine.Color(0.5f, 0.85f, 1f, 1f));
lblLevel.SetFontSize(13);

var pbXP = p.AddProgressBar("XP", 0f, new UnityEngine.Color(0.4f, 0.7f, 1f, 1f), height: 18f);

p.AddSeparator();

// ── QUICK ACTIONS ─────────────────────────────────────────────
p.AddHeader("QUICK ACTIONS");

p.AddButton("+ $50 000", () => {
    Il2CppCMS.Shared.SharedGameDataManager.Instance.AddMoneyRpc(50000);
    Print("[GarageOS] +$50,000 dodane!");
}, new UnityEngine.Color(0.15f, 0.28f, 0.1f, 1f));

p.AddButton("+ 5 000 XP", () => {
    Il2CppCMS.Player.PlayerData.AddPlayerExp(5000, true);
    Print("[GarageOS] +5000 XP dodane!");
}, new UnityEngine.Color(0.1f, 0.2f, 0.35f, 1f));

p.AddButton("MEGA CASH  ($1M)", () => {
    var sgdm = Il2CppCMS.Shared.SharedGameDataManager.Instance;
    sgdm.AddMoneyRpc(1000000 - (int)sgdm.money);
    Print("[GarageOS] Kasa ustawiona na $1,000,000!");
}, new UnityEngine.Color(0.3f, 0.22f, 0.0f, 1f));

p.AddSeparator();

// ── SPEED TUNER ───────────────────────────────────────────────
p.AddHeader("SPEED TUNER");

var pbSpeed = p.AddProgressBar("PWR", 0.3f, new UnityEngine.Color(0.9f, 0.5f, 0.1f, 1f), height: 18f);

p.AddSlider("Walk speed", 1f, 50f, 7f, v => {
    var mv = UnityEngine.Object.FindObjectOfType<Il2CppCMS.Player.Controller.PlayerMovement>();
    if (mv?.settings != null) {
        mv.settings.MaxWalkingSpeed = v;
        mv.settings.MaxRunningSpeed = v * 1.6f;
    }
    pbSpeed.SetValue(v / 50f);
}, step: 0.5f);

p.AddToggle("Turbo mode (x4)", false, v => {
    var mv = UnityEngine.Object.FindObjectOfType<Il2CppCMS.Player.Controller.PlayerMovement>();
    if (mv?.settings == null) return;
    mv.settings.MaxWalkingSpeed = v ? 28f : 7f;
    mv.settings.MaxRunningSpeed = v ? 45f : 11f;
    pbSpeed.SetValue(v ? 1f : 0.14f);
    Print("[GarageOS] Turbo: " + (v ? "ON 🔥" : "OFF"));
});

p.AddSeparator();

// ── ENVIRONMENT ───────────────────────────────────────────────
p.AddHeader("ENVIRONMENT");

p.AddSlider("Time scale", 0.1f, 5f, 1f, v => {
    UnityEngine.Time.timeScale = v;
    Print("[GarageOS] timeScale = " + v.ToString("F2"));
}, step: 0.1f);

p.AddToggle("Remove demo walls", false, v => {
    if (!v) return;
    var targets = new[] {
        "DemoWalls", "DemoVehiclesDetector",
        "Garage_Exterior_Demo_Collider",
        "Garage_Exterior_Demo_Wall_Blocked_1"
    };
    int n = 0;
    foreach (var name in targets) {
        var go = UnityEngine.GameObject.Find(name);
        if (go != null) { go.SetActive(false); n++; }
    }
    Print("[GarageOS] Usunięto " + n + " ścian demo.");
});

p.AddSeparator();

// ── ABOUT + EXIT ──────────────────────────────────────────────
var lblAuthor = p.AddLabel("GarageOS · powered by UITK Framework 0.1.0");
lblAuthor.SetColor(new UnityEngine.Color(0.4f, 0.35f, 0.2f, 1f));
lblAuthor.SetFontSize(10);

p.AddSpace(4f);

p.AddButton("✕  Zamknij panel", () => {
    CMS2026UITKFramework.FrameworkAPI.GetPanel("GARAGE OS")?.SetVisible(false);
    Print("[GarageOS] Panel ukryty. Uzyj FrameworkAPI.GetPanel(\"GARAGE OS\")?.SetVisible(true) by przywrocic.");
}, new UnityEngine.Color(0.25f, 0.05f, 0.05f, 1f));

// ── SCROLLBAR ─────────────────────────────────────────────────
p.SetScrollbarVisible(true);
p.SetDragWhenScrollable(true);

// ── UPDATE CALLBACK — live stats + FPS ───────────────────────
float _fpsTimer = 0f;
int   _frames   = 0;
float _fps      = 0f;
float _dotT     = 0f;

p.SetUpdateCallback(dt => {
    // FPS counter
    _frames++;
    _fpsTimer += dt;
    if (_fpsTimer >= 0.5f) {
        _fps      = _frames / _fpsTimer;
        _fpsTimer = 0f;
        _frames   = 0;
    }
    lblFps.SetText("FPS: " + ((int)_fps).ToString());

    // Live money + level
    var sgdm = Il2CppCMS.Shared.SharedGameDataManager.Instance;
    if (sgdm != null)
        lblMoney.SetText("$ " + ((int)sgdm.money).ToString("N0"));

    int lvl = Il2CppCMS.Player.PlayerData.PlayerLevel;
    lblLevel.SetText("LVL  " + lvl.ToString());

    // Pulsujący dot w status barze
    _dotT += dt * 2.5f;
    var dotVE = CMS2026UITKFramework.UIRuntime.WrapVE(lblStatus.GetRawPtr());
    CMS2026UITKFramework.S.Opacity(
        CMS2026UITKFramework.UIRuntime.GetStyle(dotVE),
        (UnityEngine.Mathf.Sin(_dotT) + 1f) * 0.4f + 0.3f
    );
});

// ── złoty border na money labelu przez Pro API ────────────────
var moneyVE = CMS2026UITKFramework.UIRuntime.WrapVE(lblMoney.GetRawPtr());
var ms = CMS2026UITKFramework.UIRuntime.GetStyle(moneyVE);
CMS2026UITKFramework.S.BorderColor(ms, new UnityEngine.Color(0.85f, 0.65f, 0.1f, 0.6f));
CMS2026UITKFramework.S.BorderWidth(ms, 1f);
CMS2026UITKFramework.S.BorderRadius(ms, 5f);
CMS2026UITKFramework.S.Padding(ms, 4f);

Print("[GarageOS] Panel gotowy! Zloty styl, live stats, speed tuner.");