using SardineFish.Utils;
using UnityEngine;

namespace WFC.Tilemap3D
{
    public class GameObjectTile : MonoBehaviour
    {
        public Vector3Int Chunk { get; private set; }
        
        public Vector3Int ChunkOffset { get; private set; }
        
        public int InChunkId { get; set; }
        
        private GameObject _prefab;
        
        public GameObjectTile CreateInstance(Vector3Int chunkPos, Vector3Int offset)
        {
            if (!_prefab)
                _prefab = gameObject;
            
            var tile = GameObjectPool.Get<GameObjectTile>(gameObject);
            tile._prefab = _prefab;
            tile.Chunk = chunkPos;
            tile.ChunkOffset = offset;

            return tile;
        }

        public void DestroyInstance()
        {
            GameObjectPool.Release(_prefab, gameObject);
        }
    }
}