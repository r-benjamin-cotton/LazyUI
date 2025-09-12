using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace LazyUI
{
    /// <summary>
    /// string��SerializeField�ł̓f�t�H���g�ł�'\'�G�X�P�[�v���z�����܂�Č듮����N�������B�B
    /// PropertyField�ŕ`�悷��΃G�X�P�[�v���������Ȃ��悤�Ȃ̂ŃA�g���r���[�g������Ă݂��B
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
