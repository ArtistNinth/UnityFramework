using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ResourceWnd : WndRoot
{
    public Transform transformRoot;

    public Image img1;
    public Image img2;
    public Image img3;

    private const string img1Path = "Assets/Images/general/baoji.png";
    private const string img2Path = "Assets/Images/general/current.png";
    private const string img3Path = "Assets/Images/general/discountDetail.png";

    private const string prefabPath = "Assets/Prefabs/Capsule.prefab";
    private const string prefabPath2 = "Assets/Prefabs/Sphere.prefab";

    public void ClickLoadButton()
    {
        LoadSpriteAsync(img1, img1Path);
        LoadSpriteAsync(img2, img2Path);
        LoadSpriteAsync(img3, img3Path);
    }

    public void ClickLoadPrefabButton()
    {
        GameObject go = ResourceService.Instance.Instantiate(prefabPath);
        go.transform.SetParent(transformRoot);
    }

    public void ClickLoadPrefabButton2()
    {
        ResourceService.Instance.InstantiateAsync(prefabPath2, (GameObject go) =>
        {
            go.transform.SetParent(transformRoot);
        });
    }

    public void ClickReleasePrefabButton()
    {
        for (int i = transformRoot.childCount - 1; i >= 0; i--)
        {
            ResourceService.Instance.ReleaseInstance(transformRoot.GetChild(i).gameObject, false);
        }
    }

    public void ClickClearCacheButton()
    {
        
    }
}
