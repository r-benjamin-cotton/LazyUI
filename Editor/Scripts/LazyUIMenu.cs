using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LazyUI
{
    /// <summary>
    /// Unityエディタのメニュー要素
    /// </summary>
    public static class LazyUIMenu
    {
        private const string root = "Packages/com.r-benjamin-cotton.lazyui/Runtime/Prefab/";

        private static GameObject Instantiate(string prefabName)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(root + prefabName + ".prefab");
            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            Undo.RegisterCreatedObjectUndo(instance, $"Create LazyUI.{prefabName}");
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.UserAction);
            return instance;
        }
        private static Action<GameObject, MenuCommand> PlaceUIElementRootMethod = null;
        private static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
        {
            if (PlaceUIElementRootMethod == null)
            {
                var mo = Type.GetType("UnityEditor.UI.MenuOptions, UnityEditor.UI");
                var method = mo.GetMethod("PlaceUIElementRoot", BindingFlags.Static | BindingFlags.NonPublic);
                var dg = Delegate.CreateDelegate(typeof(Action<GameObject, MenuCommand>), method);
                PlaceUIElementRootMethod = dg as Action<GameObject, MenuCommand>;
            }
            PlaceUIElementRootMethod.Invoke(element, menuCommand);
        }

        [MenuItem("GameObject/LazyUI/PropertyRadioButton", priority = 9, secondaryPriority = 1)]
        public static void CreatePropertyRadioButton(MenuCommand menuCommand)
        {
            var go = Instantiate("PropertyRadioButton");
            PlaceUIElementRoot(go, menuCommand);
        }
        [MenuItem("GameObject/LazyUI/PropertyRotarySwitch", priority = 9, secondaryPriority = 2)]
        public static void CreatePropertyRotarySwitch(MenuCommand menuCommand)
        {
            var go = Instantiate("PropertyRotarySwitch");
            PlaceUIElementRoot(go, menuCommand);
        }
        [MenuItem("GameObject/LazyUI/PropertySelector", priority = 9, secondaryPriority = 3)]
        public static void CreatePropertySelector(MenuCommand menuCommand)
        {
            var go = Instantiate("PropertySelector");
            PlaceUIElementRoot(go, menuCommand);
        }
        [MenuItem("GameObject/LazyUI/PropertySlider", priority = 9, secondaryPriority = 4)]
        public static void CreatePropertySlider(MenuCommand menuCommand)
        {
            var go = Instantiate("PropertySlider");
            PlaceUIElementRoot(go, menuCommand);
        }
        [MenuItem("GameObject/LazyUI/PropertyRangeSlider", priority = 9, secondaryPriority = 5)]
        public static void CreatePropertyRangeSlider(MenuCommand menuCommand)
        {
            var go = Instantiate("PropertyRangeSlider");
            PlaceUIElementRoot(go, menuCommand);
        }
#if false
        [MenuItem("GameObject/LazyUI/PropertySpinControl", priority = 9, secondaryPriority = 6)]
        public static void CreatePropertySpinControl(MenuCommand menuCommand)
        {
            var go = Instantiate("PropertySpinControl");
            PlaceUIElementRoot(go, menuCommand);
        }
#endif
        [MenuItem("GameObject/LazyUI/PropertyText", priority = 9, secondaryPriority = 7)]
        public static void CreatePropertyText(MenuCommand menuCommand)
        {
            var go = Instantiate("PropertyText (TMP)");
            PlaceUIElementRoot(go, menuCommand);
        }
        [MenuItem("GameObject/LazyUI/PropertyInputField", priority = 9, secondaryPriority = 8)]
        public static void CreatePropertyInputField(MenuCommand menuCommand)
        {
            var go = Instantiate("PropertyInputField (TMP)");
            PlaceUIElementRoot(go, menuCommand);
        }
    }
}
