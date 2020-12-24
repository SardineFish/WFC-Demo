using System;
using SardineFish.Utils;
using UnityEditor;
using UnityEngine;

namespace WFC.Tilemap3D
{
    [ExecuteInEditMode]
    [SelectionBase]
    public class GameObjectTile : MonoBehaviour
    {
        public Vector3Int Chunk { get; private set; }
        
        public Vector3Int ChunkOffset { get; private set; }
        
        public Vector3Int Position { get; private set; }
        
        public int InChunkId { get; set; }
        
        private GameObject _prefab;
        
        public GameObjectTile CreateInstance(Vector3Int chunkPos, Vector3Int offset, Vector3Int pos)
        {
            if (!_prefab)
                _prefab = gameObject;
            
            var tile = GameObjectPool.Get<GameObjectTile>(gameObject);
            tile._prefab = _prefab;
            tile.SetPosInternal(chunkPos, offset, pos);

            return tile;
        }

        public void DestroyInstance()
        {
            if (Application.isPlaying)
            {
                GameObjectPool.Release(_prefab, gameObject);
            }
            else
                Undo.DestroyObjectImmediate(gameObject);
        }

        internal void SetPosInternal(Vector3Int chunkPos, Vector3Int offset, Vector3Int pos)
        {
            Chunk = chunkPos;
            ChunkOffset = offset;
            Position = pos;
        }

        private void Update()
        {
            
        }
    }
}