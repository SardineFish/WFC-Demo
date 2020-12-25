using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SardineFish.Utils;
using UnityEngine;
using WFC.Tilemap3D;

namespace WFC
{
    [RequireComponent(typeof(Tilemap3D.GameObjectTilemap))]
    public class Tilemap3DPattern : MonoBehaviour, ICustomEditorEX
    {
        public bool IncludeEmptyTile = true;
        public GameObjectTile BoundaryTile;
        private GameObjectTilemap _tilemap;

        private readonly Dictionary<GameObjectTile, Pattern<GameObjectTile>> _patterns =
            new Dictionary<GameObjectTile, Pattern<GameObjectTile>>();

        public List<Pattern<GameObjectTile>> Patterns { get; } = new List<Pattern<GameObjectTile>>();

        private Pattern<GameObjectTile> _boundaryPattern;

        public Pattern<GameObjectTile> BoundaryPattern
        {
            get
            {
                if (_boundaryPattern.NotNull())
                    return _boundaryPattern;
                if (!BoundaryTile)
                    return null;
                _boundaryPattern = new Pattern<GameObjectTile>(BoundaryTile, NeighborOffset.Length);
                return _boundaryPattern;
            }
            
        }

        [DisplayInInspector()]
        private int PatternCount => _patterns.Count;
        
        public Vector3Int[] NeighborOffset => AdjacentOffset;

        private static readonly Vector3Int[] AdjacentOffset = new[]
        {
            Vector3Int.left, Vector3Int.right,
            Vector3Int.down, Vector3Int.up,
            new Vector3Int(0, 0, -1), new Vector3Int(0, 0, 1),
        };

        private void Awake()
        {
            _tilemap = GetComponent<GameObjectTilemap>();
        }

        private Pattern<GameObjectTile> _emptyPattern;
        Pattern<GameObjectTile> GetOrCreateEmptyPattern()
        {
            if (_emptyPattern is null)
            {
                _emptyPattern = new Pattern<GameObjectTile>(null, NeighborOffset.Length);

                for (var idx = 0; idx < AdjacentOffset.Length; idx++)
                {
                    _emptyPattern.Neighbors[idx].Add(_emptyPattern);
                }
            }

            return _emptyPattern;
        }

        Pattern<GameObjectTile> GetOrCreatePattern(GameObjectTile tile)
        {
            if (_patterns.TryGetValue(tile.Prefab, out var pattern))
                return pattern;
            if (tile.Prefab == BoundaryTile)
                return BoundaryPattern;
            var weight = 1f;
            if (tile.Prefab.GetComponent<TileWeight>() is TileWeight tileWeight && tileWeight)
                weight = tileWeight.Weight;
            
            var newPattern = new Pattern<GameObjectTile>(tile.Prefab, NeighborOffset.Length, weight);
            _patterns.Add(tile.Prefab, newPattern);
            return newPattern;
        }

        public void ExtractPatterns()
        {
            if (!_tilemap)
                _tilemap = GetComponent<GameObjectTilemap>();
            _patterns.Clear();
            Patterns.Clear();

            foreach (var pos in _tilemap.Bounds.allPositionsWithin)
            {
                var tile = _tilemap.GetTile(pos);
                if(!tile)
                    continue;

                var pattern = GetOrCreatePattern(tile);

                for (var idx = 0; idx < NeighborOffset.Length; idx++)
                {
                    var neighborPos = pos + NeighborOffset[idx];
                    var neighborTile = _tilemap.GetTile(neighborPos);
                    if (!neighborTile)
                    {
                        if (IncludeEmptyTile)
                        {
                            var emptyPattern = GetOrCreateEmptyPattern();
                            pattern.Neighbors[idx].Add(emptyPattern);
                            emptyPattern.Neighbors[NeighborOffset.IndexOf(-NeighborOffset[idx])].Add(pattern);
                        }
                        continue;
                    }
                    var neighborPattern = GetOrCreatePattern(neighborTile);
                    pattern.Neighbors[idx].Add(neighborPattern);
                }
            }
            
            foreach(var pattern in _patterns.Values)
                Patterns.Add(pattern);
            if (IncludeEmptyTile)
                Patterns.Add(GetOrCreateEmptyPattern());
        }
    }
}