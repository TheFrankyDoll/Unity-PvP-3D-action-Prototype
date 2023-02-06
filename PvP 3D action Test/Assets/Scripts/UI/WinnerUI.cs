using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinnerUI : MonoBehaviour
{
    /// <summary> [Синглтон] Ссылка на единсвенный экземпляр скрипта. </summary>
    public static WinnerUI main;

    public Text T_WinnerName;

    public void Awake()
    {
        main = this;
        SetActive(false);
    }

    /// <summary> Включает/Выключает этот UI. </summary>
    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }
}
