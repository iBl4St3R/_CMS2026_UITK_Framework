var p = CMS2026UITKFramework.FrameworkAPI.CreatePanel("Framework Demo", 40, 40, 320, 600, sortOrder: 9999);

p.AddHeader("Labels");
p.AddLabel("Zwykły tekst");
var lbl = p.AddLabel("Duży + kolor + border");
lbl.SetFontSize(16);
lbl.SetColor(new UnityEngine.Color(1f, 0.8f, 0.2f, 1f));
lbl.SetBorderColor(new UnityEngine.Color(0.3f, 0.7f, 1f, 1f));
lbl.SetBorderWidth(1f);
lbl.SetBorderRadius(6f);

p.AddHeader("Buttons");
p.AddButton("Normalny",  () => Print("klik!"));
p.AddButton("Zielony",   () => Print("zielony!"),  new UnityEngine.Color(0.1f, 0.35f, 0.1f, 1f));
p.AddButton("Czerwony",  () => Print("czerwony!"), new UnityEngine.Color(0.35f, 0.1f, 0.1f, 1f));

p.AddSeparator();

p.AddHeader("Toggles");
p.AddToggle("God Mode",       false, v => Print("GodMode: " + v));
p.AddToggle("Wireframe View", true,  v => Print("Wireframe: " + v));

p.AddHeader("Sliders");
p.AddSlider("Speed",   1f, 100f, 35f, v => Print("Speed: " + v));
p.AddSlider("Gravity", 0f, 200f, 98f, v => Print("Gravity: " + v));

p.AddHeader("Progress Bars");
var pbHP   = p.AddProgressBar("HP",   0.75f, UnityEngine.Color.green,                       height: 22f);
var pbXP   = p.AddProgressBar("XP",   0.42f, new UnityEngine.Color(0.3f, 0.7f, 1f, 1f),    height: 22f);
var pbFuel = p.AddProgressBar("Fuel", 0.18f, new UnityEngine.Color(0.9f, 0.45f, 0.2f, 1f), height: 22f);

p.AddHeader("Text Input");
p.AddTextInput("Wpisz nazwę gracza...", v => Print("Submit: " + v));

p.AddHeader("Dropdown");
p.AddDropdown("Tryb", new[] { "Normal", "Turbo", "Debug", "Sandbox" }, 0, i => Print("Tryb: " + i));

p.AddHeader("Color Picker");
p.AddColorPicker("Player Color", UnityEngine.Color.red, c => Print("RGB: " + c), step: 5);

p.AddSeparator();

p.AddHeader("Live Update");
var lblT     = p.AddLabel("t = 0.0s");
var pbAnim   = p.AddProgressBar("sin(t)", 0.5f, new UnityEngine.Color(0.7f, 0.4f, 1f, 1f), height: 20f);
var lblPulse = p.AddLabel("● pulsujacy label");
lblPulse.SetColor(new UnityEngine.Color(1f, 0.8f, 0.2f, 1f));

float t = 0f;
p.SetUpdateCallback(dt => {
    t += dt;
    lblT.SetText("t = " + t.ToString("F1") + "s");
    pbAnim.SetValue((UnityEngine.Mathf.Sin(t * 2f) + 1f) * 0.5f);
    var ve = CMS2026UITKFramework.UIRuntime.WrapVE(lblPulse.GetRawPtr());
    CMS2026UITKFramework.S.Opacity(
        CMS2026UITKFramework.UIRuntime.GetStyle(ve),
        (UnityEngine.Mathf.Sin(t * 4f) + 1f) * 0.5f
    );
});

p.WireHover(
    lblPulse.GetRawPtr(),
    new UnityEngine.Color(0f,    0f,    0f,    0f),
    new UnityEngine.Color(0.2f,  0.4f,  0.6f,  0.5f),
    new UnityEngine.Color(0.05f, 0.2f,  0.4f,  0.8f)
);

p.SetScrollbarVisible(true);
p.SetDragWhenScrollable(true);

var panelVE = CMS2026UITKFramework.UIRuntime.WrapVE(p.GetPanelRawPtr());
var s = CMS2026UITKFramework.UIRuntime.GetStyle(panelVE);
CMS2026UITKFramework.S.BorderRadius(s, 14f);
CMS2026UITKFramework.S.BorderColor(s, new UnityEngine.Color(0.3f, 0.7f, 1f, 1f));
CMS2026UITKFramework.S.BorderWidth(s, 2f);

var p2 = CMS2026UITKFramework.FrameworkAPI.CreatePanel("Za głównym (z:100)", 80, 80, 220, 100, sortOrder: 100);
p2.AddLabel("Jestem pod głównym panelem.");

Print("=== Demo gotowe! ===");