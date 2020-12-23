using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

namespace SardineFish.Utils
{
    public static class Utility
    {
        #region LINQExtension

        #region Comparer

        public static IComparer<T> MakeComparer<T>(Func<T, T, int> comparer)
            => new DelegateComparer<T>(comparer);

        public static IEqualityComparer<T> MakeEqualityComparer<T>(Func<T, T, bool> comparer)
            => new DelegateEqualityComparer<T>(comparer);

        public class DelegateEqualityComparer<T> : IEqualityComparer<T>
        {
            Func<T, T, bool> comparerFunc;

            public DelegateEqualityComparer(Func<T, T, bool> comparerFunc)
            {
                this.comparerFunc = comparerFunc;
            }

            public bool Equals(T x, T y)
            {
                if (comparerFunc != null)
                    return comparerFunc(x, y);
                throw new NotImplementedException();
            }

            public int GetHashCode(T obj)
            {
                return 0;
                //return obj.GetHashCode();
            }
        }

        public class DelegateComparer<T> : IComparer<T>, IEqualityComparer<T>
        {
            Func<T, T, int> ComparerFunc;

            public DelegateComparer(Func<T, T, int> comparerFunc)
            {
                ComparerFunc = comparerFunc;
            }

            public int Compare(T x, T y)
            {
                if (ComparerFunc != null)
                    return ComparerFunc(x, y);
                throw new NotImplementedException();
            }

            public bool Equals(T x, T y)
            {
                if (ComparerFunc != null)
                    return ComparerFunc(x, y) == 0;
                throw new NotImplementedException();
            }

            public int GetHashCode(T obj)
            {
                return obj.GetHashCode();
            }
        }

        public static T MinOf<T, TCompare>(this IEnumerable<T> ts, Func<T, TCompare> selector)
            where TCompare : IComparable<TCompare>
        {
            var first = true;
            TCompare minValue = default;
            T minItem = default;
            foreach (var item in ts)
            {
                if (first)
                {
                    minValue = selector(item);
                    minItem = item;
                    first = false;
                }
                else
                {
                    var value = selector(item);
                    if (value.CompareTo(minValue) < 0)
                    {
                        minValue = value;
                        minItem = item;
                    }
                }
            }

            return minItem;
        }

        public static T MaxOf<T, TCompare>(this IEnumerable<T> ts, Func<T, TCompare> selector)
            where TCompare : IComparable<TCompare>
        {
            var first = true;
            TCompare maxValue = default;
            T maxItem = default;
            foreach (var item in ts)
            {
                if (first)
                {
                    maxValue = selector(item);
                    maxItem = item;
                    first = false;
                }
                else
                {
                    var value = selector(item);
                    if (value.CompareTo(maxValue) > 0)
                    {
                        maxValue = value;
                        maxItem = item;
                    }
                }
            }

            return maxItem;
        }


        public static bool Diff<T>(this IEnumerable<T> ts, IEnumerable<T> target) where T : class
            => Diff<T, T>(ts, target, (s, t) => s == t);

        public static bool Diff<T, U>(this IEnumerable<T> ts, IEnumerable<U> target, Func<T, U, bool> comparerer)
        {
            var targetEnumerator = target.GetEnumerator();
            foreach (var element in ts)
            {
                if (!targetEnumerator.MoveNext())
                    return false;
                var targetElement = targetEnumerator.Current;
                if (!comparerer(element, targetElement))
                    return false;
            }

            if (targetEnumerator.MoveNext())
                return false;
            return true;
        }

        #endregion

        #region Generator

        public enum RectCorner
        {
            XMinYMin,
            XMinYMax,
            XMaxYMin,
            XMaxYMax,
        }

        public static IEnumerable<(int x, int y)> DiagonalIndices(int width, int height, RectCorner entryPoint)
        {
            for (int i = 0; i <= width + height - 2; i++)
            {
                int x = 0, y = 0;

                switch (entryPoint)
                {
                    case RectCorner.XMinYMin:
                        x = Math.Max(0, i - (height - 1));
                        y = i - x;
                        break;
                    case RectCorner.XMaxYMax:
                        x = Math.Max(0, width - 1 - i);
                        y = (width + height - 2) - i - x;
                        break;
                    case RectCorner.XMinYMax:
                        x = Math.Max(i - (height - 1), 0);
                        y = x + height - 1 - i;
                        break;
                    case RectCorner.XMaxYMin:
                        x = Math.Max(width - 1 - i, 0);
                        y = x - (width - 1) + i;
                        break;
                }

                while (0 <= x && x < width && 0 <= y && y < height)
                {
                    yield return (x, y);

                    switch (entryPoint)
                    {
                        case RectCorner.XMinYMin:
                        case RectCorner.XMaxYMax:
                            x++;
                            y--;
                            break;
                        case RectCorner.XMinYMax:
                        case RectCorner.XMaxYMin:
                            x++;
                            y++;
                            break;
                    }
                }
            }
        }

