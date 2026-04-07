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
        internal static Assembly UEAsm;   // UnityEngine.UIElementsModule
        internal static Assembly TRAsm;   // UnityEngine.TextRenderingModule

        // ── Core types ──────────────────────────────────────────────────────
        internal static Type VisualElementType;
        internal static Type LabelType;
        internal static Type ButtonType;
        internal static Type TextFieldType;
        internal static Type ClickableType;

        // ── Style types ─────────────────────────────────────────────────────
        internal static Type IStyleType;
        internal static Type StyleLengthType;
        internal static Type StyleColorType;
        internal static Type StyleFloatType;
        internal static Type StyleFontDefinitionType;
        internal static Type FontDefinitionType;

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

        // ── Constructors ────────────────────────────────────────────────────
        internal static ConstructorInfo StyleLengthCtor;
        internal static ConstructorInfo StyleColorCtor;
        internal static ConstructorInfo StyleFloatCtor;
        internal static ConstructorInfo StyleFontDefCtor;
        internal static ConstructorInfo StylePositionCtor;
        internal static ConstructorInfo StyleOverflowCtor;
        internal static ConstructorInfo StyleDisplayCtor;
        internal static ConstructorInfo StyleAlignCtor;
        internal static ConstructorInfo StyleJustifyCtor;
        internal static ConstructorInfo StyleTextAnchorCtor;

        // ── Font ────────────────────────────────────────────────────────────
        internal static object DefaultFontDef;

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
        }

        private static Type MakeStyleEnum(Type enumType)
            => UEAsm.GetType("UnityEngine.UIElements.StyleEnum`1").MakeGenericType(enumType);

        private static void ResolveCtors()
        {
            StyleLengthCtor = StyleLengthType.GetConstructor(new[] { typeof(float) });
            StyleColorCtor = StyleColorType.GetConstructor(new[] { typeof(Color) });
            StyleFloatCtor = StyleFloatType.GetConstructor(new[] { typeof(float) });
            StyleFontDefCtor = StyleFontDefinitionType.GetConstructor(new[] { FontDefinitionType });
            StylePositionCtor = StylePositionType.GetConstructor(new[] { PositionType });
            StyleOverflowCtor = StyleOverflowType.GetConstructor(new[] { OverflowType });
            StyleDisplayCtor = StyleDisplayType.GetConstructor(new[] { DisplayStyleType });
            StyleAlignCtor = StyleAlignType.GetConstructor(new[] { AlignType });
            StyleJustifyCtor = StyleJustifyType.GetConstructor(new[] { JustifyType });
            StyleTextAnchorCtor = StyleTextAnchorType.GetConstructor(new[] { TextAnchorType });
            StyleScaleModeCtor = StyleScaleModeType.GetConstructor(new[] { ScaleModeType });
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

        internal static void AddChild(object parent, object child)
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
    }
}