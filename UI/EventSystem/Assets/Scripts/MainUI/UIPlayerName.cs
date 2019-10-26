using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerName : MonoBehaviour
{
    private Text playerName;

    void Awake() {
        playerName = GetComponent<Text>();

        EventCenter.AddListener<string>(EventType.ChangeName, ChangeName);
    }

    void Destroy() {
        EventCenter.RemoveListener<string>(EventType.ChangeName, ChangeName);
    }

    private void ChangeName(string newName) {
        playerName.text = newName;
    }
}