        #endregion Generator

        public static void ForEach<T>(this IEnumerable<T> ts, Action<T> callback)
        {
            foreach (var item in ts)
                callback(item);
        }

        public static IEnumerable<T> RandomSequence<T>(this IEnumerable<T> list, int count)
        {
            var source = list.ToArray();

            for (var i = 0; i < count; i++)
            {
                var idx = UnityEngine.Random.Range(0, source.Length - i);
                yield return source[idx];
                source[idx] = source[count - i - 1];
            }
        }

        /// <summary>
        /// Take an element from a sequence by normalized index in range [0, 1), the index will map uniformly to each element.
        /// The method will traverse whole sequence.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="randomValue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T RandomTake<T>(this IEnumerable<T> source, float randomValue)
            => RandomTake(source, randomValue, _ => 1);

        /// <summary>
        /// Take an element by a normalized index in [0, 1), and remap each element range into by probabilityEvaluator.
        /// Basically used to take an element randomly from a weighted element sequence.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="randomValue"></param>
        /// <param name="probabilityEvaluator"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static T RandomTake<T>(this IEnumerable<T> source, float randomValue,
            Func<T, float> probabilityEvaluator)
        {
            var totalProb = source.Sum(probabilityEvaluator);
            var loc = randomValue * totalProb;
            var currentProb = 0f;
            foreach (var element in source)
            {
                var prob = probabilityEvaluator(element);
                if (currentProb + prob > loc)
                    return element;
                currentProb += prob;
            }

            throw new Exception("Random value of range.");
        }

        public static int IndexOf<T>(this IReadOnlyList<T> list, T element)
        {
            for (var i = 0; i < list.Count; i++)
                if (list[i].Equals(element))
                    return i;
            return -1;
        }

        /// <summary>
        /// Alias of Select
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="callback"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <returns></returns>
        public static IEnumerable<U> Map<T, U>(this IEnumerable<T> collection, Func<T, U> callback) =>
            collection.Select(callback);

