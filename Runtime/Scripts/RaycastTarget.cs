using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LazyUI
{
    /// <summary>
    /// 何もしないRaycastTarget
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class RaycastTarget : Graphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            base.OnPopulateMesh(vh);
            vh.Clear();
        }
#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(RaycastTarget))]
        [UnityEditor.CanEditMultipleObjects]
        public sealed class RaycastTargetEditor : UnityEditor.UI.GraphicEditor
        {
            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                //UnityEditor.EditorGUILayout.PropertyField(m_Script);
                //AppearanceControlsGUI();
                RaycastControlsGUI();
                //MaskableControlsGUI();
                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}
