//#define DONTUSE_DELEGATE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using UnityEngine;


namespace LazyUI
{
    [Flags]
    public enum PropertyValueType : uint
    {
        Invalid = 0,
        Nothing = 0,
        Everything = 0xffffffffu,

        Boolean = 1 << 0,
        Byte = 1 << 1,
        SByte = 1 << 2,
        Int16 = 1 << 3,
        UInt16 = 1 << 4,
        Int32 = 1 << 5,
        UInt32 = 1 << 6,
        Int64 = 1 << 7,
        UInt64 = 1 << 8,
        Single = 1 << 9,
        Double = 1 << 10,
        String = 1 << 11,
        Enum = 1 << 12,
        Flags = 1 << 13,
        Vector2 = 1 << 14,
        Vector3 = 1 << 15,
        Vector4 = 1 << 16,
        Quaternion = 1 << 17,
        Color = 1 << 18,
        IntRange = 1 << 19,
        FloatRange = 1 << 20,
    }

    public enum PropertyTestFunction
    {
        Always,
        Never,
        Less,
        Equal,
        LEqual,
        Greater,
        NotEqual,
        GEqual,
    }

    /// <summary>
    /// プロパティ指定アトリビュート
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class LazyPropertyAttribute : Attribute
    {
        public readonly PropertyValueType allow;
        public readonly bool withValue;
        public readonly bool writable;
        public readonly Type componentType;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="allow">受け入れる型</param>
        /// <param name="withValue">値を指定する</param>
        /// <param name="writable">書き込み可能なものに限る</param>
        /// <param name="componentType">指定コンポーネントに限る</param>
        public LazyPropertyAttribute(PropertyValueType allow = PropertyValueType.Everything, bool withValue = false, bool writable = true, Type componentType = null)
        {
            this.allow = allow;
            this.withValue = withValue;
            this.writable = writable;
            this.componentType = componentType;
        }
    }

    /// <summary>
    /// コンポーネントの特定のプロパティを指定
    /// gameObject.component.propertyName[.propertyElement] = propertyValue
    /// </summary>
    [Serializable]
    public class LazyProperty
    {
        [SerializeField]
        private GameObject gameObject;
        [SerializeField]
        private Component component;
        [SerializeField]
        private string propertyName;
        [SerializeField]
        private string propertyElement;
        [SerializeField]
        private string propertyValue;

