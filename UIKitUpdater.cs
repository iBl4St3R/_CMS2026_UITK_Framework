using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace CMS2026UITKFramework
{
    /// <summary>
    /// MonoBehaviour living on the host GameObject.
    /// Calls OnUpdate() on every registered UIPanel each frame.
    /// </summary>
    public class UIKitUpdater : MonoBehaviour
    {
        public UIKitUpdater(System.IntPtr ptr) : base(ptr) { }

        static UIKitUpdater()
        {
            ClassInjector.RegisterTypeInIl2Cpp<UIKitUpdater>();
        }

        private void Update()
        {
            var panels = FrameworkPlugin.ActivePanels;
            for (int i = panels.Count - 1; i >= 0; i--)
            {
                // Usuń zniszczone panele z rejestru
                if (panels[i] == null) { panels.RemoveAt(i); continue; }
                panels[i].OnUpdate();
            }
        }
    }
}