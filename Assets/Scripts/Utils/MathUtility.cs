using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace SardineFish.Utils
{
    public static class MathUtility
    {
        public static Vector3 ToVector3(this Vector4 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }
        public static Vector3 ToVector3(this Vector2 v)
        {
            return new Vector3(v.x, v.y, 0);
        }
        public static Vector3 ToVector3(this Vector2 v, float z)
        {
            return new Vector3(v.x, v.y, z);
        }
        public static Vector3 ToVector3XZ(this Vector2 v)
        {
            return new Vector3(v.x, 0, v.y);
        }

        public static Rect Scale(this Rect rect, Vector2 rate)
        {
            return new Rect()
            {
                min = rect.min * rate,
                max = rect.max * rate
            };
        }

        public static Rect ToRect(this Bounds bounds)
        {
            var rect = new Rect();
            rect.min = bounds.min;
            rect.max = bounds.max;
            return rect;
        }

        public static float Frac(float x)
            => x - Mathf.Floor(x);

        public static Vector2 Frac(Vector2 v)
            => new Vector2(Frac(v.x), Frac(v.y));
        public static Vector3 ClipY(this Vector3 v)
        {
            return Vector3.Scale(v, new Vector3(1, 0, 1));
        }
        public static Vector3 Set(this Vector3 v, float x=float.NaN, float y=float.NaN, float z = float.NaN)
        {
            x = float.IsNaN(x) ? v.x : x;
            y = float.IsNaN(y) ? v.y : y;
            z = float.IsNaN(z) ? v.z : z;
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// z = x - y
        /// Guarantee that z will not be different sign with x
        /// i.e. positive number will never subtract to nagtive number 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static float SubtractToZero(this float x, float y)
        {
            var z = x - y;
            if (SignInt(z) != SignInt(x))
                return 0;
            return z;
        }

        public static Vector2 ToVector2(this Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }

        public static Vector2 ToVector2XZ(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }

        public static Vector2 ToVector2(this Vector4 v)
            => new Vector2(v.x, v.y);
        public static Vector3Int ToVector3Int(this Vector2Int v, int z = 0)
            => new Vector3Int(v.x, v.y, z);
        public static Vector2Int ToVector2Int(this Vector3Int v)
            => new Vector2Int(v.x, v.y);
        public static Vector2Int ToVector2Int(this Vector2 v)
            => new Vector2Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y));
        public static Vector2Int RoundToVector2Int(Vector2 v)
            => new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));

        public static Vector3Int RoundToVector3Int(Vector3 v)
            => new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
    
        public static Vector3Int ToVector3Int(this Vector2 v, int z = 0)
            => new Vector3Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y), z);
        public static Vector3 ToVector3(this Vector2Int v, float z = 0)
            => new Vector3(v.x, v.y, z);
        public static Vector2 ToVector2(this Vector3Int v)
            => new Vector2(v.x, v.y);
        public static Vector2Int ToVector2Int(this Vector3 v)
            => new Vector2Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y));
        public static Vector2 Abs(Vector2 v) 
            => new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));

        public static Vector3 Abs(Vector3 v)
            => new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));

        /// <summary>
        /// return one of two number, which has minimal abs value. if equal, a is return.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float MinAbs(float a, float b)
            => Mathf.Abs(a) <= Mathf.Abs(b) ? a : b;

        /// <summary>
        /// return one of two number, which has maximal abs value. if equal, a is return.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float MaxAbs(float a, float b)
            => Mathf.Abs(a) >= Mathf.Abs(b) ? a : b;

        /// <summary>
        /// min(|a|, |b|), result always >= 0
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float AbsMin(float a, float b)
            => Mathf.Min(Mathf.Abs(a), Mathf.Abs(b));

        /// <summary>
        /// max(|a|, |b|), result always >= 0
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float AbsMax(float a, float b)
            => Mathf.Max(Mathf.Abs(a), Mathf.Abs(b));

        public static Vector2 Floor(Vector2 v)
            => new Vector2(Mathf.Floor(v.x), Mathf.Floor(v.y));
        public static Vector2Int FloorToInt(Vector2 v)
            => new Vector2Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y));

        public static Vector2 Ceil(Vector2 v)
            => new Vector2(Mathf.Ceil(v.x), Mathf.Ceil(v.y));
        public static Vector2Int CeilToInt(Vector2 v)
            => new Vector2Int(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.y));

        public static Vector2 Min(Vector2 a, Vector2 b)
            => new Vector2(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y));

        public static Vector2Int Min(Vector2Int a, Vector2Int b)
            => new Vector2Int(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y));

        public static Vector2 Max(Vector2 a, Vector2 b)
            => new Vector2(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));

        public static Vector2Int Max(Vector2Int a, Vector2Int b)
            => new Vector2Int(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));

        public static float MaxOf(Vector2 v)
            => Mathf.Max(v.x, v.y);

        public static bool Contains(this RectInt rect, Vector2 pos)
        {
            if (pos.x >= rect.xMin && pos.y >= rect.yMin && pos.x < rect.xMax && pos.y < rect.yMax)
                return true;
            return false;
        }

        public static Rect Shrink(this Rect rect, float width)
        {
            var newRect = rect;
            newRect.size -= width * 2 * Vector2.one;
            newRect.center = rect.center;
            return newRect;
        }

        public static Rect Shrink(this Rect rect, Vector2 offset)
        {
            var newRect = rect;
            rect.size -= offset * 2;
            newRect.center = rect.center;
            return newRect;
        }

        public static Rect Shrink(this Rect rect, RectOffset offset, bool zeroTop = false)
        {
            var newRect = rect;
            newRect.xMin += offset.left;
            newRect.xMax -= offset.right;
            if (zeroTop)
            {
                newRect.yMin += offset.top;
                newRect.yMax -= offset.bottom;
            }
            else
            {
                newRect.yMin += offset.bottom;
                newRect.yMax -= offset.top;
            }

            return newRect;
        }

        public static bool ContainsIndex<T>(this T[,] array, Vector2Int idx)
        {
            return 0 <= idx.x
                   && idx.x < array.GetLength(0)
                   && 0 <= idx.y
                   && idx.y < array.GetLength(1);
        }

        public static Color Transparent(this Color color)
            => new Color(color.r, color.g, color.b, 0);

        public static Color WithAlpha(this Color color, float alpha)
            => new Color(color.r, color.g, color.b, alpha);

        /// <summary>
        /// Cross product following right hand rule.
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static float Cross2(Vector2 u, Vector2 v)
            => u.x * v.y - u.y * v.x;

        public static float RangeMapClamped(float inA, float inB, float outA, float outB, float t)
        {
            t = (t - inA) / (inB - inA);
            t = Mathf.Clamp01(t);
            return Mathf.Lerp(outA, outB, t);
        }

        public static T ListMapClamped<T>(IList<T> list, float inA, float inB, float value)
        {
            var index = Mathf.FloorToInt((value - inA) / (inB - inA) * list.Count) % list.Count;
            return list[index];
        }

        public static T ListRangeMap<T>(float[] inputList, T[] outputList, float value)
        {
            for (var i = 1; i < inputList.Length; i++)
            {
                if (i > outputList.Length)
                    return outputList[outputList.Length - 1];
                if (inputList[i] > value)
                    return outputList[i];
            }

            return outputList[outputList.Length - 1];
        }

        public static Vector3 QuadraticBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            return p1 + (1 - t) * (1 - t) * (p0 - p1) + t * t * (p2 - p1);
        }
    

        public static Color Set(this Color color, float r = float.NaN, float g=float.NaN, float b=float.NaN, float a = float.NaN)
        {
            color.r = float.IsNaN(r) ? color.r : r;
            color.g = float.IsNaN(g) ? color.g : g;
            color.b = float.IsNaN(b) ? color.b : b;
            color.a = float.IsNaN(a) ? color.a : a;
            return color;
        }

        public static int SignInt(float x)
        {
            if (x > 0)
                return 1;
            else if (x < 0)
                return -1;
            return 0;
        }

        public static int SignInt(int x)
        {
            if (x > 0)
                return 1;
            else if (x < 0)
                return -1;
            return 0;
        }

        public static float MapAngle(float ang)
        {
            if (ang > 180)
                ang -= 360;
            else if (ang < -180)
                ang += 360;
            return ang;
        }

        public static Quaternion QuaternionBetweenVector(Vector3 u, Vector3 v)
        {
            u = u.normalized;
            v = v.normalized;
            var cosOfAng = Vector3.Dot(u, v);
            var halfCos = Mathf.Sqrt(0.5f * (1.0f + cosOfAng));
            var halfSin = Mathf.Sqrt(0.5f * (1.0f - cosOfAng));
            var axis = Vector3.Cross(u, v);
            var quaternion = new Quaternion(halfSin * axis.x, halfSin * axis.y, halfSin * axis.z, halfCos);
            return quaternion;
        }

        public static float ToAng(float y,float x)
        {
            return Mathf.Atan2(y, x) * Mathf.Rad2Deg;
        }

        public static float ToAng(Vector2 v)
        {
            return Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        }

        public static Vector2 Rotate(Vector2 v, float rad)
        {
            var rx = Mathf.Cos(rad);
            var ry = Mathf.Sin(rad);
            // (vx + vy i) * (rx + ry i)
            return new Vector2(rx * v.x - ry * v.y, rx * v.y + ry * v.x);
        }

        public static bool LineIntersect(Vector2 o1, Vector2 d1, Vector2 o2, Vector2 d2, out Vector2 point)
        {
            var div = (d1.y * d2.x - d1.x * d2.y);
            point = Vector2.zero;
            if (Mathf.Abs(div) < 0.001f)
                return false;

            var t1 = -(-d2.y * o1.x + d2.x * o1.y + d2.y * o2.x - d2.x * o2.y) / div;

            point = o1 + d1 * t1;
        
            return true;
        }
    
        public static (bool hit, float distance, Vector2 normal) BoxRaycast(Rect box, Vector2 center, Vector2 direction)
        {
            direction = direction.normalized;
            Vector2 tMin, tMax;
            if (direction.x == 0 && direction.y == 0)
                return (false, 0, Vector2.zero);
            else if (direction.x == 0)
            {
                tMin.y = (box.yMin - center.y) / direction.y;
                tMax.y = (box.yMax - center.y) / direction.y;
                tMin.x = tMax.x = float.NegativeInfinity;
                if (box.xMin <= center.x && center.x <= box.xMax)
                {
                    if (tMin.y < tMax.y)
                        return (true, tMin.y, Vector2.down);
                    return (true, tMax.y, Vector2.up);
                }
                return (false, 0, Vector2.zero);
            }
            else if (direction.y == 0)
            {
                tMin.x = (box.xMin - center.x) / direction.x;
                tMax.x = (box.xMax - center.x) / direction.x;
                tMin.y = tMax.y = float.NegativeInfinity;

                if (box.yMin <= center.y && center.y <= box.yMax)
                {
                    if (tMin.x < tMax.x)
                        return (true, tMin.x, Vector2.left);
                    return (true, tMax.x, Vector2.right);
                }
                return (false, 0, Vector2.zero);
            }

            tMin = (box.min - center) / direction; // distance to box min lines (X and Y)
            tMax = (box.max - center) / direction; // distance to box max lines (X and Y)

            var minXT = tMin.x; // min distance to vertical line
            var maxXT = tMax.x; // max distance to vertical line
            var minXNormal = Vector2.left; // normal of the vertical line which has minimal distance to center
            var minYT = tMin.y;
            var maxYT = tMax.y;
            var minYNormal = Vector2.down;

            if (tMin.x > tMax.x)
            {
                minXT = tMax.x;
                maxXT = tMin.x;
                minXNormal = Vector2.right;
            }
            if (tMin.y > tMax.y)
            {
                minYT = tMax.y;
                maxYT = tMin.y;
                minYNormal = Vector2.up;
            }

            if (minYT > maxXT || minXT > maxYT)
            {
                return (false, 0, Vector2.zero);
            }
            else if (minXT > minYT)
            {
                return (true, minXT, minXNormal);
            }
            return (true, minYT, minYNormal);

        }

        public static Vector2 Rad2Vec2(float rad)
        {
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }

        public static float DistanceToSegment(Vector3 a, Vector3 b, Vector3 point)
        {
            var v = (b - a).normalized;
            var length = Vector3.Distance(a, b);
            if (
                Vector3.Dot(point - a, v) is var l && 0 <= l && l <= length
                && Vector3.Dot(point - b, -v) is var m && 0 <= m && m <= length)
            {
                return Mathf.Abs(Vector3.Cross(point - a, v).magnitude);
            }
            else
            {
                return Mathf.Min(Vector3.Distance(a, point), Vector3.Distance(b, point));
            }
        }

        /// <summary>
        /// Get a normal vector perpendicular to given vector v.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector2 NormalVector(this Vector2 v)
        {
            return Vector3.Cross(v, Vector3.back).normalized;
        }
    
    }
}