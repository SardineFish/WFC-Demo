using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace SardineFish.Utils.Editor
{
    public static class EditorUtils
    {
        public static class Styles
        {
            public static GUIContent PlusIconContent = EditorGUIUtility.IconContent("Toolbar Plus");
            public static GUIContent MinusIconContent = EditorGUIUtility.IconContent("Toolbar Minus");
            public static GUIStyle Indent = new GUIStyle();

            static Styles()
            {
                Indent.padding.left = (int)EditorGUIUtility.singleLineHeight;
            }
        }

        public static ListDrawer<T> DrawList<T>(List<T> list)
            => new ListDrawer<T>(list);

        public static T[] DrawArray<T>(T[] array, Action headerRenderer, Func<T, T> renderCallback)
        {
            return DrawList(array.ToList())
                .Header(headerRenderer)
                .Item(renderCallback)
                .Render()
                .ToArray();
        }

        public static T[] DrawArray<T>(string lable, T[] array, Func<T, T> renderCallback)
        {
            return DrawList(array.ToList())
                .Header(lable)
                .Item(renderCallback)
                .Render()
                .ToArray();
        }


        public static Color HTMLColor(string color)
        {
            Color c;
            ColorUtility.TryParseHtmlString(color, out c);
            return c;
        }

        public static T ObjectField<T>(string label, T obj) where T : UnityEngine.Object
        {
            return EditorGUILayout.ObjectField(label, obj, typeof(T), true) as T;
        }
        public static T ObjectField<T>(T obj) where T : UnityEngine.Object
        {
            return EditorGUILayout.ObjectField(obj, typeof(T), true) as T;
        }
        public static void Verticle(GUIStyle style, Action renderContent)
        {
            if (style == null)
                EditorGUILayout.BeginVertical();
            else
                EditorGUILayout.BeginVertical(style);
            renderContent?.Invoke();
            EditorGUILayout.EndVertical();
        }

        public static bool Fold(string label, ref bool visible, Action renderCallback)
        {
            visible = EditorGUILayout.Foldout(visible, label);
            if (visible)
                Verticle(Styles.Indent, renderCallback);
            return visible;
        }

        public static void Verticle(Action renderContent)
        {
            Verticle(null, renderContent);
        }

        public static void Area(Rect rect, GUIStyle style, Action renderCallback)
        {
            GUILayout.BeginArea(rect, style);
            renderCallback?.Invoke();
            GUILayout.EndArea();
        }

        public static void Area(Rect rect, Action renderCallback)
        {
            GUILayout.BeginArea(rect);
            renderCallback?.Invoke();
            GUILayout.EndArea();
        }

        public static Rect HorizontalSplit(Rect container, int height, Action<Rect> renderCallback)
        {
            var restPart = container;
            restPart.y += height;
            restPart.height = container.height - height;
            renderCallback(new Rect(container.position, new Vector2(container.width, height)));
            return restPart;
        }

        public static void HorizontalSplit(Rect container, int height, Action<Rect> topPartRenderer,
            Action<Rect> bottomPartRenderer)
        {
            var restPart = container;
            restPart.y += height;
            restPart.height = container.height - height;
            topPartRenderer?.Invoke(new Rect(container.position, new Vector2(container.width, height)));
            bottomPartRenderer?.Invoke(restPart);
        }

        public static Rect VerticalSplit(Rect container, int width, Action<Rect> renderCallback)
        {
            var restPart = container;
            restPart.x += width;
            restPart.width = container.width - width;
            renderCallback?.Invoke(new Rect(container.position, new Vector2(width, container.height)));
            return restPart;
        }

        public static void VerticalSplit(Rect rect, int width, Action<Rect> leftPartRenderer,
            Action<Rect> rightPartRenderer)
        {
            var restPart = rect;
            restPart.x += width;
            restPart.width = rect.width - width;
            leftPartRenderer?.Invoke(new Rect(rect.position, new Vector2(width, rect.height)));
            rightPartRenderer?.Invoke(restPart);
        }
        

        public static void Horizontal(GUIStyle style, Action renderContent)
        {
            if (style == null)
                EditorGUILayout.BeginHorizontal();
            else
                EditorGUILayout.BeginHorizontal(style);
            renderContent?.Invoke();
            EditorGUILayout.EndHorizontal();
        }

        public static void Horizontal(Action renderContent)
        {
            Horizontal(null, renderContent);
        }

        public static void PopupMenu(PopupMenuItem[] items, string selected, Action<string> selectedCallback)
        {
            GenericMenu menu = new GenericMenu();
            items.ForEach(item => AddMenuItem(menu, item, "", selected, selectedCallback));
            menu.ShowAsContext();
        }

        static void AddMenuItem(GenericMenu menu, PopupMenuItem item, string path, string selectedPath, Action<string> callback)
        {
            if (item.Children == null)
            {
                var currentPath = $"{path}{item.Name}";
                menu.AddItem(new GUIContent(currentPath), currentPath == selectedPath, (data) => callback?.Invoke((string)data), currentPath);
            }
            else
            {
                var nextPath = $"{path}{item.Name}/";
                item.Children.ForEach(i => AddMenuItem(menu, i, nextPath, selectedPath, callback));
            }
        }

        public static void GridLayout(Rect container, Vector2 gridSize, int count, Action<Rect> renderCallback)
        {
            var layoutPos = container.min;
            for (var i = 0; i < count; i++)
            {
                renderCallback(new Rect(layoutPos, gridSize));
                layoutPos.x += gridSize.x;
                if (gridSize.x - container.xMin > gridSize.x)
                {
                    layoutPos.x = container.x;
                    layoutPos.y += gridSize.y;
                }
            }
        }

        public static void GridLayout<T>(Rect container, Vector2 gridSize, IEnumerable<T> collection,
            Action<Rect, T> renderCallback)
        {
            var layoutPos = container.min;
            foreach(var element in collection)
            {
                renderCallback(new Rect(layoutPos, gridSize), element);
                layoutPos.x += gridSize.x;
                if (gridSize.x - container.xMin > container.width)
                {
                    layoutPos.x = container.x;
                    layoutPos.y += gridSize.y;
                }
            }
        }
        

        public class ListDrawer<T>
        {
            bool fold = true;
            bool allowAdd = true;
            bool allowRemove = false;
            List<T> list = null;
            Func<T> onCreate = null;
            Func<T, int, bool> onRemove = null;
            Action headerRenderer = null;
            Func<T, int, T> itemRenderer = null;

            public ListDrawer(List<T> list)
            {
                this.list = list;
            }

            public ListDrawer<T> Fold(bool extend)
            {
                this.fold = extend;
                return this;
            }

            public ListDrawer<T> Header(string label)
            {
                this.headerRenderer = () => EditorGUILayout.LabelField(label);
                return this;
            }

            public ListDrawer<T> Header(Action renderer)
            {
                this.headerRenderer = renderer;
                return this;
            }

            public ListDrawer<T> Item(Action<T> itemRenderer)
            {
                return Item((t, i) =>
                {
                    itemRenderer(t);
                    return t;
                });
            }

            public ListDrawer<T> Item(Action<T, int> itemRenderer)
            {
                return Item((t, i) =>
                {
                    itemRenderer(t, i);
                    return t;
                });
            }

            public ListDrawer<T> Item(Func<T, int, T> itemRenderer)
            {
                this.itemRenderer = itemRenderer;
                return this;
            }

            public ListDrawer<T> Item(Func<T, T> itemRenderer)
                => this.Item((t, i) => itemRenderer(t));

            public ListDrawer<T> OnAdd(bool enableAdd)
            {
                this.allowAdd = enableAdd;
                return this;
            }
            public ListDrawer<T> OnAdd(Func<T> callback)
            {
                this.onCreate = callback;
                return this;
            }

            public ListDrawer<T> OnRemove(bool allowRemove)
            {
                this.allowRemove = allowRemove;
                return this;
            }
            public ListDrawer<T> OnRemove(Func<T, int, bool> callback)
            {
                this.onRemove = callback;
                return this;
            }

            public ListDrawer<T> ReadOnly()
            {
                allowAdd = allowRemove = false;
                return this;
            }
            public List<T> Render()
            {
                if (itemRenderer == null)
                    return list;

                onCreate = onCreate ?? AddItem;

                Verticle(() =>
                {
                    Horizontal(() =>
                    {
                        headerRenderer?.Invoke();
                        if (allowAdd && GUILayout.Button(Styles.PlusIconContent, GUIStyle.none, GUILayout.Width(EditorGUIUtility.singleLineHeight)))
                        {
                            list.Add(onCreate());
                        }
                    });
                    EditorGUILayout.Space();
                    Verticle(Styles.Indent, () =>
                    {
                        for (var i = 0; i < list.Count; i++)
                        {
                            Horizontal(() =>
                            {
                                if (GUILayout.Button(Styles.MinusIconContent, GUIStyle.none, GUILayout.Width(EditorGUIUtility.singleLineHeight)))
                                {
                                    if (onRemove != null && !onRemove(list[i], i))
                                    {
                                        // do nothing;
                                    }
                                    else
                                    {
                                        list.RemoveAt(i--);
                                        if (i >= list.Count)
                                            return;
                                        return;
                                    }
                                }

                                Verticle(() =>
                                {
                                    list[i] = itemRenderer.Invoke(list[i], i);
                                });
                            });
                        }
                    });
                });
                return list;
            }

            T AddItem()
            {
                if (typeof(T).IsClass)
                {
                    try
                    {
                        var instance = Activator.CreateInstance<T>();
                        return instance;
                    }
                    catch
                    {
                        Debug.LogWarning($"Failed to create instance of {typeof(T).Name}");
                    }
                }
                return default(T);
            }

        }


    }
}

public class PopupMenuItem
{
    public string Name;
    public PopupMenuItem[] Children = null;

    public PopupMenuItem()
    {
    }

    public PopupMenuItem(string name)
    {
        Name = name;
    }
}