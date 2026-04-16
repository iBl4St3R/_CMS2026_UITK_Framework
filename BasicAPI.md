# CMS2026 UITK Framework — Basic API
Version 0.2.2 | Author: Blaster

## Setup

**Option A — direct reference** (add `_CMS2026_UITK_Framework.dll` to your project):
```csharp
using CMS2026UITKFramework;
```

**Option B — reflection** (no hard dependency):
```csharp
var api = AppDomain.CurrentDomain.GetAssemblies()
    .FirstOrDefault(a => a.GetName().Name == "_CMS2026_UITK_Framework")
    ?.GetType("CMS2026UITKFramework.FrameworkAPI");
if (api == null) return; // framework not loaded
```

## Creating a Panel
```csharp
var panel = FrameworkAPI.CreatePanel(
    title:     "My Mod",
    x:         40f,
    y:         40f,
    width:     300f,
    height:    400f,
    sortOrder: 9999   // optional, higher = on top
);
```

## Panel Methods
```csharp
panel.SetVisible(bool);
panel.Toggle();
panel.Destroy();
panel.IsVisible;

panel.SetScrollbarVisible(bool);
panel.SetScrollMode(ScrollMode.Auto);         // Auto or Always
panel.SetScrollbarColors(trackColor, thumbColor);
panel.SetDragWhenScrollable(bool);
panel.ScrollTo(float y);
panel.ScrollToTop();
panel.ScrollToBottom();
panel.SetSortOrder(int);
panel.SetUpdateCallback(dt => { });
panel.GetPanelRawPtr();
```

## Elements

### Label
```csharp
var lbl = panel.AddLabel("Hello!", Color.white, height: 26f);
lbl.SetText("Updated!");
lbl.SetColor(Color.green);
lbl.SetFontSize(16);
lbl.SetSize(200f, 30f);
lbl.SetVisible(bool);
```

### Header
```csharp
var hdr = panel.AddHeader("Section Name");
// returns UILabelHandle, adds separator automatically
```

### Button
```csharp
var btn = panel.AddButton("Click me!", () => { }, bgColor: null, height: 26f);
btn.SetText("New label");
btn.SetBgColor(Color.blue);
btn.SetTextColor(Color.yellow);
btn.SetFontSize(14);
btn.SetSize(200f, 40f);
btn.SetBorderColor(Color.cyan);
btn.SetBorderWidth(2f);
btn.SetBorderRadius(8f);
btn.SetVisible(bool);
// hover/press color feedback is automatic on every button
```

### Toggle
```csharp
var tog = panel.AddToggle("God Mode", initial: false, onChange: v => { });
tog.SetValue(true);
tog.Value;
tog.SetVisible(bool);
```

### Slider
```csharp
var sld = panel.AddSlider("Speed", min: 1f, max: 100f, initial: 10f,
    onChange: v => { }, step: 1f);
sld.SetValue(50f);
sld.Value;
```

### Progress Bar
```csharp
var pb = panel.AddProgressBar("HP", initial: 0.75f, fillColor: Color.green, height: 26f);
pb.SetValue(0.5f);    // 0.0 – 1.0
pb.SetColor(Color.red);
pb.SetVisible(bool);  // hides bar, percent label and name label
pb.Value;
```

### Text Input
```csharp
var input = panel.AddTextInput("placeholder...", onSubmit: v => { }, height: 26f);
input.GetValue();
input.SetValue("text");
input.SetPlaceholder("type here...");
input.SetVisible(bool);
```

### Color Picker
```csharp
var cp = panel.AddColorPicker("Player Color", Color.white, onChange: c => { }, step: 5);
cp.GetValue();
cp.SetValue(Color.red);
cp.SetVisible(bool);  // hides all sliders, buttons and preview swatch
```

### Dropdown
```csharp
var dd = panel.AddDropdown("Mode", new[]{"Normal","Turbo","Debug"},
    selectedIndex: 0, onChange: i => { }, maxVisible: 5);
dd.SelectedIndex;
dd.SelectedValue;
dd.SetSelected(1);
dd.SetOptions(new[]{"A","B","C"}, selectedIndex: 0);
dd.SetVisible(bool);  // hides header button and section label
// dropdown list renders outside panel bounds — not clipped at bottom edge
```

### Image
```csharp
var img = panel.AddImage(texture: myTex, width: 0f, height: 80f);
img.SetTexture(tex);
img.SetSize(200f, 100f);
img.SetTint(new Color(1f, 0.5f, 0.5f, 1f));
img.SetVisible(bool);
```

### Layout Helpers
```csharp
panel.AddSeparator(color: null);
panel.AddSpace(pixels: 8f);
```

### Row (Multi-column layout)
```csharp
var row = panel.AddRow(height: 26f, gap: 4f);

row.RemainingWidth;
row.Height;

row.AddLabel("text", width: 100f, color: Color.white);
row.AddButton("Btn", width: 80f, onClick: () => { }, bgColor: null);
row.AddToggle(width: 64f, initial: false, onChange: v => { });
row.AddProgressBar(width: 120f, initial: 0.5f, fillColor: Color.green);
row.AddSeparator(separatorWidth: 1f, color: null);
row.AddSpace(pixels: 8f);

// Inline dropdown — label rendered to the left of the button
// Pass empty string "" to skip the label
row.AddDropdown("Mode:", new[]{"A","B","C"}, selectedIndex: 0,
    onChanged: i => { }, width: 120f, maxVisible: 5);
```

## Live Update
```csharp
float t = 0f;
panel.SetUpdateCallback(dt => {
    t += dt;
    pb.SetValue((Mathf.Sin(t) + 1f) * 0.5f);
    lbl.SetText($"t = {t:F1}s");
});
```

## Hover Feedback on any Element
```csharp
// Automatic on buttons.
// Manual on labels or custom VEs:
panel.WireHover(
    lbl.GetRawPtr(),
    normal: new Color(0f, 0f, 0f, 0f),
    hover:  new Color(0.2f, 0.4f, 0.6f, 0.5f),
    press:  new Color(0.1f, 0.2f, 0.4f, 0.8f)
);
```

## Loading a Texture from Disk
```csharp
byte[] bytes = System.IO.File.ReadAllBytes(@"Mods\MyMod\icon.png");
var tex = new Texture2D(2, 2);
var imgConvAsm = AppDomain.CurrentDomain.GetAssemblies()
    .First(a => a.GetName().Name == "UnityEngine.ImageConversionModule");
var imgConvType = imgConvAsm.GetType("UnityEngine.ImageConversion");
var il2Bytes = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>(bytes.Length);
for (int i = 0; i < bytes.Length; i++) il2Bytes[i] = bytes[i];
imgConvType.GetMethod("LoadImage", new Type[] { typeof(Texture2D), il2Bytes.GetType() })
    .Invoke(null, new object[] { tex, il2Bytes });
```

## Known Limitations

- UI Toolkit renders **below** the game's native Canvas during scene transitions.
- `SetScaleMode` on Image/Label has no effect — Unity 6 changed the API (`backgroundSize` struct required, not yet implemented).
- `SetSize` on Label/Button does not reflow elements below it (absolute layout).