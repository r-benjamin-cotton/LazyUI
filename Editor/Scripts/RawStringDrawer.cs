using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace LazyUI
{
    /// <summary>
    /// stringのSerializeFieldではデフォルトでは'\'エスケープが吸い込まれて誤動作を起こ事が。。
    /// PropertyFieldで描画すればエスケープを処理しないようなのでアトリビュートを作ってみた。
    /// </summary>
    [CustomPropertyDrawer(typeof(RawStringAttribute))]
    public class RawStringDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, true);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, true);
        }
    }
}
#endif
