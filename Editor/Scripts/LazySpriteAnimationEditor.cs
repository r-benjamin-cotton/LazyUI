using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LazyUI
{
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(LazySpriteAnimation), true)]
    [UnityEditor.CanEditMultipleObjects]
    public sealed class LazySpriteAnimationEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            DrawDefaultInspector();
            var anim = target as LazySpriteAnimation;
            if (anim != null)
            {
#if true
                if (!Application.isPlaying)
                {
                    UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
                }
# endif
            }
        }
        public override bool RequiresConstantRepaint()
        {
            return true;
        }
    }
#endif
}
