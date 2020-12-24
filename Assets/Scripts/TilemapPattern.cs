using System;
using System.Collections;
using System.Collections.Generic;
using SardineFish.Utils;
using UnityEngine;
using UnityEngine.Tilemaps;
using SardineFish.Utils;

namespace WFC
{
    [RequireComponent(typeof(Tilemap))]
    public class TilemapPattern : MonoBehaviour, ICustomEditorEX
    {
        public bool IncludeCorner = false;
        private readonly Dictionary<TileBase, Pattern<TileBase>> _patterns =
            new Dictionary<TileBase, Pattern<TileBase>>();

        private static readonly Vector3Int[] Adjacent = new[]
        {
            Vector3Int.right,
            Vector3Int.up,
            Vector3Int.left,
            Vector3Int.down,
        };

        private static readonly Vector3Int[] AdjacentWithCorner = new[]
        {
            Vector3Int.right,
            Vector3Int.right + Vector3Int.up,
            Vector3Int.up,
            Vector3Int.up + Vector3Int.left,
            Vector3Int.left,
            Vector3Int.left + Vector3Int.down,
            Vector3Int.down,
            Vector3Int.down + Vector3Int.right,
        };

        public Vector3Int[] NeighborOffset => IncludeCorner ? AdjacentWithCorner : Adjacent;

        public IEnumerable<Pattern<TileBase>> Patterns => _patterns.Values;

        private Tilemap _tilemap;

        private void Awake()
        {
            _tilemap = GetComponent<Tilemap>();
            ExtractPatterns();
        }

        Pattern<TileBase> GetOrCreatePattern(TileBase tile)
        {
            
            if (_patterns.TryGetValue(tile, out var pattern))
                return pattern;
            Pattern<TileBase> newPattern =
                IncludeCorner 
                    ? new Pattern<TileBase>(tile, 8) 
                    : new Pattern<TileBase>(tile, 4);
            
            _patterns.Add(tile, newPattern);
            return newPattern;
        }
        

        [EditorButton]
        public void ExtractPatterns()
        {
            _patterns.Clear();
            var bounds = _tilemap.cellBounds;
            var up = Vector2Int.up.ToVector3Int();
            var left = Vector2Int.left.ToVector3Int();
            var right = Vector2Int.right.ToVector3Int();
            var down = Vector2Int.down.ToVector3Int();
            
            foreach (var pos in bounds.Iter())
            {
                var tile = _tilemap.GetTile(pos);
                if (!tile)
                    continue;

                var pattern = GetOrCreatePattern(tile);

                for (var idx = 0; idx < NeighborOffset.Length; idx++)
                {
                    if (_tilemap.GetTile(pos + NeighborOffset[idx]) is TileBase adjacentTile)
                        pattern.Neighbors[idx].Add(GetOrCreatePattern(adjacentTile));
                }
            }
            
            
        }
    }
}