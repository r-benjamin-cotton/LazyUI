using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LazyUI
{
    /// <summary>
    /// キーボードショートカット
    /// </summary>
    public class LazyShortcut : MonoBehaviour
    {
        [Serializable]
        private class LazyShortcutEvent : UnityEvent { }

        [Serializable]
        private struct Shortcut
        {
            //public LazyShortcutEvent action;
            public LazyAction action;
            public LazyKeyCombi.KeyCombi keyCombi;
            public string prefsKey;
        }

        [SerializeField]
        private Shortcut[] shortcuts = null;

        private Shortcut[] active = null;

        private void Setup()
        {
            if ((shortcuts == null) || (shortcuts.Length == 0))
            {
                return;
            }
            active = new Shortcut[shortcuts.Length];
            for (int i = 0, end = shortcuts.Length; i < end; i++)
            {
                var sc = shortcuts[i];
                LazyKeyCombi.LoadKeyCombi(sc.prefsKey, out LazyKeyCombi.KeyCombi keyCombi, sc.keyCombi);
                var ac = new Shortcut()
                {
                    keyCombi = keyCombi,
                    action = sc.action
                };
                active[i] = ac;
                LazyKeyCombi.AddListener(ac.keyCombi, ac.action.Invoke);
            }
        }
        private void Release()
        {
            if (active != null)
            {
                foreach (var ac in active)
                {
                    LazyKeyCombi.RemoveListener(ac.keyCombi, ac.action.Invoke);
                }
                active = null;
            }
        }
        private void OnEnable()
        {
            Setup();
        }
        private void OnDisable()
        {
            Release();
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (UnityEditor.EditorApplication.isPlaying && isActiveAndEnabled)
            {
                Release();
                Setup();
            }
        }
#endif
    }
}

