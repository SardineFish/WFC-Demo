using System;
using System.Collections.Generic;
using SardineFish.Utils;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace WFC.Tilemap3D
{
    public class GameObjectTilemap : MonoBehaviour, ICustomEditorEX
    {
        public int ChunkSize = 8;
        private readonly Dictionary<Vector3Int, TileChunk> Chunks = new Dictionary<Vector3Int, TileChunk>();
        
        [DisplayInInspector("Total tiles")]
        public int Count { get; private set; }

        public BoundsInt Bounds { get; private set; }

        private void Awake()
        {
            ReloadTileFromChildren();
        }

        [EditorButton]
        public void ReloadTileFromChildren()
        {
            Chunks.Clear();
            Count = 0;
            var tiles = GetComponentsInChildren<GameObjectTile>();
            foreach (var tile in tiles)
            {
                var pos = tile.transform.localPosition.FloorToVector3Int();
                var (chunkPos, offset) = ChunkAt(pos);
                tile.SetPosInternal(chunkPos, offset, pos);
                SetTileInstance(pos, tile);
            }
        }

        [EditorButton]
        void ClearAll()
        {
            ClearAllTiles();
        }

        public void SetTile(Vector3Int pos, GameObjectTile prefab)
        {
            if (!prefab)
            {
                RemoveTile(pos);
                return;
            }

            var (chunkPos, offset) = ChunkAt(pos);
            var tile = prefab.CreateInstance(chunkPos, offset, pos);
            SetTileInstance(pos, tile);
        }
        
        void SetTileInstance(Vector3Int pos, GameObjectTile tile)
        {
            var (chunkPos, offset) = ChunkAt(pos);
            var chunk = GetOrCreateChunk(chunkPos);
                        
            tile.transform.SetParent(transform, false);
            tile.transform.localPosition = pos;
            if (!chunk.SetTile(offset, tile))
            {
                Count++;
                if (Count == 1)
                    Bounds = new BoundsInt(pos, Vector3Int.one);
                else
                    Bounds = Bounds.Encapsulate(pos);
            }
        }

        public void RemoveTile(Vector3Int pos)
        {
            var (chunkPos, offset) = ChunkAt(pos);
            if (!Chunks.TryGetValue(chunkPos, out var chunk))
                return;

            var tile = chunk.RemoveTile(offset);
            if (tile)
            {
                tile.DestroyInstance();
                Count--;
            }
            
        }

        public void ClearAllTiles()
        {
            foreach (var chunk in Chunks.Values)
            {
                foreach (var tile in chunk.TileList)
                {
                    chunk.Tiles[tile.ChunkOffset.x, tile.ChunkOffset.y, tile.ChunkOffset.z] = null;
                    tile.DestroyInstance();
                }
                chunk.TileList.Clear();
            }
            Chunks.Clear();
            Count = 0;
        }
        

        public GameObjectTile GetTile(Vector3Int pos)
        {
            var (chunkPos, offset) = ChunkAt(pos);
            if (!Chunks.TryGetValue(chunkPos, out var chunk))
                return null;
            return chunk[offset];
        }

        public GameObjectTile RayMarch(Ray ray, int distance)
        {
            return RayMarch(ray, distance, out _, out _);
        }

        public GameObjectTile RayMarch(Ray ray, int distance, out Vector3Int hitPos, out Vector3Int hitNormal)
        {
            foreach (var (pos, normal) in Utility.VoxelRayMarching(ray, distance))
            {
                var tile = GetTile(pos);
                if (tile)
                {
                    hitNormal = normal;
                    hitPos = pos;
                    return tile;   
                }
            }

            hitNormal = Vector3Int.zero;
            hitPos = Vector3Int.zero;
            return null;
        }

        (Vector3Int chunkPos, Vector3Int offset) ChunkAt(Vector3Int pos)
        {
            var floorOffset = new Vector3Int(
                pos.x < 0 ? 1 : 0,
                pos.y < 0 ? 1 : 0,
                pos.z < 0 ? 1 : 0
            );
            return (pos / ChunkSize - floorOffset, new Vector3Int(
                FloorReminder(pos.x, ChunkSize),
                FloorReminder(pos.y, ChunkSize),
                FloorReminder(pos.z, ChunkSize)
            ));
        }
        
        static int FloorReminder(int x, int m) =>
            x >= 0
                ? x % m
                : (m + x % m) % m;

        TileChunk GetOrCreateChunk(Vector3Int chunkPos)
        {
            if (Chunks.TryGetValue(chunkPos, out var chunk))
                return chunk;
            var newChunk = new TileChunk(ChunkSize);
            Chunks[chunkPos] = newChunk;
            return newChunk;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(Bounds.center, Bounds.size);
        }
    }
}