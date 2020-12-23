using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SardineFish.Utils;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace WFC
{
    [RequireComponent(typeof(Tilemap))]
    public class WFCTilemapGenerator : MonoBehaviour, ICustomEditorEX
    {
        public int Seed;
        public TilemapPattern TilemapPattern;
        public BoundsInt Bounds;
        private Tilemap _tilemap;
        private WFCGenerator2D<TileBase> _generator;

        private CoroutineRunner CoroutineRunner;

        private void Awake()
        {
            _tilemap = GetComponent<Tilemap>();
        }

        [EditorButton]
        public void Generate()
        {
            if (!TilemapPattern)
                return;

            RandomSeed();
            StartCoroutine(GenerateProgressive());
            // CoroutineRunner = new CoroutineRunner(GenerateProgressive());
            
        }

        [EditorButton]
        public void Step()
        {
            if(CoroutineRunner is null)
                return;

            CoroutineRunner.Tick();
        }
        

        IEnumerator GenerateProgressive()
        {
            _tilemap.ClearAllTiles();
            TilemapPattern.ExtractPatterns();
            _generator = new WFCGenerator2D<TileBase>(Bounds.size.ToVector2Int(), TilemapPattern.Patterns);
            _generator.Reset(Seed);
            // gameObject.ClearChildren();
            List<Tilemap> waves = GetComponentsInChildren<Tilemap>().ToList();
            waves.Remove(_tilemap);

            for (var i = waves.Count; i < _generator.Patterns.Count; i++)
            {
                var obj = new GameObject();
                obj.transform.parent = transform;
                obj.transform.position = transform.position + Vector3.forward * (i + 1);
                var tilemap = obj.AddComponent<Tilemap>();
                var renderer = obj.AddComponent<TilemapRenderer>();
                tilemap.color = Color.white.WithAlpha(0.7f);
                waves.Add(tilemap);
            }
            for (var x = 0; x < _generator.Size.x; x++)
            for (var y = 0; y < _generator.Size.y; y++)
            {
                var p = Bounds.min + new Vector3Int(x, y, 0);
                var idx = 0;
                foreach (var pattern in _generator.ChunkStates[x, y].Compatibles)
                {
                    waves[idx++].SetTile(p, pattern.Chunk);
                }
                                        
            }

            yield return null;
            
            foreach (var collapsedChunk in _generator.RunProgressive())
            {
                var pos = Bounds.min + collapsedChunk.ToVector3Int();
                var tile = _generator.ChunkStates[collapsedChunk.x, collapsedChunk.y].Pattern.Chunk;
                _tilemap.SetTile(pos, tile);
                
                foreach(var tilemap in waves)
                    tilemap.ClearAllTiles();
                
                for(var x = 0;x<_generator.Size.x;x++)
                for (var y = 0; y < _generator.Size.y; y++)
                {
                    var p = Bounds.min + new Vector3Int(x, y, 0);
                    var idx = 0;
                    foreach (var pattern in _generator.ChunkStates[x, y].Compatibles)
                    {
                        waves[idx++].SetTile(p, pattern.Chunk);
                    }
                                        
                }
                
                yield return null;
            }
        }

        [EditorButton()]
        public void RandomSeed()
        {
            Seed = new System.Random().Next();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position + Bounds.center, Bounds.size);
        }
    }
}