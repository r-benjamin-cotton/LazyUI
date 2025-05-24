using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace LazyUI
{
    /// <summary>
    /// コンポーネントのメソッド指定
    /// </summary>
    [CustomPropertyDrawer(typeof(LazyAction))]
    public class LazyActionDrawer : PropertyDrawer
    {
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
        private struct Context
        {
            public float height;
            public float width;
            public float spacing;
            public Rect rect;
            public SerializedProperty gameObject;
            public SerializedProperty component;
            public SerializedProperty method;
            public SerializedProperty parameter;
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
                    ctxt.gameObject.objectReferenceValue = newObj;
                    ctxt.component.objectReferenceValue = null;
                    ctxt.method.stringValue = "";
                    ctxt.parameter.stringValue = "";
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
                var components = go.GetComponents(typeof(MonoBehaviour)).Prepend(go.transform).ToArray();
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
                        ctxt.method.stringValue = "";
                        ctxt.parameter.stringValue = "";
                    }
                    if (newIndex >= 0)
                    {
                        cp = components[newIndex];
                    }
                }
            }
            return cp;
        }
        private static string[] GetMethodNames(MethodInfo[] methods)
        {
            return methods.Select((mi) =>
            {
                var ps = mi.GetParameters();
                if (ps.Length == 0)
                {
                    return mi.Name;
                }
                else
                {
                    return $"{mi.Name}({ps[0].ParameterType.Name})";
                }
            }).ToArray();
        }
        private static string[] GetMethodNamesFull(MethodInfo[] methods)
        {
            return methods.Select((mi) =>
            {
                var ps = mi.GetParameters();
                if (ps.Length == 0)
                {
                    return mi.Name;
                }
                else
                {
                    return $"{mi.Name}({ps[0].ParameterType.AssemblyQualifiedName})";
                }
            }).ToArray();
        }
        private static IEnumerable<MethodInfo> GetGameObjectMethods(GameObject go)
        {
            var t = go.GetType();
            yield return t.GetMethod("SetActive");
            yield return t.GetMethod("set_layer");
            yield return t.GetMethod("set_name");
            yield return t.GetMethod("set_tag");
        }
        private Type GetParameterType(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                return null;
            }
            var p = methodInfo.GetParameters();
            if ((p == null) || (p.Length == 0))
            {
                return null;
            }
            if (p.Length > 1)
            {
                return null;
            }
            return p[0].ParameterType;
        }
        private Type SelectGameObjectMethod(ref Context ctxt, GameObject go)
        {
            var r = EditorGUI.PrefixLabel(ctxt.rect, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Method"));
            ctxt.rect.y += ctxt.height + ctxt.spacing;
            var pt = default(Type);
            if (go != null)
            {
                var methods = GetGameObjectMethods(go).ToArray();
                if (methods.Length != 0)
                {
                    var methodNames = GetMethodNames(methods);
                    var methodNamesFull = GetMethodNamesFull(methods);
                    var methodIndices = new int[methodNames.Length];
                    for (int i = 0; i < methodIndices.Length; i++)
                    {
                        methodIndices[i] = i;
                    }
                    var name = ctxt.method.hasMultipleDifferentValues ? null : ctxt.method.stringValue;
                    var index = IndexOf(methodNamesFull, n => n.Equals(name));
                    EditorGUI.showMixedValue = ctxt.method.hasMultipleDifferentValues;
                    var newIndex = EditorGUI.IntPopup(r, index, methodNames, methodIndices);
                    if (newIndex != index)
                    {
                        ctxt.method.stringValue = (newIndex < 0) ? "" : methodNamesFull[newIndex];
                        ctxt.parameter.stringValue = "";
                    }
                    if (newIndex >= 0)
                    {
                        pt = GetParameterType(methods[newIndex]);
                    }
                }
            }
            return pt;
        }
        private Type SelectMethod(ref Context ctxt, Component cp)
        {
            var r = EditorGUI.PrefixLabel(ctxt.rect, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Method"));
            ctxt.rect.y += ctxt.height + ctxt.spacing;
            var pt = default(Type);
            if (cp != null)
            {
                var type = ctxt.component.objectReferenceValue.GetType();
                var methods = type.GetMethods()
                    .Where(mi =>
                    {
                        if (mi.ReturnType != typeof(void))
                        {
                            return false;
                        }
                        var p = mi.GetParameters();
                        if (p.Length == 0)
                        {
                            return true;
                        }
                        if (p.Length > 1)
                        {
                            return false;
                        }
                        var vt = LazyAction.GetValueType(p[0].ParameterType);
                        if (vt == LazyAction.ValueType.Invalid)
                        {
                            return false;
                        }
                        return true;
                    }).ToArray();
                if (methods.Length != 0)
                {
                    var methodNames = GetMethodNames(methods);
                    var methodNamesFull = GetMethodNamesFull(methods);
                    var methodIndices = new int[methodNames.Length];
                    for (int i = 0; i < methodIndices.Length; i++)
                    {
                        methodIndices[i] = i;
                    }
                    var name = ctxt.method.hasMultipleDifferentValues ? null : ctxt.method.stringValue;
                    var index = IndexOf(methodNamesFull, n => n.Equals(name));
                    EditorGUI.showMixedValue = ctxt.method.hasMultipleDifferentValues;
                    var newIndex = EditorGUI.IntPopup(r, index, methodNames, methodIndices);
                    if (newIndex != index)
                    {
                        ctxt.method.stringValue = (newIndex < 0) ? "" : methodNamesFull[newIndex];
                        ctxt.parameter.stringValue = "";
                    }
                    if (newIndex >= 0)
                    {
                        pt = GetParameterType(methods[newIndex]);
                    }
                }
            }
            return pt;
        }
        private void SpecifyValue(ref Context ctxt, Type parameterType)
        {
            var vt = LazyAction.GetValueType(parameterType);
            var r = EditorGUI.PrefixLabel(ctxt.rect, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Value"));
            ctxt.rect.y += ctxt.height + ctxt.spacing;
            if (vt != LazyAction.ValueType.Invalid)
            {
                if (!LazyAction.TryParse(ctxt.parameter.stringValue, parameterType, out object val))
                {
                    val = null;
                }
                EditorGUI.showMixedValue = ctxt.parameter.hasMultipleDifferentValues;
                switch (vt)
                {
                    case LazyAction.ValueType.Boolean:
                        {
                            if (val is not bool v)
                            {
                                v = false;
                            }
                            var b = v ? BooleanEnum.True : BooleanEnum.False;
                            var nv = EditorGUI.EnumPopup(r, b);
                            if ((val == null) || !nv.Equals(b))
                            {
                                ctxt.parameter.stringValue = LazyAction.FormatString(nv, parameterType);
                            }
                        }
                        break;
                    case LazyAction.ValueType.Int16:
                        {
                            if (val is not short v)
                            {
                                v = 0;
                            }
                            var nv = Math.Clamp(EditorGUI.IntField(r, v), short.MinValue, short.MaxValue);
                            if ((val == null) || (nv != v))
                            {
                                ctxt.parameter.stringValue = LazyAction.FormatString(nv, parameterType);
                            }
                        }
                        break;
                    case LazyAction.ValueType.UInt16:
                        {
                            var v = ctxt.parameter.stringValue;
                            var nv = GetValue(EditorGUI.TextField(r, v), ushort.MinValue, ushort.MaxValue).ToString();
                            if ((val == null) || (nv != v))
                            {
                                ctxt.parameter.stringValue = LazyAction.FormatString(nv, parameterType);
                            }
                        }
                        break;
                    case LazyAction.ValueType.Int32:
                        {
                            if (val is not int v)
                            {
                                v = 0;
                            }
                            var nv = EditorGUI.IntField(r, v);
                            if ((val == null) || (nv != v))
                            {
                                ctxt.parameter.stringValue = LazyAction.FormatString(nv, parameterType);
                            }
                        }
                        break;
                    case LazyAction.ValueType.UInt32:
                        {
                            var v = ctxt.parameter.stringValue;
                            var nv = GetValue(EditorGUI.TextField(r, v), uint.MinValue, uint.MaxValue).ToString();
                            if ((val == null) || (nv != v))
                            {
                                ctxt.parameter.stringValue = LazyAction.FormatString(nv, parameterType);
                            }
                        }
                        break;
                    case LazyAction.ValueType.Int64:
                        {
                            if (val is not long v)
                            {
                                v = 0;
                            }
                            var nv = EditorGUI.LongField(r, v);
                            if ((val == null) || (nv != v))
                            {
                                ctxt.parameter.stringValue = LazyAction.FormatString(nv, parameterType);
                            }
                        }
                        break;
                    case LazyAction.ValueType.UInt64:
                        {
                            var v = ctxt.parameter.stringValue;
                            var nv = GetValue(EditorGUI.TextField(r, v), ulong.MinValue, ulong.MaxValue).ToString();
                            if ((val == null) || (nv != v))
                            {
                                ctxt.parameter.stringValue = LazyAction.FormatString(nv, parameterType);
                            }
                        }
                        break;
                    case LazyAction.ValueType.Single:
                        {
                            if (val is not float v)
                            {
                                v = 0;
                            }
                            var nv = EditorGUI.FloatField(r, v);
                            if ((val == null) || (nv != v))
                            {
                                ctxt.parameter.stringValue = LazyAction.FormatString(nv, parameterType);
                            }
                        }
                        break;
                    case LazyAction.ValueType.Double:
                        {
                            if (val is not double v)
                            {
                                v = 0;
                            }
                            var nv = EditorGUI.DoubleField(r, v);
                            if ((val == null) || (nv != v))
                            {
                                ctxt.parameter.stringValue = LazyAction.FormatString(nv, parameterType);
                            }
                        }
                        break;
                    case LazyAction.ValueType.String:
                        {
                            var v = ctxt.parameter.stringValue;
                            var nv = EditorGUI.TextField(r, v);
                            if ((val == null) || (nv != v))
                            {
                                ctxt.parameter.stringValue = LazyAction.FormatString(nv, parameterType);
                            }
                        }
                        break;
                    case LazyAction.ValueType.Enum:
                        {
                            if (val is not Enum v)
                            {
                                v = (Enum)parameterType.GetEnumValues().GetValue(0);
                            }
                            var nv = EditorGUI.EnumPopup(r, v);
                            if ((val == null) || !nv.Equals(v))
                            {
                                ctxt.parameter.stringValue = LazyAction.FormatString(nv, parameterType);
                            }
                        }
                        break;
                    case LazyAction.ValueType.Vector2:
                        {
                            if (val is not Vector2 v)
                            {
                                v = Vector2.zero;
                            }
                            var nv = EditorGUI.Vector2Field(r, GUIContent.none, v);
                            if ((val == null) || (nv != v))
                            {
                                ctxt.parameter.stringValue = LazyAction.FormatString(nv, parameterType);
                            }
                        }
                        break;
                    case LazyAction.ValueType.Vector3:
                        {
                            if (val is not Vector3 v)
                            {
                                v = Vector3.zero;
                            }
                            var nv = EditorGUI.Vector3Field(r, GUIContent.none, v);
                            if ((val == null) || (nv != v))
                            {
                                ctxt.parameter.stringValue = LazyAction.FormatString(nv, parameterType);
                            }
                        }
                        break;
                    case LazyAction.ValueType.Vector4:
                        {
                            if (val is not Vector4 v)
                            {
                                v = Vector4.zero;
                            }
                            var nv = EditorGUI.Vector4Field(r, GUIContent.none, v);
                            if ((val == null) || (nv != v))
                            {
                                ctxt.parameter.stringValue = LazyAction.FormatString(nv, parameterType);
                            }
                        }
                        break;
                    case LazyAction.ValueType.Quaternion:
                        {
                            if (val is not Vector3 v)
                            {
                                v = Vector3.zero;
                            }
                            var nv = EditorGUI.Vector3Field(r, GUIContent.none, v);
                            if ((val == null) || (nv != v))
                            {
                                ctxt.parameter.stringValue = LazyAction.FormatString(nv, parameterType);
                            }
                        }
                        break;
                    case LazyAction.ValueType.Color:
                        {
                            if (val is not Color v)
                            {
                                v = Color.clear;
                            }
                            var nv = EditorGUI.ColorField(r, GUIContent.none, v);
                            if ((val == null) || (nv != v))
                            {
                                ctxt.parameter.stringValue = LazyAction.FormatString(nv, parameterType);
                            }
                        }
                        break;
                    case LazyAction.ValueType.Invalid:
                    default:
                        break;
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var h = EditorGUIUtility.singleLineHeight;
            var s = EditorGUIUtility.standardVerticalSpacing;
            var lines = 1;
            if (property.isExpanded)
            {
                lines += 4;
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
                ctxt.height = EditorGUIUtility.singleLineHeight;
                ctxt.width = EditorGUIUtility.currentViewWidth;
                ctxt.spacing = EditorGUIUtility.standardVerticalSpacing;

                ctxt.rect = new Rect(position.position, new Vector2(position.width, ctxt.height));
                property.isExpanded = EditorGUI.Foldout(ctxt.rect, property.isExpanded, new GUIContent(ToTitleCase(property.name)), true);
                ctxt.rect.y += ctxt.height + ctxt.spacing;
                if (property.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    //EditorGUI.indentLevel++;

                    ctxt.gameObject = property.FindPropertyRelative("gameObject");
                    ctxt.component = property.FindPropertyRelative("component");
                    ctxt.method = property.FindPropertyRelative("method");
                    ctxt.parameter = property.FindPropertyRelative("parameter");

                    var go = SelectGameObject(ref ctxt);
                    var cp = SelectComponent(ref ctxt, go);
                    var pt = default(Type);
                    if (cp is Transform)
                    {
                        pt = SelectGameObjectMethod(ref ctxt, go);
                    }
                    else
                    {
                        pt = SelectMethod(ref ctxt, cp);
                    }
                    SpecifyValue(ref ctxt, pt);

                    EditorGUI.indentLevel--;
                }
            }
            EditorGUI.showMixedValue = showMixedValue;
            EditorGUI.indentLevel = indentLevel;
            EditorGUI.EndProperty();
        }
    }
}
#endif
