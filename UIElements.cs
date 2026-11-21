using CMS2026UITKFramework;
using System;
using System.Linq;
using UnityEngine;

namespace CMS2026UITKFramework
{
    /// <summary>
    /// Style helpers — thin wrappers around UIRuntime reflection calls.
    /// </summary>
    public static class S
    {
        static object Sl(float v)
            => UIRuntime.StyleLengthCtor.Invoke(new object[] { v });

        static object Sc(Color c)
            => UIRuntime.StyleColorCtor.Invoke(new object[] { c });

        static object Sf(float v)
            => UIRuntime.StyleFloatCtor.Invoke(new object[] { v });

        static void SetProp(object style, string name, object value)
            => UIRuntime.IStyleType.GetProperty(name).SetValue(style, value);

        public static void Left(object s, float v) => SetProp(s, "left", Sl(v));
        public static void Right(object s, float v) => SetProp(s, "right", Sl(v));
        public static void Top(object s, float v) => SetProp(s, "top", Sl(v));
        public static void Bottom(object s, float v) => SetProp(s, "bottom", Sl(v));
        public static void Width(object s, float v) => SetProp(s, "width", Sl(v));
        public static void Height(object s, float v) => SetProp(s, "height", Sl(v));
        public static void MinHeight(object s, float v) => SetProp(s, "minHeight", Sl(v));
        public static void Opacity(object s, float v) => SetProp(s, "opacity", Sf(v));

        public static void BgColor(object s, Color c) => SetProp(s, "backgroundColor", Sc(c));
        public static void Color(object s, Color c) => SetProp(s, "color", Sc(c));

        public static void Font(object s) => SetProp(s, "unityFontDefinition", UIRuntime.StyleFontDefCtor.Invoke(new object[] { UIRuntime.DefaultFontDef }));

        public static void WhiteSpaceNormal(object s)
        {
            var wsType = UIRuntime.UEAsm.GetType("UnityEngine.UIElements.WhiteSpace");
            var swsType = UIRuntime.UEAsm.GetType("UnityEngine.UIElements.StyleEnum`1").MakeGenericType(wsType);
            var ctor = swsType.GetConstructor(new[] { wsType });
            SetProp(s, "whiteSpace", ctor.Invoke(new object[] { Enum.Parse(wsType, "Normal") }));
        }


        public static void Position(object s, string v)
            => SetProp(s, "position",
                UIRuntime.StylePositionCtor.Invoke(
                    new object[] { Enum.Parse(UIRuntime.PositionType, v) }));

        public static void Overflow(object s, string v)
            => SetProp(s, "overflow",
                UIRuntime.StyleOverflowCtor.Invoke(
                    new object[] { Enum.Parse(UIRuntime.OverflowType, v) }));

        public static void Display(object s, bool show)
            => SetProp(s, "display",
                UIRuntime.StyleDisplayCtor.Invoke(
                    new object[] { Enum.Parse(UIRuntime.DisplayStyleType,
                                              show ? "Flex" : "None") }));

        public static void TextAlign(object s, UnityEngine.TextAnchor a)
            => SetProp(s, "unityTextAlign",
                UIRuntime.StyleTextAnchorCtor.Invoke(new object[] { a }));

        public static void Padding(object s, float v)
        {
            SetProp(s, "paddingLeft", Sl(v));
            SetProp(s, "paddingRight", Sl(v));
            SetProp(s, "paddingTop", Sl(v));
            SetProp(s, "paddingBottom", Sl(v));
        }

        public static void FontSize(object s, int px)
    => SetProp(s, "fontSize", Sl(px));   // StyleLength przyjmuje int też

        public static void BorderRadius(object s, float v)
        {
            SetProp(s, "borderTopLeftRadius", Sl(v));
            SetProp(s, "borderTopRightRadius", Sl(v));
            SetProp(s, "borderBottomLeftRadius", Sl(v));
            SetProp(s, "borderBottomRightRadius", Sl(v));
        }

