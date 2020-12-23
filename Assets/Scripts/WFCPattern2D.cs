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
    public class WFCPattern2D<T>
    {
        public T Chunk;
        public WFCPattern2D<T>[] Neighbors = new WFCPattern2D<T>[4];
        public WFCPattern2D<T> Up => Neighbors[(int)Orientation2D.Up];
        public WFCPattern2D<T> Left => Neighbors[(int) Orientation2D.Left];
        public WFCPattern2D<T> Right => Neighbors[(int) Orientation2D.Right];
        public WFCPattern2D<T> Down => Neighbors[(int) Orientation2D.Down];
    }
}