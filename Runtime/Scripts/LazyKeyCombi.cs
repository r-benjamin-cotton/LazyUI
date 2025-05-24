using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LazyUI
{
    /// <summary>
    /// キー組み合わせ入力管理
    /// </summary>
    public static class LazyKeyCombi
    {
        /// <summary>
        /// 修飾キー
        /// </summary>
        [Flags]
        public enum Modifier
        {
            None = 0,
            Shift = 1 << 0,
            Ctrl = 1 << 1,
            Alt = 1 << 2,
            Meta = 1 << 3,
            CapsLock = 1 << 4,
            NumLock = 1 << 5,
            ScrollLock = 1 << 6,

            LeftShift = (1 << 8),
            RightShift = (1 << 9),
            LeftCtrl = (1 << 10),
            RightCtrl = (1 << 11),
            LeftAlt = (1 << 12),
            RightAlt = (1 << 13),
            LeftMeta = (1 << 14),
            RightMeta = (1 << 15),
        }
        [Serializable]
        public struct KeyCombi
        {
            public Modifier modifier;
            public Key[] keys;
            public KeyCombi(Modifier modifier, Key[] keys)
            {
                this.modifier = modifier;
                this.keys = keys;
            }
            public readonly bool Empty
            {
                get
                {
                    return (keys == null) || (keys.Length == 0);
                }
            }
            public readonly KeyCombi Clone()
            {
                return new KeyCombi(modifier, Empty ? null : keys.ToArray());
            }
            public readonly bool Equals(KeyCombi kc)
            {
                if (kc.modifier != modifier)
                {
                    return false;
                }
                if (kc.keys == null)
                {
                    return keys == null;
                }
                if (keys == null)
                {
                    return false;
                }
                if (kc.keys.Length != keys.Length)
                {
                    return false;
                }
                for (int i = 0; i < keys.Length; i++)
                {
                    if (kc.keys[i] != keys[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            public override readonly bool Equals(object obj)
            {
                if (obj is KeyCombi keyCombi)
                {
                    return Equals(keyCombi);
                }
                return false;
            }
            public override readonly int GetHashCode()
            {
                unchecked
                {
                    int hash = modifier.GetHashCode();
                    if (keys != null)
                    {
                        hash ^= keys.Length;
                        for (int i = 0; i < keys.Length; i++)
                        {
                            hash += 0x55555555;
                            hash ^= keys[i].GetHashCode();
                        }
                    }
                    return hash;
                }
            }
            public override readonly string ToString()
            {
#if true
                var ks = "";
                if (modifier != Modifier.None)
                {
                    ks = ',' + modifier.ToString();
                }
                if (keys != null)
                {
                    foreach (var k in keys.Where((k) => keyValues.Contains(k)))
                    {
                        ks += ',' + k.ToString();
                    }
                }
                if (string.IsNullOrEmpty(ks))
                {
                    return "None";
                }
                else
                {
                    return ks[1..];
                }
#else
                var ks = modifier.ToString();
                if (keys != null)
                {
                    foreach (var k in keys.Where((k) => keyValues.Contains(k)))
                    {
                        ks += ',' + k.ToString();
                    }
                }
                return ks;
#endif
            }
        }
        private class Listener
        {
            public Action action;
        }
        private static readonly Dictionary<KeyCombi, Listener> listeners = new();

        /// <summary>
        /// 有効なキーの値(修飾キー等を除外)
        /// </summary>
        private static readonly HashSet<Key> keyValues = (Enum.GetValues(typeof(Key)) as Key[]).Where((key) =>
        {
            if ((key == Key.LeftShift) || (key == Key.RightShift))
            {
                return false;
            }
            if ((key == Key.LeftCtrl) || (key == Key.RightCtrl))
            {
                return false;
            }
            if ((key == Key.LeftAlt) || (key == Key.RightAlt))
            {
                return false;
            }
            if ((key == Key.LeftMeta) || (key == Key.RightMeta))
            {
                return false;
            }
            if (key == Key.IMESelected)
            {
                return false;
            }
#if false
            if (key == Key.ContextMenu)
            {
                return false;
            }
#endif
            if ((key == Key.CapsLock) || (key == Key.NumLock) || (key == Key.ScrollLock))
            {
                return false;
            }
            if (key == Key.None)
            {
                return false;
            }
            return true;
        }).ToHashSet();

        private static bool interactable = true;
        private static bool interactablex = false;
        private static bool isPressedAnyKey = false;
        private static Modifier currentModifier = 0;
        private static Keyboard keyboard = null;

        /// <summary>
        /// 有効なキーか
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsValidKey(Key key)
        {
            return keyValues.Contains(key);
        }
        /// <summary>
        /// キーがすべて有効か
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static bool IsValidKeys(Key[] keys)
        {
            if ((keys == null) || (keys.Length == 0))
            {
                return false;
            }
            if (!keys.All((k) => keyValues.Contains(k)))
            {
#if UNITY_EDITOR
                LazyDebug.LogWarning($"Invalid keys {keys}");
#endif
                return false;
            }
            return true;
        }
        /// <summary>
        /// 指定した修飾キーが押されているかどうか
        /// </summary>
        /// <param name="modifier"></param>
        /// <returns></returns>
        public static bool IsPressedModifier(Modifier modifier)
        {
            if (!Interactable)
            {
                return false;
            }
            var mask = (Modifier)(-1);
            mask &= ((modifier & (Modifier.LeftShift | Modifier.RightShift)) == 0) ? ~(Modifier.LeftShift | Modifier.RightShift) : ~Modifier.Shift;
            mask &= ((modifier & (Modifier.LeftCtrl | Modifier.RightCtrl)) == 0) ? ~(Modifier.LeftCtrl | Modifier.RightCtrl) : ~Modifier.Ctrl;
            mask &= ((modifier & (Modifier.LeftAlt | Modifier.RightAlt)) == 0) ? ~(Modifier.LeftAlt | Modifier.RightAlt) : ~Modifier.Alt;
            mask &= ((modifier & (Modifier.LeftMeta | Modifier.RightMeta)) == 0) ? ~(Modifier.LeftMeta | Modifier.RightMeta) : ~Modifier.Meta;
            return (currentModifier & mask) == modifier;
        }
        /// <summary>
        /// 修飾キー以外の何らかのキーが押されている
        /// </summary>
        /// <returns></returns>
        public static bool IsPressedAnyKey()
        {
            if (!Interactable)
            {
                return false;
            }
            return isPressedAnyKey;
        }
        /// <summary>
        /// キーの状態取得
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool GetKey(Key key)
        {
            if (!Interactable)
            {
                return false;
            }
            return keyboard[key].isPressed;
        }
        /// <summary>
        /// キーの押下イベントを取得
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool GetKeyDown(Key key)
        {
            if (!Interactable)
            {
                return false;
            }
            return keyboard[key].wasPressedThisFrame;
        }
        /// <summary>
        /// キーの解放イベントを取得
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool GetKeyUp(Key key)
        {
            if (!Interactable)
            {
                return false;
            }
            return keyboard[key].wasReleasedThisFrame;
        }
        /// <summary>
        /// 入力有効状態
        /// </summary>
        public static bool Interactable
        {
            get
            {
                return interactablex;
            }
            set
            {
                interactable = value;
            }
        }
        public static bool WasPressedThisFrame(KeyCombi keyCombi)
        {
            if (!interactablex)
            {
                return false;
            }
            if (!isPressedAnyKey)
            {
                return false;
            }
            if (keyCombi.Empty)
            {
                return false;
            }
            if (!IsValidKeys(keyCombi.keys))
            {
                return false;
            }
            if (!IsPressedModifier(keyCombi.modifier))
            {
                return false;
            }
            var wasPressed = false;
            for (int i = 0, end = keyCombi.keys.Length; i < end; i++)
            {
                var key = keyCombi.keys[i];
                if (!keyboard[key].isPressed)
                {
                    return false;
                }
                if (keyboard[key].wasPressedThisFrame)
                {
                    wasPressed = true;
                }
            }
            if (!wasPressed)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// キーの組み合わせを設定から復元
        /// </summary>
        /// <param name="prefsKey"></param>
        /// <param name="modifier"></param>
        /// <param name="keys"></param>
        public static void LoadKeyCombi(string prefsKey, out KeyCombi keyCombi, KeyCombi defaultKeyCombi)
        {
            var ks = defaultKeyCombi.ToString();
            var str = LazyPlayerPrefs.GetValue(prefsKey, ks);
            var ss = str.Split(',');
            var modifier = Modifier.None;
            var keyList = new List<Key>();
            var error = false;
            foreach (var s in ss)
            {
                if (string.IsNullOrEmpty(s))
                {
                    continue;
                }
                if (Enum.TryParse(s, out Modifier mm))
                {
                    modifier |= mm;
                }
                else if (Enum.TryParse(s, out Key kk))
                {
                    keyList.Add(kk);
                }
                else
                {
                    error = true;
                }
            }
            if (error)
            {
                LazyDebug.LogWarning($"Illegal data.. {str}");
            }
            keyCombi = new KeyCombi(modifier, keyList.ToArray());
        }
        /// <summary>
        /// キーの組み合わせを設定へ保存
        /// </summary>
        /// <param name="prefsKey"></param>
        /// <param name="modifier"></param>
        /// <param name="keys"></param>
        public static void SaveKeyCombi(string prefsKey, KeyCombi keyCombi)
        {
#if UNITY_EDITOR
            if (!IsValidKeys(keyCombi.keys))
            {
                return;
            }
#endif
            var ks = keyCombi.ToString();
            LazyPlayerPrefs.SetValue(prefsKey, ks);
        }

        /// <summary>
        /// リスナーを登録
        /// </summary>
        /// <param name="keyCombi"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static void AddListener(KeyCombi keyCombi, Action callback)
        {
            if (keyCombi.Empty)
            {
                return;
            }
            if (!IsValidKeys(keyCombi.keys))
            {
                return;
            }
            if (listeners.TryGetValue(keyCombi, out Listener listener))
            {
                if (listener.action != null)
                {
                    LazyDebug.LogWarning($"{keyCombi}: multi listener.");
                }
                listener.action += callback;
            }
            else
            {
                listener = new Listener()
                {
                    action = callback
                };
                listeners.Add(keyCombi.Clone(), listener);
            }
        }
        /// <summary>
        /// リスナーを解除
        /// </summary>
        /// <param name="keyCombi"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static void RemoveListener(KeyCombi keyCombi, Action callback)
        {
            if (keyCombi.Empty)
            {
                return;
            }
            if (!IsValidKeys(keyCombi.keys))
            {
                return;
            }
            if (listeners.TryGetValue(keyCombi, out Listener listener))
            {
                listener.action -= callback;
                if (listener.action == null)
                {
                    listeners.Remove(keyCombi);
                }
            }
        }

        private static void UpdateInteractable()
        {
            interactablex = false;
            if (keyboard == null)
            {
                return;
            }
            if (!Application.isFocused)
            {
                return;
            }
            if (Input.imeCompositionMode == IMECompositionMode.On)
            {
                return;
            }
            interactablex = interactable;
        }
        private static void UpdateKeys()
        {
            currentModifier = 0;
            isPressedAnyKey = false;
            if (!interactablex)
            {
                return;
            }
            if (keyboard[Key.LeftShift].isPressed)
            {
                currentModifier |= Modifier.Shift | Modifier.LeftShift;
            }
            if (keyboard[Key.RightShift].isPressed)
            {
                currentModifier |= Modifier.Shift | Modifier.RightShift;
            }
            if (keyboard[Key.LeftCtrl].isPressed)
            {
                currentModifier |= Modifier.Ctrl | Modifier.LeftCtrl;
            }
            if (keyboard[Key.RightCtrl].isPressed)
            {
                currentModifier |= Modifier.Ctrl | Modifier.RightCtrl;
            }
            if (keyboard[Key.LeftAlt].isPressed)
            {
                currentModifier |= Modifier.Alt | Modifier.LeftAlt;
            }
            if (keyboard[Key.RightAlt].isPressed)
            {
                currentModifier |= Modifier.Alt | Modifier.RightAlt;
            }
            if (keyboard[Key.LeftMeta].isPressed)
            {
                currentModifier |= Modifier.Meta | Modifier.LeftMeta;
            }
            if (keyboard[Key.RightMeta].isPressed)
            {
                currentModifier |= Modifier.Meta | Modifier.RightMeta;
            }
#if false
            if (keyboard[Key.ContextMenu].isPressed)
            {
                currentModifier |= Modifier.ContextMenu;
            }
#endif
            if (keyboard[Key.CapsLock].isPressed)
            {
                currentModifier |= Modifier.CapsLock;
            }
            if (keyboard[Key.NumLock].isPressed)
            {
                currentModifier |= Modifier.NumLock;
            }
            if (keyboard[Key.ScrollLock].isPressed)
            {
                currentModifier |= Modifier.ScrollLock;
            }
            foreach (var key in keyValues)
            {
                var ctrl = keyboard[key];
                if (ctrl.isPressed)
                {
                    isPressedAnyKey = true;
                    break;
                }
            }
        }
        private static void ProcessListener()
        {
            if (!interactablex)
            {
                return;
            }
            if (!isPressedAnyKey)
            {
                return;
            }
            foreach (var kl in listeners)
            {
                var keyCombi = kl.Key;
                var listener = kl.Value;
                if (listener.action == null)
                {
                    return;
                }
                if (!IsPressedModifier(keyCombi.modifier))
                {
                    continue;
                }
                var wasPressed = false;
                var i = 0;
                var end = keyCombi.keys.Length;
                for (; i < end; i++)
                {
                    var key = keyCombi.keys[i];
                    if (!keyboard[key].isPressed)
                    {
                        break;
                    }
                    if (keyboard[key].wasPressedThisFrame)
                    {
                        wasPressed = true;
                    }
                }
                if ((i != end) || !wasPressed)
                {
                    continue;
                }
                listener.action?.Invoke();
            }
        }
        private static void Update()
        {
            UpdateInteractable();
            UpdateKeys();
            ProcessListener();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Awake()
        {
            Input.imeCompositionMode = IMECompositionMode.Auto;
            keyboard = InputSystem.GetDevice<Keyboard>();
            LazyCallbacker.RegisterCallback(LazyCallbacker.CallbackType.YieldNull, 0, Update);
        }
    }
}
