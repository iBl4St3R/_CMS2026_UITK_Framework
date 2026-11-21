using CMS2026UITKFramework;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CMS2026UITKFramework
{
    /// <summary>
    /// Resolves and caches all UIToolkit types via reflection.
    /// Must be initialized before any panel is built.
    /// </summary>
    public static class UIRuntime
    {
        public static bool IsAvailable { get; private set; }

        // ── Assemblies ──────────────────────────────────────────────────────
        public static Assembly UEAsm;   // UnityEngine.UIElementsModule
        public static Assembly TRAsm;   // UnityEngine.TextRenderingModule

        // ── Core types ──────────────────────────────────────────────────────
        public static Type VisualElementType;
        public static Type LabelType;
        public static Type ButtonType;
        public static Type TextFieldType;
        public static Type ClickableType;

        // ── Style types ─────────────────────────────────────────────────────
        internal static Type IStyleType;
        internal static Type StyleLengthType;
        internal static Type StyleColorType;
        internal static Type StyleFloatType;
        internal static Type StyleFontDefinitionType;
        internal static Type FontDefinitionType;
        internal static Type ScaleType;
        internal static Type StyleScaleType;

        // ── Enum style wrappers ─────────────────────────────────────────────
        internal static Type PositionType;
        internal static Type StylePositionType;
        internal static Type OverflowType;
        internal static Type StyleOverflowType;
        internal static Type DisplayStyleType;
        internal static Type StyleDisplayType;
        internal static Type AlignType;
        internal static Type StyleAlignType;
        internal static Type JustifyType;
        internal static Type StyleJustifyType;
        internal static Type TextAnchorType;
        internal static Type StyleTextAnchorType;
        internal static Type ScaleModeType;
        internal static Type StyleScaleModeType;
        internal static ConstructorInfo StyleScaleModeCtor;

        internal static Type FontStyleType;
        internal static Type StyleFontStyleType;

        // ── Constructors ────────────────────────────────────────────────────
        internal static ConstructorInfo StyleLengthCtor;
        internal static ConstructorInfo StyleColorCtor;
        internal static ConstructorInfo StyleFloatCtor;
        internal static ConstructorInfo StyleFontDefCtor;
        internal static ConstructorInfo StylePositionCtor;
        internal static ConstructorInfo ScaleCtor;
        internal static ConstructorInfo StyleScaleCtor;
        internal static ConstructorInfo StyleOverflowCtor;
        internal static ConstructorInfo StyleDisplayCtor;
        internal static ConstructorInfo StyleAlignCtor;
        internal static ConstructorInfo StyleJustifyCtor;
        internal static ConstructorInfo StyleTextAnchorCtor;
        internal static ConstructorInfo StyleFontStyleCtor;

        // ── Layout / background (0.4.0) — resolved separately, never blocks init ──
        internal static Type LengthType;
        internal static Type LengthUnitType;
        internal static ConstructorInfo LengthCtor;              // Length(float, LengthUnit)
        internal static ConstructorInfo StyleLengthFromLengthCtor; // StyleLength(Length)
        internal static Type FlexDirectionType;
        internal static ConstructorInfo StyleFlexDirectionCtor;
        internal static Type WrapType;
        internal static ConstructorInfo StyleWrapCtor;
        internal static Type BackgroundSizeType;
        internal static Type BackgroundSizeTypeEnum;
        internal static ConstructorInfo BackgroundSizeFromTypeCtor;   // BackgroundSize(BackgroundSizeType)
        internal static ConstructorInfo BackgroundSizeFromLengthsCtor; // BackgroundSize(Length, Length)
        internal static ConstructorInfo StyleBackgroundSizeCtor;
        internal static Type BackgroundPositionType;
        internal static Type BackgroundPositionKeywordType;
        internal static ConstructorInfo BackgroundPositionCtor;       // BackgroundPosition(BackgroundPositionKeyword)
        internal static ConstructorInfo StyleBackgroundPositionCtor;
        internal static Type BackgroundRepeatType;
        internal static Type RepeatType;
        internal static ConstructorInfo BackgroundRepeatCtor;         // BackgroundRepeat(Repeat, Repeat)
        internal static ConstructorInfo StyleBackgroundRepeatCtor;

        // ── Font ────────────────────────────────────────────────────────────
        internal static object DefaultFontDef;
        private static readonly System.Collections.Generic.Dictionary<string, Font> _fontCache = new();

        // ── PanelSettings + UIDocument ──────────────────────────────────────
        internal static Type PanelSettingsType;
        internal static Type UIDocumentType;
        internal static Type PanelScaleModeType;

        public static bool TryResolveTypes()
        {
            try
            {
                var allAsm = AppDomain.CurrentDomain.GetAssemblies();

                UEAsm = allAsm.FirstOrDefault(a =>
                    a.GetName().Name == "UnityEngine.UIElementsModule");
                TRAsm = allAsm.FirstOrDefault(a =>
                    a.GetName().Name == "UnityEngine.TextRenderingModule");

                if (UEAsm == null || TRAsm == null)
                {
                    FrameworkLog.Msg("[UIRuntime] Missing assemblies — UIToolkit unavailable");
                    return false;
                }

                ResolveTypes();
                ResolveCtors();
                SetupFont();
                ResolveLayoutTypes();

                IsAvailable = true;
                return true;
            }
            catch (Exception ex)
            {
                FrameworkPlugin.Log.Error($"[UIRuntime] Init failed: {ex.Message}");
                IsAvailable = false;
                return false;
            }
        }

        private static void ResolveTypes()
        {
            VisualElementType = UEAsm.GetType("UnityEngine.UIElements.VisualElement");
            LabelType = UEAsm.GetType("UnityEngine.UIElements.Label");
            ButtonType = UEAsm.GetType("UnityEngine.UIElements.Button");
            TextFieldType = UEAsm.GetType("UnityEngine.UIElements.TextField");
            ClickableType = UEAsm.GetType("UnityEngine.UIElements.Clickable");
            PanelSettingsType = UEAsm.GetType("UnityEngine.UIElements.PanelSettings");
            UIDocumentType = UEAsm.GetType("UnityEngine.UIElements.UIDocument");
            PanelScaleModeType = UEAsm.GetType("UnityEngine.UIElements.PanelScaleMode");

            IStyleType = UEAsm.GetType("UnityEngine.UIElements.IStyle");
            StyleLengthType = UEAsm.GetType("UnityEngine.UIElements.StyleLength");
            StyleColorType = UEAsm.GetType("UnityEngine.UIElements.StyleColor");
            StyleFloatType = UEAsm.GetType("UnityEngine.UIElements.StyleFloat");
            FontDefinitionType = UEAsm.GetType("UnityEngine.UIElements.FontDefinition");
            StyleFontDefinitionType = UEAsm.GetType("UnityEngine.UIElements.StyleFontDefinition");

            ScaleType = UEAsm.GetType("UnityEngine.UIElements.Scale");
            StyleScaleType = UEAsm.GetType("UnityEngine.UIElements.StyleScale");

            PositionType = UEAsm.GetType("UnityEngine.UIElements.Position");
            StylePositionType = MakeStyleEnum(PositionType);

            OverflowType = UEAsm.GetType("UnityEngine.UIElements.Overflow");
            StyleOverflowType = MakeStyleEnum(OverflowType);

            DisplayStyleType = UEAsm.GetType("UnityEngine.UIElements.DisplayStyle");
            StyleDisplayType = MakeStyleEnum(DisplayStyleType);

            AlignType = UEAsm.GetType("UnityEngine.UIElements.Align");
            StyleAlignType = MakeStyleEnum(AlignType);

            JustifyType = UEAsm.GetType("UnityEngine.UIElements.Justify");
            StyleJustifyType = MakeStyleEnum(JustifyType);

            TextAnchorType = typeof(UnityEngine.TextAnchor);
            StyleTextAnchorType = MakeStyleEnum(TextAnchorType);
            ScaleModeType = typeof(UnityEngine.ScaleMode);
            StyleScaleModeType = MakeStyleEnum(ScaleModeType);
            FontStyleType = typeof(UnityEngine.FontStyle);
            StyleFontStyleType = MakeStyleEnum(FontStyleType);
        }

        private static Type MakeStyleEnum(Type enumType)
            => UEAsm.GetType("UnityEngine.UIElements.StyleEnum`1").MakeGenericType(enumType);

        private static void ResolveCtors()
        {
            StyleLengthCtor = StyleLengthType.GetConstructor(new[] { typeof(float) });
            StyleColorCtor = StyleColorType.GetConstructor(new[] { typeof(Color) });
            StyleFloatCtor = StyleFloatType.GetConstructor(new[] { typeof(float) });
            StyleFontDefCtor = StyleFontDefinitionType.GetConstructor(new[] { FontDefinitionType });
            ScaleCtor = ScaleType.GetConstructor(new[] { typeof(Vector3) });
            StyleScaleCtor = StyleScaleType.GetConstructor(new[] { ScaleType });
            StylePositionCtor = StylePositionType.GetConstructor(new[] { PositionType });
            StyleOverflowCtor = StyleOverflowType.GetConstructor(new[] { OverflowType });
            StyleDisplayCtor = StyleDisplayType.GetConstructor(new[] { DisplayStyleType });
            StyleAlignCtor = StyleAlignType.GetConstructor(new[] { AlignType });
            StyleJustifyCtor = StyleJustifyType.GetConstructor(new[] { JustifyType });
            StyleTextAnchorCtor = StyleTextAnchorType.GetConstructor(new[] { TextAnchorType });
            StyleScaleModeCtor = StyleScaleModeType.GetConstructor(new[] { ScaleModeType });
            StyleFontStyleCtor = StyleFontStyleType.GetConstructor(new[] { FontStyleType });
        }

        /// <summary>Flex, percent, margin and background-size types (0.4.0). Failure only disables those helpers, never the framework.</summary>
        private static void ResolveLayoutTypes()
        {
            try
            {
                LengthType = UEAsm.GetType("UnityEngine.UIElements.Length");
                LengthUnitType = UEAsm.GetType("UnityEngine.UIElements.LengthUnit");
                LengthCtor = LengthType?.GetConstructor(new[] { typeof(float), LengthUnitType });
                StyleLengthFromLengthCtor = LengthType != null ? StyleLengthType.GetConstructor(new[] { LengthType }) : null;

                FlexDirectionType = UEAsm.GetType("UnityEngine.UIElements.FlexDirection");
                if (FlexDirectionType != null) StyleFlexDirectionCtor = MakeStyleEnum(FlexDirectionType).GetConstructor(new[] { FlexDirectionType });
                WrapType = UEAsm.GetType("UnityEngine.UIElements.Wrap");
                if (WrapType != null) StyleWrapCtor = MakeStyleEnum(WrapType).GetConstructor(new[] { WrapType });

                BackgroundSizeType = UEAsm.GetType("UnityEngine.UIElements.BackgroundSize");
                BackgroundSizeTypeEnum = UEAsm.GetType("UnityEngine.UIElements.BackgroundSizeType");
                if (BackgroundSizeType != null && BackgroundSizeTypeEnum != null)
                {
                    BackgroundSizeFromTypeCtor = BackgroundSizeType.GetConstructor(new[] { BackgroundSizeTypeEnum });
                    if (LengthType != null) BackgroundSizeFromLengthsCtor = BackgroundSizeType.GetConstructor(new[] { LengthType, LengthType });
                    StyleBackgroundSizeCtor = UEAsm.GetType("UnityEngine.UIElements.StyleBackgroundSize")?.GetConstructor(new[] { BackgroundSizeType });
                }

                BackgroundPositionType = UEAsm.GetType("UnityEngine.UIElements.BackgroundPosition");
                BackgroundPositionKeywordType = UEAsm.GetType("UnityEngine.UIElements.BackgroundPositionKeyword");
                if (BackgroundPositionType != null && BackgroundPositionKeywordType != null)
                {
                    BackgroundPositionCtor = BackgroundPositionType.GetConstructor(new[] { BackgroundPositionKeywordType });
                    StyleBackgroundPositionCtor = UEAsm.GetType("UnityEngine.UIElements.StyleBackgroundPosition")?.GetConstructor(new[] { BackgroundPositionType });
                }

                BackgroundRepeatType = UEAsm.GetType("UnityEngine.UIElements.BackgroundRepeat");
                RepeatType = UEAsm.GetType("UnityEngine.UIElements.Repeat");
                if (BackgroundRepeatType != null && RepeatType != null)
                {
                    BackgroundRepeatCtor = BackgroundRepeatType.GetConstructor(new[] { RepeatType, RepeatType });
                    StyleBackgroundRepeatCtor = UEAsm.GetType("UnityEngine.UIElements.StyleBackgroundRepeat")?.GetConstructor(new[] { BackgroundRepeatType });
                }
            }
            catch (Exception ex)
            {
                FrameworkPlugin.Log.Warning($"[UIRuntime] Layout types unavailable (flex/percent/scale-mode helpers disabled): {ex.Message}");
            }
        }

        private static void SetupFont()
        {
            var fontType = TRAsm.GetType("UnityEngine.Font");
            var builtIn = Resources.GetBuiltinResource(
                Il2CppInterop.Runtime.Il2CppType.From(fontType), "LegacyRuntime.ttf");
            var wrapped = Activator.CreateInstance(fontType, new object[] { builtIn.Pointer });
            DefaultFontDef = FontDefinitionType
                .GetMethod("FromFont")
                .Invoke(null, new object[] { wrapped });
        }

        // ── Internal helpers used by builders ───────────────────────────────

        public static object NewVE()
            => Activator.CreateInstance(VisualElementType);

        public static object WrapVE(IntPtr ptr)
           => Activator.CreateInstance(VisualElementType, new object[] { ptr });

        public static object GetStyle(object ve)
            => VisualElementType.GetProperty("style").GetValue(ve);

        public static IntPtr GetPtr(object ve)
            => ((Il2CppSystem.Object)ve).Pointer;

        public static void AddChild(object parent, object child)
            => VisualElementType
                .GetMethod("Add", new[] { VisualElementType })
                .Invoke(parent, new object[] { child });

        internal static object CreatePanelSettings()
        {
            var il2Type = Il2CppInterop.Runtime.Il2CppType.From(PanelSettingsType);
            var raw = UnityEngine.ScriptableObject.CreateInstance(il2Type);
            var wrap = Activator.CreateInstance(PanelSettingsType, new object[] { raw.Pointer });

            PanelSettingsType.GetProperty("scaleMode")
                .SetValue(wrap, Enum.Parse(PanelScaleModeType, "ConstantPixelSize"));
            PanelSettingsType.GetProperty("scale").SetValue(wrap, 1.0f);
            PanelSettingsType.GetProperty("sortingOrder").SetValue(wrap, 9999);

            // Wycisz warning — przypisz pusty ThemeStyleSheet jeśli istnieje
            try
            {
                var tssType = UEAsm.GetType("UnityEngine.UIElements.ThemeStyleSheet");
                if (tssType != null)
                {
                    var tssIl2 = Il2CppInterop.Runtime.Il2CppType.From(tssType);
                    var tss = UnityEngine.ScriptableObject.CreateInstance(tssIl2);
                    var tssWrap = Activator.CreateInstance(tssType, new object[] { tss.Pointer });
                    var prop = PanelSettingsType.GetProperty("themeStyleSheet");
                    prop?.SetValue(wrap, tssWrap);
                }
            }
            catch { /* jeśli się nie uda — nie szkodzi, warning jest tylko kosmetyczny */ }

            return wrap;
        }

        internal static object CreateWorldSpacePanelSettings(bool colliderIsTrigger, float pixelsPerUnitOverride, out float pixelsPerUnit)
        {
            var il2Type = Il2CppInterop.Runtime.Il2CppType.From(PanelSettingsType);
            var raw = UnityEngine.ScriptableObject.CreateInstance(il2Type);
            var wrap = Activator.CreateInstance(PanelSettingsType, new object[] { raw.Pointer });

            PanelSettingsType.GetProperty("scaleMode").SetValue(wrap, Enum.Parse(PanelScaleModeType, "ConstantPixelSize"));
            PanelSettingsType.GetProperty("scale").SetValue(wrap, 1.0f);
            PanelSettingsType.GetProperty("sortingOrder").SetValue(wrap, 9999);

            var renderModeType = UEAsm.GetType("UnityEngine.UIElements.PanelRenderMode");
            PanelSettingsType.GetProperty("renderMode").SetValue(wrap, Enum.Parse(renderModeType, "WorldSpace"));
            PanelSettingsType.GetProperty("colliderIsTrigger").SetValue(wrap, colliderIsTrigger);

            if (pixelsPerUnitOverride > 0f)
                PanelSettingsType.GetProperty("pixelsPerUnit").SetValue(wrap, pixelsPerUnitOverride);
            pixelsPerUnit = (float)PanelSettingsType.GetProperty("pixelsPerUnit").GetValue(wrap);

            try
            {
                var tssType = UEAsm.GetType("UnityEngine.UIElements.ThemeStyleSheet");
                if (tssType != null)
                {
                    var tssIl2 = Il2CppInterop.Runtime.Il2CppType.From(tssType);
                    var tss = UnityEngine.ScriptableObject.CreateInstance(tssIl2);
                    var tssWrap = Activator.CreateInstance(tssType, new object[] { tss.Pointer });
                    var prop = PanelSettingsType.GetProperty("themeStyleSheet");
                    prop?.SetValue(wrap, tssWrap);
                }
            }
            catch { /* kosmetyczne, nie krytyczne */ }

            return wrap;
        }


        public static void SetBackgroundImage(object ve, Texture2D tex)
        {
            if (tex == null) return;
            try
            {
                var bgType = UEAsm.GetType("UnityEngine.UIElements.Background");
                var sbgType = UEAsm.GetType("UnityEngine.UIElements.StyleBackground");
                var fromTex = bgType.GetMethod("FromTexture2D",
                                  BindingFlags.Public | BindingFlags.Static);
                var il2Tex = Activator.CreateInstance(typeof(Texture2D), new object[] { tex.Pointer });
                var bgValue = fromTex.Invoke(null, new object[] { il2Tex });
                var sbgCtor = sbgType.GetConstructor(new[] { bgType });
                var sbgValue = sbgCtor.Invoke(new object[] { bgValue });
                IStyleType.GetProperty("backgroundImage").SetValue(GetStyle(ve), sbgValue);
            }
            catch (Exception ex)
            {
                FrameworkPlugin.Log.Error($"[UIRuntime] SetBackgroundImage: {ex.Message}");
            }
        }

        /// <summary>Same as SetBackgroundImage but binds a live RenderTexture (used by the media player for video playback) instead of a static Texture2D.</summary>
        public static void SetBackgroundRenderTexture(object ve, RenderTexture rt)
        {
            if (rt == null) return;
            try
            {
                var bgType = UEAsm.GetType("UnityEngine.UIElements.Background");
                var sbgType = UEAsm.GetType("UnityEngine.UIElements.StyleBackground");
                var fromRT = bgType.GetMethod("FromRenderTexture", BindingFlags.Public | BindingFlags.Static);
                var il2Rt = Activator.CreateInstance(typeof(RenderTexture), new object[] { rt.Pointer });
                var bgValue = fromRT.Invoke(null, new object[] { il2Rt });
                var sbgCtor = sbgType.GetConstructor(new[] { bgType });
                var sbgValue = sbgCtor.Invoke(new object[] { bgValue });
                IStyleType.GetProperty("backgroundImage").SetValue(GetStyle(ve), sbgValue);
            }
            catch (Exception ex)
            {
                FrameworkPlugin.Log.Error($"[UIRuntime] SetBackgroundRenderTexture: {ex.Message}");
            }
        }

        /// <summary>Sets a Sprite as the element background (Background.FromSprite). Unlike sprite.texture, only the sprite's rect is drawn, so atlas icons work.</summary>
        public static void SetBackgroundSprite(object ve, Sprite sprite)
        {
            if (sprite == null) return;
            try
            {
                var bgType = UEAsm.GetType("UnityEngine.UIElements.Background");
                var sbgType = UEAsm.GetType("UnityEngine.UIElements.StyleBackground");
                var fromSprite = bgType.GetMethod("FromSprite", BindingFlags.Public | BindingFlags.Static);
                var il2Sprite = Activator.CreateInstance(typeof(Sprite), new object[] { sprite.Pointer });
                var bgValue = fromSprite.Invoke(null, new object[] { il2Sprite });
                var sbgValue = sbgType.GetConstructor(new[] { bgType }).Invoke(new object[] { bgValue });
                IStyleType.GetProperty("backgroundImage").SetValue(GetStyle(ve), sbgValue);
            }
            catch (Exception ex)
            {
                FrameworkPlugin.Log.Error($"[UIRuntime] SetBackgroundSprite: {ex.Message}");
            }
        }

        /// <summary>Finds a loaded font by name (exact, then case-insensitive), e.g. "Rubik-VariableFont_wght". Returns null when not found. Main thread only.</summary>
        public static Font FindFont(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            try
            {
                if (_fontCache.TryGetValue(name, out var cached) && cached != null) return cached;

                var all = Resources.FindObjectsOfTypeAll(Il2CppInterop.Runtime.Il2CppType.Of<Font>());
                Font exact = null, ci = null;
                foreach (var o in all)
                {
                    if (o == null) continue;
                    if (exact == null && o.name == name) exact = o.TryCast<Font>();
                    else if (ci == null && string.Equals(o.name, name, StringComparison.OrdinalIgnoreCase)) ci = o.TryCast<Font>();
                }
                var found = exact ?? ci;
                if (found != null) { _fontCache[name] = found; return found; }
                FrameworkPlugin.Log.Warning($"[UIRuntime] FindFont: font '{name}' not found");
            }
            catch (Exception ex)
            {
                FrameworkPlugin.Log.Warning($"[UIRuntime] FindFont: {ex.Message}");
            }
            return null;
        }

        /// <summary>FontDefinition.FromFont(font) boxed for style setters; null when font is null or conversion fails.</summary>
        internal static object MakeFontDefinition(Font font)
        {
            if (font == null) return null;
            try
            {
                var il2Font = Activator.CreateInstance(typeof(Font), new object[] { font.Pointer });
                return FontDefinitionType.GetMethod("FromFont").Invoke(null, new object[] { il2Font });
            }
            catch (Exception ex)
            {
                FrameworkPlugin.Log.Warning($"[UIRuntime] MakeFontDefinition: {ex.Message}");
                return null;
            }
        }

        /// <summary>True when the focused element of the UI Toolkit panel that owns <paramref name="ve"/> is a text input (TextField and friends).</summary>
        internal static bool IsTextInputFocusedIn(object ve)
        {
            if (ve == null) return false;
            try
            {
                var panelObj = VisualElementType.GetProperty("panel")?.GetValue(ve);
                var fc = panelObj?.GetType().GetProperty("focusController")?.GetValue(panelObj);
                var focused = fc?.GetType().GetProperty("focusedElement")?.GetValue(fc);
                if (focused == null) return false;
                // Wrapper ma typ zadeklarowany (Focusable); prawdziwy typ bierzemy z IL2CPP. Przycisk po kliknięciu też trzyma fokus, więc liczymy tylko elementy tekstowe.
                if (focused is Il2CppSystem.Object o) return (o.GetIl2CppType()?.Name ?? "").Contains("Text");
                return false;
            }
            catch { return false; }
        }
    }
}