using System.Collections.Generic;
using SardineFish.Utils;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace WFC.Tilemap3D
{
    public class Tilemap3D : MonoBehaviour, ICustomEditorEX
    {
        public int ChunkSize = 8;
        private readonly Dictionary<Vector3Int, TileChunk> Chunks = new Dictionary<Vector3Int, TileChunk>();
        
        [DisplayInInspector("Total tiles")]
        public int Count { get; private set; }

        [EditorButton]
        void UpdateTileFromChildren()
        {
            var tiles = GetComponentsInChildren<GameObjectTile>();
            foreach (var tile in tiles)
            {
                SetTileInstance(tile.transform.localPosition.FloorToVector3Int(), tile);
            }
        }

        public void SetTile(Vector3Int pos, GameObjectTile prefab)
        {
            if (!prefab)
            {
                RemoveTile(pos);
                return;
            }

            var (chunkPos, offset) = ChunkAt(pos);
            var tile = prefab.CreateInstance(chunkPos, offset);
            SetTileInstance(pos, tile);
        }
        
        void SetTileInstance(Vector3Int pos, GameObjectTile tile)
        {
            var (chunkPos, offset) = ChunkAt(pos);
            var chunk = GetOrCreateChunk(chunkPos);
                        
            tile.transform.SetParent(transform, false);
            tile.transform.localPosition = pos;
            chunk.SetTile(pos, tile);
        }

        public void RemoveTile(Vector3Int pos)
        {
            var (chunkPos, offset) = ChunkAt(pos);
            if (!Chunks.TryGetValue(chunkPos, out var chunk))
                return;

            var tile = chunk.RemoveTile(offset);
            if (tile)
                tile.DestroyInstance();
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
        }
        

        public GameObjectTile GetTile(Vector3Int pos)
        {
            var (chunkPos, offset) = ChunkAt(pos);
            if (!Chunks.TryGetValue(chunkPos, out var chunk))
                return null;
            return chunk[offset];
        }

        (Vector3Int chunkPos, Vector3Int offset) ChunkAt(Vector3Int pos)
        {
            return (pos / ChunkSize, pos.Modulo(ChunkSize));
        }

        TileChunk GetOrCreateChunk(Vector3Int chunkPos)
        {
            if (Chunks.TryGetValue(chunkPos, out var chunk))
                return chunk;
            var newChunk = new TileChunk(ChunkSize);
            Chunks[chunkPos] = newChunk;
            return newChunk;
        }
    }
}