        public static void BorderColor(object s, Color c)
        {
            SetProp(s, "borderTopColor", Sc(c));
            SetProp(s, "borderRightColor", Sc(c));
            SetProp(s, "borderBottomColor", Sc(c));
            SetProp(s, "borderLeftColor", Sc(c));
        }

        public static void BorderWidth(object s, float v)
        {
            SetProp(s, "borderTopWidth", Sf(v));
            SetProp(s, "borderRightWidth", Sf(v));
            SetProp(s, "borderBottomWidth", Sf(v));
            SetProp(s, "borderLeftWidth", Sf(v));


        }

        public static void BgTint(object s, Color c) => SetProp(s, "unityBackgroundImageTintColor", Sc(c));

        public static void FontBold(object s, bool bold)
            => SetProp(s, "unityFontStyleAndWeight", UIRuntime.StyleFontStyleCtor.Invoke(new object[] { bold ? UnityEngine.FontStyle.Bold : UnityEngine.FontStyle.Normal }));

        public static void TextOutlineWidth(object s, float v)
        {
            try { SetProp(s, "unityTextOutlineWidth", Sf(v)); } catch { }
        }

        public static void TextOutlineColor(object s, Color c)
        {
            try { SetProp(s, "unityTextOutlineColor", Sc(c)); } catch { }
        }

        /// <summary>Best-effort text-shadow via UnityEngine.UIElements.TextShadow (Unity 2022.2+). Silently no-ops on older UIToolkit versions that don't have this type.</summary>
        public static void TextShadow(object s, Color color, float offsetX, float offsetY, float blur)
        {
            try
            {
                var shadowStructType = UIRuntime.UEAsm.GetType("UnityEngine.UIElements.TextShadow");
                var styleShadowType = UIRuntime.UEAsm.GetType("UnityEngine.UIElements.StyleTextShadow");
                if (shadowStructType == null || styleShadowType == null)
                {
                    FrameworkPlugin.Log.Warning("[S] TextShadow: TextShadow/StyleTextShadow type not found in this UIToolkit version");
                    return;
                }

                var shadow = Activator.CreateInstance(shadowStructType);
                SetStructMember(shadow, shadowStructType, "offset", new Vector2(offsetX, offsetY));
                SetStructMember(shadow, shadowStructType, "blurRadius", blur);
                SetStructMember(shadow, shadowStructType, "color", color);

                // Try the simple 1-arg ctor first, then fall back to any ctor whose FIRST
                // parameter is TextShadow (e.g. StyleTextShadow(TextShadow, StyleKeyword) in
                // some UIToolkit versions), filling any extra parameters with default values.
                var ctor = styleShadowType.GetConstructor(new[] { shadowStructType });
                object styleShadowValue;
                if (ctor != null)
                {
                    styleShadowValue = ctor.Invoke(new object[] { shadow });
                }
                else
                {
                    var candidate = styleShadowType.GetConstructors()
                        .FirstOrDefault(c => c.GetParameters().Length > 0 && c.GetParameters()[0].ParameterType == shadowStructType);
                    if (candidate == null)
                    {
                        FrameworkPlugin.Log.Warning("[S] TextShadow: no StyleTextShadow constructor accepting TextShadow found");
                        return;
                    }
                    var pars = candidate.GetParameters();
                    var args = new object[pars.Length];
                    args[0] = shadow;
                    for (int i = 1; i < pars.Length; i++)
                        args[i] = pars[i].ParameterType.IsValueType ? Activator.CreateInstance(pars[i].ParameterType) : null;
                    styleShadowValue = candidate.Invoke(args);
                }

                SetProp(s, "textShadow", styleShadowValue);
            }
            catch (Exception ex) { FrameworkPlugin.Log.Warning($"[S] TextShadow failed: {ex.Message}"); }
        }

        private static void SetStructMember(object instance, Type t, string name, object value)
        {
            var f = t.GetField(name);
            if (f != null) { f.SetValue(instance, value); return; }
            t.GetProperty(name)?.SetValue(instance, value);
        }

