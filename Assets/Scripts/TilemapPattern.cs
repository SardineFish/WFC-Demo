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
        private readonly Dictionary<TileBase, Pattern2D<TileBase>> _patterns =
            new Dictionary<TileBase, Pattern2D<TileBase>>();

        public IEnumerable<Pattern2D<TileBase>> Patterns => _patterns.Values;

        private Tilemap _tilemap;

        private void Awake()
        {
            _tilemap = GetComponent<Tilemap>();
            ExtractPatterns();
        }

        Pattern2D<TileBase> GetOrCreatePattern(TileBase tile)
        {
            
            if (_patterns.TryGetValue(tile, out var pattern))
                return pattern;
            var newPattern = new Pattern2D<TileBase>(tile);
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

                if (_tilemap.GetTile(pos + left) is TileBase leftTile)
                    pattern.Left.Add(GetOrCreatePattern(leftTile));
                if (_tilemap.GetTile(pos + right) is TileBase rightTile)
                    pattern.Right.Add(GetOrCreatePattern(rightTile));
                if (_tilemap.GetTile(pos + up) is TileBase upTile)
                    pattern.Up.Add(GetOrCreatePattern(upTile));
                if (_tilemap.GetTile(pos + down) is TileBase downTile)
                    pattern.Down.Add(GetOrCreatePattern(downTile));
            }
            
            
        }
    }
}