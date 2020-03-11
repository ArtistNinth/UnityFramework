using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager:Singleton<ResourceManager>
{
    protected Dictionary<uint, ResourceItem> resourceItemDic = new Dictionary<uint, ResourceItem>();

    public void AddResourceItem(uint crc, string assetPath, string ABName, List<string> dependencyABNames)
    {
        ResourceItem item = new ResourceItem();
        item.crc = crc;
        item.assetPath = assetPath;
        item.ABName = ABName;
        item.dependencyABNames = dependencyABNames;

        resourceItemDic.Add(item.crc, item);
    }

    public ResourceItem LoadResourceAB(uint crc)
    {
        ResourceItem item = resourceItemDic[crc];
        if (item.assetBundle == null)
        {
            item.assetBundle = AssetBundleManager.Instance.LoadAssetBundle(item.ABName);
            if (item.dependencyABNames != null)
            {
                for (int i = 0; i < item.dependencyABNames.Count; i++)
                {
                    AssetBundleManager.Instance.LoadAssetBundle(item.dependencyABNames[i]);
                }
            }
        }
        return item;
    }

    public void UnLoadResourceAB(uint crc)
    {
        ResourceItem item = resourceItemDic[crc];
        if (item.dependencyABNames != null)
        {
            for (int i = 0; i < item.dependencyABNames.Count; i++)
            {
                AssetBundleManager.Instance.UnLoadAssetBundle(item.dependencyABNames[i]);
            }
        }
        AssetBundleManager.Instance.UnLoadAssetBundle(item.ABName);
    }
}

public class ResourceItem
{
    public uint crc = 0;
    public string assetPath = string.Empty;
    public string ABName = string.Empty;
    public List<string> dependencyABNames = null;

    public AssetBundle assetBundle = null;
    public UnityEngine.Object obj = null;
}