        private static readonly ReadOnlyDictionary<Type, PropertyValueType> typePropertyValueTypeMap = new(new Dictionary<Type, PropertyValueType>()
        {
            { typeof(bool), PropertyValueType.Boolean },
            { typeof(byte), PropertyValueType.Byte },
            { typeof(sbyte), PropertyValueType.SByte },
            { typeof(short), PropertyValueType.Int16 },
            { typeof(ushort), PropertyValueType.UInt16 },
            { typeof(int), PropertyValueType.Int32 },
            { typeof(uint), PropertyValueType.UInt32 },
            { typeof(long), PropertyValueType.Int64 },
            { typeof(ulong), PropertyValueType.UInt64 },
            { typeof(float), PropertyValueType.Single },
            { typeof(double), PropertyValueType.Double },
            { typeof(string), PropertyValueType.String },
            { typeof(Enum), PropertyValueType.Enum},
            { typeof(Vector2), PropertyValueType.Vector2 },
            { typeof(Vector3), PropertyValueType.Vector3 },
            { typeof(Vector4), PropertyValueType.Vector4 },
            { typeof(Quaternion), PropertyValueType.Quaternion },
            { typeof(Color), PropertyValueType.Color },
            { typeof(Range<int>), PropertyValueType.IntRange },
            { typeof(Range<float>), PropertyValueType.FloatRange },
        });
        private static readonly ReadOnlyDictionary<PropertyValueType, Type> propertyValueTypeTypeMap = new(new Dictionary<PropertyValueType, Type>()
        {
            { PropertyValueType.Invalid, null },
            { PropertyValueType.Boolean, typeof(bool) },
            { PropertyValueType.Byte, typeof(byte) },
            { PropertyValueType.SByte, typeof(sbyte) },
            { PropertyValueType.Int16, typeof(short) },
            { PropertyValueType.UInt16, typeof(ushort) },
            { PropertyValueType.Int32, typeof(int) },
            { PropertyValueType.UInt32, typeof(uint) },
            { PropertyValueType.Int64, typeof(long) },
            { PropertyValueType.UInt64, typeof(ulong) },
            { PropertyValueType.Single, typeof(float) },
            { PropertyValueType.Double, typeof(double) },
            { PropertyValueType.String, typeof(string) },
            { PropertyValueType.Enum, null },
            { PropertyValueType.Vector2, typeof(Vector2) },
            { PropertyValueType.Vector3, typeof(Vector3) },
            { PropertyValueType.Vector4, typeof(Vector4) },
            { PropertyValueType.Quaternion, typeof(Quaternion) },
            { PropertyValueType.Color, typeof(Color) },
        });
        private static readonly ReadOnlyDictionary<PropertyValueType, Type> propertyValueTypeSetterTypeMap = new(new Dictionary<PropertyValueType, Type>()
        {
            { PropertyValueType.Invalid, null },
            { PropertyValueType.Boolean, typeof(Action<bool>) },
            { PropertyValueType.Byte, typeof(Action<byte>) },
            { PropertyValueType.SByte, typeof(Action<sbyte>) },
            { PropertyValueType.Int16, typeof(Action<short>) },
            { PropertyValueType.UInt16, typeof(Action<ushort>) },
            { PropertyValueType.Int32, typeof(Action<int>) },
            { PropertyValueType.UInt32, typeof(Action<uint>) },
            { PropertyValueType.Int64, typeof(Action<long>) },
            { PropertyValueType.UInt64, typeof(Action<ulong>) },
            { PropertyValueType.Single, typeof(Action<float>) },
            { PropertyValueType.Double, typeof(Action<double>) },
            { PropertyValueType.String, typeof(Action<string>) },
            { PropertyValueType.Enum, null },
            { PropertyValueType.Vector2, typeof(Action<Vector2>) },
            { PropertyValueType.Vector3, typeof(Action<Vector3>) },
            { PropertyValueType.Vector4, typeof(Action<Vector4>) },
            { PropertyValueType.Quaternion, typeof(Action<Quaternion>) },
            { PropertyValueType.Color, typeof(Action<Color>) },
        });
        private static readonly ReadOnlyDictionary<PropertyValueType, Type> propertyValueTypeGetterTypeMap = new(new Dictionary<PropertyValueType, Type>()
        {
            { PropertyValueType.Invalid, null },
            { PropertyValueType.Boolean, typeof(Func<bool>) },
            { PropertyValueType.Byte, typeof(Func<byte>) },
            { PropertyValueType.SByte, typeof(Func<sbyte>) },
            { PropertyValueType.Int16, typeof(Func<short>) },
            { PropertyValueType.UInt16, typeof(Func<ushort>) },
            { PropertyValueType.Int32, typeof(Func<int>) },
            { PropertyValueType.UInt32, typeof(Func<uint>) },
            { PropertyValueType.Int64, typeof(Func<long>) },
            { PropertyValueType.UInt64, typeof(Func<ulong>) },
            { PropertyValueType.Single, typeof(Func<float>) },
            { PropertyValueType.Double, typeof(Func<double>) },
            { PropertyValueType.String, typeof(Func<string>) },
            { PropertyValueType.Enum, null },
            { PropertyValueType.Vector2, typeof(Func<Vector2>) },
            { PropertyValueType.Vector3, typeof(Func<Vector3>) },
            { PropertyValueType.Vector4, typeof(Func<Vector4>) },
            { PropertyValueType.Quaternion, typeof(Func<Quaternion>) },
            { PropertyValueType.Color, typeof(Func<Color>) },
        });
#if false
        private class PropertyGetter<T>
        {
            private readonly Delegate _propertyGetterDelegate = null;
            public PropertyGetter(PropertyInfo propertyInfo, object instance)
            {
                var methodInfo = propertyInfo.GetGetMethod();
                if (methodInfo != null)
                {
                    try
                    {
                        _propertyGetterDelegate = Delegate.CreateDelegate(typeof(Func<T>), instance, methodInfo);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            public T GetValue()
            {
                if (_propertyGetterDelegate is not Func<T> func)
                {
                    return default;
                }
                return func();
            }
        }
        private class PropertySetter<T>
        {
            private readonly Delegate _propertySetterDelegate = null;
            public PropertySetter(PropertyInfo propertyInfo, object instance)
            {
                    var methodInfo = propertyInfo.GetSetMethod();
                    if (methodInfo != null)
                    {
                        try
                        {
                            _propertySetterDelegate = Delegate.CreateDelegate(typeof(Action<T>), instance, methodInfo);
                        }
                        catch (Exception ex)
                        {
                        }
                }
            }
            public void SetValue(T value)
            {
                if (_propertySetterDelegate is not Action<T> action)
                {
                    return;
                }
                action(value);
            }
        }
#endif
        private bool invalid = false;
        private bool invalidRange = false;

        private bool _cantWrite = false;
        private PropertyInfo _propertyInfo = null;
        private PropertyInfo _elementPropertyInfo = null;
        private FieldInfo _elementFieldInfo = null;
        private Type _propertyType = null;
        private PropertyValueType _propertyValueType = PropertyValueType.Invalid;
        private object _propertyValue = null;

        private bool _structRange = false;
        private FieldInfo _propertyStructRangeMinFieldInfo = null;
        private FieldInfo _propertyStructRangeMaxFieldInfo = null;

        private PropertyInfo _propertyRangePropertyInfo = null;
        private FieldInfo _propertyRangeMinFieldInfo = null;
        private FieldInfo _propertyRangeMaxFieldInfo = null;

        private Type _enumType = null;
        private PropertyValueType _enumValueType = PropertyValueType.Invalid;
        private Array _enumValues = null;

        private bool _useDelegate = false;
        private Delegate _propertySetterDelegate = null;
        private Delegate _propertyGetterDelegate = null;
        private Delegate _propertyRangeGetterDelegate = null;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private string _lastErrorMessage = null;
#endif

        public Type GetPropertyType()
        {
            if (!Validate())
            {
                return null;
            }
            return _propertyType;
        }
        public PropertyValueType GetValueType()
        {
            if (!Validate())
            {
                return PropertyValueType.Invalid;
            }
            return _propertyValueType;
        }
        public PropertyValueType GetEnumValueType()
        {
            if (!Validate())
            {
                return PropertyValueType.Invalid;
            }
            return _enumValueType;
        }
        public object GetValue()
        {
            if (!Validate())
            {
                return null;
            }
            if (_useDelegate && (_propertyGetterDelegate != null))
            {
                return _propertyGetterDelegate.DynamicInvoke();
            }
            var val = _propertyInfo.GetValue(component);
            if (val == null)
            {
                return null;
            }
            if (!_structRange)
            {
                if (_elementFieldInfo != null)
                {
                    return _elementFieldInfo.GetValue(val);
                }
                else if (_elementPropertyInfo != null)
                {
                    return _elementPropertyInfo.GetValue(val);
                }
                else
                {
                    return val;
                }
            }
            else
            {
                // property:Range<struct>.min/max.element => Range<int/float>
                var minVal = _propertyStructRangeMinFieldInfo.GetValue(val);
                var maxVal = _propertyStructRangeMaxFieldInfo.GetValue(val);
                object vmin;
                object vmax;
                if (_elementFieldInfo != null)
                {
                    vmin = _elementFieldInfo.GetValue(minVal);
                    vmax = _elementFieldInfo.GetValue(maxVal);
                }
                else if (_elementPropertyInfo != null)
                {
                    vmin = _elementPropertyInfo.GetValue(minVal);
                    vmax = _elementPropertyInfo.GetValue(maxVal);
                }
                else
                {
                    return null;
                }
                switch (_propertyValueType)
                {
                    case PropertyValueType.IntRange:
                        {
                            if ((vmin is int v0) && (vmax is int v1))
                            {
                                return new Range<int>(v0, v1);
                            }
                        }
                        break;
                    case PropertyValueType.FloatRange:
                        {
                            if ((vmin is float v0) && (vmax is float v1))
                            {
                                return new Range<float>(v0, v1);
                            }
                        }
                        break;
                    default:
                        break;
                }
                return null;
            }
        }
        private bool TryGetValue<T0, T1>(out T0 v0, T1 v1) where T0 : struct where T1 : struct
        {
            if (v1 is not T0 vt)
            {
                v0 = default;
                return false;
            }
            else
            {
                v0 = vt;
                return true;
            }
        }
        public bool TryGetValue<T>(out T value) where T : struct
        {
            value = default;
            if (!Validate())
            {
                return false;
            }
            if (_propertyGetterDelegate is Func<T> getter)
            {
                value = getter();
                return true;
            }
            var val = _propertyInfo.GetValue(component);
            if (val == null)
            {
                return false;
            }
            if (!_structRange)
            {
                object vo;
                if (_elementFieldInfo != null)
                {
                    vo = _elementFieldInfo.GetValue(val);
                }
                else if (_elementPropertyInfo != null)
                {
                    vo = _elementPropertyInfo.GetValue(val);
                }
                else
                {
                    vo = val;
                }
                if (vo is not T vt)
                {
                    return false;
                }
                value = vt;
                return true;
            }
            else
            {
                // property:Range<struct>.min/max.element => Range<int/float>
                var minVal = _propertyStructRangeMinFieldInfo.GetValue(val);
                var maxVal = _propertyStructRangeMaxFieldInfo.GetValue(val);
                object vmin;
                object vmax;
                if (_elementFieldInfo != null)
                {
                    vmin = _elementFieldInfo.GetValue(minVal);
                    vmax = _elementFieldInfo.GetValue(maxVal);
                }
                else if (_elementPropertyInfo != null)
                {
                    vmin = _elementPropertyInfo.GetValue(minVal);
                    vmax = _elementPropertyInfo.GetValue(maxVal);
                }
                else
                {
                    return false;
                }
                switch (_propertyValueType)
                {
                    case PropertyValueType.IntRange:
                        {
                            if ((vmin is int v0) && (vmax is int v1))
                            {
                                var vt = new Range<int>(v0, v1);
                                return TryGetValue(out value, vt);
                            }
                        }
                        break;
                    case PropertyValueType.FloatRange:
                        {
                            if ((vmin is float v0) && (vmax is float v1))
                            {
                                var vt = new Range<float>(v0, v1);
                                return TryGetValue(out value, vt);
                            }
                        }
                        break;
                    default:
                        break;
                }
                return false;
            }
        }
        public T GetValue<T>(T defaultValue = default) where T : struct
        {
            if (!TryGetValue(out T val))
            {
                return defaultValue;
            }
            return val;
        }
        public bool SetValue<T>(Range<T> value) where T : struct, IEquatable<T>
        {
            if (!Validate())
            {
                return false;
            }
            if (_cantWrite)
            {
                return false;
            }
            if (_propertySetterDelegate is Action<Range<T>> setter)
            {
                setter(value);
                return true;
            }
            if (!_structRange)
            {
                return SetValue((object)value);
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (typeof(Range<T>) != _propertyType)
            {
                _lastErrorMessage = $"SetValue<{typeof(T)}>(): Type mismatch <{_propertyType}>";
                return false;
            }
#endif
            if (_elementFieldInfo != null)
            {
                var val = _propertyInfo.GetValue(component);
                var valMin = _propertyStructRangeMinFieldInfo.GetValue(val);
                var valMax = _propertyStructRangeMaxFieldInfo.GetValue(val);
                _elementFieldInfo.SetValue(valMin, value.MinValue);
                _elementFieldInfo.SetValue(valMax, value.MaxValue);
                _propertyStructRangeMinFieldInfo.SetValue(val, valMin);
                _propertyStructRangeMaxFieldInfo.SetValue(val, valMax);
                _propertyInfo.SetValue(component, val);
                return true;
            }
            else if (_elementPropertyInfo != null)
            {
                var val = _propertyInfo.GetValue(component);
                var valMin = _propertyStructRangeMinFieldInfo.GetValue(val);
                var valMax = _propertyStructRangeMaxFieldInfo.GetValue(val);
                _elementPropertyInfo.SetValue(valMin, value.MinValue);
                _elementPropertyInfo.SetValue(valMax, value.MaxValue);
                _propertyStructRangeMinFieldInfo.SetValue(val, valMin);
                _propertyStructRangeMaxFieldInfo.SetValue(val, valMax);
                _propertyInfo.SetValue(component, val);
                return true;
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                _lastErrorMessage = $"SetValue<{typeof(T)}>(): Illegal..";
#endif
                return false;
            }
        }
        public bool SetValue(object value)
        {
            if (!Validate())
            {
                return false;
            }
            if (_cantWrite)
            {
                return false;
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var valueType = (value != null) ? value.GetType() : _propertyType;
            if (valueType != _propertyType)
            {
                _lastErrorMessage = $"SetValue<{valueType}>(): Type mismatch <{_propertyType}>";
                return false;
            }
#endif
            if (_useDelegate && (_propertySetterDelegate != null))
            {
                _propertySetterDelegate.DynamicInvoke(value);
                return true;
            }
            if (_elementFieldInfo != null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (_elementFieldInfo.FieldType != valueType)
                {
                    _lastErrorMessage = $"SetValue(): Type mismatch  <{_elementFieldInfo.FieldType}> // {valueType}";
                    return false;
                }
#endif
                var val = _propertyInfo.GetValue(component);
                _elementFieldInfo.SetValue(val, value);
                _propertyInfo.SetValue(component, val);
                return true;
            }
            else if (_elementPropertyInfo != null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if ((_elementPropertyInfo.SetMethod == null) || !_elementPropertyInfo.SetMethod.IsPublic)
                {
                    _lastErrorMessage = $"SetValue(): Can't write <{_elementPropertyInfo.PropertyType}>";
                    _cantWrite = true;
                    return false;
                }
                if (_elementPropertyInfo.PropertyType != valueType)
                {
                    _lastErrorMessage = $"SetValue(): Type mismatch <{_elementPropertyInfo.PropertyType}> // {valueType}";
                    return false;
                }
#endif
                var val = _propertyInfo.GetValue(component);
                _elementPropertyInfo.SetValue(val, value);
                _propertyInfo.SetValue(component, val);
                return true;
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (_propertyInfo.PropertyType != valueType)
                {
                    _lastErrorMessage = $"SetValue(): Type mismatch <{_propertyInfo.PropertyType}> // {valueType}";
                    return false;
                }
#endif
                _propertyInfo.SetValue(component, value);
                return true;
            }
        }
        public bool SetValue<T>(T value) where T : IConvertible
        {
            if (!Validate())
            {
                return false;
            }
            if (_cantWrite)
            {
                return false;
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var valueType = typeof(T);
            if ((valueType != _propertyType) && (valueType != _enumType))
            {
                _lastErrorMessage = $"SetValue<{valueType}>(): Type mismatch <{_propertyType}>";
                return false;
            }
#endif
            if (_propertySetterDelegate is Action<T> setter)
            {
                setter(value);
                return true;
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if ((_propertyInfo.SetMethod == null) || !_propertyInfo.SetMethod.IsPublic)
            {
                _lastErrorMessage = $"SetValue(): Can't write <{_propertyType}>";
                _cantWrite = true;
                return false;
            }
#endif
            if (_elementFieldInfo != null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (_elementFieldInfo.FieldType != valueType)
                {
                    _lastErrorMessage = $"SetValue(): Type mismatch  <{_elementFieldInfo.FieldType}> // {valueType}";
                    return false;
                }
#endif
                var val = _propertyInfo.GetValue(component);
                _elementFieldInfo.SetValue(val, value);
                _propertyInfo.SetValue(component, val);
                return true;
            }
            else if (_elementPropertyInfo != null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if ((_elementPropertyInfo.SetMethod == null) || !_elementPropertyInfo.SetMethod.IsPublic)
                {
                    _lastErrorMessage = $"SetValue(): Can't write <{_elementPropertyInfo.PropertyType}>";
                    _cantWrite = true;
                    return false;
                }
                if (_elementPropertyInfo.PropertyType != valueType)
                {
                    _lastErrorMessage = $"SetValue(): Type mismatch <{_elementPropertyInfo.PropertyType}> // {valueType}";
                    return false;
                }
#endif
                var val = _propertyInfo.GetValue(component);
                _elementPropertyInfo.SetValue(val, value);
                _propertyInfo.SetValue(component, val);
                return true;
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (_propertyInfo.PropertyType != valueType)
                {
                    _lastErrorMessage = $"SetValue(): Type mismatch <{_propertyInfo.PropertyType}> // {valueType}";
                    return false;
                }
#endif
                _propertyInfo.SetValue(component, value);
                return true;
            }
        }
        public int GetEnumValueCount()
        {
            return (_enumValues == null) ? 0 : _enumValues.Length;
        }
        private int GetEnumValueIndex<T>() where T : struct
        {
            if (!TryGetValue(out T val))
            {
                return -1;
            }
            if (_enumValues is not T[] tbl)
            {
                return -1;
            }
            return Array.IndexOf(tbl, val);
        }
        public int GetEnumValueIndex()
        {
            switch (_enumValueType)
            {
                default:
                case PropertyValueType.Boolean:
                case PropertyValueType.String:
                case PropertyValueType.Enum:
                case PropertyValueType.Vector2:
                case PropertyValueType.Vector3:
                case PropertyValueType.Vector4:
                case PropertyValueType.Color:
                case PropertyValueType.Quaternion:
                    return -1;
                case PropertyValueType.Byte:
                    return GetEnumValueIndex<byte>();
                case PropertyValueType.SByte:
                    return GetEnumValueIndex<sbyte>();
                case PropertyValueType.Int16:
                    return GetEnumValueIndex<short>();
                case PropertyValueType.UInt16:
                    return GetEnumValueIndex<ushort>();
                case PropertyValueType.Int32:
                    return GetEnumValueIndex<int>();
                case PropertyValueType.UInt32:
                    return GetEnumValueIndex<uint>();
                case PropertyValueType.Int64:
                    return GetEnumValueIndex<long>();
                case PropertyValueType.UInt64:
                    return GetEnumValueIndex<ulong>();
            }
        }
        private void SetEnumValueIndex<T>(int index) where T : IConvertible
        {
            if (_enumValues is not T[] tbl)
            {
                return;
            }
            if ((index < 0) || (index >= tbl.Length))
            {
                return;
            }
            SetValue<T>(tbl[index]);
        }
        public void SetEnumValueIndex(int index)
        {
            switch (_enumValueType)
            {
                default:
                case PropertyValueType.Boolean:
                case PropertyValueType.String:
                case PropertyValueType.Enum:
                case PropertyValueType.Vector2:
                case PropertyValueType.Vector3:
                case PropertyValueType.Vector4:
                case PropertyValueType.Color:
                case PropertyValueType.Quaternion:
                    break;
                case PropertyValueType.Byte:
                    SetEnumValueIndex<byte>(index);
                    break;
                case PropertyValueType.SByte:
                    SetEnumValueIndex<sbyte>(index);
                    break;
                case PropertyValueType.Int16:
                    SetEnumValueIndex<short>(index);
                    break;
                case PropertyValueType.UInt16:
                    SetEnumValueIndex<ushort>(index);
                    break;
                case PropertyValueType.Int32:
                    SetEnumValueIndex<int>(index);
                    break;
                case PropertyValueType.UInt32:
                    SetEnumValueIndex<uint>(index);
                    break;
                case PropertyValueType.Int64:
                    SetEnumValueIndex<long>(index);
                    break;
                case PropertyValueType.UInt64:
                    SetEnumValueIndex<ulong>(index);
                    break;
            }
        }
        public bool TryGetRange<T>(out Range<T> range) where T : struct, IEquatable<T>
        {
            range = default;
            if (!ValidatePropertyRangeInfo())
            {
                return false;
            }
            if (_propertyRangeGetterDelegate is Func<Range<T>> getter)
            {
                range = getter();
                return true;
            }
            var val = _propertyRangePropertyInfo.GetValue(component);
            if (val == null)
            {
                return false;
            }
            if (_elementFieldInfo != null)
            {
                var minVal = _propertyRangeMinFieldInfo.GetValue(val);
                var maxVal = _propertyRangeMaxFieldInfo.GetValue(val);
                var vmin = _elementFieldInfo.GetValue(minVal);
                var vmax = _elementFieldInfo.GetValue(maxVal);
                if ((vmin is T v0) && (vmax is T v1))
                {
                    range = new Range<T>(v0, v1);
                    return true;
                }
            }
            else if (_elementPropertyInfo != null)
            {
                var minVal = _propertyRangeMinFieldInfo.GetValue(val);
                var maxVal = _propertyRangeMaxFieldInfo.GetValue(val);
                var vmin = _elementPropertyInfo.GetValue(minVal);
                var vmax = _elementPropertyInfo.GetValue(maxVal);
                if ((vmin is T v0) && (vmax is T v1))
                {
                    range = new Range<T>(v0, v1);
                    return true;
                }
            }
            else
            {
                if (val is Range<T> r)
                {
                    range = r;
                    return true;
                }
            }
            return false;
        }
        public object GetSpecificValue()
        {
            if (!Validate())
            {
                return null;
            }
            return _propertyValue;
        }
        public bool TryGetSpecificValue<T>(out T value) where T : struct
        {
            if (!Validate())
            {
                value = default;
                return false;
            }
            if (_propertyValue is not T val)
            {
                if (_propertyType.IsEnum && (_propertyType.GetEnumUnderlyingType() == typeof(T)))
                {
                    value = (T)_propertyValue;
                    return true;
                }
                value = default;
                return true;
            }
            value = val;
            return true;
        }
        public bool IsEmpty()
        {
#if true
            if (string.IsNullOrEmpty(propertyName))
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
#else
            if (ReferenceEquals(gameObject, null))
            {
                return true;
            }
#endif
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

        public bool TryTestValue(PropertyTestFunction function, out bool result)
        {
            result = false;
            if (!Validate())
            {
                return false;
            }
            var vt = GetValueType();
#if true
            if (vt == PropertyValueType.Enum)
            {
                vt = GetEnumValueType();
                if (vt == PropertyValueType.Invalid)
                {
                    vt = GetValueType();
                }
            }
#endif
            switch (vt)
            {
                default:
                case PropertyValueType.Invalid:
                    {
                        result = false;
                        return false;
                    }
                case PropertyValueType.Boolean:
                    {
                        if (!TryGetValue(out bool v0) || !TryGetSpecificValue(out bool v1))
                        {
                            return false;
                        }
                        result = Compare(function, v0, v1);
                    }
                    break;
                case PropertyValueType.Byte:
                    {
                        if (!TryGetValue(out byte v0) || !TryGetSpecificValue(out byte v1))
                        {
                            return false;
                        }
                        result = Compare(function, v0, v1);
                    }
                    break;
                case PropertyValueType.SByte:
                    {
                        if (!TryGetValue(out sbyte v0) || !TryGetSpecificValue(out sbyte v1))
                        {
                            return false;
                        }
                        result = Compare(function, v0, v1);
                    }
                    break;
                case PropertyValueType.Int16:
                    {
                        if (!TryGetValue(out short v0) || !TryGetSpecificValue(out short v1))
                        {
                            return false;
                        }
                        result = Compare(function, v0, v1);
                    }
                    break;
                case PropertyValueType.UInt16:
                    {
                        if (!TryGetValue(out ushort v0) || !TryGetSpecificValue(out ushort v1))
                        {
                            return false;
                        }
                        result = Compare(function, v0, v1);
                    }
                    break;
                case PropertyValueType.Int32:
                    {
                        if (!TryGetValue(out int v0) || !TryGetSpecificValue(out int v1))
                        {
                            return false;
                        }
                        result = Compare(function, v0, v1);
                    }
                    break;
                case PropertyValueType.UInt32:
                    {
                        if (!TryGetValue(out uint v0) || !TryGetSpecificValue(out uint v1))
                        {
                            return false;
                        }
                        result = Compare(function, v0, v1);
                    }
                    break;
                case PropertyValueType.Int64:
                    {
                        if (!TryGetValue(out long v0) || !TryGetSpecificValue(out long v1))
                        {
                            return false;
                        }
                        result = Compare(function, v0, v1);
                    }
                    break;
                case PropertyValueType.UInt64:
                    {
                        if (!TryGetValue(out ulong v0) || !TryGetSpecificValue(out ulong v1))
                        {
                            return false;
                        }
                        result = Compare(function, v0, v1);
                    }
                    break;
                case PropertyValueType.Single:
                    {
                        if (!TryGetValue(out float v0) || !TryGetSpecificValue(out float v1))
                        {
                            return false;
                        }
                        result = Compare(function, v0, v1);
                    }
                    break;
                case PropertyValueType.Double:
                    {
                        if (!TryGetValue(out double v0) || !TryGetSpecificValue(out double v1))
                        {
                            return false;
                        }
                        result = Compare(function, v0, v1);
                    }
                    break;
                case PropertyValueType.String:
                    {
                        var v0 = GetValue() as string;
                        var v1 = _propertyValue as string;
                        result = Compare(function, v0, v1);
                    }
                    break;
                case PropertyValueType.Enum:
                    {
                        var value0 = GetValue();
                        var value1 = GetSpecificValue();
                        if ((value0 is not Enum e) || (value1 is not Enum))
                        {
                            return false;
                        }
                        var v0 = (int)value0;
                        var v1 = (int)value1;
                        result = Compare(function, v0, v1);
                    }
                    break;
                case PropertyValueType.Flags:
                    {
                        var value0 = GetValue();
                        var value1 = GetSpecificValue();
                        if ((value0 is not Enum) || (value1 is not Enum))
                        {
                            return false;
                        }
                        if ((value0 is not int v0) || (value1 is not int v1))
                        {
                            return false;
                        }
                        result = TestFlags(function, v0, v1);
                    }
                    break;
                case PropertyValueType.Vector2:
                    {
                        if (!TryGetValue(out Vector2 v0) || !TryGetSpecificValue(out Vector2 v1))
                        {
                            return false;
                        }
                        result = Equals(function, v0, v1);
                    }
                    break;
                case PropertyValueType.Vector3:
                    {
                        if (!TryGetValue(out Vector3 v0) || !TryGetSpecificValue(out Vector3 v1))
                        {
                            return false;
                        }
                        result = Equals(function, v0, v1);
                    }
                    break;
                case PropertyValueType.Vector4:
                    {
                        if (!TryGetValue(out Vector4 v0) || !TryGetSpecificValue(out Vector4 v1))
                        {
                            return false;
                        }
                        result = Equals(function, v0, v1);
                    }
                    break;
                case PropertyValueType.Quaternion:
                    {
                        if (!TryGetValue(out Quaternion v0) || !TryGetSpecificValue(out Quaternion v1))
                        {
                            return false;
                        }
                        result = Equals(function, v0, v1);
                    }
                    break;
                case PropertyValueType.Color:
                    {
                        if (!TryGetValue(out Color v0) || !TryGetSpecificValue(out Color v1))
                        {
                            return false;
                        }
                        result = Equals(function, v0, v1);
                    }
                    break;
                case PropertyValueType.IntRange:
                    {
                        if (!TryGetValue(out Range<int> v0) || !TryGetSpecificValue(out Range<int> v1))
                        {
                            return false;
                        }
                        result = Equals(function, v0, v1);
                    }
                    break;
                case PropertyValueType.FloatRange:
                    {
                        if (!TryGetValue(out Range<float> v0) || !TryGetSpecificValue(out Range<float> v1))
                        {
                            return false;
                        }
                        result = Equals(function, v0, v1);
                    }
                    break;
            }
            return true;
        }

        private void ClearPropertyInfos()
        {
            _cantWrite = false;
            _propertyInfo = null;
            _elementFieldInfo = null;
            _elementPropertyInfo = null;
            _propertyValue = null;
            _propertyType = null;
            _propertyValueType = PropertyValueType.Invalid;

            _structRange = false;
            _propertyStructRangeMinFieldInfo = null;
            _propertyStructRangeMaxFieldInfo = null;

            _propertyRangePropertyInfo = null;
            _propertyRangeMinFieldInfo = null;
            _propertyRangeMaxFieldInfo = null;

            _enumType = null;
            _enumValueType = PropertyValueType.Invalid;
            _enumValues = null;

            _useDelegate = false;
            _propertySetterDelegate = null;
            _propertyGetterDelegate = null;
            _propertyRangeGetterDelegate = null;
        }
        public void Invalidate()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _lastErrorMessage = null;
#endif
            invalid = false;
            invalidRange = false;
            ClearPropertyInfos();
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
            if (!SetupPropertyInfos())
            {
                ClearPropertyInfos();
                invalid = true;
                return false;
            }
            return true;
        }
        private bool SetupPropertyInfos()
        {
            if (_propertyType != null)
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
            if (component is Transform)
            {
                switch (propertyName)
                {
                    case "active":
                        _propertyType = typeof(bool);
                        _propertyValueType = PropertyValueType.Boolean;
                        _propertyGetterDelegate = new Func<bool>(() => gameObject.activeInHierarchy);
                        _propertySetterDelegate = new Action<bool>((v) => gameObject.SetActive(v));
                        break;
                    case "layer":
                        _propertyType = typeof(int);
                        _propertyValueType = PropertyValueType.Int32;
                        _propertyGetterDelegate = new Func<int>(() => gameObject.layer);
                        _propertySetterDelegate = new Action<int>((v) => gameObject.layer = v);
                        break;
                    case "name":
                        _propertyType = typeof(string);
                        _propertyValueType = PropertyValueType.String;
                        _propertyGetterDelegate = new Func<string>(() => gameObject.name);
                        _propertySetterDelegate = new Action<string>((v) => gameObject.name = v);
                        break;
                    case "tag":
                        _propertyType = typeof(string);
                        _propertyValueType = PropertyValueType.String;
                        _propertyGetterDelegate = new Func<string>(() => gameObject.tag);
                        _propertySetterDelegate = new Action<string>((v) => gameObject.tag = v);
                        break;
                    default:
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        _lastErrorMessage = $"Missing Property";
#endif
                        return false;
                }
                _useDelegate = true;
                if (!string.IsNullOrEmpty(propertyValue))
                {
                    if (!TryParse(propertyValue, _propertyValueType, out object result, _propertyType))
                    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        _lastErrorMessage = $"Illegal PropertyValue<{_propertyType}:{_propertyValueType}> {propertyValue}";
#endif
                        return false;
                    }
#if true
                    if (_propertyType == typeof(Quaternion))
                    {
                        if (result is Vector3 v)
                        {
                            result = Quaternion.Euler(v);
                        }
                    }
#endif
                    _propertyValue = result;
                }
                return true;
            }
            if (string.IsNullOrEmpty(propertyName))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                _lastErrorMessage = $"Empty name";
#endif
                return false;
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ReferenceEquals(component.gameObject, gameObject))
            {
                _lastErrorMessage = $"Illegal [{component.gameObject.name} // {gameObject.name}]";
                return false;
            }
#endif
            var componentType = component.GetType();
            _propertyInfo = componentType.GetProperty(propertyName);
            if (_propertyInfo == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                _lastErrorMessage = $"Missing Property";
#endif
                return false;
            }
            _cantWrite = (_propertyInfo.SetMethod == null) || !_propertyInfo.SetMethod.IsPublic;

            Type rt = null;
            if (!string.IsNullOrEmpty(propertyElement))
            {
                var pt = _propertyInfo.PropertyType;
                if (pt.IsGenericType && pt.GetGenericTypeDefinition() == typeof(Range<>))
                {
                    _structRange = true;
                    _propertyStructRangeMinFieldInfo = pt.GetField("min", BindingFlags.Instance | BindingFlags.NonPublic);
                    _propertyStructRangeMaxFieldInfo = pt.GetField("max", BindingFlags.Instance | BindingFlags.NonPublic);
                    pt = pt.GetGenericArguments()[0];
                }
                {
                    _elementFieldInfo = pt.GetField(propertyElement, BindingFlags.Instance | BindingFlags.Public);
                }
                if (_elementFieldInfo == null)
                {
                    _elementPropertyInfo = pt.GetProperty(propertyElement, BindingFlags.Instance | BindingFlags.Public);
                }
                if ((_elementFieldInfo == null) && (_elementPropertyInfo == null))
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    _lastErrorMessage = $"Missing Element";
#endif
                    return false;
                }
                if (_structRange)
                {
                    var et = (_elementFieldInfo != null) ? _elementFieldInfo.FieldType : _elementPropertyInfo.PropertyType;
                    if (et == typeof(int))
                    {
                        rt = typeof(Range<int>);
                    }
                    if (et == typeof(float))
                    {
                        rt = typeof(Range<float>);
                    }
                    if (rt == null)
                    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        _lastErrorMessage = $"Illegal Element type <{et}>";
#endif
                        return false;
                    }
                }
            }
            if (rt != null)
            {
                _propertyType = rt;
            }
            else if (_elementFieldInfo != null)
            {
                _propertyType = _elementFieldInfo.FieldType;
            }
            else if (_elementPropertyInfo != null)
            {
                _propertyType = _elementPropertyInfo.PropertyType;
            }
            else
            {
                _propertyType = _propertyInfo.PropertyType;
            }
            _propertyValueType = GetPropertyValueType(_propertyType);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_propertyValueType == PropertyValueType.Invalid)
            {
                _lastErrorMessage = $"Illegal PropertyType<{_propertyType}>";
                return false;
            }
#endif
            if (!string.IsNullOrEmpty(propertyValue))
            {
                if (!TryParse(propertyValue, _propertyValueType, out object result, _propertyType))
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    _lastErrorMessage = $"Illegal PropertyValue<{_propertyType}:{_propertyValueType}> {propertyValue}";
#endif
                    return false;
                }
                _propertyValue = result;
            }
            if (_propertyType.IsEnum)
            {
                _enumType = _propertyType.GetEnumUnderlyingType();
                _enumValueType = GetPropertyValueType(_enumType);
                _enumValues = _propertyType.GetEnumValues();
            }
#if !DONTUSE_DELEGATE
            if ((_elementFieldInfo == null) && (_elementPropertyInfo == null) && !_structRange)
            {
                var valueType = (_enumValueType != PropertyValueType.Invalid) ? _enumValueType : _propertyValueType;
                _propertyGetterDelegate = GetGetterDelegate(component, _propertyInfo, valueType);
                if (!_cantWrite)
                {
                    _propertySetterDelegate = GetSetterDelegate(component, _propertyInfo, valueType);
                }
            }
#endif
            return true;
        }
#if !DONTUSE_DELEGATE
        private Delegate GetSetterDelegate(object instance, PropertyInfo propertyInfo, PropertyValueType valueType)
        {
            if (!propertyValueTypeSetterTypeMap.TryGetValue(valueType, out Type type))
            {
                return null;
            }
            return GetSetterDelegate(instance, propertyInfo, type);
        }
        private Delegate GetSetterDelegate(object instance, PropertyInfo propertyInfo, Type actionType)
        {
            if (actionType == null)
            {
                return null;
            }
            var methodInfo = propertyInfo.GetSetMethod();
            if (methodInfo == null)
            {
                return null;
            }
            try
            {
                return Delegate.CreateDelegate(actionType, instance, methodInfo);
            }
            catch (Exception ex)
            {
                LazyDebug.LogWarning($"CreateDelegate: failed: {instance} {propertyInfo} {actionType}\n{ex}");
                return null;
            }
        }
        private Delegate GetGetterDelegate(object instance, PropertyInfo propertyInfo, PropertyValueType valueType)
        {
            if (!propertyValueTypeGetterTypeMap.TryGetValue(valueType, out Type type))
            {
                return null;
            }
            return GetGetterDelegate(instance, propertyInfo, type);
        }
        private Delegate GetGetterDelegate(object instance, PropertyInfo propertyInfo, Type funcType)
        {
            if (funcType == null)
            {
                return null;
            }
            var methodInfo = propertyInfo.GetGetMethod();
            if (methodInfo == null)
            {
                return null;
            }
            try
            {
                return Delegate.CreateDelegate(funcType, instance, methodInfo);
            }
            catch (Exception ex)
            {
                LazyDebug.LogWarning($"CreateDelegate: failed: {instance} {propertyInfo} {funcType}\n{ex}");
                return null;
            }
        }
#endif
        private bool ValidatePropertyRangeInfo()
        {
            if (!Validate())
            {
                return false;
            }
            if (invalidRange)
            {
                return false;
            }
            if (_propertyRangePropertyInfo != null)
            {
                return true;
            }
            invalidRange = true;
            var propertyRangePropertyInfo = component.GetType().GetProperty(propertyName + "Range");
            if (propertyRangePropertyInfo == null)
            {
                return false;
            }
            var pt = propertyRangePropertyInfo.PropertyType;
            if ((pt == null) || !pt.IsGenericType)
            {
                return false;
            }
            var gt = pt.GetGenericTypeDefinition();
            var rt = typeof(Range<>);
            if (gt != rt)
            {
                return false;
            }
            var ag = pt.GetGenericArguments();
            if ((ag == null) || (ag.Length != 1))
            {
                return false;
            }
            if (_elementFieldInfo != null)
            {
                if ((_structRange ? pt : ag[0]) != _propertyInfo.PropertyType)
                {
                    return false;
                }
                ag[0] = _elementFieldInfo.FieldType;
            }
            else if (_elementPropertyInfo != null)
            {
                if ((_structRange ? pt : ag[0]) != _propertyInfo.PropertyType)
                {
                    return false;
                }
                ag[0] = _elementPropertyInfo.PropertyType;
            }
#if true
            else
            {
                switch (_propertyValueType)
                {
                    case PropertyValueType.IntRange:
                        if (ag[0] == typeof(int))
                        {
                            _propertyRangeGetterDelegate = Delegate.CreateDelegate(typeof(Func<Range<int>>), component, propertyRangePropertyInfo.GetGetMethod());
                            _propertyRangePropertyInfo = propertyRangePropertyInfo;
                            invalidRange = false;
                            return true;
                        }
                        break;
                    case PropertyValueType.FloatRange:
                        if (ag[0] == typeof(float))
                        {
                            _propertyRangeGetterDelegate = Delegate.CreateDelegate(typeof(Func<Range<float>>), component, propertyRangePropertyInfo.GetGetMethod());
                            _propertyRangePropertyInfo = propertyRangePropertyInfo;
                            invalidRange = false;
                            return true;
                        }
                        break;
                    default:
                        break;
                }
            }
#endif
            switch (_propertyValueType)
            {
                case PropertyValueType.IntRange:
                    if (ag[0] != typeof(int))
                    {
                        return false;
                    }
                    break;
                case PropertyValueType.FloatRange:
                    if (ag[0] != typeof(float))
                    {
                        return false;
                    }
                    break;
                default:
                    if (ag[0] != _propertyType)
                    {
                        return false;
                    }
                    break;
            }
            _propertyRangePropertyInfo = propertyRangePropertyInfo;
            _propertyRangeMinFieldInfo = pt.GetField("min", BindingFlags.Instance | BindingFlags.NonPublic);
            _propertyRangeMaxFieldInfo = pt.GetField("max", BindingFlags.Instance | BindingFlags.NonPublic);
            invalidRange = false;
            return true;
        }