        public static void Scale(object s, float x, float y) => SetProp(s, "scale", UIRuntime.StyleScaleCtor.Invoke(new object[] { UIRuntime.ScaleCtor.Invoke(new object[] { new Vector3(x, y, 1f) }) }));

        /// <summary>Unity 6: maps ScaleMode to backgroundSize + backgroundPositionX/Y (center) + backgroundRepeat (no-repeat). ScaleToFit = contain, ScaleAndCrop = cover, StretchToFill = 100% x 100%.</summary>
        public static void BgScaleMode(object s, UnityEngine.ScaleMode mode)
        {
            try
            {
                if (UIRuntime.StyleBackgroundSizeCtor == null || UIRuntime.BackgroundSizeFromTypeCtor == null) { FrameworkPlugin.Log.Warning("[S] BgScaleMode: backgroundSize types not available"); return; }

                object size;
                if (mode == UnityEngine.ScaleMode.StretchToFill)
                {
                    var full = PercentLength(100f);
                    if (full == null || UIRuntime.BackgroundSizeFromLengthsCtor == null) return;
                    size = UIRuntime.BackgroundSizeFromLengthsCtor.Invoke(new[] { full, full });
                }
                else
                {
                    string sizeType = mode == UnityEngine.ScaleMode.ScaleAndCrop ? "Cover" : "Contain";
                    size = UIRuntime.BackgroundSizeFromTypeCtor.Invoke(new[] { Enum.Parse(UIRuntime.BackgroundSizeTypeEnum, sizeType) });
                }
                SetProp(s, "backgroundSize", UIRuntime.StyleBackgroundSizeCtor.Invoke(new[] { size }));

                if (UIRuntime.BackgroundPositionCtor != null && UIRuntime.StyleBackgroundPositionCtor != null)
                {
                    var center = UIRuntime.BackgroundPositionCtor.Invoke(new[] { Enum.Parse(UIRuntime.BackgroundPositionKeywordType, "Center") });
                    SetProp(s, "backgroundPositionX", UIRuntime.StyleBackgroundPositionCtor.Invoke(new[] { center }));
                    SetProp(s, "backgroundPositionY", UIRuntime.StyleBackgroundPositionCtor.Invoke(new[] { center }));
                }

                if (UIRuntime.BackgroundRepeatCtor != null && UIRuntime.StyleBackgroundRepeatCtor != null)
                {
                    var noRep = Enum.Parse(UIRuntime.RepeatType, "NoRepeat");
                    var rep = UIRuntime.BackgroundRepeatCtor.Invoke(new[] { noRep, noRep });
                    SetProp(s, "backgroundRepeat", UIRuntime.StyleBackgroundRepeatCtor.Invoke(new[] { rep }));
                }
            }
            catch (Exception ex) { FrameworkPlugin.Log.Warning($"[S] BgScaleMode failed: {ex.Message}"); }
        }

        // ── 0.4.0: font, flex, margins, percent sizes ─────────────────────────

        /// <summary>Applies the given font (e.g. from UIRuntime.FindFont). Null font = same as S.Font(s) (default LegacyRuntime).</summary>
        public static void Font(object s, UnityEngine.Font font)
        {
            var def = UIRuntime.MakeFontDefinition(font);
            if (def == null) { Font(s); return; }
            SetProp(s, "unityFontDefinition", UIRuntime.StyleFontDefCtor.Invoke(new object[] { def }));
        }

        /// <summary>Applies an already built FontDefinition (internal: panel default font cache).</summary>
        internal static void FontDef(object s, object fontDefinition)
        {
            if (fontDefinition == null) { Font(s); return; }
            SetProp(s, "unityFontDefinition", UIRuntime.StyleFontDefCtor.Invoke(new object[] { fontDefinition }));
        }

        /// <summary>row = true: children laid out left to right; false: top to bottom (Unity default).</summary>
        public static void FlexDirection(object s, bool row)
        {
            if (UIRuntime.StyleFlexDirectionCtor == null) return;
            SetProp(s, "flexDirection", UIRuntime.StyleFlexDirectionCtor.Invoke(new[] { Enum.Parse(UIRuntime.FlexDirectionType, row ? "Row" : "Column") }));
        }

