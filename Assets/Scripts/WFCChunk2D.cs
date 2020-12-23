using UnityEngine;

namespace WFC
{
    public struct WFCChunk2D<T>
    {
        public T Data;
        public Vector2Int Position;
        public int Orientation;
    }
}