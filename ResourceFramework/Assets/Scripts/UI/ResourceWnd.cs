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

    public void ClickLoadButton()
    {
        LoadSpriteAsync(img1, img1Path);
        LoadSpriteAsync(img2, img2Path);
        LoadSpriteAsync(img3, img3Path);
    }

    public void ClickUnLoadBtn()
    {
        ResourceService.Instance.UnLoadResource(img1Path);
    }

    public void ClickLoadPrefabButton()
    {
        ResourceService.Instance.InstantiateAsync(prefabPath, (GameObject go) =>
        {
            go.transform.SetParent(transformRoot);
        });
    }

    public void ClickReleasePrefabButton()
    {
        for (int i = 0; i < transformRoot.childCount; i++)
        {
            ResourceService.Instance.ReleaseInstance(transformRoot.GetChild(i).gameObject, true);
        }
    }
}
