using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SardineFish.Utils;
using UnityEditor;
using UnityEngine;

namespace SardineFish.Utils.Editor
{
    [CustomAttributeEditor(typeof(DisplayInInspectorAttribute))]
    class DisplayInInspectorEditor : AttributeEditor
    {
        public override void OnEdit(MemberInfo member, CustomEditorAttribute attr)
        {
            var attribute = attr as DisplayInInspectorAttribute;
            if (attribute == null)
                return;

            object value = "<error>";
            if (member.MemberType == MemberTypes.Property)
                value = (member as PropertyInfo)?.GetValue(target);
            else if (member.MemberType == MemberTypes.Field)
                value = (member as FieldInfo)?.GetValue(target);
            RenderValue(attribute.Label == "" ? member.Name : attribute.Label, value, attribute);

        }

        void RenderValue(string label, object value, DisplayInInspectorAttribute attribute)
        {
            switch (value)
            {
                case Array array:
                {
                    if (attribute.InlineArray)
                        RenderInlineArray(label, array);
                    else
                        RenderExpandArray(label, array);
                    break;
                }
                case IDictionary dict:
                {
                    RenderDictionary(label, dict);
                    break;
                }
                default:
                    RenderDefaultValue(label, value);
                    break;
            }
        }

        void RenderDefaultValue(string label, object value)
        {
            EditorUtils.Horizontal(() =>
            {
                EditorGUILayout.LabelField(label);
                EditorGUILayout.LabelField(value.ToString());
            });
        }

        void RenderDictionary(string label, IDictionary dict)
        {
            EditorGUILayout.LabelField(label);
            EditorUtils.Verticle(
                EditorUtils.Styles.Indent,
                () =>
                {
                    
                    foreach (var key in dict.Keys)
                    {
                        var value = dict[key];
                        EditorGUILayout.LabelField($"[{key}] {value}");
                    }
                });
        }

        void RenderInlineArray(string label, Array array)
        {
            EditorUtils.Horizontal(() =>
            {
                EditorGUILayout.LabelField(label);
                var str = "[";
                var i = 0;
                foreach (var element in array)
                {
                    if (i == 0)
                        str += element.ToString();
                    else
                        str += "," + element.ToString();
                    ++i;
                }
                str += "]";
                EditorGUILayout.LabelField(str);
            });
        }

        void RenderExpandArray(string label, Array array)
        {
            EditorGUILayout.LabelField(label);
            EditorUtils.Verticle(
                EditorUtils.Styles.Indent,
                () =>
                {
                    var i = 0;
                    foreach (var element in array)
                    {
                        EditorGUILayout.LabelField($"[{i}] {element}");
                        ++i;
                    }
                });
        }
    }
}