        public bool Equals(LazyProperty other)
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
            if (propertyName != other.propertyName)
            {
                return false;
            }
            if (propertyElement != other.propertyElement)
            {
                return false;
            }
            if (propertyValue != other.propertyValue)
            {
                return false;
            }
            return true;
        }
        public override bool Equals(object obj)
        {
            if (obj is not LazyProperty ps)
            {
                return false;
            }
            return Equals(ps);
        }
        public static bool operator ==(LazyProperty a, LazyProperty b)
        {
            if ((a is null) || (b is null))
            {
                return (a is null) && (b is null);
            }
            return a.Equals(b);
        }
        public static bool operator !=(LazyProperty a, LazyProperty b)
        {
            if ((a is null) || (b is null))
            {
                return (a is not null) || (b is not null);
            }
            return !a.Equals(b);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(gameObject, component, propertyName, propertyElement, propertyValue);
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
                str = $"{str}:GameObject.{(string.IsNullOrEmpty(propertyName) ? "<Empty>" : propertyName)}";
            }
            else
            {
                str = $"{str}:{component.GetType().Name}.{(string.IsNullOrEmpty(propertyName) ? "<Empty>" : propertyName)}";
                if (!string.IsNullOrEmpty(propertyElement))
                {
                    str = $"{str}.{propertyElement}";
                }
            }
            if (!string.IsNullOrEmpty(propertyValue))
            {
                str = $"{str}={propertyValue}";
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!string.IsNullOrEmpty(_lastErrorMessage))
            {
                str = $"{str}\n{_lastErrorMessage}";
            }
#endif
            return str;
        }



