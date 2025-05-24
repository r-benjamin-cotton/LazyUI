using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LazyUI
{
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(AnchorCoordinator), true)]
    [UnityEditor.CanEditMultipleObjects]
    public sealed class AnchorCoordinatorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            DrawDefaultInspector();
            UnityEditor.EditorGUILayout.Space();

            serializedObject.Update();
            var ac = target as AnchorCoordinator;
            var rectTransform = ac.gameObject.transform as RectTransform;
            var p = rectTransform.localPosition;
            var r = rectTransform.GetRectInParentZ();
            var rps = new Vector3(r.xMin, r.yMin, p.z);
            var prs = (rectTransform.parent as RectTransform).rect.size;
            var pos = UnityEditor.EditorGUILayout.Vector3Field($"position {prs}", rps);
            if (pos != rps)
            {
                var dt = pos - rps;
                p += dt;
                rectTransform.localPosition = p;
                UnityEditor.EditorUtility.SetDirty(target);
            }
            //serializedObject.ApplyModifiedProperties();
        }
    }
#endif
        }
