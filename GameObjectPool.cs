using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace SardineFish.Utils
{
    
    public class GameObjectPool : MonoBehaviour
    {
        private Dictionary<GameObject, Pool> prefabPools = new Dictionary<GameObject, Pool>();
        private Dictionary<Type, Pool> _perComponentPools = new Dictionary<Type, Pool>();

        private static GameObjectPool _instance;

        private static GameObjectPool Instance
        {
            get
            {
                if (!_instance)
                    _instance = CreateGameObjectPoolRoot();
                return _instance;
            }
        }

        #region Types
        
        private static class PerComponentPool<T> where T : Component
        {
            public static Pool ObjectPool;
            private static string DefaultName = "[GameObject]";

            public static T Get()
                => Get(DefaultName);
            public static T Get(string name)
            {
                if (Application.isPlaying)
                    return GetOrCreatePool().Get(name).GetComponent<T>();
                else
                    return Allocator().GetComponent<T>();
            }

            public static void Release(T component)
            {
                if (Application.isPlaying)
                    GetOrCreatePool().Release(component.gameObject);
                else
                    DestroyImmediate(component.gameObject);
            }

            public static void PreAlloc(int count)
            {
                if (Application.isPlaying)
                    GetOrCreatePool().PreAlloc(count);
            }

            static Pool GetOrCreatePool()
            {
                if(ObjectPool is null)
                    CreatePool();
                return ObjectPool;
            }

            static void CreatePool()
            {
                var container = new GameObject("[Pool]" + typeof(T).Name);
                container.transform.SetParent(GameObjectPool.Instance.transform, false);
                ObjectPool = new Pool(container, Allocator);
                Instance._perComponentPools.Add(typeof(T), ObjectPool);
            }

            static GameObject Allocator()
            {
                var obj = new GameObject(DefaultName);
                obj.AddComponent<T>();
                return obj;
            }
        }

        private class Pool
        {
            public string defaultObjectName = "[GameObject]";
            
            public Func<GameObject> Allocator;
            public GameObject ObjectCollection;
            Stack<GameObject> objectPool = new Stack<GameObject>();

            public Pool(GameObject objectCollection, Func<GameObject> allocator)
            {
                ObjectCollection = objectCollection;
                Allocator = allocator;
            }
            
            GameObject CreateObject()
            {
                var newObj = Allocator.Invoke();
                newObj.name = defaultObjectName;
                return newObj;
            }

            public GameObject Get()
            {
                if (objectPool.Count > 0)
                {
                    var obj = objectPool.Pop();
                    obj.SetActive(true);
                    // obj.transform.parent = null;
                    obj.transform.SetParent(null);
                    return obj;
                }

                return CreateObject();
            }

            public GameObject Get(string name)
            {
                var obj = Get();
                obj.name = name;
                return obj;
            }

            public void Release(GameObject obj)
            {
                if (!obj)
                    return;
                // obj.transform.parent = transform;
                obj.transform.SetParent(ObjectCollection.transform, false);
                obj.SetActive(false);
                objectPool.Push(obj);
            }

            public void PreAlloc(int count)
            {
                for (var i = 0; i < count; i++)
                {
                    var obj = CreateObject();
                    Release(obj);
                }
            }
        }
        
        #endregion
        
        #region PrefabPool

        public static GameObject Get(GameObject prefab)
        {
            if (Application.isPlaying)
                return GetOrCreatePrefabPool(prefab).Get();
            else
                return Instantiate(prefab);
        }

        public static GameObject Get(GameObject prefab, string name)
        {
            if(Application.isPlaying)
                return GetOrCreatePrefabPool(prefab).Get(name);
            else
            {
                var obj = Instantiate(prefab);
                obj.name = name;
                return obj;
            }
        }
        
        public static void Release(GameObject prefab, GameObject obj)
        {
            if (Application.isPlaying)
                GetOrCreatePrefabPool(prefab).Release(obj);
            else
                DestroyImmediate(obj);
        }

        public static T Get<T>(GameObject prefab) where T : Component
            => Get(prefab)?.GetComponent<T>();

        public static T Get<T>(GameObject prefab, string name) where T : Component
            => Get(prefab, name).GetComponent<T>();

        public static void Release<T>(GameObject prefab, T component) where T : Component
        {
            if(component && component.gameObject)
                Release(prefab, component.gameObject);
        }

        public static void PreAlloc(GameObject prefab, int count)
        {
            if (Application.isPlaying)
                GetOrCreatePrefabPool(prefab).PreAlloc(count);
        }

        static Pool GetOrCreatePrefabPool(GameObject prefab)
        {
            Assert.IsTrue(Application.isPlaying);
            if (Instance.prefabPools.ContainsKey(prefab))
            {
                var existedPool = Instance.prefabPools[prefab];
                if (!(existedPool is null))
                    return existedPool;
            }
            var pool = CreatePrefabPool(prefab);
            Instance.prefabPools[prefab] = pool;
            return pool;
        }

        static Pool CreatePrefabPool(GameObject prefab)
        {
            var pool = new Pool(new GameObject(), ()=>Instantiate(prefab));
            pool.ObjectCollection.transform.parent = Instance.transform;
            pool.defaultObjectName = prefab.name;
            pool.ObjectCollection.name = "[Pool]" + prefab.name;
            return pool;
        }
        
        #endregion

        #region PerComponentPool

        public static T Get<T>(string name) where T : Component
            => PerComponentPool<T>.Get(name);

        public static T Get<T>() where T : Component
            => PerComponentPool<T>.Get();

        public static void Release<T>(T component) where T : Component
            => PerComponentPool<T>.Release(component);

        public static void PreAlloc<T>(int count) where T : Component
            => PerComponentPool<T>.PreAlloc(count);

        #endregion

        // private void OnDestroy()
        // {
        //     _instance = null;
        // }

        static GameObjectPool CreateGameObjectPoolRoot()
        {
            Assert.IsTrue(Application.isPlaying);
            var obj = new GameObject();
            obj.name = "[GameObjectPool]";
            var pool = obj.AddComponent<GameObjectPool>();
            DontDestroyOnLoad(obj);
            return pool;
        }
    }
}