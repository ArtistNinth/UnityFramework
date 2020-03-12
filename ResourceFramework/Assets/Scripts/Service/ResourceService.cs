using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ResourceService:Singleton<ResourceService>
{
    private static Dictionary<uint, ResourceItem> loadedResourceItemDic = new Dictionary<uint, ResourceItem>();
    private static Dictionary<uint, AssetBundleRequest> loadingResourceItemDic = new Dictionary<uint, AssetBundleRequest>();

    private static Dictionary<uint, ClassObjectPool<GameObject>> instanceDic = new Dictionary<uint, ClassObjectPool<GameObject>>();
    private static Dictionary<GameObject, uint> goCrcDic = new Dictionary<GameObject, uint>();
    private Transform recycleTrans;

    public delegate void LoadCompleteCallback(UnityEngine.Object asset);
    public delegate void InstantiateCompleteCallback(GameObject go);
    
    public void Init(Transform recycleTrans)
    {
        this.recycleTrans = recycleTrans;
    }

    #region Asset
    public UnityEngine.Object LoadResource<T>(string path) where T:UnityEngine.Object
    {
        uint crc = Crc32.GetCrc32(path);

        UnityEngine.Object asset;

        if (loadedResourceItemDic.ContainsKey(crc))
        {
            asset = loadedResourceItemDic[crc].asset;
        }
        else
        {
            ResourceItem item = ResourceManager.Instance.LoadResourceAB(crc);
            asset = item.assetBundle.LoadAsset<T>(path);
            this.CacheResource(ref item, asset);
        }

        return asset;
    }

    public void UnLoadResource(string path)
    {
        uint crc = Crc32.GetCrc32(path);
        ResourceItem item = loadedResourceItemDic[crc];
        item.asset = null;
        loadedResourceItemDic.Remove(crc);        
        ResourceManager.Instance.UnLoadResourceAB(crc);
    }

    public void LoadResourceAsync<T>(string path, LoadCompleteCallback callback = null) where T : UnityEngine.Object
    {
        uint crc = Crc32.GetCrc32(path);

        if (loadedResourceItemDic.ContainsKey(crc))
        {
            UnityEngine.Object asset = loadedResourceItemDic[crc].asset;
            if (callback != null)
            {
                callback(asset);
            }
        }
        else
        {
            ResourceItem item = ResourceManager.Instance.LoadResourceAB(crc);
            AssetBundleRequest request = null;
            if (!loadingResourceItemDic.ContainsKey(crc))
            {
                request = item.assetBundle.LoadAssetAsync<T>(path);
                loadingResourceItemDic.Add(crc, request);
            }
            else
            {
                request = loadingResourceItemDic[crc];
            }

            request.completed += (AsyncOperation obj) =>
            {
                this.CacheResource(ref item, request.asset);
                loadingResourceItemDic.Remove(crc);

                if (callback != null)
                {
                    callback(request.asset);
                }                
            };
        }
    }

    private void CacheResource(ref ResourceItem item, UnityEngine.Object asset)
    {
        item.asset = asset;
        loadedResourceItemDic.Add(item.crc, item);
    }
    #endregion

    #region Prefab
    public GameObject Instantiate(string path)
    {
        uint crc = Crc32.GetCrc32(path);

        GameObject instance = null;

        if (!instanceDic.ContainsKey(crc))
        {
            ClassObjectPool<GameObject> pool = ObjectManager.Instance.GetOrCreateClassPool<GameObject>(Constants.PrefabInstanceSize);
            instanceDic.Add(crc, pool);
        }

        ClassObjectPool<GameObject> instancePool = instanceDic[crc];
        instance = instancePool.Spawn(false);
        if (instance == null)
        {
            GameObject go = this.LoadResource<GameObject>(path) as GameObject;
            instance = GameObject.Instantiate(go);
            goCrcDic.Add(instance, crc);
        }

        return instance;
    }

    public void InstantiateAsync(string path,InstantiateCompleteCallback callback)
    {
        uint crc = Crc32.GetCrc32(path);

        GameObject instance = null;

        if (!instanceDic.ContainsKey(crc))
        {
            ClassObjectPool<GameObject> pool = ObjectManager.Instance.GetOrCreateClassPool<GameObject>(Constants.PrefabInstanceSize);
            instanceDic.Add(crc, pool);
        }

        ClassObjectPool<GameObject> instancePool = instanceDic[crc];
        instance = instancePool.Spawn(false);
        if (instance == null)
        {
            this.LoadResourceAsync<GameObject>(path, (UnityEngine.Object go) => {
                instance = GameObject.Instantiate(go as GameObject);
                goCrcDic.Add(instance, crc);
                if (callback != null)
                {
                    callback(instance);
                }
            });
        }
        else
        {
            if (callback != null)
            {
                callback(instance);
            }
        }
    }

    public void ReleaseInstance(GameObject go,bool destroyGameObject)
    {
        uint crc = goCrcDic[go];
        ClassObjectPool<GameObject> instancePool = instanceDic[crc];

        if (destroyGameObject)
        {
            GameObject.Destroy(go);
        }
        else
        {
            go.transform.SetParent(recycleTrans);
            instancePool.Recyle(go);
        }
    }
    #endregion
}
