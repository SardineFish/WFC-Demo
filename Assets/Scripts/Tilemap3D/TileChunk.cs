using System.Collections.Generic;
using UnityEngine;

namespace WFC.Tilemap3D
{
    public class TileChunk
    {
        public readonly GameObjectTile[,,] Tiles;
        public readonly List<GameObjectTile> TileList;

        public TileChunk(int chunkSize)
        {
            Tiles = new GameObjectTile[chunkSize, chunkSize,chunkSize];
            TileList = new List<GameObjectTile>(chunkSize * chunkSize * chunkSize);
        }

        public GameObjectTile this[int x, int y, int z]
        {
            get => Tiles[x, y, z];
            // set => Tiles[x, y, z] = value;
        }

        public GameObjectTile this[Vector3Int pos]
        {
            get => Tiles[pos.x, pos.y, pos.z];
            // set => Tiles[pos.x, pos.y, pos.z] = value;
        }

        public GameObjectTile SetTile(Vector3Int offset, GameObjectTile tile)
        {
            var oldTile = this[offset];
            if (oldTile)
                TileList[oldTile.InChunkId] = tile;
            else 
                TileList.Add(tile);

            Tiles[offset.x, offset.y, offset.z] = tile;
            return oldTile;
        }

        public GameObjectTile RemoveTile(Vector3Int offset)
        {
            var tile = this[offset];
            if (tile)
            {
                if (TileList.Count > 1 && tile.InChunkId != TileList.Count - 1)
                {
                    TileList[tile.InChunkId] = TileList[TileList.Count - 1];
                    TileList[tile.InChunkId].InChunkId = tile.InChunkId;
                }
                TileList.RemoveAt(TileList.Count - 1);
            }

            return tile;
        }
        
    }
}