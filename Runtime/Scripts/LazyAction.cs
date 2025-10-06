using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using UnityEngine;


namespace LazyUI
{
    /// <summary>
    /// コンポーネントのメソッドを指定し、Invokeで呼び出す。
    /// ※戻り値がvoidで引数は無しか組み込み型一つのメソッド
    /// gameObject.component.method.Invoke(parameter)
    /// </summary>
    [Serializable]
    public class LazyAction
    {
        [SerializeField]
        private GameObject gameObject;
        [SerializeField]
        private Component component;
        [SerializeField]
        private string method;
        [SerializeField]
        private string parameter;

        public enum ValueType
        {
            Invalid = -1,
            Boolean,
            Byte,
            SByte,
            Int16,
            UInt16,
            Int32,
            UInt32,
            Int64,
            UInt64,
            Single,
            Double,
            String,
            Enum,
            Vector2,
            Vector3,
            Vector4,
            Quaternion,
            Color,
        }

        private static readonly LazyReadOnlyDictionary<Type, ValueType> typeValueTypeMap = new(new Dictionary<Type, ValueType>()
        {
            { typeof(bool), ValueType.Boolean },
            { typeof(byte), ValueType.Byte },
            { typeof(sbyte), ValueType.SByte },
            { typeof(short), ValueType.Int16 },
            { typeof(ushort), ValueType.UInt16 },
            { typeof(int), ValueType.Int32 },
            { typeof(uint), ValueType.UInt32 },
            { typeof(long), ValueType.Int64 },
            { typeof(ulong), ValueType.UInt64 },
            { typeof(float), ValueType.Single },
            { typeof(double), ValueType.Double },
            { typeof(string), ValueType.String },
            { typeof(Enum), ValueType.Enum},
            { typeof(Vector2), ValueType.Vector2 },
            { typeof(Vector3), ValueType.Vector3 },
            { typeof(Vector4), ValueType.Vector4 },
            { typeof(Quaternion), ValueType.Quaternion },
            { typeof(Color), ValueType.Color },
        });
#if false
        private class MethodAction<T>
        {
            private readonly Delegate _methodDelegate = null;
            public MethodAction(object instance, MethodInfo methodInfo)
            {
                if (methodInfo != null)
                {
                    try
                    {
                        _methodDelegate = Delegate.CreateDelegate(typeof(Action<T>), instance, methodInfo);
                    }
                    catch (Exception ex)
                    {
                        LazyDebug.LogWarning(ex);
                    }
                }
            }
            public void Invoke(T value)
            {
                if (_methodDelegate is not Action<T> action)
                {
                    return;
                }
                action.Invoke(value);
            }
        }
#endif
        private bool invalid = false;

        private MethodInfo _methodInfo = null;
        private Type _parameterType = null;
        private object _parameterValue = null;
        private readonly object[] _parameters = new object[1];


#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private string _lastErrorMessage = null;
#endif

        public void Invoke()
        {
            if (!Validate())
            {
                return;
            }
            _methodInfo.Invoke(component, (_parameterValue == null) ? null : _parameters);
        }
        public bool IsEmpty()
        {
            if (string.IsNullOrEmpty(method))
            {
                return true;
            }
            if (component == null)
            {
                return true;
            }
            if (gameObject == null)
            {
                return true;
            }
            return false;
        }
        public bool IsValid()
        {
            if (!Validate())
            {
                return false;
            }
            return true;
        }

