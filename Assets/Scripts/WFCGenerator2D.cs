using System;
using System.Collections;
using System.Collections.Generic;
using SardineFish.Utils;
using UnityEngine;
using Random = System.Random;

namespace WFC
{
    public class WFCGenerator2D<T>
    {
        private const float EntropyBias = 0.01f;
        public Vector2Int Size { get; private set; }
        public readonly HashSet<Pattern2D<T>> Patterns = new HashSet<Pattern2D<T>>();

        public ChunkState<Pattern2D<T>>[,] ChunkStates;

        private Random _random;
        private readonly Stack<Vector2Int> _propagationStack = new Stack<Vector2Int>();

        private static readonly Vector2Int[] AdjacentDelta = new[]
        {
            Vector2Int.right,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.down,
        };

        public WFCGenerator2D(Vector2Int size, IEnumerable<Pattern2D<T>> patterns)
        {
            Size = size;
            Patterns = new HashSet<Pattern2D<T>>(patterns);
            ChunkStates = new ChunkState<Pattern2D<T>>[size.x, size.y];
            _random = new Random();
        }

        public void Resize(Vector2Int size)
        {
            Size = size;
            ChunkStates = new ChunkState<Pattern2D<T>>[size.x, size.y];
        }

        public void Reset(int seed)
        {
            _propagationStack.Clear();
            _random = new Random(seed);
            
            for (var x = 0; x < Size.x; x++)
            for (var y = 0; y < Size.y; y++)
            {
                ChunkStates[x, y] = new ChunkState<Pattern2D<T>>(Patterns, 4);
            }
        }

        public IEnumerable<Vector2Int> RunProgressive()
        {
            while (true)
            {
                if (!Observe(out var chunkPos))
                    yield break;

                // PropagateProgressive();
                CoroutineRunner.Run(PropagateProgressive().GetEnumerator());
                // foreach (var t in PropagateProgressive())
                // {
                //     yield return chunkPos;
                // }

                yield return chunkPos;
            }
        }

        bool Observe(out Vector2Int chunkPos)
        {
            var minChunk = -Vector2Int.one;
            var minEntropy = float.MaxValue;
            
            for (var x = 0; x < Size.x; x++)
            for (var y = 0; y < Size.y; y++)
            {
                var chunk = ChunkStates[x, y];
                if (chunk.Definite)
                    continue;

                // Add a little random bias to pick randomly with multiple min entropy chunk
                float bias = EntropyBias * (float)_random.NextDouble();
                if (chunk.Entropy + bias < minEntropy)
                {
                    minChunk = new Vector2Int(x, y);
                    minEntropy = chunk.Entropy + bias;
                }
            }

            chunkPos = default;
            if (minChunk.x < 0)
                return false;

            var observeChunk = ChunkStates[minChunk.x, minChunk.y];
            var pattern = observeChunk.Compatibles
                .RandomTake((float) _random.NextDouble(), ptn => ptn.Weight);
            observeChunk.CollapseTo(pattern);

            ChunkStates[minChunk.x, minChunk.y] = observeChunk;
            _propagationStack.Push(minChunk);

            chunkPos = minChunk;
            return true;
        }

        IEnumerable PropagateProgressive()
        {
            while (_propagationStack.Count > 0)
            {
                var changedPos = _propagationStack.Pop();
                var chunk = ChunkStates[changedPos.x, changedPos.y];

                for(var dir = 0; dir < 4; dir++)
                {
                    var delta = AdjacentDelta[dir];
                    var adjacentPos = changedPos + delta;
                    if (!Size.Contains(adjacentPos))
                        continue;

                    var adjacent = ChunkStates[adjacentPos.x, adjacentPos.y];
                    if (adjacent.UpdateCompatibleFrom(chunk.CompatibleAdjacent[dir]))
                    {
                        _propagationStack.Push(adjacentPos);
                        ChunkStates[adjacentPos.x, adjacentPos.y] = adjacent;
                    }
                }

                yield return null;
            }
            
        }

        bool UpdateChunk(Vector2Int pos)
        {
            var chunk = ChunkStates[pos.x, pos.y];
            var compatibles = ObjectPool<HashSet<Pattern2D<T>>>.Get();
            for (var dir = 0; dir < 4; dir++)
            {
                var delta = AdjacentDelta[dir];
                var adjacentPos = pos + delta;
                if (!Size.Contains(adjacentPos))
                    continue;
                var adjacent = ChunkStates[adjacentPos.x, adjacentPos.y];
                compatibles.UnionWith(adjacent.CompatibleAdjacent[(dir + 2) % 4]);
            }

            if (chunk.UpdateCompatibleFrom(compatibles))
            {
                ChunkStates[pos.x, pos.y] = chunk;
                compatibles.Clear();
                ObjectPool<HashSet<Pattern2D<T>>>.Release(compatibles);
                return true;
            }
            
            compatibles.Clear();
            ObjectPool<HashSet<Pattern2D<T>>>.Release(compatibles);
            return false;

        }
        
    }
}