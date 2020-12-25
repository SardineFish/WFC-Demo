using System;
using System.Collections;
using System.Collections.Generic;
using SardineFish.Utils;
using UnityEngine;
using Random = System.Random;

namespace WFC
{
    public class WFCGenerator<T>
    {
        private const float EntropyBias = 0.01f;
        public Vector3Int Size { get; private set; }
        public readonly HashSet<Pattern<T>> Patterns = new HashSet<Pattern<T>>();

        public ChunkState<Pattern<T>>[,,] ChunkStates;

        private Random _random;
        private readonly Stack<Vector3Int> _propagationStack = new Stack<Vector3Int>();

        public Vector3Int[] NeighborOffset { get; private set; }

        // private static readonly Vector2Int[] AdjacentDelta = new[]
        // {
        //     Vector2Int.right,
        //     Vector2Int.up,
        //     Vector2Int.left,
        //     Vector2Int.down,
        // };

        public WFCGenerator(Vector3Int size, Vector3Int[] neighborOffset, IEnumerable<Pattern<T>> patterns)
        {
            Size = size;
            Patterns = new HashSet<Pattern<T>>(patterns);
            ChunkStates = new ChunkState<Pattern<T>>[size.x, size.y, size.z];
            NeighborOffset = neighborOffset;
            _random = new Random();
        }

        public void Resize(Vector3Int size)
        {
            Size = size;
            ChunkStates = new ChunkState<Pattern<T>>[size.x, size.y, size.z];
        }

        public void Reset(int seed)
        {
            _propagationStack.Clear();
            _random = new Random(seed);
            
            for (var x = 0; x < Size.x; x++)
            for (var y = 0; y < Size.y; y++)
            for (var z = 0; z < Size.z; z++)
            {
                ChunkStates[x, y, z] = new ChunkState<Pattern<T>>(Patterns, NeighborOffset.Length);
            }
        }

        public void SetBoundaryPattern(Pattern<T> boundPattern)
        {
            var bounds = new BoundsInt(Vector3Int.zero, Size);
            for (var idx = 0; idx < NeighborOffset.Length; idx++)
            {
                var offset = NeighborOffset[idx];
                var contraryIdx = NeighborOffset.IndexOf(-offset);
                foreach (var pos in bounds.BoundaryIter(offset))
                {
                    var chunk = ChunkStates[pos.x, pos.y, pos.z];
                    var count = chunk.Compatibles.Count;
                    // chunk.Compatibles.RemoveWhere(p => !p.Neighbors[idx].Contains(boundPattern));
                    chunk.UpdateCompatibleFrom(boundPattern.Neighbors[contraryIdx]);
                    
                    if(count != chunk.Compatibles.Count)
                        _propagationStack.Push(pos);
                }
            }
            
            CoroutineRunner.Run(PropagateProgressive().GetEnumerator());
        }

        public IEnumerable<Vector3Int> RunProgressive()
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

        bool Observe(out Vector3Int chunkPos)
        {
            var minChunk = -Vector3Int.one;
            var minEntropy = float.MaxValue;
            
            for (var x = 0; x < Size.x; x++)
            for (var y = 0; y < Size.y; y++)
            for (var z = 0; z < Size.z; z++)
            {
                var chunk = ChunkStates[x, y, z];
                if (chunk.Definite)
                    continue;

                // Add a little random bias to pick randomly with multiple min entropy chunk
                float bias = EntropyBias * (float) _random.NextDouble();
                if (chunk.Entropy + bias < minEntropy)
                {
                    minChunk = new Vector3Int(x, y, z);
                    minEntropy = chunk.Entropy + bias;
                }
            }

            chunkPos = default;
            if (minChunk.x < 0)
                return false;

            var observeChunk = ChunkStates[minChunk.x, minChunk.y, minChunk.z];
            var pattern = observeChunk.Compatibles
                .RandomTake((float) _random.NextDouble(), ptn => ptn.Weight);
            observeChunk.CollapseTo(pattern);

            ChunkStates[minChunk.x, minChunk.y, minChunk.z] = observeChunk;
            _propagationStack.Push(minChunk);

            chunkPos = minChunk;
            return true;
        }

        IEnumerable PropagateProgressive()
        {
            while (_propagationStack.Count > 0)
            {
                var changedPos = _propagationStack.Pop();
                var chunk = ChunkStates[changedPos.x, changedPos.y, changedPos.z];

                for(var dir = 0; dir < NeighborOffset.Length; dir++)
                {
                    var delta = NeighborOffset[dir];
                    var adjacentPos = changedPos + delta;
                    if (!Size.Contains(adjacentPos))
                        continue;

                    var adjacent = ChunkStates[adjacentPos.x, adjacentPos.y, adjacentPos.z];
                    if (adjacent.UpdateCompatibleFrom(chunk.CompatibleAdjacent[dir]))
                    {
                        _propagationStack.Push(adjacentPos);
                        ChunkStates[adjacentPos.x, adjacentPos.y, adjacentPos.z] = adjacent;
                    }
                }

                yield return null;
            }
            
        }

        
    }
}