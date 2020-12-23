using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace WFC
{
    [RequireComponent(typeof(Tilemap))]
    public class TilemapPattern : MonoBehaviour
    {
        public readonly List<WFCPattern2D<TileBase>> Patterns = new List<WFCPattern2D<TileBase>>();

        private Tilemap _tilemap;
        
        private void Awake()
        {
            ExtractPatterns();
            
            _tilemap = GetComponent<Tilemap>();
        }

        public void ExtractPatterns()
        {
            
        }
    }
}