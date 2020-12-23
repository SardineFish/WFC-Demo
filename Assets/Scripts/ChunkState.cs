using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WFC
{
    public struct ChunkState<TPattern> where  TPattern : IPattern<TPattern>
    {
        public HashSet<TPattern> Compatibles;
        public HashSet<TPattern>[] CompatibleAdjacent;
        public float Entropy;
        public bool Definite;

        private TPattern _pattern;
        /// <summary>
        /// Get collapsed pattern
        /// </summary>
        /// <exception cref="Exception"></exception>
        public TPattern Pattern
        {
            get
            {
                if (!Definite)
                    throw new Exception("Chunk not determined.");
                return _pattern;
            }
        }

        /// <summary>
        /// Construct the chunk state with all possible patterns.
        /// </summary>
        /// <param name="patterns"></param>
        public ChunkState(IEnumerable<TPattern> patterns, int adjacentCount)
        {
            Definite = false;
            Compatibles = new HashSet<TPattern>(patterns);
            CompatibleAdjacent = new HashSet<TPattern>[adjacentCount];

            for (var i = 0; i < adjacentCount; i++)
                CompatibleAdjacent[i] = new HashSet<TPattern>();

            Entropy = 0;
            _pattern = default;
            
            UpdateEntropy();
            UpdateAdjacent();
        }

        /// <summary>
        /// Remove a possible pattern in this chunk and update the entropy.
        /// The return value indicate whether this chunk has any changes.
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public bool Ban(TPattern pattern)
        {
            if (Compatibles.Remove(pattern))
            {
                UpdateEntropy();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Update the compatible patterns of this chunk from given patterns set.
        /// The return value indicate whether compatible patterns has any changes.
        /// Will also update compatible adjacent if necessary.
        /// </summary>
        /// <param name="patterns"></param>
        /// <returns></returns>
        public bool UpdateCompatibleFrom(IEnumerable<TPattern> patterns)
        {
            if (!Compatibles.IsSubsetOf(patterns))
            {
                Compatibles.IntersectWith(patterns);
                UpdateAdjacent();
                UpdateEntropy();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Collapse this chunk to specific pattern, the entropy will set to zero 
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public void CollapseTo(TPattern pattern)
        {
            Compatibles.Clear();
            Compatibles.Add(pattern);
            _pattern = pattern;
            Entropy = 0;
            Definite = true;
            UpdateAdjacent();
        }

        /// <summary>
        /// Force update the compatible adjacent pattern of this chunk.
        /// </summary>
        void UpdateAdjacent()
        {
            for (var i = 0; i < CompatibleAdjacent.Length; i++)
            {
                CompatibleAdjacent[i].Clear();
                foreach (var pattern in Compatibles)
                {
                    CompatibleAdjacent[i].UnionWith(pattern.GetAdjacent(i));
                }
            }
        }

        /// <summary>
        /// Force update the entropy of this chunk.
        /// </summary>
        void UpdateEntropy()
        {
            var sumOfWeight = Compatibles.Sum(pattern => pattern.Weight);
            var sumOfWeightLogWeight = Compatibles.Sum(pattern => pattern.Weight * Mathf.Log(pattern.Weight));
            Entropy = Mathf.Log(sumOfWeight) - sumOfWeightLogWeight / sumOfWeight;
        }
    }
}