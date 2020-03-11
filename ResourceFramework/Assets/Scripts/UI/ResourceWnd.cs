using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

public class ResourceWnd : WndRoot
{
    public Image img1;
    public Image img2;
    public Image img3;

    private const string img1Path = "Assets/Images/general/baoji.png";
    private const string img2Path = "Assets/Images/general/current.png";
    private const string img3Path = "Assets/Images/general/discountDetail.png";

    public void ClickLoadButton()
    {
        LoadSprite(img1, img1Path);
        LoadSprite(img2, img2Path);
        LoadSprite(img3, img3Path);
    }

    public void ClickUnLoadBtn()
    {
        ResourceService.Instance.UnLoadResource(img1Path);
    }
}
