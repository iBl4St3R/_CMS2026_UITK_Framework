[center][url=https://github.com/iBl4St3R/_CMS2026_UITK_Framework/blob/main/CHANGELOG.md][img]https://img.shields.io/badge/Full%20Changelog-GitHub-181717?style=for-the-badge&logo=github[/img][/url][/center]
[center][size=6][b]UITK_Framework[/b][/size][/center]

[color=yellow][b]⚠️ NOTICE[/b][/color]
[quote]Once the full game is available to play, this tool will no longer support the demo in any capacity.[/quote]

[center]
[color=skyblue][b]Version:[/b] 0.4.0[/color] | [color=green][b]License:[/b] MIT[/color] | [color=red][b]MelonLoader:[/b] v0.7.2+[/color] | [color=orange][b]Game:[/b] CMS2027[/color][/center]
[center][/center]
[center][color=orange][url=https://github.com/iBl4St3R/_CMS2026_UITK_Framework][img]https://img.shields.io/badge/GitHub-Source%20Code-181717?style=for-the-badge&logo=github[/img][/url][/color]
[/center]

[center]─────────────────────────────────────────[/center]

[color=yellow][size=4][b]⚠️ IMPORTANT FOR PLAYERS[/b][/size][/color]

[color=red][b]THIS IS NOT A STANDALONE GAME MOD.[/b][/color] It is a developer tool and a library required by other mods to display their user interfaces. Only install this if another mod requires it or if you are a mod developer.

[b]Update to 0.4.0 if you use BlastCoreOS.[/b] It needs the new features from this version. Older mods keep working on 0.4.0 without changes.

[center]─────────────────────────────────────────[/center]

A UI framework for [b]Car Mechanic Simulator 2027[/b] mods.
Interfaces with Unity 6's UI Toolkit via MelonLoader and IL2CPP, giving mod developers a clean, fluent API to build in-game panels.

[center]─────────────────────────────────────────[/center]

[size=4][b]🆕 What's new in 0.4.0[/b][/size]

Only new API was added. Nothing was removed or renamed, so mods built for 0.3.1 keep working.
[list]
[*]Sprites as backgrounds: [font=Courier New]UIRuntime.SetBackgroundSprite[/font] and [font=Courier New]UIImageHandle.SetSprite[/font]. Icons from game atlases work.[/*]
[*]Fonts: [font=Courier New]UIRuntime.FindFont[/font], [font=Courier New]S.Font(style, Font)[/font] and [font=Courier New]UIPanel.SetDefaultFont[/font].[/*]
[*]Flex layout, margins, percent sizes and min/max sizes in [font=Courier New]S[/font].[/*]
[*][font=Courier New]UIPanel.IsTextInputFocused[/font] so your hotkeys can ignore typing.[/*]
[*]Fixed: [font=Courier New]SetScaleMode[/font] and [font=Courier New]S.BgScaleMode[/font] now work.[/*]
[*]Fixed: prompt icons no longer turn into plain colored circles after loading a save.[/*]
[/list]

[center]─────────────────────────────────────────[/center]

[size=4][b]📂 Samples & Examples[/b][/size]

If you want to see the framework in action or learn how to implement specific features, check the [url=https://github.com/iBl4St3R/_CMS2026_UITK_Framework/tree/main/Samples][b]Samples folder[/b][/url] in the repository. It contains ready-to-use code snippets for various UI layouts. 

[center]─────────────────────────────────────────[/center]

[size=4][b]🛠 Installation[/b][/size]

[list=1]
[*]Copy [font=Courier New]_CMS2026_UITK_Framework.dll[/font] and the [font=Courier New]_CMS2026_UITK_Framework[/font] folder (prompt icons) into your [font=Courier New]Mods/[/font] folder.[/*]
[*]The framework loads automatically with [font=Courier New]MelonPriority(-100)[/font], so it initializes before other mods.[/*]
[/list]

[center]─────────────────────────────────────────[/center]

[size=4][b]🚀 Quick Start[/b][/size]

[quote]using CMS2026UITKFramework;

public class MyMod : MelonMod
{
public override void OnSceneWasLoaded(int idx, string name)
{
if (!FrameworkAPI.IsReady) return;

var panel = FrameworkAPI.CreatePanel("My Mod", 40, 40, 300, 400);

panel.AddHeader("Controls");
panel.AddButton("Do something", () => MelonLogger.Msg("clicked!"));
panel.AddToggle("God Mode", false, v => SetGodMode(v));
panel.AddSlider("Speed", 1f, 100f, 10f, v => SetSpeed(v));

panel.SetScrollbarVisible(true);
panel.SetDragWhenScrollable(true);
}
}[/quote]
[size=4][b]✨ Features[/b][/size]

[list]
[*][font=Courier New]AddLabel[/font]: static or dynamic text[/*]
[*][font=Courier New]AddHeader[/font]: section title with separator[/*]
[*][font=Courier New]AddButton[/font]: clickable button with auto hover/press[/*]
[*][font=Courier New]AddToggle[/font]: ON/OFF switch with callback[/*]
[*][font=Courier New]AddSlider[/font]: value slider with +/- buttons and drag[/*]
[*][font=Courier New]AddProgressBar[/font]: horizontal bar from 0 to 1[/*]
[*][font=Courier New]AddTextInput[/font]: single line text field[/*]
[*][font=Courier New]AddColorPicker[/font]: RGB picker with channel sliders[/*]
[*][font=Courier New]AddDropdown[/font]: expandable option list[/*]
[*][font=Courier New]AddImage[/font]: texture or sprite display with tint support[/*]
[*][font=Courier New]AddSeparator[/font]: horizontal divider for visual grouping[/*]
[*][font=Courier New]AddSpace[/font]: empty vertical gap for layout spacing[/*]
[*][font=Courier New]AddRow[/font]: horizontal row container for multi column layouts[/*]
[/list]

[b]Panel capabilities:[/b]
[list]
[*]Draggable title bar and mouse wheel scrolling.[/*]
[*]Sort order control for z-layering between multiple panels.[/*]
[*]Per-frame update callbacks for animations and live data tracking.[/*]
[*]World space panels rendered onto objects in the game world.[/*]
[*]Prompt queue, keybinding shortcuts bar and world markers.[/*]
[*]Advanced styling (rounded corners, borders, opacity, flex layout, custom fonts) via Pro API.[/*]
[/list]

[center]─────────────────────────────────────────[/center]

[size=4][b]📚 Documentation[/b][/size]

Detailed documentation is available for both beginners and advanced users:
[list]
[*]📖 [url=https://github.com/iBl4St3R/_CMS2026_UITK_Framework/blob/main/BasicAPI.md][b]Basic API Documentation[/b][/url]: recommended for most modders. Covers elements, callbacks, and basic panel setup.[/*]
[*]🛡️ [url=https://github.com/iBl4St3R/_CMS2026_UITK_Framework/blob/main/ProAPI.md][b]Pro API Documentation[/b][/url]: for advanced usage, including raw pointers, S{} styles, and custom Visual Elements.[/*]
[*]📝 [url=https://github.com/iBl4St3R/_CMS2026_UITK_Framework/blob/main/CHANGELOG.md][b]Changelog[/b][/url]: full history of all versions.[/*]
[/list]

[center]─────────────────────────────────────────[/center]

[size=4][b]⚠️ Known Limitations[/b][/size]

[list]
[*]Framework UI renders below the game's native Canvas during scene loading screens due to Unity architecture.[/*]
[*][font=Courier New]SetSize[/font] on Labels/Buttons uses absolute positioning and does not trigger element reflow.[/*]
[*]The default font has no emoji glyphs, so emoji render as empty boxes.[/*]
[/list]



[size=4][b]📄 License[/b][/size]

This framework is released under the [url=https://github.com/iBl4St3R/_CMS2026_UITK_Framework/blob/main/LICENSE][b]MIT License[/b][/url]. 
