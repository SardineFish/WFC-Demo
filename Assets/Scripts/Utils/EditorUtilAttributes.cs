using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SardineFish.Utils
{
    public interface ICustomEditorEX
    {
    }

    public abstract class CustomEditorAttribute : Attribute
    {

    }


    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class EditorButtonAttribute : CustomEditorAttribute
    {
        public string Label { get; private set; }

        public EditorButtonAttribute(string label = "")
        {
            Label = label;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class DisplayInInspectorAttribute : CustomEditorAttribute
    {

        public DisplayInInspectorAttribute(string label = "") : base()
        {
            Label = label;
        }

        public string Label { get; }
        public bool InlineArray { get; set; } = false;
    }

    public interface INotifyOnReload
    {
        void OnReload();
    }
}