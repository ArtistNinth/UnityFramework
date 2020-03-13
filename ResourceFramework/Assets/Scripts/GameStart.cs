using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    public Transform recycleTrans;

    void Awake()
    {
        AssetBundleManager.Instance.LoadAssetBundleConfig();
        ResourceService.Instance.Init(recycleTrans);
    }

    void PreLoadResource()
    {
        List<string> pathList = new List<string>();
        pathList.Add("Assets/Images/general/baoji.png");
        pathList.Add("Assets/Images/general/current.png");
        pathList.Add("Assets/Images/general/discountDetail.png");

        ResourceService.Instance.PreLoadResource<Sprite>(pathList);
    }
}