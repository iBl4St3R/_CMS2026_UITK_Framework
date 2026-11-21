// try_api040: test nowego API 0.4.0 (G1–G5). Otwiera panel testowy "UITK 0.4.0 test"; zamknięcie: ponowne uruchomienie skryptu usuwa stary panel.
// Uruchom po zbudowaniu frameworka 0.4.0 w VS: runfile C:\Users\usmat\source\repos\_CMS2026_UITK_Framework\Samples\try_api040.cs
var P = "[probe040] ";
try { CMS2026UITKFramework.FrameworkAPI.DestroyPanel("UITK 0.4.0 test"); } catch { }
Print(P + "framework v" + CMS2026UITKFramework.FrameworkAPI.Version);

// ── G2: FindFont + SetDefaultFont (przed Build, więc tytuł też) ──
UnityEngine.Font rubik = null;
try { rubik = CMS2026UITKFramework.UIRuntime.FindFont("Rubik-VariableFont_wght"); Print(P + "G2 FindFont Rubik=" + (rubik != null ? rubik.name : "NULL")); } catch (Exception ex) { Print(P + "G2 FindFont EX " + ex.Message); }
try { Print(P + "G2 FindFont missing=" + (CMS2026UITKFramework.UIRuntime.FindFont("NoSuchFont_xyz") == null ? "null OK" : "NOT NULL")); } catch (Exception ex) { Print(P + "G2 missing EX " + ex.Message); }

var panel = CMS2026UITKFramework.UIPanel.Create("UITK 0.4.0 test", 80f, 80f, 520f, 640f);
try { panel.SetDefaultFont(rubik); } catch (Exception ex) { Print(P + "G2 SetDefaultFont EX " + ex.Message); }
panel.Build();
panel.AddLabel("Rubik: title and this label (G2)");

// ── G1: Sprite z atlasu gry ──
UnityEngine.Sprite spr = null;
try
{
    var all = UnityEngine.Resources.FindObjectsOfTypeAll(Il2CppType.Of<UnityEngine.Sprite>());
    string[] wanted = { "cat_icon_engine", "icon_onlist" };
    for (int i = 0; i < all.Length && spr == null; i++) { var o = all[i]; if (o != null && wanted.Contains(o.name)) spr = o.TryCast<UnityEngine.Sprite>(); }
    for (int i = 0; i < all.Length && spr == null; i++) { var s = all[i]?.TryCast<UnityEngine.Sprite>(); if (s != null && s.texture != null && s.rect.width < s.texture.width) spr = s; }
    Print(P + "G1 sprite=" + (spr != null ? spr.name + " rect=" + spr.rect + " tex=" + spr.texture.width + "x" + spr.texture.height : "NONE"));
    panel.AddLabel("G1: sprite (one icon, not the whole atlas):");
    var img1 = panel.AddImage(null, 64f, 64f);
    img1.SetSprite(spr);
}
catch (Exception ex) { Print(P + "G1 EX " + ex.Message); }

// ── G3: SetScaleMode na wąskim obrazie (ten sam sprite, 3 tryby) ──
try
{
    panel.AddLabel("G3: ScaleToFit / ScaleAndCrop / StretchToFill (200x60):");
    var setMode = typeof(CMS2026UITKFramework.UIImageHandle).GetMethod("SetScaleMode");
    var modeType = setMode.GetParameters()[0].ParameterType; // UnityEngine.ScaleMode (IMGUIModule, poza referencjami REPL)
    foreach (var m in new[] { "ScaleToFit", "ScaleAndCrop", "StretchToFill" })
    {
        var img = panel.AddImage(null, 200f, 60f);
        img.SetSprite(spr);
        setMode.Invoke(img, new object[] { Enum.Parse(modeType, m) });
    }
    Print(P + "G3 scale modes applied");
}
catch (Exception ex) { Print(P + "G3 EX " + ex.Message); }

// ── G4: flex z zawijaniem, procenty, marginesy (kontener wiersza przez AddRaw) ──
try
{
    panel.AddLabel("G4: 7 cards, wrap, 30% width each, centered:");
    var row = panel.AddRow(110f);
    var box = CMS2026UITKFramework.UIRuntime.NewVE();
    var bs = CMS2026UITKFramework.UIRuntime.GetStyle(box);
    CMS2026UITKFramework.S.Width(bs, 480f); CMS2026UITKFramework.S.Height(bs, 110f);
    CMS2026UITKFramework.S.BgColor(bs, new Color(0.15f, 0.15f, 0.2f, 1f));
    CMS2026UITKFramework.S.FlexDirection(bs, true);
    CMS2026UITKFramework.S.FlexWrap(bs, true);
    CMS2026UITKFramework.S.JustifyContent(bs, "Center");
    CMS2026UITKFramework.S.AlignItems(bs, "Center");
    CMS2026UITKFramework.S.JustifyContent(bs, "NotAValue"); // ma dać warning w logu i nic nie zmienić
    for (int i = 0; i < 7; i++)
    {
        var c = CMS2026UITKFramework.UIRuntime.NewVE();
        var cs = CMS2026UITKFramework.UIRuntime.GetStyle(c);
        CMS2026UITKFramework.S.WidthPercent(cs, 30f);
        CMS2026UITKFramework.S.Height(cs, 30f);
        CMS2026UITKFramework.S.Margin(cs, 3f);
        CMS2026UITKFramework.S.MaxWidth(cs, 140f);
        CMS2026UITKFramework.S.BgColor(cs, new Color(0.3f + i * 0.08f, 0.5f, 0.8f, 1f));
        CMS2026UITKFramework.UIRuntime.AddChild(box, c);
    }
    row.AddRaw(box, 480f);
    Print(P + "G4 flex box built");
}
catch (Exception ex) { Print(P + "G4 EX " + ex.Message); }

// ── G5: IsTextInputFocused (log przy każdej zmianie) ──
try
{
    panel.AddLabel("G5: click the field, type, then click a label:");
    panel.AddTextInput("type here");
    bool last = false;
    panel.SetUpdateCallback(dt =>
    {
        bool now = panel.IsTextInputFocused;
        if (now != last) { last = now; Log(P + "G5 IsTextInputFocused=" + now); }
    });
    Print(P + "G5 watching focus");
}
catch (Exception ex) { Print(P + "G5 EX " + ex.Message); }

CMS2026UITKFramework.CursorManager.Request();
Print(P + "done (cursor requested once; close panel by running the script again or ui_list)");
