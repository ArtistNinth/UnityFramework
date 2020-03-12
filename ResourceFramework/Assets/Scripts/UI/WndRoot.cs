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

    protected void LoadSpriteAsync(Image img, string path)
    {
        ResourceService.Instance.LoadResourceAsync<Sprite>(path, (UnityEngine.Object asset) =>
        {
            img.sprite = asset as Sprite;
        });
    }
}