        /// <summary>
        /// Get elements which is not null
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> source)
        {
            foreach (var item in source)
            {
                if (item != null)
                    yield return item;
            }
        }

        public static TResult Merge<TResult, Tkey, TElement>(this IGrouping<Tkey, TElement> group, TResult mergeTarget,
            Func<TElement, TResult, TResult> mergeFunc)
        {
            foreach (var element in group)
            {
                mergeTarget = mergeFunc(element, mergeTarget);
            }

            return mergeTarget;
        }

        public static bool All<T>(this IEnumerable<T> ts, Func<T, int, bool> predicate)
        {
            int idx = 0;
            foreach (var item in ts)
            {
                if (!predicate(item, idx++))
                    return false;
            }

            return true;
        }

        public static bool Any<T>(this IEnumerable<T> ts, Func<T, int, bool> predicate)
        {
            int idx = 0;
            foreach (var item in ts)
            {
                if (predicate(item, idx++))
                    return true;
            }

            return false;
        }

        public static Vector2 Sum<T>(this IEnumerable<T> ts, Func<T, Vector2> selector)
        {
            Vector2 v = Vector2.zero;
            foreach (var el in ts)
            {
                v += selector(el);
            }

            return v;
        }

        public static Vector3 Sum<T>(this IEnumerable<T> ts, Func<T, Vector3> selector)
        {
            Vector3 v = Vector3.zero;
            foreach (var el in ts)
            {
                v += selector(el);
            }

            return v;
        }

        public static T Tail<T>(this IList<T> list)
            => list[list.Count - 1];

        public static bool IsEmpty<T>(this ICollection<T> collection)
            => collection.Count <= 0;

        public static bool NotEmpty<T>(this ICollection<T> collection)
            => collection.Count > 0;

        public static T PopBack<T>(this IList<T> list)
        {
            if (list.Count > 0)
            {
                var element = list[list.Count - 1];
                list.RemoveAt(list.Count - 1);
                return element;
            }

            throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// Try to get an element with specific index. Will return null except throwing an exception with an invalid index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static T TryGet<T>(this List<T> list, int index) where T : class
        {
            if (index < 0 || index >= list.Count)
                return null;
            return list[index];
        }

        /// <summary>
        /// Try to get an element with specific index. Will return null except throwing an exception with an invalid index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static T TryGet<T>(this T[] array, int index) where T : class
        {
            if (index < 0 || index >= array.Length)
                return null;
            return array[index];
        }

        public static string Join(this IEnumerable<string> list, string seperator)
        {
            var output = "";
            var enumerator = list.GetEnumerator();
            if (enumerator.MoveNext())
                output = enumerator.Current;
            while (enumerator.MoveNext())
                output = output + seperator + enumerator.Current;
            return output;
        }

        public static string StringJoin(string seperator, params string[] strings)
        {
            if (strings.Length <= 0)
                return null;
            var output = strings[0];
            for (var i = 1; i < strings.Length; i++)
                output = output + seperator + strings[i];
            return output;
        }

        #endregion

        #region UnityHelper

        #region GameObjectHelper

        public static IEnumerable<GameObject> GetChildren(this GameObject gameObject)
        {
            for (var i = 0; i < gameObject.transform.childCount; i++)
            {
                yield return gameObject.transform.GetChild(i).gameObject;
            }
        }

        public static GameObject Instantiate(this UnityEngine.Object self, GameObject original, Scene scene)
        {
            var obj = UnityEngine.Object.Instantiate(original);
            SceneManager.MoveGameObjectToScene(obj, scene);
            return obj;
        }

        public static GameObject Instantiate(GameObject original, Scene scene)
        {
            var obj = UnityEngine.Object.Instantiate(original);
            SceneManager.MoveGameObjectToScene(obj, scene);
            return obj;
        }

        public static GameObject Instantiate(GameObject original, GameObject parent)
        {
            var obj = Instantiate(original, parent.scene);
            obj.transform.SetParent(parent.transform);
            return obj;
        }

        public static GameObject Instantiate(GameObject original, GameObject parent, Vector3 relativePosition,
            Quaternion relativeRotation)
        {
            var obj = Instantiate(original, parent);
            obj.transform.localPosition = relativePosition;
            obj.transform.localRotation = relativeRotation;
            return obj;
        }

        public static void ClearChildren(this GameObject self)
        {
            self.GetChildren().ForEach((child) =>
            {
                child.ClearChildren();
                GameObject.Destroy(child);
            });
        }

        public static void ClearChildImmediate(this GameObject self)
        {
            while (self.transform.childCount > 0)
            {
                var obj = self.transform.GetChild(0).gameObject;
                obj.ClearChildImmediate();
                GameObject.DestroyImmediate(obj);
            }
        }

        public static void SetLayerRecursive(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            gameObject.GetChildren().ForEach(child => child.SetLayerRecursive(layer));
        }

        public static T GetInterface<T>(this Component component)
            => (T) (object) (component.GetComponents<Component>().Where(c => c is T).FirstOrDefault());

        public static T GetInterface<T>(this GameObject obj)
            => (T) (object) obj.GetComponents<Component>().Where(c => c is T).FirstOrDefault();


        public static void ForceDestroy(GameObject gameObject)
        {
            if (Application.isPlaying)
                GameObject.Destroy(gameObject);
            else
                GameObject.DestroyImmediate(gameObject);
        }

        public static void DestroyChildren(this GameObject gameObject)
        {
            if (Application.isPlaying)
            {
                foreach (var child in gameObject.GetChildren())
                {
                    child.SetActive(false);
                    GameObject.Destroy(child);
                }
            }
            else
            {
                var count = gameObject.transform.childCount;
                for (var i = 0; i < count; i++)
                {
                    GameObject.DestroyImmediate(gameObject.transform.GetChild(0).gameObject);
                }
            }
        }

        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : UnityEngine.Component
        {
            var component = gameObject.GetComponent<T>();
            if (!component)
                return gameObject.AddComponent<T>();
            return component;
        }

        #endregion GameObjectHelper

        #region Coroutine & Async

        public static void NextFrame(this MonoBehaviour context, Action callback)
        {
            context.StartCoroutine(NextFrameCoroutine(callback));
        }

        public static IEnumerator NextFrameCoroutine(Action callback)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            callback?.Invoke();
        }

        public static Coroutine NumericAnimate(this MonoBehaviour context, float time, Action<float> tick,
            Action complete = null)
        {
            return context.StartCoroutine(NumericAnimateEnumerator(time, tick, complete));
        }

        public static IEnumerator NumericAnimateEnumerator(float time, Action<float> callback, Action complete)
        {
            var startTime = Time.time;
            for (float t = 0; t < time; t = Time.time - startTime)
            {
                callback?.Invoke(t / time);
                yield return new WaitForEndOfFrame();
            }

            callback?.Invoke(1);
            complete?.Invoke();
        }

        public static Coroutine WaitForSecond(this MonoBehaviour context, Action callback, float seconds = 0)
        {
            return context.StartCoroutine(WaitForSecondEnumerator(callback, seconds));
        }

        public static Coroutine SetTimeout(this MonoBehaviour context, Action callback, float seconds = 0)
            => WaitForSecond(context, callback, seconds);

        public static Coroutine SetInterval(this MonoBehaviour context, Action callback, float seconds = 0)
        {
            return context.StartCoroutine(IntervalCoroutine(callback, seconds));
        }

        public static IEnumerator IntervalCoroutine(Action callback, float seconds)
        {
            while (true)
            {
                yield return new WaitForSeconds(seconds);
                callback?.Invoke();
            }
        }

        public static IEnumerator WaitForSecondEnumerator(Action callback, float seconds = 0)
        {
            yield return new WaitForSeconds(seconds);
            callback?.Invoke();
        }

        public static IEnumerable<float> Timer(float time)
        {
            for (var startTime = Time.time; Time.time < startTime + time;)
            {
                yield return Time.time - startTime;
            }

            yield return time;
        }

        public static IEnumerable<float> FixedTimer(float time)
        {
            for (var startTime = Time.fixedTime; Time.fixedTime < startTime + time;)
            {
                yield return Time.fixedTime - startTime;
            }

            yield return time;
        }

        public static IEnumerable<float> TimerNormalized(float time)
        {
            if (time == 0)
            {
                yield return 1;
                yield break;
            }

            foreach (var t in Timer(time))
            {
                yield return t / time;
            }
        }

        public static IEnumerable<float> FixedTimerNormalized(float time)
        {
            foreach (var t in FixedTimer(time))
            {
                yield return t / time;
            }
        }

        public class CallbackYieldInstruction : CustomYieldInstruction
        {
            Func<bool> callback;
            public override bool keepWaiting => callback?.Invoke() ?? true;

            public CallbackYieldInstruction(Func<bool> callback)
            {
                this.callback = callback;
            }
        }

        public static IEnumerable<int> Times(int times)
        {
            for (var i = 0; i < times; i++)
            {
                yield return i;
            }
        }

        #endregion Coroutine & Async

        #region UIHelper

        public static IEnumerator ShowUI(UnityEngine.UI.Graphic ui, float time, float targetAlpha = 1)
        {
            var color = ui.color;
            color.a = 0;
            ui.gameObject.SetActive(true);
            foreach (var t in TimerNormalized(time))
            {
                color.a = t * targetAlpha;
                ui.color = color;
                yield return null;
            }
        }

        public static IEnumerator ShowUI(CanvasGroup canvasGroup, float time)
        {
            time = (1 - canvasGroup.alpha) * time;
            canvasGroup.alpha = 0;
            canvasGroup.gameObject.SetActive(true);
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            var alpha = canvasGroup.alpha;
            foreach (var t in TimerNormalized(time))
            {
                canvasGroup.alpha = alpha + t * (1 - alpha);
                yield return null;
            }
        }

        public static async Task ShowUIAsync(CanvasGroup canvasGroup, float time)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            time = (1 - canvasGroup.alpha) * time;
            canvasGroup.alpha = 0;
            canvasGroup.gameObject.SetActive(true);
            var alpha = canvasGroup.alpha;
            foreach (var t in TimerNormalized(time))
            {
                canvasGroup.alpha = alpha + t * (1 - alpha);
                await Task.Yield();
            }
        }

        public static IEnumerator HideUI(CanvasGroup canvasGroup, float time, bool deactivate = true)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            foreach (var t in TimerNormalized(time))
            {
                canvasGroup.alpha = 1 - t;
                yield return null;
            }

            canvasGroup.alpha = 0;
            if (deactivate)
                canvasGroup.gameObject.SetActive(false);
        }

        public static async Task HideUIAsync(CanvasGroup canvasGroup, float time, bool deactivate = true)
        {
            foreach (var t in TimerNormalized(time))
            {
                canvasGroup.alpha = 1 - t;
                await Task.Yield();
            }

            canvasGroup.alpha = 0;
            if (deactivate)
                canvasGroup.gameObject.SetActive(false);
        }

        public static IEnumerator HideUI(UnityEngine.UI.Graphic ui, float time, bool deactive = false)
        {
            var color = ui.color;
            color.a = 1;
            foreach (var t in TimerNormalized(time))
            {
                color.a = 1 - t;
                ui.color = color;
                yield return null;
            }

            if (deactive)
                ui.gameObject.SetActive(false);
        }

        public static Task WaitForClick(this Button button)
        {
            return WaitForClick<object>(button, null);
        }

        public static async Task<T> WaitForClick<T>(this Button button, T param)
        {
            var promise = new TaskCompletionSource<T>();
            button.onClick.AddListener(ClickCallback);
            return await promise.Task;

            void ClickCallback()
            {
                button.onClick.RemoveListener(ClickCallback);
                promise.SetResult(param);
            }
        }

        #endregion UIHelper

        #region Rendering

        public static Matrix4x4 ProjectionToWorldMatrix(Camera camera)
        {
            return (camera.projectionMatrix * camera.worldToCameraMatrix).inverse;
        }

        public static (Mesh, Matrix4x4) GenerateFullScreenQuad(Camera camera)
        {
            var mesh = new Mesh();
            mesh.Clear();
            mesh.vertices = new Vector3[]
            {
                new Vector3(-1, -1, 0),
                new Vector3(1, -1, 0),
                new Vector3(1, 1, 0),
                new Vector3(-1, 1, 0),
            };
            mesh.triangles = new int[]
            {
                0, 1, 2,
                2, 3, 0
            };
            mesh.uv = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1),
            };
            var transform = ProjectionToWorldMatrix(camera);
            return (mesh, transform);
        }

        /// <summary>
        /// Generate quad with vertices from (-0.5, -0.5, 0) to (0.5, 0.5, 0)
        /// </summary>
        /// <returns></returns>
        public static Mesh GenerateQuadXY()
        {
            var mesh = new Mesh();
            mesh.vertices = new[]
            {
                new Vector3(-.5f, -.5f, 0),
                new Vector3(.5f, -.5f, 0),
                new Vector3(.5f, .5f, 0),
                new Vector3(-.5f, .5f, 0),
            };
            mesh.triangles = new int[]
            {
                0, 2, 1,
                0, 3, 2,
            };
            mesh.uv = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1),
            };
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh _fullNDCQuad;

        public static Mesh GetFullNDCQuad()
        {
            if (_fullNDCQuad)
                return _fullNDCQuad;
            _fullNDCQuad = new Mesh();
            _fullNDCQuad.vertices = new[]
            {
                new Vector3(-1, -1, 0),
                new Vector3(1, -1, 0),
                new Vector3(1, 1, 0),
                new Vector3(-1, 1, 0),
            };
            _fullNDCQuad.triangles = new int[]
            {
                0, 2, 1,
                0, 3, 2,
            };
            _fullNDCQuad.uv = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1),
            };
            _fullNDCQuad.RecalculateBounds();
            _fullNDCQuad.RecalculateNormals();
            _fullNDCQuad.RecalculateTangents();
            _fullNDCQuad.UploadMeshData(true);
            return _fullNDCQuad;
        }

        public static void DebugDrawTriangle(Vector2 a, Vector2 b, Vector2 c, Color color)
        {
            Debug.DrawLine(a, b, color);
            Debug.DrawLine(b, c, color);
            Debug.DrawLine(c, a, color);
        }

        public static void DebugDrawTriangle(Vector2 a, Vector2 b, Vector2 c)
            => DebugDrawTriangle(a, b, c, Color.white);

        public static void DebugDrawPolygon(IEnumerable<Vector2> verts, Color color)
        {
            var first = verts.First();
            var previous = first;
            foreach (var vert in verts)
            {
                Debug.DrawLine(previous, vert, color);
                previous = vert;
            }

            Debug.DrawLine(previous, first, color);
        }

        public static void DebugDrawSector(Vector2 center, float radius, float fromRad, float toRad, Color color,
            int subdivide = 12)
        {
            var previousPoint = center;
            for (var i = 0; i <= subdivide; i++)
            {
                var t = (float) i / subdivide;
                var ang = Mathf.Lerp(fromRad, toRad, t);
                var p = new Vector2(
                    Mathf.Cos(ang) * radius,
                    Mathf.Sin(ang) * radius);
                p += center;

                Debug.DrawLine(previousPoint, p, color);
                previousPoint = p;
            }

            Debug.DrawLine(previousPoint, center, color);
        }

        #endregion Rendering

        #region Misc

        public static Color RandomColor()
        {
            return new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
        }

        public enum GenericPlatform
        {
            Desktop,
            Mobile,
        }

        public static GenericPlatform GetGenericPlatform(RuntimePlatform platform)
        {
            if (platform == RuntimePlatform.WindowsEditor ||
                platform == RuntimePlatform.WindowsPlayer ||
                platform == RuntimePlatform.LinuxEditor ||
                platform == RuntimePlatform.LinuxPlayer ||
                platform == RuntimePlatform.OSXEditor ||
                platform == RuntimePlatform.OSXPlayer)
                return GenericPlatform.Desktop;
            return GenericPlatform.Mobile;
        }

        public static IEnumerable<T> FindObjectOfAll<T>() where T : UnityEngine.Component
        {
            return Resources.FindObjectsOfTypeAll<T>()
                .Where(obj => obj.gameObject.scene != null && obj.gameObject.scene.isLoaded);
        }

        public static bool IsInHierarchy(this GameObject gameObject)
        {
            return gameObject && gameObject.scene != null && gameObject.scene.name != null;
        }

        public static Color SetAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        #endregion Misc

        #endregion UnityHelper

        #region Misc

        public static bool NotNull(this object obj) => !(obj is null);

        // Copy from https://stackoverflow.com/questions/20156/is-there-an-easy-way-to-create-ordinals-in-c
        public static string ToOrdinal(this int num)
        {
            if (num <= 0) return num.ToString();

            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }

            switch (num % 10)
            {
                case 1:
                    return num + "st";
                case 2:
                    return num + "nd";
                case 3:
                    return num + "rd";
                default:
                    return num + "th";
            }
        }

        public class OffsetArray<T> : IList<T>
        {
            private T[] _array;
            private int _zeroIndexOffset;


            public IEnumerable<int> Indices
            {
                get
                {
                    for (var i = 0; i < Count; i++)
                    {
                        yield return i + _zeroIndexOffset;
                    }
                }
            }

            public int StartIndex => _zeroIndexOffset;

            public T this[int index]
            {
                get => _array[index - _zeroIndexOffset];
                set => _array[index - _zeroIndexOffset] = value;
            }


            public OffsetArray(int startIndex, int size)
            {
                _zeroIndexOffset = startIndex;
                _array = new T[size];
            }

            public IEnumerator<T> GetEnumerator()
            {
                for (var i = 0; i < Count; i++)
                {
                    var idx = _zeroIndexOffset + i;
                    idx = idx >= Count
                        ? idx - Count
                        : idx;
                    yield return _array[idx];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            void ICollection<T>.Add(T item)
            {
                throw new NotSupportedException();
            }

            void ICollection<T>.Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(T item)
            {
                return _array.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                throw new NotImplementedException();
                // _array.CopyTo(array, arrayIndex);
            }

            bool ICollection<T>.Remove(T item)
            {
                throw new NotSupportedException();
            }

            public int Count => _array.Length;

            public bool IsReadOnly => _array.IsReadOnly;

            public int IndexOf(T item)
            {
                return _array.IndexOf(item) + _zeroIndexOffset;
            }

            void IList<T>.Insert(int index, T item)
            {
                throw new NotSupportedException();
            }

            void IList<T>.RemoveAt(int index)
            {
                throw new NotSupportedException();
            }
        }

        #endregion
    }
}