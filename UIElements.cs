using CMS2026UITKFramework;
using System;
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
        public static void Top(object s, float v) => SetProp(s, "top", Sl(v));
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

        public static void Scale(object s, float x, float y) => SetProp(s, "scale", UIRuntime.StyleScaleCtor.Invoke(new object[] { UIRuntime.ScaleCtor.Invoke(new object[] { new Vector3(x, y, 1f) }) }));

        public static void BgScaleMode(object s, UnityEngine.ScaleMode mode)
        {
            // Unity 6 używa backgroundSize zamiast unityBackgroundScaleMode
            // ScaleToFit/ScaleAndCrop = "cover", StretchToFill = "contain" / auto
            // Na razie zostawiamy jako no-op z logiem — do zbadania
        }

    }
}