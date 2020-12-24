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
    public class Pattern<T> : IPattern<Pattern<T>>
    {
        public T Chunk { get; }

        public readonly HashSet<Pattern<T>>[] Neighbors;
            // = new[]
            // {
            //     new HashSet<Pattern<T>>(),
            //     new HashSet<Pattern<T>>(),
            //     new HashSet<Pattern<T>>(),
            //     new HashSet<Pattern<T>>(),
            // };

        // public HashSet<Pattern<T>> Up => Neighbors[(int) Orientation2D.Up];
        //
        // public HashSet<Pattern<T>> Left => Neighbors[(int) Orientation2D.Left];
        //
        // public HashSet<Pattern<T>> Right => Neighbors[(int) Orientation2D.Right];
        //
        // public HashSet<Pattern<T>> Down => Neighbors[(int) Orientation2D.Down];

        public Pattern(T chunkData, int adjacentCount)
        {
            Chunk = chunkData;
            Neighbors = new HashSet<Pattern<T>>[adjacentCount];
            for (var i = 0; i < adjacentCount; i++)
            {
                Neighbors[i] = new HashSet<Pattern<T>>();
            }
        }

        public float Weight => 1;
        public IEnumerable<Pattern<T>> GetAdjacent(int i) => Neighbors[i];
    }
}