#define SAVE_TO_INI_FILE
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace LazyUI
{
    /// <summary>
    /// プレイヤー設定をiniファイルへ書き込む。
    /// ※windows専用、ほかのプラットフォームではUnityのPlayerPrefsを参照。
    /// </summary>
    public static class LazyPlayerPrefs
    {
        private enum ValueType
        {
            Empty,
            String,
            Bool,
            Int,
            Float,
            Vector2,
            Vector3,
            Color,
            Enum,
        }
        private readonly struct Value
        {
            private int GetEnumIndex()
            {
                var values = Enum.GetValues(enumType);
                for (int i = 0; i < values.Length; i++)
                {
                    if ((int)values.GetValue(i) == intValue)
                    {
                        return i;
                    }
                }
                return -1;
            }
            private string GetEnumName()
            {
                var index = GetEnumIndex();
                if (index < 0)
                {
                    return index.ToString();
                }
                var names = Enum.GetNames(enumType);
                return names[index];
            }
            private static bool StringTryParse(string value, out string result)
            {
                result = null;
                var vt = value.Trim();
                if ((vt[0] != '"') || (vt[^1] != '"'))
                {
                    return false;
                }
                result = vt[1..^1];
                return true;
            }
            private static bool FloatsTryParse(string value, out float[] result)
            {
                result = null;
                var vt = value.Trim();
                if ((vt[0] != '(') || (vt[^1] != ')'))
                {
                    return false;
                }
                vt = vt[1..^1];
                var sp = vt.Split(",");
                var tmp = new float[sp.Length];
                for (int i = 0; i < tmp.Length; i++)
                {
                    if (!float.TryParse(sp[i], out tmp[i]))
                    {
                        return false;
                    }
                }
                result = tmp;
                return true;
            }
            public static bool TryParse(string value, Value templateValue, out Value result)
            {
                if (templateValue.valueType == ValueType.Enum)
                {
                    return TryParseEnum(value, templateValue.enumType, out result);
                }
                else
                {
                    return TryParse(value, templateValue.valueType, out result);
                }
            }

            private static bool TryParseEnum(string value, Type enumType, out Value result)
            {
                if ((enumType == null) || !enumType.IsEnum)
                {
                    result = default;
                    return false;
                }
                var underlyingType = Enum.GetUnderlyingType(enumType);
                if (underlyingType != typeof(int))
                {
                    throw new InvalidCastException();
                }
                result = default;
                var vt = value.Trim();
                if (Enum.TryParse(enumType, vt, out object enumValue))
                {
                    result = new Value(enumType, (int)enumValue);
                    return true;
                }
                if (int.TryParse(vt, out int intValue))
                {
                    result = new Value(enumType, intValue);
                    return true;
                }
                return false;
            }
            public static bool TryParse<TEnum>(string value, out Value result) where TEnum : Enum
            {
                return TryParseEnum(value, typeof(TEnum), out result);
            }
            public static bool TryParse(string value, ValueType valueType, out Value result)
            {
                result = default;
                if (string.IsNullOrEmpty(value))
                {
                    return false;
                }
                switch (valueType)
                {
                    case ValueType.Empty:
                        return true;
                    case ValueType.String:
                        if (StringTryParse(value, out string stringValue))
                        {
                            result = new Value(stringValue);
                            return true;
                        }
                        break;
                    case ValueType.Bool:
                        if (bool.TryParse(value, out bool boolValue))
                        {
                            result = new Value(boolValue);
                            return true;
                        }
                        break;
                    case ValueType.Int:
                        if (int.TryParse(value, out int intValue))
                        {
                            result = new Value(intValue);
                            return true;
                        }
                        break;
                    case ValueType.Float:
                        if (float.TryParse(value, out float floatValue))
                        {
                            result = new Value(floatValue);
                            return true;
                        }
                        break;
                    case ValueType.Vector2:
                        if (FloatsTryParse(value, out float[] vector2Value))
                        {
                            if (vector2Value.Length != 2)
                            {
                                break;
                            }
                            result = new Value(new Vector2(vector2Value[0], vector2Value[1]));
                            return true;
                        }
                        break;
                    case ValueType.Vector3:
                        if (FloatsTryParse(value, out float[] vector3Value))
                        {
                            if (vector3Value.Length != 3)
                            {
                                break;
                            }
                            result = new Value(new Vector3(vector3Value[0], vector3Value[1], vector3Value[2]));
                            return true;
                        }
                        break;
                    case ValueType.Color:
                        if (FloatsTryParse(value, out float[] colorValue))
                        {
                            if (colorValue.Length != 4)
                            {
                                break;
                            }
                            result = new Value(new Color(colorValue[0], colorValue[1], colorValue[2], colorValue[3]));
                            return true;
                        }
                        break;
                }
                return false;
            }
            public static bool operator ==(Value left, Value right)
            {
                return left.Equals(right);
            }
            public static bool operator !=(Value left, Value right)
            {
                return !left.Equals(right);
            }
            public readonly ValueType valueType;
            private readonly string stringValue;
            private readonly int intValue;
            private readonly Vector4 floatValue;
            private readonly Type enumType;
            public bool Equals(Value other)
            {
                if (valueType != other.valueType)
                {
                    return false;
                }
                switch (valueType)
                {
                    case ValueType.Empty:
                        return true;
                    case ValueType.String:
                        return stringValue == other.stringValue;
                    case ValueType.Bool:
                    case ValueType.Int:
                        return intValue == other.intValue;
                    case ValueType.Float:
                    case ValueType.Vector2:
                    case ValueType.Vector3:
                    case ValueType.Color:
                        return floatValue == other.floatValue;
                    case ValueType.Enum:
                        if (enumType != other.enumType)
                        {
                            return false;
                        }
                        return intValue == other.intValue;
                    default:
                        return false;
                }
            }
            public override bool Equals(object obj)
            {
                if (obj is Value v0)
                {
                    return Equals(v0);
                }
                return false;
            }
            public override int GetHashCode()
            {
                return HashCode.Combine(valueType, stringValue, intValue, floatValue, enumType);
            }
            public override string ToString()
            {
                switch (valueType)
                {
                    case ValueType.String:
                        return $"\"{stringValue}\"";
                    case ValueType.Bool:
                        return (intValue != 0).ToString();
                    case ValueType.Int:
                        return intValue.ToString();
                    case ValueType.Float:
                        return floatValue.x.ToString();
                    case ValueType.Vector2:
                        return $"({floatValue.x},{floatValue.y})";
                    case ValueType.Vector3:
                        return $"({floatValue.x},{floatValue.y},{floatValue.z})";
                    case ValueType.Color:
                        return $"({floatValue.x},{floatValue.y},{floatValue.z},{floatValue.w})";
                    case ValueType.Enum:
                        return $"{GetEnumName()}";
                    case ValueType.Empty:
                    default:
                        return "";
                }
            }
            public Value(string value)
            {
                valueType = ValueType.String;
                stringValue = value;
                intValue = default;
                floatValue = default;
                enumType = default;
            }
            public Value(bool value)
            {
                valueType = ValueType.Bool;
                stringValue = default;
                intValue = value ? 1 : 0;
                floatValue = default;
                enumType = default;
            }
            public Value(int value)
            {
                valueType = ValueType.Int;
                stringValue = default;
                intValue = value;
                floatValue = default;
                enumType = default;
            }
            public Value(float value)
            {
                valueType = ValueType.Float;
                stringValue = default;
                intValue = default;
                floatValue = new Vector4(value, 0, 0, 0);
                enumType = default;
            }
            public Value(Vector2 value)
            {
                valueType = ValueType.Vector2;
                stringValue = default;
                intValue = default;
                floatValue = value;
                enumType = default;
            }
            public Value(Vector3 value)
            {
                valueType = ValueType.Vector3;
                stringValue = default;
                intValue = default;
                floatValue = value;
                enumType = default;
            }
            public Value(Color color)
            {
                valueType = ValueType.Color;
                stringValue = default;
                intValue = default;
                floatValue = color;
                enumType = default;
            }
            public Value(Type enumType, int enumValue)
            {
                valueType = ValueType.Enum;
                stringValue = default;
                intValue = enumValue;
                floatValue = default;
                this.enumType = enumType;
            }
            public static Value ConstructValue<TEnum>(TEnum enumValue) where TEnum : Enum
            {
                var enumType = typeof(TEnum);
                var underlyingType = Enum.GetUnderlyingType(enumType);
                if (underlyingType == typeof(int))
                {
                    return new Value(enumType, (int)(object)enumValue);
                }
                throw new InvalidCastException();
            }
            public static explicit operator string(Value value)
            {
                if (value.valueType != ValueType.String)
                {
                    throw new InvalidCastException();
                }
                return value.stringValue;
            }
            public static explicit operator bool(Value value)
            {
                if (value.valueType != ValueType.Bool)
                {
                    throw new InvalidCastException();
                }
                return value.intValue != 0;
            }
            public static explicit operator int(Value value)
            {
                if (value.valueType != ValueType.Int)
                {
                    throw new InvalidCastException();
                }
                return value.intValue;
            }
            public static explicit operator float(Value value)
            {
                if (value.valueType != ValueType.Float)
                {
                    throw new InvalidCastException();
                }
                return value.floatValue.x;
            }
            public static explicit operator Vector2(Value value)
            {
                if (value.valueType != ValueType.Vector2)
                {
                    throw new InvalidCastException();
                }
                return value.floatValue;
            }
            public static explicit operator Vector3(Value value)
            {
                if (value.valueType != ValueType.Vector3)
                {
                    throw new InvalidCastException();
                }
                return value.floatValue;
            }
            public static explicit operator Color(Value value)
            {
                if (value.valueType != ValueType.Color)
                {
                    throw new InvalidCastException();
                }
                return value.floatValue;
            }
            public TEnum GetEnumValue<TEnum>() where TEnum : Enum
            {
                if (valueType != ValueType.Enum)
                {
                    throw new InvalidCastException();
                }
                if (enumType != typeof(TEnum))
                {
                    throw new InvalidCastException();
                }
                return (TEnum)(object)intValue;
            }
        }
        private static Dictionary<string, Value> valueMap = null;
        private static Dictionary<string, Value> ValueMap
        {
            get
            {
                if (valueMap == null)
                {
                    valueMap = new Dictionary<string, Value>(StringComparer.OrdinalIgnoreCase);
                    LazyCallbacker.RegisterCallback(LazyCallbacker.CallbackType.Quitting, 100, Quitting);
                }
                return valueMap;
            }
        }

        private static bool loaded = false;
        private static bool dirty = false;

#if SAVE_TO_INI_FILE && UNITY_STANDALONE_WIN
        private static string PrivateProfileFileName
        {
            get { return Application.dataPath + "/../PlayerPrefs.ini"; }
        }
        private static string SectionName
        {
            get { return Application.productName; }
        }
        private static Dictionary<string, string> stringMap = null;
        private static List<string> stringListBefore = null;
        private static List<string> stringListAfter = null;

        private static void BuildStringMap(System.IO.StreamReader sr, string sectionName)
        {
            var ln = 0;
            for (; ; )
            {
                ln++;
                var l = sr.ReadLine();
                if (l == null)
                {
                    stringListBefore.Add($"[{sectionName}]");
                    return;
                }
                stringListBefore.Add(l);
                var s = l.Trim();
                var ch = s[0];
                if (ch != '[')
                {
                    continue;
                }
                var idx = s.IndexOf(']');
                if (idx < 0)
                {
                    continue;
                }
                var section = s[1..idx];
                if (section.Equals(sectionName, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
            }
            for (; ; )
            {
                ln++;
                var l = sr.ReadLine();
                if (l == null)
                {
                    return;
                }
                var s = l.Trim();
                var ch = s[0];
                if (ch == '[')
                {
                    stringListAfter.Add(l);
                    break;
                }
                if (ch == ';')
                {
                    stringMap.Add($"[{ln}]", l);
                    continue;
                }
                var ss = s.Split('=');
                if (ss.Length != 2)
                {
                    stringMap.Add($"[{ln}]", l);
                    continue;
                }
                var strKey = ss[0].Trim();
                var strVal = ss[1].Trim();
                stringMap[strKey] = strVal;
            }
            for (; ; )
            {
                ln++;
                var l = sr.ReadLine();
                if (l == null)
                {
                    return;
                }
                stringListAfter.Add(l);
            }
        }
        private static void LoadPrivateProfiles(bool force = false)
        {
            if (!force && (stringMap != null))
            {
                return;
            }
            loaded = false;
            stringMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            stringListBefore = new List<string>();
            stringListAfter = new List<string>();
            try
            {
                var sectionName = SectionName;
                if (System.IO.File.Exists(PrivateProfileFileName))
                {
                    using var sr = new System.IO.StreamReader(PrivateProfileFileName);
                    BuildStringMap(sr, sectionName);
                    loaded = true;
                    LazyDebug.Log("LazyPlayerPrefs: Loaded..");
                }
                else
                {
                    stringListBefore.Add($"[{sectionName}]");
                }
            }
            catch (Exception ec)
            {
                Debug.LogWarning(ec.Message);
            }
        }
        private static string ReadPrivateProfileString(string key)
        {
            LoadPrivateProfiles();
            if (true != stringMap?.TryGetValue(key.Trim(), out string value))
            {
                return "";
            }
            return value;
        }
        private static void WritePrivateProfileString(string key, string value)
        {
            LoadPrivateProfiles();
            key = key.Trim();
            value = value.Trim();
            stringMap[key] = value;
        }
        private static void FlushPrivateProfiles()
        {
            if (stringMap == null)
            {
                return;
            }
            try
            {
                using var sr = new System.IO.StreamWriter(PrivateProfileFileName, false, System.Text.Encoding.UTF8);
                foreach (var l in stringListBefore)
                {
                    sr.WriteLine(l);
                }
                var map = loaded ? stringMap.AsEnumerable() : stringMap.OrderBy((kv) => kv.Key);
                foreach (var kv in map)
                {
                    if (kv.Key[0] == '[')
                    {
                        sr.WriteLine(kv.Value);
                    }
                    else
                    {
                        sr.WriteLine($"  {kv.Key} = {kv.Value}");
                    }
                }
                foreach (var l in stringListAfter)
                {
                    sr.WriteLine(l);
                }
            }
            catch (Exception ec)
            {
                LazyDebug.LogWarning(ec.Message);
            }
        }
        private static void DeletePrivateProfiles()
        {
            if (System.IO.File.Exists(PrivateProfileFileName))
            {
                System.IO.File.Delete(PrivateProfileFileName);
            }
        }
#else
        private static string ReadPrivateProfileString(string key)
        {
            return PlayerPrefs.GetString(key, null);
        }
        private static void WritePrivateProfileString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }
        private static void FlushPrivateProfiles()
        {
            PlayerPrefs.Save();
        }
        private static void LoadPrivateProfiles(bool force = false)
        {
        }
        private static void DeletePrivateProfiles()
        {
            PlayerPrefs.DeleteAll();
        }
#endif

        private static Value GetValue(string key, Value defaultValue)
        {
            if (!ValueMap.TryGetValue(key, out Value value))
            {
                var v = ReadPrivateProfileString(key);
                if (!Value.TryParse(v, defaultValue, out value))
                {
                    value = defaultValue;
                }
                ValueMap[key] = value;
            }
#if DEBUG
            if (value.valueType != defaultValue.valueType)
            {
                throw new ArgumentException();
            }
#endif
            return value;
        }
        public static TEnum GetValue<TEnum>(string key, TEnum defaultValue) where TEnum : Enum
        {
#if false
            if (!ValueMap.TryGetValue(key, out Value value))
            {
                var v = ReadPrivateProfileString(key);
                if (!Value.TryParse<TEnum>(v, out value))
                {
                    value = Value.ConstructValue(defaultValue);
                }
                ValueMap[key] = value;
            }
#if DEBUG
            if (value.valueType != ValueType.Enum)
            {
                throw new ArgumentException();
            }
#endif
            return value.GetEnumValue<TEnum>();
#else
            return GetValue(key, Value.ConstructValue(defaultValue)).GetEnumValue<TEnum>();
#endif
        }
        public static string GetValue(string key, string defaultValue)
        {
            return (string)GetValue(key, new Value(defaultValue));
        }
        public static bool GetValue(string key, bool defaultValue)
        {
            return (bool)GetValue(key, new Value(defaultValue));
        }
        public static int GetValue(string key, int defaultValue)
        {
            return (int)GetValue(key, new Value(defaultValue));
        }
        public static float GetValue(string key, float defaultValue)
        {
            return (float)GetValue(key, new Value(defaultValue));
        }
        public static Vector2 GetValue(string key, Vector2 defaultValue)
        {
            return (Vector2)GetValue(key, new Value(defaultValue));
        }
        public static Vector3 GetValue(string key, Vector3 defaultValue)
        {
            return (Vector3)GetValue(key, new Value(defaultValue));
        }
        public static Color GetValue(string key, Color defaultValue)
        {
            return (Color)GetValue(key, new Value(defaultValue));
        }

        private static void Quitting()
        {
#if UNITY_EDITOR
            if (!loaded)
            {
                Dump();
            }
            else
#endif
            {
                Save();
            }
        }
        private static void SetValue(string key, Value v1)
        {
            if (ValueMap.TryGetValue(key, out Value v0))
            {
#if DEBUG
                if (v1.valueType != v0.valueType)
                {
                    throw new ArgumentException();
                }
#endif
                if (v1 == v0)
                {
                    return;
                }
            }
            dirty = true;
            ValueMap[key] = v1;
        }
        public static void SetValue<TEnum>(string key, TEnum value) where TEnum : Enum
        {
            SetValue(key, Value.ConstructValue(value));
        }
        public static void SetValue(string key, string value)
        {
            SetValue(key, new Value(value));
        }
        public static void SetValue(string key, bool value)
        {
            SetValue(key, new Value(value));
        }
        public static void SetValue(string key, int value)
        {
            SetValue(key, new Value(value));
        }
        public static void SetValue(string key, float value)
        {
            SetValue(key, new Value(value));
        }
        public static void SetValue(string key, Vector2 value)
        {
            SetValue(key, new Value(value));
        }
        public static void SetValue(string key, Vector3 value)
        {
            SetValue(key, new Value(value));
        }
        public static void SetValue(string key, Color value)
        {
            SetValue(key, new Value(value));
        }
#if UNITY_EDITOR
        [UnityEditor.MenuItem("LazyUI/LazyPlayerPrefs.Save()")]
#endif
        public static void Save()
        {
            if (!dirty)
            {
                return;
            }
            dirty = false;
            LazyDebug.Log("LazyUIPlayerPrefs.Saved()");
            foreach (var v in ValueMap)
            {
                WritePrivateProfileString(v.Key, v.Value.ToString());
            }
            FlushPrivateProfiles();
        }
#if false
        public static void Save(string key)
        {
            if (!dirty)
            {
                return;
            }
            dirty = false;
            if (ValueMap.TryGetValue(key, out Value value))
            {
                WritePrivateProfileString(key, value.ToString());
                FlushPrivateProfiles();
            }
        }
#endif
#if UNITY_EDITOR
        [UnityEditor.MenuItem("LazyUI/LazyPlayerPrefs.Load()")]
#endif
        public static void Load()
        {
            LoadPrivateProfiles(true);
            //LazyDebug.Log("LazyUIPlayerPrefs.Loaded()");
        }
#if UNITY_EDITOR
        [UnityEditor.MenuItem("LazyUI/LazyPlayerPrefs.Dump()")]
#endif
        public static void Dump()
        {
            var str = "LazyUIPlayerPrefs.Dump()\n";
            foreach (var v in ValueMap)
            {
                str += $"{v.Key}={v.Value}\n";
            }
            LazyDebug.Log(str);
        }
#if false
        public static void Dump(string key)
        {
            if (ValueMap.TryGetValue(key, out Value value))
            {
                LazyDebug.Log($"{key}={value}\n");
            }
        }
#endif
#if UNITY_EDITOR
        [UnityEditor.MenuItem("LazyUI/LazyPlayerPrefs.Delete()")]
#endif
        public static void Delete()
        {
            DeletePrivateProfiles();
        }
    }
}
