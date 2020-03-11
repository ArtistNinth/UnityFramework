using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WndRoot : MonoBehaviour
{
    protected void LoadSprite(Image img, string path)
    {
        Sprite sprite = ResourceService.Instance.LoadResource<Sprite>(path) as Sprite;
        img.sprite = sprite;
    }
}