        public static void FlexGrow(object s, float v) => SetProp(s, "flexGrow", Sf(v));
        public static void FlexShrink(object s, float v) => SetProp(s, "flexShrink", Sf(v));

        public static void FlexWrap(object s, bool wrap)
        {
            if (UIRuntime.StyleWrapCtor == null) return;
            SetProp(s, "flexWrap", UIRuntime.StyleWrapCtor.Invoke(new[] { Enum.Parse(UIRuntime.WrapType, wrap ? "Wrap" : "NoWrap") }));
        }

        /// <summary>"FlexStart", "Center", "FlexEnd", "SpaceBetween", "SpaceAround" (also "SpaceEvenly"). Case-insensitive; unknown value = warning, no change.</summary>
        public static void JustifyContent(object s, string v)
        {
            var e = ParseEnum(UIRuntime.JustifyType, v, "JustifyContent");
            if (e != null) SetProp(s, "justifyContent", UIRuntime.StyleJustifyCtor.Invoke(new[] { e }));
        }

        /// <summary>"FlexStart", "Center", "FlexEnd", "Stretch" (also "Auto"). Case-insensitive; unknown value = warning, no change.</summary>
        public static void AlignItems(object s, string v)
        {
            var e = ParseEnum(UIRuntime.AlignType, v, "AlignItems");
            if (e != null) SetProp(s, "alignItems", UIRuntime.StyleAlignCtor.Invoke(new[] { e }));
        }

        public static void Margin(object s, float v)
        {
            SetProp(s, "marginLeft", Sl(v));
            SetProp(s, "marginRight", Sl(v));
            SetProp(s, "marginTop", Sl(v));
            SetProp(s, "marginBottom", Sl(v));
        }

        public static void MarginLeft(object s, float v) => SetProp(s, "marginLeft", Sl(v));
        public static void MarginRight(object s, float v) => SetProp(s, "marginRight", Sl(v));
        public static void MarginTop(object s, float v) => SetProp(s, "marginTop", Sl(v));
        public static void MarginBottom(object s, float v) => SetProp(s, "marginBottom", Sl(v));

        /// <summary>Width as percent of the parent (0–100).</summary>
        public static void WidthPercent(object s, float percent)
        {
            var sl = PercentStyleLength(percent);
            if (sl != null) SetProp(s, "width", sl);
        }

        /// <summary>Height as percent of the parent (0–100).</summary>
        public static void HeightPercent(object s, float percent)
        {
            var sl = PercentStyleLength(percent);
            if (sl != null) SetProp(s, "height", sl);
        }

        public static void MinWidth(object s, float v) => SetProp(s, "minWidth", Sl(v));
        public static void MaxWidth(object s, float v) => SetProp(s, "maxWidth", Sl(v));
        public static void MaxHeight(object s, float v) => SetProp(s, "maxHeight", Sl(v));

        static object PercentLength(float percent)
        {
            if (UIRuntime.LengthCtor == null) return null;
            return UIRuntime.LengthCtor.Invoke(new[] { (object)percent, Enum.Parse(UIRuntime.LengthUnitType, "Percent") });
        }

        static object PercentStyleLength(float percent)
        {
            var len = PercentLength(percent);
            if (len == null || UIRuntime.StyleLengthFromLengthCtor == null) { FrameworkPlugin.Log.Warning("[S] Percent length not available"); return null; }
            return UIRuntime.StyleLengthFromLengthCtor.Invoke(new[] { len });
        }

        static object ParseEnum(Type enumType, string v, string caller)
        {
            if (enumType != null && !string.IsNullOrEmpty(v))
            {
                try { return Enum.Parse(enumType, v, true); } catch { }
            }
            FrameworkPlugin.Log.Warning($"[S] {caller}: unknown value '{v}'");
            return null;
        }

    }
}