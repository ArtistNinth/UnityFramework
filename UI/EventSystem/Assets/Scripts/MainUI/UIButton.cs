using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButton : MonoBehaviour
{
    private Button btnChangeName;

    void Awake()
    {
        btnChangeName = GetComponent<Button>();
        btnChangeName.onClick.AddListener(() => {
            EventCenter.Broadcast<string>(EventType.ChangeName, "Hello Tom");
        });
    }

}
