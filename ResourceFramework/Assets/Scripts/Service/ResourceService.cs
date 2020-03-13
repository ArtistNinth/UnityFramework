using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ResourceService:Singleton<ResourceService>
{
    private Dictionary<uint, ResourceItem> loadedResourceItemDic = new Dictionary<uint, ResourceItem>();
    private Dictionary<uint, AssetBundleRequest> loadingResourceItemDic = new Dictionary<uint, AssetBundleRequest>();
    private Dictionary<uint, List<GameObject>> prefabPoolDic = new Dictionary<uint, List<GameObject>>();
    private Dictionary<int, uint> guidCrcDic = new Dictionary<int, uint>();
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

    public void PreLoadResource<T>(List<string> pathList) where T:UnityEngine.Object
    {
        for (int i = 0; i < pathList.Count; i++)
        {
            string path = pathList[i];
            uint crc = Crc32.GetCrc32(path);

            if (!loadedResourceItemDic.ContainsKey(crc))
            {
                ResourceItem item = ResourceManager.Instance.LoadResourceAB(crc);
                UnityEngine.Object asset = item.assetBundle.LoadAsset<T>(path);
                this.CacheResource(ref item, asset);
            }
        }
    }
    #endregion

    #region Prefab
    public GameObject Instantiate(string path)
    {
        uint crc = Crc32.GetCrc32(path);

        GameObject instance = null;
        if (prefabPoolDic.ContainsKey(crc))
        {
            List<GameObject> prefabList = prefabPoolDic[crc];
            if (prefabList != null && prefabList.Count > 0)
            {
                instance = prefabList[0];
                prefabList.RemoveAt(0);
            }
        }

        if (instance == null)
        {
            GameObject go = this.LoadResource<GameObject>(path) as GameObject;
            instance = GameObject.Instantiate(go);
            guidCrcDic.Add(instance.GetInstanceID(), crc);
        }

        return instance;
    }

    public void InstantiateAsync(string path,InstantiateCompleteCallback callback)
    {
        uint crc = Crc32.GetCrc32(path);

        GameObject instance = null;

        if (prefabPoolDic.ContainsKey(crc))
        {
            List<GameObject> prefabList = prefabPoolDic[crc];
            if (prefabList != null && prefabList.Count > 0)
            {
                instance = prefabList[0];
                prefabList.RemoveAt(0);
            }
        }

        if (instance == null)
        {
            this.LoadResourceAsync<GameObject>(path, (UnityEngine.Object go) =>
            {
                instance = GameObject.Instantiate(go as GameObject);
                guidCrcDic.Add(instance.GetInstanceID(), crc);
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
        uint crc = guidCrcDic[go.GetInstanceID()];

        if (destroyGameObject)
        {
            guidCrcDic.Remove(go.GetInstanceID());
            GameObject.Destroy(go);
        }
        else
        {
            go.transform.SetParent(recycleTrans);
            List<GameObject> prefabList = null;
            if (!prefabPoolDic.ContainsKey(crc))
            {
                prefabList = new List<GameObject>();
                prefabPoolDic.Add(crc, prefabList);
            }
            else
            {
                prefabList = prefabPoolDic[crc];
            }

            prefabList.Add(go);
        }
    }
    #endregion

    
}
