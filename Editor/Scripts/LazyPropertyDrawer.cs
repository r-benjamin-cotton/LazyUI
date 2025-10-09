using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace LazyUI
{
    /// <summary>
    /// コンポーネントのプロパティ指定をするカスタムプロパティ描画
    /// </summary>
    [CustomPropertyDrawer(typeof(LazyProperty))]
    public class LazyPropertyDrawer : PropertyDrawer
    {
        [MenuItem("GameObject/Dump Properties")]
        private static void DumpProperties()
        {
            LazyDebug.Log(typeof(LazyProperty).Name);
            foreach (var gameObject in Selection.gameObjects)
            {
                var components = gameObject.GetComponentsInChildren(typeof(Component), true);
                foreach (var component in components)
                {
                    using var so = new SerializedObject(component);
                    var sp = so.GetIterator();
                    while (sp.NextVisible(true))
                    {
                        if (sp.propertyType != SerializedPropertyType.Generic)
                        {
                            continue;
                        }
                        if (sp.contentHash == 0)
                        {
                            continue;
                        }
                        try
                        {
                            var ps = sp.boxedValue as LazyProperty;
                            if (ps != null)
                            {
                                var t = sp.serializedObject.targetObject as Component;
                                if (t != null)
                                {
                                    ps.Invalidate();
                                    LazyDebug.Log($"{t.transform.GetFullPath()} => {ps}");
                                    ps.Validate();
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }

        private enum BooleanEnum
        {
            False,
            True,
        }
        private ulong GetValue(string value, ulong min, ulong max)
        {
            unchecked
            {
                if (value == null)
                {
                    return 0;
                }
                ulong v = 0;
                foreach (var c in value)
                {
                    if ((c < '0') || (c > '9'))
                    {
                        return 0;
                    }
                    var nv = v * 10 + (ulong)(c - '0');
                    if ((nv < v) || (nv >= max))
                    {
                        return max;
                    }
                    v = nv;
                }
                if (v <= min)
                {
                    return min;
                }
                return v;
            }
        }
        private static int IndexOf<T>(IEnumerable<T> e, Predicate<T> predicate)
        {
            var index = 0;
            foreach (var t in e)
            {
                if (predicate(t))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }
        private string ToTitleCase(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            var ca = str.ToArray();
            ca[0] = char.ToUpper(ca[0]);
            return new string(ca);
        }
        private LazyPropertyAttribute GetSpec(SerializedProperty property)
        {
            var condition = PropertyValueType.Everything;
            var withValue = true;
            var writable = true;
            var componentType = typeof(MonoBehaviour);
            foreach (var obj in property.serializedObject.targetObjects)
            {
                var fi = obj.GetType().GetField(fieldInfo.Name);
                var spc = fieldInfo.GetCustomAttribute<LazyPropertyAttribute>();
                if (spc != null)
                {
                    if (spc.componentType != null)
                    {
                        if (ReferenceEquals(componentType, spc.componentType) || spc.componentType.IsSubclassOf(componentType))
                        {
                            componentType = spc.componentType;
                        }
                        else
                        {
                            componentType = typeof(LazyPropertyAttribute); // << Dummy!!!!
                        }
                    }
                    condition &= spc.allow;
                    if (!spc.withValue)
                    {
                        withValue = false;
                    }
                    if (!spc.writable)
                    {
                        writable = false;
                    }
                }
            }
            return new LazyPropertyAttribute(condition, withValue, writable, componentType);
        }
        private static readonly string[] ignoreProperties = new string[]
        {
            "name",
            "hideFlags",
            "enabled",
            "isActiveAndEnabled",
            "useGUILayout",
            "runInEditMode",
        };
        private static bool IsIgnored(PropertyInfo propertyInfo)
        {
            var name = propertyInfo.Name;
            foreach (var pn in ignoreProperties)
            {
                if (name.Equals(pn))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool IsAllowedType(PropertyValueType vt, PropertyValueType allow)
        {
            return (allow & vt) != 0;
        }
        private struct Context
        {
            public LazyPropertyAttribute spec;
            public float height;
            public float width;
            public float spacing;
            public Rect rect;
            public SerializedProperty gameObject;
            public SerializedProperty component;
            public SerializedProperty propertyName;
            public SerializedProperty propertyElement;
            public SerializedProperty propertyValue;
            public bool withStruct;
        }
        private GameObject SelectGameObject(ref Context ctxt)
        {
            var r = EditorGUI.PrefixLabel(ctxt.rect, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("GameObject"));
            ctxt.rect.y += ctxt.height + ctxt.spacing;
            {
                EditorGUI.showMixedValue = ctxt.gameObject.hasMultipleDifferentValues;
                var newObj = EditorGUI.ObjectField(r, ctxt.gameObject.objectReferenceValue, typeof(GameObject), true);
                if (!ReferenceEquals(ctxt.gameObject.objectReferenceValue, newObj))
                {
                    var clear = true;
                    ctxt.gameObject.objectReferenceValue = newObj;
#if true
                    if (ctxt.component.objectReferenceValue != null)
                    {
                        var ct = ctxt.component.objectReferenceValue.GetType();
                        var cp = (newObj as GameObject)?.GetComponent(ct);
                        if (cp != null)
                        {
                            clear = false;
                        }
                    }
#endif
                    if (clear)
                    {
                        ctxt.component.objectReferenceValue = null;
                        ctxt.propertyName.stringValue = "";
                        ctxt.propertyElement.stringValue = "";
                        ctxt.propertyValue.stringValue = "";
                    }
                }
            }
            return ctxt.gameObject.hasMultipleDifferentValues ? null : (ctxt.gameObject.objectReferenceValue as GameObject);
        }
        private Component SelectComponent(ref Context ctxt, GameObject go)
        {
            var r = EditorGUI.PrefixLabel(ctxt.rect, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Component"));
            ctxt.rect.y += ctxt.height + ctxt.spacing;
            var cp = default(Component);
            if (go != null)
            {
                var spec = ctxt.spec;
                var components = go.GetComponents(typeof(MonoBehaviour))
                    .Where((cp) =>
                    {
                        if (cp == null)
                        {
                            return false;
                        }
                        return cp.GetType().IsSubclassOf(spec.componentType);
                    }).Prepend(go.transform).ToArray();
                if (components.Length != 0)
                {
                    var componentNames = components.Select((cp) => cp is Transform ? "GameObject" : cp.GetType().Name).ToArray();
                    var componentValues = new int[componentNames.Length];
                    for (int i = 0; i < componentValues.Length; i++)
                    {
                        componentValues[i] = i;
                    }
                    var name = (ctxt.component.hasMultipleDifferentValues || (ctxt.component.objectReferenceValue == null)) ? null : (ctxt.component.objectReferenceValue is Transform ? "GameObject" : ctxt.component.objectReferenceValue.GetType().Name);
                    var index = IndexOf(componentNames, n => n.Equals(name));
                    EditorGUI.showMixedValue = ctxt.component.hasMultipleDifferentValues;
                    var newIndex = EditorGUI.IntPopup(r, index, componentNames, componentValues);
                    if (newIndex != index)
                    {
                        ctxt.component.objectReferenceValue = (newIndex < 0) ? null : components[newIndex];
                        ctxt.propertyName.stringValue = "";
                        ctxt.propertyElement.stringValue = "";
                        ctxt.propertyValue.stringValue = "";
                    }
                    if (newIndex >= 0)
                    {
                        cp = components[newIndex];
                    }
                }
            }
            return cp;
        }
        private struct GameObjectPropertyInfo
        {
            public string name;
            public bool canWrite;
            public Type propertyType;
            public PropertyValueType vt;
            public GameObjectPropertyInfo(string name, bool canWrite, Type propertyType)
            {
                this.name = name;
                this.canWrite = canWrite;
                this.propertyType = propertyType;
                vt = LazyProperty.GetPropertyValueType(propertyType);
            }
        }
        private readonly GameObjectPropertyInfo[] gameObjectPropertyInfos = new GameObjectPropertyInfo[]
        {
            new ("active", true, typeof(bool)),
            new ("layer", true, typeof(int)),
            new ("name", true, typeof(string)),
            new ("tag", true, typeof(string)),
        };
        private Type SelectGameObjectProperty(ref Context ctxt, GameObject go)
        {
            var r = EditorGUI.PrefixLabel(ctxt.rect, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Property"));
            ctxt.rect.y += ctxt.height + ctxt.spacing;
            var pt = default(Type);
            if (go != null)
            {
                var spec = ctxt.spec;
                var properties = gameObjectPropertyInfos
                    .Where(pi =>
                    {
                        if (spec.writable && !pi.canWrite)
                        {
                            return false;
                        }
                        if (IsAllowedType(pi.vt, spec.allow))
                        {
                            return true;
                        }
                        return false;
                    })
                    .ToArray();
                if (properties.Length != 0)
                {
                    var propertyNames = properties.Select((pi) => pi.name).ToArray();
                    var propertyIndices = new int[propertyNames.Length];
                    for (int i = 0; i < propertyIndices.Length; i++)
                    {
                        propertyIndices[i] = i;
                    }
                    var name = ctxt.propertyName.hasMultipleDifferentValues ? null : ctxt.propertyName.stringValue;
                    var index = IndexOf(propertyNames, n => n.Equals(name));
                    EditorGUI.showMixedValue = ctxt.propertyName.hasMultipleDifferentValues;
                    var newIndex = EditorGUI.IntPopup(r, index, propertyNames, propertyIndices);
                    if (newIndex != index)
                    {
                        ctxt.propertyName.stringValue = (newIndex < 0) ? "" : propertyNames[newIndex];
                        ctxt.propertyElement.stringValue = "";
                        ctxt.propertyValue.stringValue = "";
                    }
                    if (newIndex >= 0)
                    {
                        pt = properties[newIndex].propertyType;
                    }
                }
            }
            return pt;
        }
        private PropertyInfo SelectProperty(ref Context ctxt, Component cp)
        {
            var r = EditorGUI.PrefixLabel(ctxt.rect, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Property"));
            ctxt.rect.y += ctxt.height + ctxt.spacing;
            var pi = default(PropertyInfo);
            if (cp != null)
            {
                var spec = ctxt.spec;
                var type = ctxt.component.objectReferenceValue.GetType();
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(pi =>
                    {
                        if (IsIgnored(pi))
                        {
                            return false;
                        }
                        if (spec.writable && ((pi.SetMethod == null) || !pi.SetMethod.IsPublic))
                        {
                            return false;
                        }
                        var vt = LazyProperty.GetPropertyValueType(pi.PropertyType);
                        if (IsAllowedType(vt, spec.allow))
                        {
                            return true;
                        }
#if false
                        if ((spec.allow & PropertyValueType.Struct) == 0)
                        {
                            return false;
                        }
#endif
                        if (!pi.PropertyType.IsValueType)
                        {
                            return false;
                        }
                        var allow = spec.allow;
                        var px = pi.PropertyType;
                        if (px.IsGenericType && (px.GetGenericTypeDefinition() == typeof(LazyRange<>)))
                        {
                            px = px.GetGenericArguments()[0];
                            if ((spec.allow & PropertyValueType.IntRange) != 0)
                            {
                                allow |= PropertyValueType.Int32;
                            }
                            if ((spec.allow & PropertyValueType.FloatRange) != 0)
                            {
                                allow |= PropertyValueType.Single;
                            }
                        }
                        var fields = px.GetFields(BindingFlags.Public | BindingFlags.Instance);
                        foreach (var fi in fields)
                        {
                            var ft = LazyProperty.GetPropertyValueType(fi.FieldType);
                            if (IsAllowedType(ft, allow))
                            {
                                return true;
                            }
                        }
                        var properties = px.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        foreach (var pp in properties)
                        {
                            if (spec.writable && ((pp.SetMethod == null) || !pp.SetMethod.IsPublic))
                            {
                                return false;
                            }
                            var pt = LazyProperty.GetPropertyValueType(pp.PropertyType);
                            if (IsAllowedType(pt, allow))
                            {
                                return true;
                            }
                        }
                        return false;
                    })
                    .ToArray();
                if (properties.Length != 0)
                {
                    var propertyNames = properties.Select((pi) => pi.Name).ToArray();
                    var propertyIndices = new int[propertyNames.Length];
                    for (int i = 0; i < propertyIndices.Length; i++)
                    {
                        propertyIndices[i] = i;
                    }
                    var name = ctxt.propertyName.hasMultipleDifferentValues ? null : ctxt.propertyName.stringValue;
                    var index = IndexOf(propertyNames, n => n.Equals(name));
                    EditorGUI.showMixedValue = ctxt.propertyName.hasMultipleDifferentValues;
                    var newIndex = EditorGUI.IntPopup(r, index, propertyNames, propertyIndices);
                    if (newIndex != index)
                    {
                        ctxt.propertyName.stringValue = (newIndex < 0) ? "" : propertyNames[newIndex];
                        ctxt.propertyElement.stringValue = "";
                        ctxt.propertyValue.stringValue = "";
                    }
                    if (newIndex >= 0)
                    {
                        pi = properties[newIndex];
                    }
                }
            }
            return pi;
        }
        private (PropertyValueType vt, string name)[] GetElements(ref Context ctxt, PropertyInfo pi)
        {
            var spec = ctxt.spec;
            var allow = spec.allow & ~(PropertyValueType.IntRange | PropertyValueType.FloatRange);
            var px = pi.PropertyType;
            if (px.IsGenericType && (px.GetGenericTypeDefinition() == typeof(LazyRange<>)))
            {
                px = px.GetGenericArguments()[0];
                if ((spec.allow & PropertyValueType.IntRange) != 0)
                {
                    allow |= PropertyValueType.Int32;
                }
                if ((spec.allow & PropertyValueType.FloatRange) != 0)
                {
                    allow |= PropertyValueType.Single;
                }
            }
            var fields = px.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Select(fi => (vt: LazyProperty.GetPropertyValueType(fi.FieldType), name: fi.Name))
                .Where((vv) => IsAllowedType(vv.vt, allow));
            var properties = px.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(pi =>
                {
                    if (pi.GetIndexParameters().Length != 0)
                    {
                        return false;
                    }
                    if (spec.writable && ((pi.SetMethod == null) || !pi.SetMethod.IsPublic))
                    {
                        return false;
                    }
                    var pt = LazyProperty.GetPropertyValueType(pi.PropertyType);
                    if (!IsAllowedType(pt, allow))
                    {
                        return false;
                    }
                    return true;
                })
               .Select(pi => (vt: LazyProperty.GetPropertyValueType(pi.PropertyType), name: pi.Name));
            return fields.Concat(properties).ToArray();
        }
        private void SelectElement(ref Context ctxt, PropertyInfo pi)
        {
            if (pi != null)
            {
                var spec = ctxt.spec;
                var vt = LazyProperty.GetPropertyValueType(pi.PropertyType);
                if (!IsAllowedType(vt, spec.allow))
                {
                    vt = PropertyValueType.Invalid;
                    var r = EditorGUI.PrefixLabel(ctxt.rect, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Element"));
                    ctxt.rect.y += ctxt.height + ctxt.spacing;
                    {
                        var elements = GetElements(ref ctxt, pi);
                        if (elements.Length != 0)
                        {
                            var elementNames = elements.Select(e => e.name).ToArray();
                            var elementIndices = new int[elementNames.Length];
                            for (int i = 0; i < elementIndices.Length; i++)
                            {
                                elementIndices[i] = i;
                            }
                            var name = ctxt.propertyElement.hasMultipleDifferentValues ? null : ctxt.propertyElement.stringValue;
                            var index = IndexOf(elementNames, n => n.Equals(name));
                            EditorGUI.showMixedValue = ctxt.propertyElement.hasMultipleDifferentValues;
                            var newIndex = EditorGUI.IntPopup(r, index, elementNames, elementIndices);
                            if (newIndex != index)
                            {
                                ctxt.propertyElement.stringValue = (newIndex < 0) ? ".invalid" : elementNames[newIndex];
                                ctxt.propertyValue.stringValue = "";
                            }
                            if (newIndex >= 0)
                            {
                                vt = elements[newIndex].vt;
                            }
                        }
                    }
                    ctxt.withStruct = true;
                }
                else
                {
                    ctxt.withStruct = false;
                    ctxt.propertyElement.stringValue = "";
                }
            }
        }
        private void SpecifyValue(ref Context ctxt, Type propertyType)
        {
            var spec = ctxt.spec;
            if (spec.withValue)
            {
                var vt = LazyProperty.GetPropertyValueType(propertyType);
                var r = EditorGUI.PrefixLabel(ctxt.rect, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Value"));
                ctxt.rect.y += ctxt.height + ctxt.spacing;
                if (vt != PropertyValueType.Invalid)
                {
                    if (!LazyProperty.TryParse(ctxt.propertyValue.stringValue, vt, out object val, propertyType))
                    {
                        val = null;
                    }
                    EditorGUI.showMixedValue = ctxt.propertyValue.hasMultipleDifferentValues;
                    switch (vt)
                    {
                        case PropertyValueType.Boolean:
                            {
                                if (val is not bool v)
                                {
                                    v = false;
                                }
                                var b = v ? BooleanEnum.True : BooleanEnum.False;
                                var nv = EditorGUI.EnumPopup(r, b);
                                if ((val == null) || !nv.Equals(b))
                                {
                                    ctxt.propertyValue.stringValue = LazyProperty.FormatString(nv, vt);
                                }
                            }
                            break;
                        case PropertyValueType.Int16:
                            {
                                if (val is not short v)
                                {
                                    v = 0;
                                }
                                var nv = Math.Clamp(EditorGUI.IntField(r, v), short.MinValue, short.MaxValue);
                                if ((val == null) || (nv != v))
                                {
                                    ctxt.propertyValue.stringValue = LazyProperty.FormatString(nv, vt);
                                }
                            }
                            break;
                        case PropertyValueType.UInt16:
                            {
                                var v = ctxt.propertyValue.stringValue;
                                var nv = GetValue(EditorGUI.TextField(r, v), ushort.MinValue, ushort.MaxValue).ToString();
                                if ((val == null) || (nv != v))
                                {
                                    ctxt.propertyValue.stringValue = LazyProperty.FormatString(nv, vt);
                                }
                            }
                            break;
                        case PropertyValueType.Int32:
                            {
                                if (val is not int v)
                                {
                                    v = 0;
                                }
                                var nv = EditorGUI.IntField(r, v);
                                if ((val == null) || (nv != v))
                                {
                                    ctxt.propertyValue.stringValue = LazyProperty.FormatString(nv, vt);
                                }
                            }
                            break;
                        case PropertyValueType.UInt32:
                            {
                                var v = ctxt.propertyValue.stringValue;
                                var nv = GetValue(EditorGUI.TextField(r, v), uint.MinValue, uint.MaxValue).ToString();
                                if ((val == null) || (nv != v))
                                {
                                    ctxt.propertyValue.stringValue = LazyProperty.FormatString(nv, vt);
                                }
                            }
                            break;
                        case PropertyValueType.Int64:
                            {
                                if (val is not long v)
                                {
                                    v = 0;
                                }
                                var nv = EditorGUI.LongField(r, v);
                                if ((val == null) || (nv != v))
                                {
                                    ctxt.propertyValue.stringValue = LazyProperty.FormatString(nv, vt);
                                }
                            }
                            break;
                        case PropertyValueType.UInt64:
                            {
                                var v = ctxt.propertyValue.stringValue;
                                var nv = GetValue(EditorGUI.TextField(r, v), ulong.MinValue, ulong.MaxValue).ToString();
                                if ((val == null) || (nv != v))
                                {
                                    ctxt.propertyValue.stringValue = LazyProperty.FormatString(nv, vt);
                                }
                            }
                            break;
                        case PropertyValueType.Single:
                            {
                                if (val is not float v)
                                {
                                    v = 0;
                                }
                                var nv = EditorGUI.FloatField(r, v);
                                if ((val == null) || (nv != v))
                                {
                                    ctxt.propertyValue.stringValue = LazyProperty.FormatString(nv, vt);
                                }
                            }
                            break;
                        case PropertyValueType.Double:
                            {
                                if (val is not double v)
                                {
                                    v = 0;
                                }
                                var nv = EditorGUI.DoubleField(r, v);
                                if ((val == null) || (nv != v))
                                {
                                    ctxt.propertyValue.stringValue = LazyProperty.FormatString(nv, vt);
                                }
                            }
                            break;
                        case PropertyValueType.String:
                            {
                                var v = ctxt.propertyValue.stringValue;
                                var nv = EditorGUI.TextField(r, v);
                                if ((val == null) || (nv != v))
                                {
                                    ctxt.propertyValue.stringValue = LazyProperty.FormatString(nv, vt);
                                }
                            }
                            break;
                        case PropertyValueType.Enum:
                            {
                                if (val is not Enum v)
                                {
                                    v = (Enum)propertyType.GetEnumValues().GetValue(0);
                                }
                                var nv = EditorGUI.EnumPopup(r, v);
                                if ((val == null) || !nv.Equals(v))
                                {
                                    ctxt.propertyValue.stringValue = LazyProperty.FormatString(nv, vt);
                                }
                            }
                            break;
                        case PropertyValueType.Flags:
                            {
                                if (val is not Enum v)
                                {
                                    v = (Enum)Enum.ToObject(propertyType, 0);
                                }
                                var nv = EditorGUI.EnumFlagsField(r, v);
                                if ((val == null) || !nv.Equals(v))
                                {
                                    ctxt.propertyValue.stringValue = LazyProperty.FormatString(nv, vt);
                                }
                            }
                            break;
                        case PropertyValueType.Vector2:
                            {
                                if (val is not Vector2 v)
                                {
                                    v = Vector2.zero;
                                }
                                var nv = EditorGUI.Vector2Field(r, GUIContent.none, v);
                                if ((val == null) || (nv != v))
                                {
                                    ctxt.propertyValue.stringValue = LazyProperty.FormatString(nv, vt);
                                }
                            }
                            break;
                        case PropertyValueType.Vector3:
                            {
                                if (val is not Vector3 v)
                                {
                                    v = Vector3.zero;
                                }
                                var nv = EditorGUI.Vector3Field(r, GUIContent.none, v);
                                if ((val == null) || (nv != v))
                                {
                                    ctxt.propertyValue.stringValue = LazyProperty.FormatString(nv, vt);
                                }
                            }
                            break;
                        case PropertyValueType.Vector4:
                            {
                                if (val is not Vector4 v)
                                {
                                    v = Vector4.zero;
                                }
                                var nv = EditorGUI.Vector4Field(r, GUIContent.none, v);
                                if ((val == null) || (nv != v))
                                {
                                    ctxt.propertyValue.stringValue = LazyProperty.FormatString(nv, vt);
                                }
                            }
                            break;
                        case PropertyValueType.Quaternion:
                            {
                                if (val is not Vector3 v)
                                {
                                    v = Vector3.zero;
                                }
                                var nv = EditorGUI.Vector3Field(r, GUIContent.none, v);
                                if ((val == null) || (nv != v))
                                {
                                    ctxt.propertyValue.stringValue = LazyProperty.FormatString(nv, vt);
                                }
                            }
                            break;
                        case PropertyValueType.Color:
                            {
                                if (val is not Color v)
                                {
                                    v = Color.clear;
                                }
                                var nv = EditorGUI.ColorField(r, GUIContent.none, v);
                                if ((val == null) || (nv != v))
                                {
                                    ctxt.propertyValue.stringValue = LazyProperty.FormatString(nv, vt);
                                }
                            }
                            break;
                        case PropertyValueType.IntRange:
                            {
                                if (val is not LazyRange<int> v)
                                {
                                    v = default;
                                }
                                var rl = r;
                                var rr = r;
                                var hw = r.width / 2;
                                rl.xMax -= hw;
                                rr.xMin += hw;
                                var nv0 = EditorGUI.IntField(rl, v.MinValue);
                                var nv1 = EditorGUI.IntField(rl, v.MaxValue);
                                var nv = new LazyRange<int>(nv0, nv1);
                                if ((val == null) || (nv != v))
                                {
                                    ctxt.propertyValue.stringValue = LazyProperty.FormatString(nv, vt);
                                }
                            }
                            break;
                        case PropertyValueType.FloatRange:
                            {
                                if (val is not LazyRange<float> v)
                                {
                                    v = default;
                                }
                                var rl = r;
                                var rr = r;
                                var hw = r.width / 2;
                                rl.xMax -= hw;
                                rr.xMin += hw;
                                var nv0 = EditorGUI.FloatField(rl, v.MinValue);
                                var nv1 = EditorGUI.FloatField(rl, v.MaxValue);
                                var nv = new LazyRange<float>(nv0, nv1);
                                if ((val == null) || (nv != v))
                                {
                                    ctxt.propertyValue.stringValue = LazyProperty.FormatString(nv, vt);
                                }
                            }
                            break;
                        case PropertyValueType.Invalid:
                        //case PropertyValueType.Struct:
                        default:
                            break;
                    }
                }
            }
            else
            {
                if (!ctxt.propertyValue.hasMultipleDifferentValues)
                {
                    if (!string.IsNullOrEmpty(ctxt.propertyValue.stringValue))
                    {
                        ctxt.propertyValue.stringValue = "";
                    }
                }
            }
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var propertyElement = property.FindPropertyRelative("propertyElement");
            var withStruct = !string.IsNullOrEmpty(propertyElement.stringValue);

            var spec = GetSpec(property);
            var h = EditorGUIUtility.singleLineHeight;
            var s = EditorGUIUtility.standardVerticalSpacing;
            var lines = 1;
            if (property.isExpanded)
            {
                lines += 3;
                if (withStruct)
                {
                    lines += 1;
                }
                if (spec.withValue)
                {
                    lines += 1;
                }
            }
            return (h + s) * lines;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var indentLevel = EditorGUI.indentLevel;
            var showMixedValue = EditorGUI.showMixedValue;
            {
                var ctxt = default(Context);
                ctxt.spec = GetSpec(property);
                ctxt.height = EditorGUIUtility.singleLineHeight;
                ctxt.width = EditorGUIUtility.currentViewWidth;
                ctxt.spacing = EditorGUIUtility.standardVerticalSpacing;

#if false
                // propertydrawerからfoldoutを使うと一段落ちてしまうので補正
                // しかし三角部分がクリックできなくなる。。
                EditorGUI.indentLevel--;
                position = EditorGUI.IndentedRect(position);
                EditorGUI.indentLevel++;
#endif
                ctxt.rect = new Rect(position.position, new Vector2(position.width, ctxt.height));
                property.isExpanded = EditorGUI.Foldout(ctxt.rect, property.isExpanded, new GUIContent(ToTitleCase(property.name)), true);
                ctxt.rect.y += ctxt.height + ctxt.spacing;
                if (property.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    //EditorGUI.indentLevel++;

                    ctxt.gameObject = property.FindPropertyRelative("gameObject");
                    ctxt.component = property.FindPropertyRelative("component");
                    ctxt.propertyName = property.FindPropertyRelative("propertyName");
                    ctxt.propertyElement = property.FindPropertyRelative("propertyElement");
                    ctxt.propertyValue = property.FindPropertyRelative("propertyValue");

                    var go = SelectGameObject(ref ctxt);
                    var cp = SelectComponent(ref ctxt, go);
                    var pt = default(Type);
                    if (cp is Transform)
                    {
                        pt = SelectGameObjectProperty(ref ctxt, go);
                    }
                    else
                    {
                        var pi = SelectProperty(ref ctxt, cp);
                        SelectElement(ref ctxt, pi);
                        pt = pi?.PropertyType;
                    }
                    SpecifyValue(ref ctxt, pt);

                    EditorGUI.indentLevel--;
                }
                EditorGUI.EndFoldoutHeaderGroup();
            }
            EditorGUI.showMixedValue = showMixedValue;
            EditorGUI.indentLevel = indentLevel;
            EditorGUI.EndProperty();
        }
    }
}
#endif
