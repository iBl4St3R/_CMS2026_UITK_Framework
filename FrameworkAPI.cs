using CMS2026UITKFramework;
using System;
using UnityEngine;

namespace CMS2026UITKFramework
{
    /// <summary>
    /// Public API for mods using _CMS2026UIFramework.
    /// 
    /// Option A (direct reference):
    ///     var panel = FrameworkAPI.CreatePanel("My Mod", 40, 40, 400, 300);
    ///
    /// Option B (reflection — no hard dependency):
    ///     var apiType = AppDomain...GetType("CMS2026UIFramework.FrameworkAPI");
    ///     apiType.GetMethod("CreatePanel").Invoke(null, new object[]{"My Mod",40f,40f,400f,300f});
    /// </summary>
    public static class FrameworkAPI
    {
        public static string Version => FrameworkPlugin.Version;
        public static bool IsReady => UIRuntime.IsAvailable;

        /// <summary>
        /// Creates a draggable panel and returns it.
        /// Returns null if UIToolkit is unavailable.
        /// </summary>
        public static UIPanel CreatePanel(string title,
                                          float x, float y,
                                          float width, float height)
            => UIPanel.Create(title, x, y, width, height);

        // Kolejne metody będą tu dodawane wraz z rozwojem frameworka:
        // AddLabel, AddButton, AddToggle, AddSlider...
    }
}