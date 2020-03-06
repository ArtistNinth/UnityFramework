using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class AssetBundleManager:Singleton<AssetBundleManager>
{
    protected Dictionary<uint, AssetBundleItem> ABItemDic = new Dictionary<uint, AssetBundleItem>();    //key:Crc
    protected ClassObjectPool<AssetBundleItem> ABItemPool = ObjectManager.Instance.GetOrCreateClassPool<AssetBundleItem>(Constants.ABItemPoolSize);

    public bool LoadAssetBundleConfig()
    {
        ABItemDic.Clear();

        AssetBundle ab = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, Constants.ABConfigBinABName));
        TextAsset ta = ab.LoadAsset<TextAsset>(Constants.ABConfigBinABName);

        MemoryStream ms = new MemoryStream(ta.bytes);
        BinaryFormatter formatter = new BinaryFormatter();
        AssetBundleConfig abConfig = formatter.Deserialize(ms) as AssetBundleConfig;
        ms.Close();

        for (int i = 0; i < abConfig.abList.Count; i++)
        {
            ABBase abBase = abConfig.abList[i];
            ResourceManager.Instance.AddResourceItem(abBase.Crc, abBase.Path, abBase.ABName, abBase.DependencyABName);
        }

        return true;
    }

    public AssetBundle LoadAssetBundle(string ABName)
    {
        AssetBundleItem abItem;
        uint crc = Crc32.GetCrc32(ABName);
        if (!ABItemDic.TryGetValue(crc, out abItem))
        {
            abItem = ABItemPool.Spawn(true);
            abItem.assetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, ABName));
            ABItemDic.Add(crc,abItem);
        }
        abItem.refCount++;

        return abItem.assetBundle;
    }

    public void UnLoadAssetBundle(string ABName)
    {
        uint crc = Crc32.GetCrc32(ABName);
        if (ABItemDic.ContainsKey(crc))
        {
            AssetBundleItem abItem = ABItemDic[crc];
            abItem.refCount--;
            if (abItem.refCount <= 0 && abItem.assetBundle != null)
            {
                abItem.assetBundle.Unload(true);
                abItem.Reset();
                ABItemPool.Recyle(abItem);
                ABItemDic.Remove(crc);
            }
        }
    }
}

public class AssetBundleItem
{
    public AssetBundle assetBundle;
    public int refCount;

    public void Reset()
    {
        assetBundle = null;
        refCount = 0;
    }
}