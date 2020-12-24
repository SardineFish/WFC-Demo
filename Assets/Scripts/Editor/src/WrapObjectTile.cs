using SardineFish.Utils;
using UnityEditor;
using UnityEngine;
using WFC.Tilemap3D;

namespace WFC.Editor
{
    public class WrapObjectTile
    {
        [MenuItem("Utils/WrapSelectedObjectToTile")]
        private static void WrapToTile()
        {
            var objs = Selection.gameObjects;
            if (objs.Length > 0)
            {
                Undo.RecordObjects(objs, "Wrap to GameObjectTile");
            }
            
            foreach (var obj in Selection.gameObjects)
            {
                var tile = new GameObject($"Tile-{obj.name}");
                tile.AddComponent<GameObjectTile>();
                tile.transform.position = obj.transform.position.FloorToVector3Int();
                tile.transform.SetParent(obj.transform.parent);
                obj.transform.SetParent(tile.transform);
            }
        }
    }
}