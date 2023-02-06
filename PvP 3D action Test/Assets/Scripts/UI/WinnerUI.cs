using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinnerUI : MonoBehaviour
{
    /// <summary> [��������] ������ �� ����������� ��������� �������. </summary>
    public static WinnerUI main;

    public Text T_WinnerName;

    public void Awake()
    {
        main = this;
        SetActive(false);
    }

    /// <summary> ��������/��������� ���� UI. </summary>
    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }
}
