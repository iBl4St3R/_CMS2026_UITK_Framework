# CMS2026 UITK Framework — Pro API
Version 0.1.0 | For advanced modders

## Raw Pointer Access

Every handle exposes its underlying IL2CPP pointer:
```csharp
IntPtr ptr = lbl.GetRawPtr();
IntPtr ptr = btn.GetRawPtr();
IntPtr ptr = tog.GetRawPtr();        // button ptr
IntPtr ptr = input.GetRawPtr();
IntPtr ptr = img.GetRawPtr();
IntPtr ptr = slider.GetFillPtr();
IntPtr ptr = slider.GetValueLblPtr();
IntPtr ptr = pb.GetFillPtr();
IntPtr ptr = pb.GetLabelPtr();
IntPtr ptr = cp.GetPreviewPtr();
IntPtr ptr = cp.GetFillPtr(channel); // 0=R 1=G 2=B
IntPtr ptr = cp.GetValueLblPtr(channel);
IntPtr ptr = panel.GetPanelRawPtr();
```

## UIRuntime Public Helpers
```csharp
// Wrap a raw pointer into a VisualElement object
object ve = UIRuntime.WrapVE(IntPtr ptr);

// Get the IStyle object from a VisualElement
object style = UIRuntime.GetStyle(object ve);

// Create a new empty VisualElement
object ve = UIRuntime.NewVE();

// Get IL2CPP pointer from a VisualElement
IntPtr ptr = UIRuntime.GetPtr(object ve);

// Check if framework is ready
bool ready = UIRuntime.IsAvailable;
```

## S{} — Style Helpers

Apply any style property directly to an IStyle object:
```csharp
var ve    = UIRuntime.WrapVE(ptr);
var style = UIRuntime.GetStyle(ve);

S.Left(style, 10f);
S.Top(style, 20f);
S.Width(style, 200f);
S.Height(style, 50f);
S.BgColor(style, Color.black);
S.Color(style, Color.white);
S.Opacity(style, 0.5f);
S.FontSize(style, 16);
S.Font(style);                              // apply default font
S.TextAlign(style, TextAnchor.MiddleCenter);
S.Padding(style, 4f);
S.Position(style, "Absolute");             // or "Relative"
S.Overflow(style, "Hidden");               // or "Visible"
S.Display(style, bool);
S.BorderColor(style, Color.cyan);
S.BorderWidth(style, 2f);
S.BorderRadius(style, 8f);
S.BgTint(style, Color.red);
S.BgScaleMode(style, ScaleMode.ScaleToFit); // Unity 6: no-op, pending fix
```

## Rounded Panel Example
```csharp
var p = FrameworkAPI.CreatePanel("MyPanel", 100, 100, 300, 400);

var panelVE = UIRuntime.WrapVE(p.GetPanelRawPtr());
var s = UIRuntime.GetStyle(panelVE);
S.BorderRadius(s, 16f);
S.BorderColor(s, new Color(0.4f, 0.8f, 1f, 1f));
S.BorderWidth(s, 2f);
```

## Custom Opacity Animation via UpdateCallback
```csharp
float t = 0f;
var lbl = panel.AddLabel("Pulsing text");
panel.SetUpdateCallback(dt => {
    t += dt;
    var ve = UIRuntime.WrapVE(lbl.GetRawPtr());
    S.Opacity(UIRuntime.GetStyle(ve), (Mathf.Sin(t * 2f) + 1f) * 0.5f);
});
```

## Custom VisualElement from scratch
```csharp
// Create a VE, style it, and add it to an existing panel's content
// (use GetPanelRawPtr as parent — note: this places it on the panel root)
var ve = UIRuntime.NewVE();
var s  = UIRuntime.GetStyle(ve);
S.Position(s, "Absolute");
S.Left(s, 10f); S.Top(s, 50f);
S.Width(s, 100f); S.Height(s, 100f);
S.BgColor(s, Color.red);
S.BorderRadius(s, 50f); // circle

// Add to panel root
var panelVE = UIRuntime.WrapVE(panel.GetPanelRawPtr());
UIRuntime.VisualElementType
    .GetMethod("Add", new[] { UIRuntime.VisualElementType })
    .Invoke(panelVE, new object[] { ve });
```

## WireHover on any Pointer
```csharp
panel.WireHover(
    ptr:    someHandle.GetRawPtr(),
    normal: new Color(0.1f, 0.1f, 0.1f, 1f),
    hover:  new Color(0.2f, 0.4f, 0.6f, 1f),
    press:  new Color(0.05f, 0.2f, 0.4f, 1f)
);
```

## Sort Order Management
```csharp
// Set at creation
var p = FrameworkAPI.CreatePanel("Top", 0, 0, 300, 300, sortOrder: 10000);

// Change at runtime — bring to front
p.SetSortOrder(10000);

// Send to back
p.SetSortOrder(100);
```

## Accessing Panels by Name
```csharp
var p = FrameworkAPI.GetPanel("My Panel Title");
p?.SetVisible(false);

bool destroyed = FrameworkAPI.DestroyPanel("My Panel Title");
```

## Reflection-Only Usage (no DLL reference)
```csharp
var fw = AppDomain.CurrentDomain.GetAssemblies()
    .FirstOrDefault(a => a.GetName().Name == "_CMS2026_UITK_Framework");
if (fw == null) { MelonLogger.Msg("Framework not loaded"); return; }

var api    = fw.GetType("CMS2026UITKFramework.FrameworkAPI");
bool ready = (bool)api.GetProperty("IsReady").GetValue(null);
if (!ready) return;

var panel = api.GetMethod("CreatePanel")
    .Invoke(null, new object[] { "My Mod", 40f, 40f, 300f, 400f, 9999 });
```

## UIRuntime Public Types (for advanced reflection)
```csharp
UIRuntime.VisualElementType   // UnityEngine.UIElements.VisualElement
UIRuntime.LabelType           // UnityEngine.UIElements.Label
UIRuntime.ButtonType          // UnityEngine.UIElements.Button
UIRuntime.TextFieldType       // UnityEngine.UIElements.TextField
UIRuntime.PanelSettingsType   // UnityEngine.UIElements.PanelSettings
UIRuntime.IStyleType          // UnityEngine.UIElements.IStyle (internal — use GetStyle())
```