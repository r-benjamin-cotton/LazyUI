using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LazyUI
{
    /// <summary>
    /// プロパティの監視条件
    /// </summary>
    //[Serializable]
    //[DisallowMultipleComponent]// Unityの配列操作が複数だとエラーになる？
    public class PropertyCondition : MonoBehaviour
    {
        [Serializable]
        public class PropertyConditionEvent : UnityEvent<bool> { }

        public enum Logic
        {
            OR,
            AND,
        }
        [Serializable]
        public struct Expression
        {
            public Logic logic;
            public PropertyTestFunction function;
            [LazyProperty(PropertyValueType.Everything, true, false)]
            public LazyProperty property;
        }

        /// <summary>
        /// 条件式の説明を記載するため
        /// </summary>
        [SerializeField, Multiline]
        private string memo = "";

        [SerializeField]
        private Expression[] expressions = null;

        [SerializeField]
        [LazyProperty(PropertyValueType.Boolean)]
        private LazyProperty targetProperty = default;

        //[SerializeField]
        public PropertyConditionEvent onValueChanged = new();

        private bool condition = false;

        public bool Condition => condition;

        private enum TriState
        {
            HiZ,
            False,
            True,
        }
        public static bool Evaluate(Expression[] expressions)
        {
            if ((expressions == null) || (expressions.Length == 0))
            {
                return false;
            }
            var state = TriState.HiZ;
            for (int i = 0; i < expressions.Length; i++)
            {
                if (expressions[i].property == null)
                {
                    continue;
                }
                if (!expressions[i].property.TryTestValue(expressions[i].function, out bool v))
                {
                    v = false;
                }
                switch (expressions[i].logic)
                {
                    default:
                    case Logic.OR:
                        if (state == TriState.True)
                        {
                            return true;
                        }
                        else
                        {
                            state = v ? TriState.True : TriState.HiZ;
                        }
                        break;
                    case Logic.AND:
                        if (v)
                        {
                            if (state == TriState.HiZ)
                            {
                                state = TriState.True;
                            }
                        }
                        else
                        {
                            state = TriState.False;
                        }
                        break;
                }
            }
            return state == TriState.True;
        }
        public static void Setup(Expression[] expressions, Transform transform = null, bool warn = false, string memo = null)
        {
            if ((expressions == null) || (expressions.Length == 0))
            {
                return;
            }
            for (int i = 0; i < expressions.Length; i++)
            {
                if (expressions[i].property == null)
                {
                    expressions[i].property = new();
                }
                expressions[i].property.Invalidate();
                if (expressions[i].property.IsEmpty())
                {
                    continue;
                }
                if (!expressions[i].property.IsValid() && warn)
                {
                    if (string.IsNullOrEmpty(memo))
                    {
                        LazyDebug.LogWarning($"{transform.GetFullPath()}: [{i}:{expressions[i].property}] is not valid");
                    }
                    else
                    {
                        LazyDebug.LogWarning($"{memo}\n{transform.GetFullPath()}: [{i}:{expressions[i].property}] is not valid");
                    }
                }
            }
        }
        private void UpdateState(bool notify)
        {
            var cond = condition;
            condition = Evaluate(expressions);
            if (condition != targetProperty.GetValue(false))
            {
                targetProperty.SetValue(condition);
            }
            if ((condition != cond) && notify)
            {
                onValueChanged?.Invoke(cond);
            }
        }
        private void UpdateState()
        {
            UpdateState(true);
        }
        private void Awake()
        {
            Setup(expressions, transform, true, memo);
        }
        private void OnEnable()
        {
            condition = targetProperty.GetValue(false);
            LazyCallbacker.RegisterCallback(LazyCallbacker.CallbackType.YieldNull, 0, UpdateState);
        }
        private void OnDisable()
        {
            LazyCallbacker.RemoveCallback(LazyCallbacker.CallbackType.YieldNull, 0, UpdateState);
        }
#if false //UNITY_EDITOR
        private void DelayedUpdate()
        {
            if ((this == null) || !isActiveAndEnabled)
            {
                return;
            }
            Setup(expressions);
            //UpdateState(false);
        }
        private void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall += () => DelayedUpdate();
        }
#endif
    }
}