        public static Type GetPropertyValueType(PropertyValueType valueType)
        {
            if (propertyValueTypeTypeMap.TryGetValue(valueType, out Type type))
            {
                return type;
            }
            return null;
        }
        public static PropertyValueType GetPropertyValueType(Type type)
        {
            if (type == null)
            {
                return PropertyValueType.Invalid;
            }
            if (type.IsEnum)
            {
                foreach (var attr in type.CustomAttributes)
                {
                    if (attr.AttributeType == typeof(FlagsAttribute))
                    {
                        return PropertyValueType.Flags;
                    }
                }
                return PropertyValueType.Enum;
            }
            if (typePropertyValueTypeMap.TryGetValue(type, out PropertyValueType valueType))
            {
                return valueType;
            }
            return PropertyValueType.Invalid;
        }
        public static object Convert<T>(T value, PropertyValueType conversionType) where T : struct, IConvertible
        {
            var ct = GetPropertyValueType(conversionType);
            if (ct == null)
            {
                return null;
            }
            try
            {
                return value.ToType(ct, null);
            }
            catch
            {
                return null;
            }
        }
        private static bool Compare<T>(PropertyTestFunction func, T v0, T v1) where T : IComparable<T>
        {
            switch (func)
            {
                case PropertyTestFunction.Always:
                    return true;
                case PropertyTestFunction.Never:
                    return false;
                case PropertyTestFunction.Less:
                    return v0.CompareTo(v1) < 0;
                case PropertyTestFunction.Equal:
                    return v0.CompareTo(v1) == 0;
                case PropertyTestFunction.LEqual:
                    return v0.CompareTo(v1) <= 0;
                case PropertyTestFunction.Greater:
                    return v0.CompareTo(v1) > 0;
                case PropertyTestFunction.NotEqual:
                    return v0.CompareTo(v1) != 0;
                case PropertyTestFunction.GEqual:
                    return v0.CompareTo(v1) >= 0;
                default:
                    return false;
            }
        }
        private static bool Equals<T>(PropertyTestFunction func, T v0, T v1) where T : IEquatable<T>
        {
            switch (func)
            {
                case PropertyTestFunction.Always:
                    return true;
                case PropertyTestFunction.Never:
                    return false;
                case PropertyTestFunction.Less:
                    return false;
                case PropertyTestFunction.Equal:
                    return v0.Equals(v1);
                case PropertyTestFunction.LEqual:
                    return false;
                case PropertyTestFunction.Greater:
                    return false;
                case PropertyTestFunction.NotEqual:
                    return !v0.Equals(v1);
                case PropertyTestFunction.GEqual:
                    return false;
                default:
                    return false;
            }
        }
        private static bool TestFlags(PropertyTestFunction func, int v0, int v1)
        {
            switch (func)
            {
                case PropertyTestFunction.Always:
                    return true;
                case PropertyTestFunction.Never:
                    return false;
                case PropertyTestFunction.Less:
                    return false;
                case PropertyTestFunction.Equal:
                    return (v0 & v1) == v1;
                case PropertyTestFunction.LEqual:
                    return false;
                case PropertyTestFunction.Greater:
                    return false;
                case PropertyTestFunction.NotEqual:
                    return (v0 & v1) != v1;
                case PropertyTestFunction.GEqual:
                    return false;
                default:
                    return false;
            }
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
        public static bool TryParse(string value, PropertyValueType propertyValueType, out object result, Type enumType = null)
        {
            if ((propertyValueType != PropertyValueType.String) && string.IsNullOrEmpty(value))
            {
                result = null;
                return false;
            }
            switch (propertyValueType)
            {
                default:
                case PropertyValueType.Invalid:
                    break;
                case PropertyValueType.Boolean:
                    {
                        if (bool.TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case PropertyValueType.Byte:
                    {
                        if (byte.TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case PropertyValueType.SByte:
                    {
                        if (sbyte.TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case PropertyValueType.Int16:
                    {
                        if (short.TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case PropertyValueType.UInt16:
                    {
                        if (ushort.TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case PropertyValueType.Int32:
                    {
                        if (int.TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case PropertyValueType.UInt32:
                    {
                        if (uint.TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case PropertyValueType.Int64:
                    {
                        if (long.TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case PropertyValueType.UInt64:
                    {
                        if (ulong.TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case PropertyValueType.Single:
                    {
                        if (float.TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case PropertyValueType.Double:
                    {
                        if (double.TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case PropertyValueType.String:
                    {
                        result = value;
                        return true;
                    }
                //break;
                case PropertyValueType.Enum:
                case PropertyValueType.Flags:
                    {
                        if (Enum.TryParse(enumType, value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case PropertyValueType.Vector2:
                    {
                        if (Vector2TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case PropertyValueType.Vector3:
                    {
                        if (Vector3TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case PropertyValueType.Vector4:
                    {
                        if (Vector4TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case PropertyValueType.Quaternion:
                    {
                        //if (QuaternionTryParse(value, out var v))
                        if (Vector3TryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case PropertyValueType.Color:
                    {
                        if (ColorTryParse(value, out var v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case PropertyValueType.IntRange:
                    {
                        if (Range<int>.TryParse(value, out Range<int> v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
                case PropertyValueType.FloatRange:
                    {
                        if (Range<float>.TryParse(value, out Range<float> v))
                        {
                            result = v;
                            return true;
                        }
                    }
                    break;
            }
            result = null;
            return false;
        }
        public static string FormatString(object value, PropertyValueType propertyValueType, string format = null)
        {
            var text = "";
            if (propertyValueType == PropertyValueType.Invalid)
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
                switch (propertyValueType)
                {
                    default:
                    case PropertyValueType.Invalid:
                        break;
                    case PropertyValueType.Boolean:
                    case PropertyValueType.Byte:
                    case PropertyValueType.SByte:
                    case PropertyValueType.Int16:
                    case PropertyValueType.UInt16:
                    case PropertyValueType.Int32:
                    case PropertyValueType.UInt32:
                    case PropertyValueType.Int64:
                    case PropertyValueType.UInt64:
                    case PropertyValueType.Single:
                    case PropertyValueType.Double:
                    case PropertyValueType.String:
                    case PropertyValueType.Enum:
                    case PropertyValueType.Flags:
                        {
                            text = value.ToString();
                        }
                        break;
                    case PropertyValueType.Vector2:
                        {
                            var v0 = (Vector2)value;
                            text = $"({v0.x},{v0.y})";
                        }
                        break;
                    case PropertyValueType.Vector3:
                        {
                            var v0 = (Vector3)value;
                            text = $"({v0.x},{v0.y},{v0.z})";
                        }
                        break;
                    case PropertyValueType.Vector4:
                        {
                            var v0 = (Vector4)value;
                            text = $"({v0.x},{v0.y},{v0.z},{v0.w})";
                        }
                        break;
                    case PropertyValueType.Quaternion:
                        {
#if true
                            var v0 = (Vector3)value;
                            text = $"({v0.x},{v0.y},{v0.z})";
#else
                            var v0 = (Quaternion)value;
                            text = $"({v0.x},{v0.y},{v0.z},{v0.w})";
#endif
                        }
                        break;
                    case PropertyValueType.Color:
                        {
                            var v0 = (Color)value;
                            text = $"RGBA({v0.r},{v0.g},{v0.b},{v0.a})";
                        }
                        break;
                    case PropertyValueType.IntRange:
                    case PropertyValueType.FloatRange:
                        {
                            text = value.ToString();
                        }
                        break;
                }
#endif
            }
            else
            {
                switch (propertyValueType)
                {
                    default:
                    case PropertyValueType.Invalid:
                        break;
                    case PropertyValueType.Boolean:
                        text = ((bool)value).ToString();
                        break;
                    case PropertyValueType.Byte:
                        text = ((byte)value).ToString(format);
                        break;
                    case PropertyValueType.SByte:
                        text = ((sbyte)value).ToString(format);
                        break;
                    case PropertyValueType.Int16:
                        text = ((short)value).ToString(format);
                        break;
                    case PropertyValueType.UInt16:
                        text = ((ushort)value).ToString(format);
                        break;
                    case PropertyValueType.Int32:
                        text = ((int)value).ToString(format);
                        break;
                    case PropertyValueType.UInt32:
                        text = ((uint)value).ToString(format);
                        break;
                    case PropertyValueType.Int64:
                        text = ((long)value).ToString(format);
                        break;
                    case PropertyValueType.UInt64:
                        text = ((ulong)value).ToString(format);
                        break;
                    case PropertyValueType.Single:
                        text = ((float)value).ToString(format);
                        break;
                    case PropertyValueType.Double:
                        text = ((double)value).ToString(format);
                        break;
                    case PropertyValueType.String:
                    case PropertyValueType.Enum:
                    case PropertyValueType.Flags:
                        text = value.ToString();
                        break;
                    case PropertyValueType.Vector2:
                        text = ((Vector2)value).ToString(format);
                        break;
                    case PropertyValueType.Vector3:
                        text = ((Vector3)value).ToString(format);
                        break;
                    case PropertyValueType.Vector4:
                        text = ((Vector4)value).ToString(format);
                        break;
                    case PropertyValueType.Quaternion:
                        text = ((Vector3)value).ToString(format);
                        //text = ((Quaternion)value).ToString(format);
                        break;
                    case PropertyValueType.Color:
                        text = ((Color)value).ToString(format);
                        break;
                    case PropertyValueType.IntRange:
                        {
                            var v0 = (Range<int>)value;
                            text = $"{{{v0.MinValue.ToString(format)},{v0.MaxValue.ToString(format)}}}";
                        }
                        break;
                    case PropertyValueType.FloatRange:
                        {
                            var v0 = (Range<float>)value;
                            text = $"{{{v0.MinValue.ToString(format)},{v0.MaxValue.ToString(format)}}}";
                        }
                        break;
                }
            }
            return text;
        }
    }
}
