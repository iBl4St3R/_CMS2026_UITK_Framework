// ════════════════════════════════════════════════════
//   PHONE UI  ·  GTA-style  ·  UITK REPL  (fixed)
// ════════════════════════════════════════════════════

var modsDir = @"C:\Program Files (x86)\Steam\steamapps\common\Car Mechanic Simulator 2026 Demo\Mods\CMS2026SimpleConsole\";

// ── Texture loader helper ─────────────────────────────
Func<string, UnityEngine.Texture2D> loadTex = filename => {
    var path = modsDir + filename;
    if (!System.IO.File.Exists(path)) { Print("Missing: " + filename); return null; }
    var bytes = System.IO.File.ReadAllBytes(path);
    var tex = new UnityEngine.Texture2D(2, 2);
    var asm = System.AppDomain.CurrentDomain.GetAssemblies()
        .First(a => a.GetName().Name == "UnityEngine.ImageConversionModule");
    var il2b = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>(bytes.Length);
    for (int i = 0; i < bytes.Length; i++) il2b[i] = bytes[i];
    asm.GetType("UnityEngine.ImageConversion")
       .GetMethod("LoadImage", new System.Type[]{ typeof(UnityEngine.Texture2D), il2b.GetType() })
       .Invoke(null, new object[]{ tex, il2b });
    return tex;
};

// ── Load textures ─────────────────────────────────────
var texWall     = loadTex("phone_wallpaper.png");
var texPhone    = loadTex("app_phone.png");
var texMessages = loadTex("app_messages.png");
var texMaps     = loadTex("app_maps.png");
var texCamera   = loadTex("app_camera.png");
var texMusic    = loadTex("app_music.png");
var texSettings = loadTex("app_settings.png");
var texGarage   = loadTex("app_garage.png");
var texBrowser  = loadTex("app_browser.png");

// ── Colors ────────────────────────────────────────────
var C_BLACK  = new UnityEngine.Color(0.00f, 0.00f, 0.00f, 1.00f);
var C_BAR    = new UnityEngine.Color(0.00f, 0.00f, 0.00f, 0.65f);
var C_WHITE  = new UnityEngine.Color(1.00f, 1.00f, 1.00f, 1.00f);
var C_TRANS  = new UnityEngine.Color(0.00f, 0.00f, 0.00f, 0.00f);

// ── Panel — phone shape ───────────────────────────────
var p = CMS2026UITKFramework.FrameworkAPI.CreatePanel("Phone", 80, 30, 320, 580, sortOrder: 10000);
p.SetScrollbarVisible(false);
p.SetDragWhenScrollable(true);

var pve = CMS2026UITKFramework.UIRuntime.WrapVE(p.GetPanelRawPtr());
var pst = CMS2026UITKFramework.UIRuntime.GetStyle(pve);
CMS2026UITKFramework.S.BgColor(pst,      C_BLACK);
CMS2026UITKFramework.S.BorderRadius(pst, 32f);
CMS2026UITKFramework.S.BorderWidth(pst,  3f);
CMS2026UITKFramework.S.BorderColor(pst,  new UnityEngine.Color(0.25f, 0.25f, 0.30f, 1f));

// ── Wallpaper ─────────────────────────────────────────
p.AddImage(texWall, 0f, 520f);

p.AddSpace(-520f);

// ── Status bar ────────────────────────────────────────
// FIX: UIRowBuilder nie ma GetRawPtr — pomijamy stylizację tła statusbara
// (alternatywnie: stwórz własny VE przed AddRow i wstaw go przez UIRuntime.AddChild)
var statusRow = p.AddRow(28f, 4f);
var lblTime   = statusRow.AddLabel(System.DateTime.Now.ToString("HH:mm"), 80f, C_WHITE);
lblTime.SetFontSize(13);

p.AddSpace(8f);

// ── App grid ──────────────────────────────────────────
const float ICO  = 60f;
const float IGAP = 12f;

var apps = new (string label, UnityEngine.Texture2D tex, System.Action onClick)[] {
    ("Phone",    texPhone,    () => Print("App: Phone")),
    ("Messages", texMessages, () => Print("App: Messages")),
    ("Maps",     texMaps,     () => Print("App: Maps")),
    ("Camera",   texCamera,   () => Print("App: Camera")),
    ("Music",    texMusic,    () => Print("App: Music")),
    ("Settings", texSettings, () => Print("App: Settings")),
    ("Garage",   texGarage,   () => Print("App: Garage")),
    ("Browser",  texBrowser,  () => Print("App: Browser")),
};

// FIX: UIRowBuilder nie ma AddRow ani AddImage.
// Tworzymy każdą ikonę jako raw VE i wstrzykujemy przez AddRaw().
Func<UnityEngine.Texture2D, System.IntPtr> makeIcon = tex => {
    var ve = CMS2026UITKFramework.UIRuntime.NewVE();
    var s  = CMS2026UITKFramework.UIRuntime.GetStyle(ve);
    CMS2026UITKFramework.S.Width(s,        ICO);
    CMS2026UITKFramework.S.Height(s,       ICO);
    CMS2026UITKFramework.S.BorderRadius(s, 14f);
    if (tex != null) {
        // SetBackgroundImage to metoda wewnętrzna UIRuntime używana przez AddImage
        CMS2026UITKFramework.UIRuntime.SetBackgroundImage(ve, tex);
    } else {
        CMS2026UITKFramework.S.BgColor(s, new UnityEngine.Color(0.2f, 0.3f, 0.5f, 1f));
    }
    return CMS2026UITKFramework.UIRuntime.GetPtr(ve);
};

