using System;
using System.Collections;
using SardineFish.Utils;
using UnityEngine;
using WFC.Tilemap3D;

namespace WFC
{
    [RequireComponent(typeof(GameObjectTilemap))]
    public class WFCTilemap3DGenerator : MonoBehaviour, ICustomEditorEX
    {
        public int Seed;
        public BoundsInt Bounds;
        public Tilemap3DPattern PatternGenerator;
        
        private CoroutineRunner _coroutineRunner;
        private WFCGenerator<GameObjectTile> _generator;
        private GameObjectTilemap _tilemap;

        private void Awake()
        {
            _tilemap = GetComponent<GameObjectTilemap>();
        }

        [EditorButton]
        public void Random()
        {
            Seed = new System.Random().Next();
        }

        [EditorButton]
        public void Generate()
        {
            if(!PatternGenerator)
                return;
            _tilemap.ClearAllTiles();
            PatternGenerator.ExtractPatterns();
            _generator = new WFCGenerator<GameObjectTile>(Bounds.size, PatternGenerator.NeighborOffset, PatternGenerator.Patterns);
            
            StartCoroutine(RunProgressive());
        }

        IEnumerator RunProgressive()
        {
            _generator.Reset(Seed);

            foreach (var pos in _generator.RunProgressive())
            {
                var tile = _generator.ChunkStates[pos.x, pos.y, pos.z].Pattern.Chunk;
                
                _tilemap.SetTile(pos + Bounds.min, tile);

                yield return null;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Bounds.center, Bounds.size);
        }
    }
}