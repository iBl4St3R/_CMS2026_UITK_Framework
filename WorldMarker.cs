using CMS2026UITKFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CMS2026UITKFramework
{
    /// <summary>
    /// Public API: show the game's own map-marker icons anywhere in the world, from any mod,
    /// without a hard reference to Assembly-CSharp. Wraps CMS.UI.Markers.MapMarkersManager
    /// via reflection — exactly what the game itself uses to mark tools/places on screen.
    ///
    /// USAGE (Option A — direct reference):
    ///     string id = WorldMarker.Show(new Vector3(3042f, 2420f, 2421f), "Computer", 10f);
    ///     WorldMarker.Hide(id);        // optional early removal
    ///     WorldMarker.ClearAll();      // remove every marker shown by this framework
    ///
    /// USAGE (Option B — reflection, no hard dependency on this assembly):
    ///     var t = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).First(x => x.FullName == "CMS2026UITKFramework.WorldMarker");
    ///     string id = (string)t.GetMethod("Show").Invoke(null, new object[] { new Vector3(3042f, 2420f, 2421f), "Computer", 10f, true });
    ///
    /// ── markerType (string, case-sensitive) — one of Il2CppCMS.UI.Markers.MapMarkerType: ──────
    ///   None, General, Car, CarOrder, Exit, ExitTaxi, Computer, GarageLook, Light,
    ///   Management, Orders, Radio, Upgrades, CarWash, DeliveryPickup, DiagnosticPath,
    ///   Dyno, FuelDispenser, Garage, GasStation, OfficeParking, PaintShop, TestTrack,
    ///   BatteryCharger, BrakeLathe, ToolComputer, DetailToolkit, EngineCrane, EngineStand,
    ///   Lifter, LightsAlignment, OilDrain, RepairTableBody, RepairTableParts, ScrapTable,
    ///   SpringClamp, TireChanger, UpgradeParts, Warehouse, Welder, WheelBalancer,
    ///   WheelsAlignment, WindowTint, PaintShopCar, PaintShopParts, TutorialMove, Bed, Coffee
    ///
    /// Sprite selection is 100% automatic (handled internally by the game's MapMarkersConfig) —
    /// you never pick a sprite directly, only the MapMarkerType name. Confirmed live sprite
    /// mapping (verified in-scene, GO=SmallMarker(Clone), component=CMS.UI.Markers.REDMarkers):
    ///   Car->marker_car | CarOrder->marker_car_order | Exit->marker_exit | ExitTaxi->marker_exit_taxi
    ///   Bed->marker_other_bed | Coffee->marker_other_coffee | Computer->marker_other_computer
    ///   GarageLook->marker_other_garagelook | Light->marker_other_light | Management->marker_other_management
    ///   Orders->marker_other_orders | Radio->marker_other_radio | Upgrades->marker_other_upgrades
    ///   CarWash->marker_place_carwash | DeliveryPickup->marker_place_deliverypickup
    ///   DiagnosticPath->marker_place_diagnosticpath | Dyno->marker_place_dyno
    ///   FuelDispenser->marker_place_fueldispenser | Garage->marker_place_garage
    ///   GasStation->marker_place_gasstation | OfficeParking->marker_place_officeparking
    ///   PaintShop->marker_place_paintshop | TestTrack->marker_place_testtrack
    ///   BatteryCharger->marker_tool_batterycharger | BrakeLathe->marker_tool_brakelathe
    ///   DetailToolkit->marker_tool_detailtoolkit | EngineCrane->marker_tool_enginecrane
    ///   EngineStand->marker_tool_enginestand | Lifter->marker_tool_lifter
    ///   LightsAlignment->marker_tool_lightsalignment | OilDrain->marker_tool_oildrain
    ///   RepairTableBody->marker_tool_repairtable_body | RepairTableParts->marker_tool_repairtable_parts
    ///   ScrapTable->marker_tool_scraptable | SpringClamp->marker_tool_springclamp
    ///   TireChanger->marker_tool_tirechanger | UpgradeParts->marker_tool_upgradeparts
    ///   Warehouse->marker_tool_warehouse | Welder->marker_tool_welder
    ///   WheelBalancer->marker_tool_wheelbalancer | WheelsAlignment->marker_tool_wheelsalignment
    ///   WindowTint->marker_tool_windowtint | TutorialMove->marker_tutorial_move
    ///   [None/General/ToolComputer/PaintShopCar/PaintShopParts — no sprite confirmed live yet, test before relying on them]
    ///
    /// Internals: spawns an invisible anchor GameObject at worldPosition (DontDestroyOnLoad) and
    /// calls the live scene's MapMarkersManager.AddObjectToMark(anchor, markerType, forceStay, toolUid) —
    /// same call path the game uses for its own tools. forceStay=true (default) keeps the icon visible
    /// regardless of the player's current map zone/mode, without opening the full marker panel
    /// (no ShowForceStayOnly() call needed on your side — this API does that internally).
    /// toolUid is auto-generated (>= 900000000) to avoid clashing with real game tool ids.
    /// durationSeconds &lt;= 0 means the marker stays until you call Hide()/ClearAll() manually.
    /// </summary>
    public static class WorldMarker
    {
        public static string Show(Vector3 worldPosition, string markerType, float durationSeconds = 0f, bool forceStay = true) => WorldMarkerManager.ShowMarker(worldPosition, markerType, durationSeconds, forceStay);

        public static bool Hide(string id) => WorldMarkerManager.HideMarker(id);

        /// <summary>Removes every marker currently shown by this framework (across all mods). Call this before showing a new one if you want a clean slate.</summary>
        public static void ClearAll() => WorldMarkerManager.ClearAll();
    }

    internal static class WorldMarkerManager
    {
        private class ActiveMarker
        {
            public string Id;
            public GameObject Anchor;
            public float TimeLeft;
            public bool HasDuration;
        }

        private static readonly List<ActiveMarker> _active = new();
        private static readonly List<GameObject> _pendingDestroy = new();

        private static Type _managerType;
        private static Type _markerTypeEnum;
        private static MethodInfo _addObjectToMark;
        private static MethodInfo _removeObjectToMark;
        private static MethodInfo _showForceStayOnly;
        private static MethodInfo _resetForcedData;
        private static uint _nextToolUid = 900000000u;

        private static bool ResolveTypes()
        {
            if (_managerType != null && _markerTypeEnum != null) return true;
            try
            {
                var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => { try { return a.GetTypes(); } catch { return Type.EmptyTypes; } });

                foreach (var t in allTypes)
                {
                    if (_managerType == null && t.FullName == "Il2CppCMS.UI.Markers.MapMarkersManager") _managerType = t;
                    else if (_markerTypeEnum == null && t.FullName == "Il2CppCMS.UI.Markers.MapMarkerType") _markerTypeEnum = t;
                    if (_managerType != null && _markerTypeEnum != null) break;
                }

                if (_managerType == null || _markerTypeEnum == null) return false;

                _addObjectToMark = _managerType.GetMethod("AddObjectToMark");
                _removeObjectToMark = _managerType.GetMethod("RemoveObjectToMark");
                _showForceStayOnly = _managerType.GetMethod("ShowForceStayOnly");
                _resetForcedData = _managerType.GetMethod("ResetForcedData", new[] { typeof(GameObject), typeof(bool) });
                return _addObjectToMark != null && _removeObjectToMark != null;
            }
            catch (Exception ex) { FrameworkPlugin.Log.Warning($"[WorldMarker] Type resolve failed: {ex.Message}"); return false; }
        }

        private static MonoBehaviour FindLiveManager()
        {
            var all = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true);
            foreach (var mb in all)
            {
                try { if (mb.GetIl2CppType().FullName == "CMS.UI.Markers.MapMarkersManager") return mb; }
                catch { }
            }
            return null;
        }

        private static object WrapManager(MonoBehaviour mb) => Activator.CreateInstance(_managerType, new object[] { ((Component)mb).Pointer });

        internal static string ShowMarker(Vector3 pos, string markerType, float durationSeconds, bool forceStay)
        {
            if (!ResolveTypes()) { FrameworkPlugin.Log.Warning("[WorldMarker] MapMarkersManager type not found — wrong game version?"); return null; }

            object markerTypeValue;
            try { markerTypeValue = Enum.Parse(_markerTypeEnum, markerType); }
            catch { FrameworkPlugin.Log.Warning($"[WorldMarker] Unknown markerType '{markerType}'. See WorldMarker.cs doc-comment for valid names."); return null; }

            var mgrMb = FindLiveManager();
            if (mgrMb == null) { FrameworkPlugin.Log.Warning("[WorldMarker] No live MapMarkersManager instance in scene."); return null; }

            var anchor = new GameObject("_CMS2026UIKit_WorldMarker");
            UnityEngine.Object.DontDestroyOnLoad(anchor);
            anchor.transform.position = pos;

            uint toolUid = _nextToolUid++;
            var mgrWrapped = WrapManager(mgrMb);
            _addObjectToMark.Invoke(mgrWrapped, new object[] { anchor, markerTypeValue, forceStay, toolUid });
            if (forceStay) _showForceStayOnly?.Invoke(mgrWrapped, null);

            string id = Guid.NewGuid().ToString("N").Substring(0, 8);
            _active.Add(new ActiveMarker { Id = id, Anchor = anchor, TimeLeft = durationSeconds, HasDuration = durationSeconds > 0f });
            return id;
        }

        internal static bool HideMarker(string id)
        {
            int idx = _active.FindIndex(m => m.Id == id);
            if (idx < 0) return false;
            RemoveInternal(_active[idx]);
            _active.RemoveAt(idx);
            return true;
        }

        internal static void ClearAll()
        {
            foreach (var m in _active) RemoveInternal(m);
            _active.Clear();
        }

        private static void RemoveInternal(ActiveMarker m)
        {
            try
            {
                if (m.Anchor != null && ResolveTypes())
                {
                    var mgrMb = FindLiveManager();
                    if (mgrMb != null)
                    {
                        var wrapped = WrapManager(mgrMb);
                        // forceStay markers live in a separate forcedEntities dictionary — RemoveObjectToMark
                        // alone only clears the main placesToMark list and leaves a forced entry behind,
                        // which is exactly what orphans the rendered icon into a frozen 2D indicator.
                        _resetForcedData?.Invoke(wrapped, new object[] { m.Anchor, true });
                        _removeObjectToMark.Invoke(wrapped, new object[] { m.Anchor });
                        _showForceStayOnly?.Invoke(wrapped, null);
                    }
                }
            }
            catch (Exception ex) { FrameworkPlugin.Log.Warning($"[WorldMarker] RemoveObjectToMark failed: {ex.Message}"); }

            if (m.Anchor != null) _pendingDestroy.Add(m.Anchor);
        }

        // ── Tick — called every frame from UIKitUpdater ─────────────────────
        // ── Tick — called every frame from UIKitUpdater ─────────────────────
        internal static void Tick(float dt)
        {
            if (_pendingDestroy.Count > 0)
            {
                foreach (var go in _pendingDestroy)
                    if (go != null) UnityEngine.Object.Destroy(go);
                _pendingDestroy.Clear();
            }

            if (_active.Count == 0) return;
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var m = _active[i];
                if (m.Anchor == null) { _active.RemoveAt(i); continue; }
                if (!m.HasDuration) continue;

                m.TimeLeft -= dt;
                if (m.TimeLeft <= 0f) { RemoveInternal(m); _active.RemoveAt(i); }
            }
        }
    }
}



//// pokaż ikonę komputera na 10 sekund
//string id = WorldMarker.Show(new Vector3(3042f, 2420f, 2421f), "Computer", 10f);

//// posprzątaj wszystko przed pokazaniem nowej ikony
//WorldMarker.ClearAll();

//// albo tylko konkretną
//WorldMarker.Hide(id);