        private void ClearMethodInfos()
        {
            _methodInfo = null;
            _parameterType = null;
            _parameterValue = null;
            _parameters[0] = null;
        }
        public void Invalidate()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _lastErrorMessage = null;
#endif
            invalid = false;
            ClearMethodInfos();
        }
        public bool Validate()
        {
            if (component == null)
            {
                return false;
            }
            if (invalid)
            {
                return false;
            }
            if (!SetupMethofInfos())
            {
                ClearMethodInfos();
                invalid = true;
                return false;
            }
            return true;
        }
        private bool SetupMethofInfos()
        {
            if (_methodInfo != null)
            {
                return true;
            }

            if (gameObject == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                _lastErrorMessage = $"gameObject is null";
#endif
                return false;
            }
            if (component == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                _lastErrorMessage = $"component is null";
#endif
                return false;
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ReferenceEquals(component.gameObject, gameObject))
            {
                _lastErrorMessage = $"Illegal component [{component.gameObject.name} // {gameObject.name}]";
                return false;
            }
#endif

            if (string.IsNullOrEmpty(method))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                _lastErrorMessage = $"No method";
#endif
                return false;
            }
            var methodName = method;
            {
                var bp = method.IndexOf('(');
                if (bp >= 0)
                {
                    var ep = method.IndexOf(')');
                    if (ep <= bp)
                    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        _lastErrorMessage = $"Illegal Method<{component.GetType().Name}.{method}>";
#endif
                        return false;
                    }
                    methodName = method[..bp];
                    var typeName = method.Substring(bp + 1, ep - bp - 1);
                    _parameterType = Type.GetType(typeName);
                    if (_parameterType == null)
                    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        _lastErrorMessage = $"Illegal Method<{component.GetType().Name}.{method}>";
#endif
                        return false;
                    }
                }
            }
            {
                var targetType = (component is Transform) ? typeof(GameObject) : component.GetType();
                if (_parameterType == null)
                {
                    _methodInfo = targetType.GetMethod(methodName);
                }
                else
                {
                    _methodInfo = targetType.GetMethod(methodName, new Type[] { _parameterType });
                }
                if ((_methodInfo == null) || !_methodInfo.IsPublic)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    _lastErrorMessage = $"Illegal Method<{component.GetType().Name}.{method}>";
#endif
                    return false;
                }
            }
            if (!string.IsNullOrEmpty(parameter))
            {
                if (!TryParse(parameter, _parameterType, out object result))
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    _lastErrorMessage = $"Illegal ParameterValue<{_parameterType}> {parameter}";
#endif
                    return false;
                }
#if true
                if (_parameterType == typeof(Quaternion))
                {
                    if (result is Vector3 v)
                    {
                        result = Quaternion.Euler(v);
                    }
                }
