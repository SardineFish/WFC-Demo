using UnityEngine;

namespace WFC
{
    public struct Chunk2D<T>
    {
        public T Data;
        public Vector2Int Position;
        public int Orientation;
    }
}