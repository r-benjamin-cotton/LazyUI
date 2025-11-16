using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace LazyUI
{
    /// <summary>
    /// イベントコールバックを集約
    /// ※記述を簡素化できるように？
    /// ※常駐型
    /// </summary>
    [HideInInspector]
    [ExecuteAlways]
    public class LazyCallbacker : MonoBehaviour
    {
        private static LazyCallbacker instance = null;

        private class CallbackInfo
        {
            public int priority;
            public string name;
            public Action action;
            public bool once;
            public override string ToString()
            {
                return $"{priority}:{name}";
            }
        }
        private static readonly List<CallbackInfo>[] callbacks;
        private static readonly List<CallbackInfo> active;
        static LazyCallbacker()
        {
            var num = Enum.GetNames(typeof(CallbackType)).Length;
            callbacks = new List<CallbackInfo>[num];
            active = new List<CallbackInfo>();
            for (int i = 0; i < num; i++)
            {
                callbacks[i] = new List<CallbackInfo>();
            }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Awake()
        {
            if (ReferenceEquals(instance, null))
            {
                Install();
            }
        }
        private static void Install()
        {
            var go = new GameObject("LazyCallbacker", typeof(LazyCallbacker))
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            DontDestroyOnLoad(go);
            instance = go.GetComponent<LazyCallbacker>();
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            RenderPipelineManager.beginFrameRendering += OnBeginFrameRendering;
            RenderPipelineManager.endFrameRendering += OnEndFrameRendering;
            Application.quitting += OnApplicationQuitting;
        }

        public enum CallbackType
        {
            SceneLoaded,
            SceneUnloaded,
            FixedUpdate,
            WaitForFixedUpdate,
            Update,
            YieldNull,
            LateUpdate,
            BeginFrameRendering,
            EndFrameRendering,
            WaitForEndOfFrame,
            Quitting,
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static UnityEngine.Object GetTargetObject(Action action)
        {
            if (action == null)
            {
                return null;
            }
            var il = action.GetInvocationList();
            if ((il == null) || (il.Length == 0))
            {
                return null;
            }
            return il[0].Target as UnityEngine.Object;
        }
#endif
        public static void RegisterCallbackOnce(CallbackType callbackType, int priority, Action action)
        {
            RegisterCallback(callbackType, priority, action, true);
        }

        public static void RegisterCallback(CallbackType callbackType, int priority, Action action, bool once = false)
        {
            if (action == null)
            {
                return;
            }
            //LazyDebug.LogWarning($"LazyCallbacker.RegisterCallback({callbackType}, {priority}, {GetTargetObject(action)?.name})");
            var cl = callbacks[(int)callbackType];
            for (int i = 0; i < cl.Count; i++)
            {
                if ((cl[i].priority == priority) && (cl[i].action == action))
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    LazyDebug.LogWarning($"LazyCallbacker.RegisterCallback({callbackType}, {priority}, {GetTargetObject(action)?.name}) : already registerd..");
#endif
                    return;
                }
            }
            var ci = new CallbackInfo();
            ci.priority = priority;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var to = GetTargetObject(action);
            ci.name = to != null ? $"{priority}:{to.GetType()}.{to.name}" : $"{priority}";
#endif
            ci.action = action;
            ci.once = once;
#if true
            for (int i = 0; i < cl.Count; i++)
            {
                if (cl[i].priority > priority)
                {
                    cl.Insert(i, ci);
                    return;
                }
            }
            cl.Add(ci);
#else
            cl.Add((priority, action));
            cl.Sort((a, b) => a.priority.CompareTo(b.priority));
#endif
        }
        public static bool RemoveCallback(CallbackType callbackType, int priority, Action action)
        {
            var cl = callbacks[(int)callbackType];
            for (int i = 0; i < cl.Count; i++)
            {
                if ((cl[i].priority == priority) && (cl[i].action == action))
                {
                    cl.RemoveAt(i);
                    //LazyDebug.LogWarning($"LazyCallbacker.RemoveCallback({callbackType}, {priority}, {GetTargetObject(action)?.name}) removed..");
                    return true;
                }
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            LazyDebug.LogWarning($"LazyCallbacker.RemoveCallback({callbackType}, {priority}, {GetTargetObject(action)?.name}) missing..");
#endif
            return false;
        }

        /// <summary>
        /// DestoryされたUnityObjectを削除
        /// </summary>
        /// <param name="ct"></param>
        /// <param name="cl"></param>
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static void CheckTarget(CallbackType ct, List<CallbackInfo> cl)
        {
            for (int i = 0; i < cl.Count; i++)
            {
                var ci = cl[i];
                var il = ci.action.GetInvocationList();
                foreach (var dg in il)
                {
                    if (dg.Target is UnityEngine.Object obj)
                    {
                        if (obj == null)
                        {
                            if (!ci.once)
                            {
                                LazyDebug.LogWarning($"LazyCallbacker: {ct}: {obj.GetType()}:{ci.name} find destroyed object..");
                            }
                            cl.RemoveAt(i);
                            i--;
                            continue;
                        }
                    }
                }
            }
        }
#endif
        private static void Invoke(CallbackType ct)
        {
            var cl = callbacks[(int)ct];
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            CheckTarget(ct, cl);
#endif
            active.AddRange(cl);
            foreach (var ci in active)
            {
                ci.action.Invoke();
                if (ci.once)
                {
                    RemoveCallback(ct, ci.priority, ci.action);
                }
            }
            active.Clear();
        }
        private static void OnApplicationQuitting()
        {
            Invoke(CallbackType.Quitting);
        }
        private static void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            Invoke(CallbackType.SceneLoaded);
        }
        private static void OnSceneUnloaded(Scene arg0)
        {
            Invoke(CallbackType.SceneUnloaded);
        }
        private static void OnEndFrameRendering(ScriptableRenderContext arg1, Camera[] arg2)
        {
            Invoke(CallbackType.BeginFrameRendering);
        }
        private static void OnBeginFrameRendering(ScriptableRenderContext arg1, Camera[] arg2)
        {
            Invoke(CallbackType.EndFrameRendering);
        }

        private IEnumerator WaitForFixedUpdateCoroutine()
        {
            var waitForFixedUpdate = new WaitForFixedUpdate();
            for (; ; )
            {
                yield return waitForFixedUpdate;
                Invoke(CallbackType.WaitForFixedUpdate);
            }
        }
        private IEnumerator YieldNullCoroutine()
        {
            for (; ; )
            {
                yield return null;
                Invoke(CallbackType.YieldNull);
            }
        }
        private IEnumerator WaitForEndOfFrameCoroutine()
        {
            var waitForEndOfFrame = new WaitForEndOfFrame();
            for (; ; )
            {
                yield return waitForEndOfFrame;
                Invoke(CallbackType.WaitForEndOfFrame);
            }
        }
        private void FixedUpdate()
        {
            Invoke(CallbackType.FixedUpdate);
        }
        private void Update()
        {
            Invoke(CallbackType.Update);
        }
        private void LateUpdate()
        {
            Invoke(CallbackType.LateUpdate);
        }
        private void OnEnable()
        {
            StartCoroutine(WaitForFixedUpdateCoroutine());
            StartCoroutine(YieldNullCoroutine());
            StartCoroutine(WaitForEndOfFrameCoroutine());
        }
    }
}
