using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WFC
{
    public enum Orientation2D : int
    {
        Right = 0,
        Up = 1,
        Left = 2,
        Down = 3,
    }
    public class Pattern2D<T> : IPattern<Pattern2D<T>>
    {
        public T Chunk { get; }

        public readonly HashSet<Pattern2D<T>>[] Neighbors = new[]
        {
            new HashSet<Pattern2D<T>>(),
            new HashSet<Pattern2D<T>>(),
            new HashSet<Pattern2D<T>>(),
            new HashSet<Pattern2D<T>>(),
        };

        public HashSet<Pattern2D<T>> Up => Neighbors[(int) Orientation2D.Up];

        public HashSet<Pattern2D<T>> Left => Neighbors[(int) Orientation2D.Left];

        public HashSet<Pattern2D<T>> Right => Neighbors[(int) Orientation2D.Right];

        public HashSet<Pattern2D<T>> Down => Neighbors[(int) Orientation2D.Down];

        public Pattern2D(T chunkData)
        {
            Chunk = chunkData;
        }

        public float Weight => 1;
        public IEnumerable<Pattern2D<T>> GetAdjacent(int i) => Neighbors[i];
    }
}