// Row 1 — apps 0–3
var appRow1 = p.AddRow(ICO, IGAP);
for (int i = 0; i < 4; i++) {
    var icoVE  = CMS2026UITKFramework.UIRuntime.NewVE();
    var icoSt  = CMS2026UITKFramework.UIRuntime.GetStyle(icoVE);
    CMS2026UITKFramework.S.Width(icoSt,        ICO);
    CMS2026UITKFramework.S.Height(icoSt,       ICO);
    CMS2026UITKFramework.S.BorderRadius(icoSt, 14f);
    if (apps[i].tex != null)
        CMS2026UITKFramework.UIRuntime.SetBackgroundImage(icoVE, apps[i].tex);
    else
        CMS2026UITKFramework.S.BgColor(icoSt, new UnityEngine.Color(0.2f, 0.3f, 0.5f, 1f));

    var icoPtr = CMS2026UITKFramework.UIRuntime.GetPtr(icoVE);
    appRow1.AddRaw(icoVE, ICO + IGAP);  // AddRaw przesuwa kursor o ICO+IGAP
    p.WireHover(icoPtr, C_TRANS,
        new UnityEngine.Color(1f,1f,1f,0.15f),
        new UnityEngine.Color(1f,1f,1f,0.30f));
}

p.AddSpace(IGAP);

// Row 2 — apps 4–7
var appRow2 = p.AddRow(ICO, IGAP);
for (int i = 4; i < 8; i++) {
    var icoVE  = CMS2026UITKFramework.UIRuntime.NewVE();
    var icoSt  = CMS2026UITKFramework.UIRuntime.GetStyle(icoVE);
    CMS2026UITKFramework.S.Width(icoSt,        ICO);
    CMS2026UITKFramework.S.Height(icoSt,       ICO);
    CMS2026UITKFramework.S.BorderRadius(icoSt, 14f);
    if (apps[i].tex != null)
        CMS2026UITKFramework.UIRuntime.SetBackgroundImage(icoVE, apps[i].tex);
    else
        CMS2026UITKFramework.S.BgColor(icoSt, new UnityEngine.Color(0.2f, 0.3f, 0.5f, 1f));

    var icoPtr = CMS2026UITKFramework.UIRuntime.GetPtr(icoVE);
    appRow2.AddRaw(icoVE, ICO + IGAP);
    p.WireHover(icoPtr, C_TRANS,
        new UnityEngine.Color(1f,1f,1f,0.15f),
        new UnityEngine.Color(1f,1f,1f,0.30f));
}

p.AddSpace(16f);

// ── Dock ──────────────────────────────────────────────
// FIX: AddSeparator zwraca void — nie przypisujemy do var
p.AddSeparator(new UnityEngine.Color(0.3f,0.3f,0.35f,0.5f));
p.AddSpace(6f);

var dockApps = new (UnityEngine.Texture2D tex, string label)[] {
    (texPhone,    "Phone"),
    (texMessages, "Msg"),
    (texBrowser,  "Web"),
    (texSettings, "Set"),
};

// FIX: UIRowBuilder nie ma AddImage — używamy AddRaw
var dockRow = p.AddRow(ICO, 6f);
foreach (var da in dockApps) {
    var dve  = CMS2026UITKFramework.UIRuntime.NewVE();
    var dst  = CMS2026UITKFramework.UIRuntime.GetStyle(dve);
    CMS2026UITKFramework.S.Width(dst,        ICO);
    CMS2026UITKFramework.S.Height(dst,       ICO);
    CMS2026UITKFramework.S.BorderRadius(dst, 14f);
    if (da.tex != null)
        CMS2026UITKFramework.UIRuntime.SetBackgroundImage(dve, da.tex);
    else
        CMS2026UITKFramework.S.BgColor(dst, new UnityEngine.Color(0.2f, 0.3f, 0.5f, 1f));

    var dptr = CMS2026UITKFramework.UIRuntime.GetPtr(dve);
    dockRow.AddRaw(dve, ICO + IGAP);
    p.WireHover(dptr, C_TRANS,
        new UnityEngine.Color(1f,1f,1f,0.15f),
        new UnityEngine.Color(1f,1f,1f,0.30f));
}

// ── Home indicator ────────────────────────────────────
var homeRow = p.AddRow(6f, 0f);
var homeBar = homeRow.AddLabel("", 120f, C_TRANS);
var hve = CMS2026UITKFramework.UIRuntime.WrapVE(homeBar.GetRawPtr());
CMS2026UITKFramework.S.BgColor(
    CMS2026UITKFramework.UIRuntime.GetStyle(hve),
    new UnityEngine.Color(0.8f, 0.8f, 0.8f, 0.6f));
CMS2026UITKFramework.S.BorderRadius(
    CMS2026UITKFramework.UIRuntime.GetStyle(hve), 3f);

// ── Live clock ────────────────────────────────────────
p.SetUpdateCallback(dt => {
    lblTime.SetText(System.DateTime.Now.ToString("HH:mm"));
});

Print("Phone ready! Textures from: " + modsDir);