#endif
                _parameterValue = result;
            }
            {
                _parameters[0] = _parameterValue;
            }
            return true;
        }

        public bool Equals(LazyAction other)
        {
            if (other == null)
            {
                return false;
            }
            if (!ReferenceEquals(gameObject, other.gameObject))
            {
                return false;
            }
            if (!ReferenceEquals(component, other.component))
            {
                return false;
            }
            if (method != other.method)
            {
                return false;
            }
            if (parameter != other.parameter)
            {
                return false;
            }
            return true;
        }
        public override bool Equals(object obj)
        {
            if (obj is not LazyAction ps)
            {
                return false;
            }
            return Equals(ps);
        }
        public static bool operator ==(LazyAction a, LazyAction b)
        {
            if ((a is null) || (b is null))
            {
                return (a is null) && (b is null);
            }
            return a.Equals(b);
        }
        public static bool operator !=(LazyAction a, LazyAction b)
        {
            if ((a is null) || (b is null))
            {
                return (a is not null) || (b is not null);
            }
            return !a.Equals(b);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(gameObject, component, method, parameter);
        }
        public override string ToString()
        {
            if (gameObject == null)
            {
                return "<Empty>";
            }
            if (component == null)
            {
                return $"{gameObject.transform.GetFullPath()}:<Empty>";
            }
            var str = gameObject.transform.GetFullPath();
            if (component is Transform)
            {
                str = $"{str}:GameObject.{(string.IsNullOrEmpty(method) ? "<Empty>" : method)}";
            }
            else
            {
                str = $"{str}:{component.GetType().Name}.{(string.IsNullOrEmpty(method) ? "<Empty>" : method)}";
            }
            if (!string.IsNullOrEmpty(parameter))
            {
                str = $"{str}={parameter}";
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!string.IsNullOrEmpty(_lastErrorMessage))
            {
                str = $"{str}\n{_lastErrorMessage}";
            }
#endif
            return str;
        }


        private static bool Vector2TryParse(string value, out Vector2 result)
        {
            result = default;
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }
            value = value.Trim();
            if (!value.StartsWith('(') || !value.EndsWith(')'))
            {
                return false;
            }
            var tt = value[1..^1].Split(',');
            if (tt.Length != 2)
            {
                return false;
            }
            if (!float.TryParse(tt[0], out var v0))
            {
                return false;
            }
            if (!float.TryParse(tt[1], out var v1))
            {
                return false;
            }
            result = new Vector2(v0, v1);
            return true;
        }
        private static bool Vector3TryParse(string value, out Vector3 result)
        {
            result = default;
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }
            value = value.Trim();
            if (!value.StartsWith('(') || !value.EndsWith(')'))
            {
                return false;
            }
            var tt = value[1..^1].Split(',');
            if (tt.Length != 3)
            {
                return false;
            }
            if (!float.TryParse(tt[0], out var v0))
            {
                return false;
            }
            if (!float.TryParse(tt[1], out var v1))
            {
                return false;
            }
            if (!float.TryParse(tt[2], out var v2))
            {
                return false;
            }
            result = new Vector3(v0, v1, v2);
            return true;
        }
        private static bool Vector4TryParse(string value, out Vector4 result)
        {
            result = default;
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }
            value = value.Trim();
            if (!value.StartsWith('(') || !value.EndsWith(')'))
            {
                return false;
            }
            var tt = value[1..^1].Split(',');
            if (tt.Length != 4)
            {
                return false;
            }
            if (!float.TryParse(tt[0], out var v0))
            {
                return false;
            }
            if (!float.TryParse(tt[1], out var v1))
            {
                return false;
            }
            if (!float.TryParse(tt[2], out var v2))
            {
                return false;
            }
            if (!float.TryParse(tt[3], out var v3))
            {
                return false;
            }
            result = new Vector4(v0, v1, v2, v3);
            return true;
        }
        private static bool QuaternionTryParse(string value, out Quaternion result)
        {
            result = default;
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }
            value = value.Trim();
            if (!value.StartsWith('(') || !value.EndsWith(')'))
            {
                return false;
            }
            var tt = value[1..^1].Split(',');
            if (tt.Length != 4)
            {
                return false;
            }
            if (!float.TryParse(tt[0], out var v0))
            {
                return false;
            }
            if (!float.TryParse(tt[1], out var v1))
            {
                return false;
            }
            if (!float.TryParse(tt[2], out var v2))
            {
                return false;
            }
            if (!float.TryParse(tt[3], out var v3))
            {
                return false;
            }
            result = new Quaternion(v0, v1, v2, v3);
            return true;
        }
        private static bool ColorTryParse(string value, out Color result)
        {
            result = default;
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }
            value = value.Trim();
            if (!value.StartsWith("RGBA(") || !value.EndsWith(')'))
            {
                return false;
            }
            var tt = value[5..^1].Split(',');
            if (tt.Length != 4)
            {
                return false;
            }
            if (!float.TryParse(tt[0], out var v0))
            {
                return false;
            }
            if (!float.TryParse(tt[1], out var v1))
            {
                return false;
            }
            if (!float.TryParse(tt[2], out var v2))
            {
                return false;
            }
            if (!float.TryParse(tt[3], out var v3))
            {
                return false;
            }
            result = new Color(v0, v1, v2, v3);
            return true;
        }
        public static ValueType GetValueType(Type type)
        {
            if (type == null)
            {
                return ValueType.Invalid;
            }
            if (type.IsEnum)
            {
                return ValueType.Enum;
            }
            if (typeValueTypeMap.TryGetValue(type, out ValueType valueType))
            {
                return valueType;
            }
            return ValueType.Invalid;
        }
        public static bool TryParse(string value, Type parameterType, out object result)
        {
            result = null;
            var valueType = GetValueType(parameterType);
            if ((valueType == ValueType.Invalid) || ((valueType != ValueType.String) && string.IsNullOrEmpty(value)))
            {
                return false;
            }
            switch (valueType)
            {
                default:
                case ValueType.Invalid:
                    break;
                case ValueType.Boolean:
                    {
                        if (bool.TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case ValueType.Byte:
                    {
                        if (byte.TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case ValueType.SByte:
                    {
                        if (sbyte.TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case ValueType.Int16:
                    {
                        if (short.TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case ValueType.UInt16:
                    {
                        if (ushort.TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case ValueType.Int32:
                    {
                        if (int.TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case ValueType.UInt32:
                    {
                        if (uint.TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case ValueType.Int64:
                    {
                        if (long.TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case ValueType.UInt64:
                    {
                        if (ulong.TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case ValueType.Single:
                    {
                        if (float.TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case ValueType.Double:
                    {
                        if (double.TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case ValueType.String:
                    {
                        result = value;
                        return true;
                    }
                //break;
                case ValueType.Enum:
                    {
                        if (Enum.TryParse(parameterType, value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case ValueType.Vector2:
                    {
                        if (Vector2TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case ValueType.Vector3:
                    {
                        if (Vector3TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case ValueType.Vector4:
                    {
                        if (Vector4TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case ValueType.Quaternion:
                    {
                        //if (QuaternionTryParse(value, out var v))
                        if (Vector3TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case ValueType.Color:
                    {
                        if (ColorTryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
            }
            return false;
        }
        public static string FormatString(object value, Type parameterType, string format = null)
        {
            var text = "";
            var valueType = GetValueType(parameterType);
            if (valueType == ValueType.Invalid)
            {
                text = "";
            }
            else if (value == null)
            {
                text = "";
            }
            else if (string.IsNullOrEmpty(format))
            {
#if true
                text = value?.ToString();
#else
                switch (valueType)
                {
                    default:
                    case ValueType.Invalid:
                        break;
                    case ValueType.Boolean:
                    case ValueType.Byte:
                    case ValueType.SByte:
                    case ValueType.Int16:
                    case ValueType.UInt16:
                    case ValueType.Int32:
                    case ValueType.UInt32:
                    case ValueType.Int64:
                    case ValueType.UInt64:
                    case ValueType.Single:
                    case ValueType.Double:
                    case ValueType.String:
                    case ValueType.Enum:
                        {
                            text = value.ToString();
                        }
                        break;
                    case ValueType.Vector2:
                        {
                            var v0 = (Vector2)value;
                            text = $"({v0.x},{v0.y})";
                        }
                        break;
                    case ValueType.Vector3:
                        {
                            var v0 = (Vector3)value;
                            text = $"({v0.x},{v0.y},{v0.z})";
                        }
                        break;
                    case ValueType.Vector4:
                        {
                            var v0 = (Vector4)value;
                            text = $"({v0.x},{v0.y},{v0.z},{v0.w})";
                        }
                        break;
                    case ValueType.Quaternion:
                        {
#if false
                            var v0 = (Quaternion)value;
                            text = $"({v0.x},{v0.y},{v0.z},{v0.w})";
#else
                            var v0 = (Vector3)value;
                            text = $"({v0.x},{v0.y},{v0.z})";
#endif
                        }
                        break;
                    case ValueType.Color:
                        {
                            var v0 = (Color)value;
                            text = $"RGBA({v0.r},{v0.g},{v0.b},{v0.a})";
                        }
                        break;
                }
#endif
            }
            else
            {
                switch (valueType)
                {
                    default:
                    case ValueType.Invalid:
                        break;
                    case ValueType.Boolean:
                        text = ((bool)value).ToString();
                        break;
                    case ValueType.Byte:
                        text = ((byte)value).ToString(format);
                        break;
                    case ValueType.SByte:
                        text = ((sbyte)value).ToString(format);
                        break;
                    case ValueType.Int16:
                        text = ((short)value).ToString(format);
                        break;
                    case ValueType.UInt16:
                        text = ((ushort)value).ToString(format);
                        break;
                    case ValueType.Int32:
                        text = ((int)value).ToString(format);
                        break;
                    case ValueType.UInt32:
                        text = ((uint)value).ToString(format);
                        break;
                    case ValueType.Int64:
                        text = ((long)value).ToString(format);
                        break;
                    case ValueType.UInt64:
                        text = ((ulong)value).ToString(format);
                        break;
                    case ValueType.Single:
                        text = ((float)value).ToString(format);
                        break;
                    case ValueType.Double:
                        text = ((double)value).ToString(format);
                        break;
                    case ValueType.String:
                    case ValueType.Enum:
                        text = value.ToString();
                        break;
                    case ValueType.Vector2:
                        text = ((Vector2)value).ToString(format);
                        break;
                    case ValueType.Vector3:
                        text = ((Vector3)value).ToString(format);
                        break;
                    case ValueType.Vector4:
                        text = ((Vector4)value).ToString(format);
                        break;
                    case ValueType.Quaternion:
                        //text = ((Quaternion)value).ToString(format);
                        text = ((Vector3)value).ToString(format);
                        break;
                    case ValueType.Color:
                        text = ((Color)value).ToString(format);
                        break;
                }
            }
            return text;
        }
    }
}
