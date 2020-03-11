using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ResourceService:Singleton<ResourceService>
{
    private static Dictionary<uint, ResourceItem> loadedResourceItemDic = new Dictionary<uint, ResourceItem>();

    public UnityEngine.Object LoadResource<T>(string path) where T:UnityEngine.Object
    {
        uint crc = Crc32.GetCrc32(path);

        UnityEngine.Object res;

        if (loadedResourceItemDic.ContainsKey(crc))
        {
            res = loadedResourceItemDic[crc].obj;
        }
        else
        {
            ResourceItem item = ResourceManager.Instance.LoadResourceAB(crc);
            res = item.assetBundle.LoadAsset<T>(path);
            item.obj = res;
            loadedResourceItemDic.Add(crc, item);
        }

        return res;
    }

    public void UnLoadResource(string path)
    {
        uint crc = Crc32.GetCrc32(path);
        ResourceItem item = loadedResourceItemDic[crc];
        item.obj = null;
        loadedResourceItemDic.Remove(crc);        
        ResourceManager.Instance.UnLoadResourceAB(crc);
    }
}
