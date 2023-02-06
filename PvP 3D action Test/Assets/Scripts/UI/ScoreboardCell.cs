using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

/// <summary>
/// ��� ������ ������ ���������� ����������, � ��������� ������ [...]
/// </summary>
public class ScoreboardCell : MonoBehaviour
{
    public string PlayerName;
    public int Score;

    // ����� ������� ���������� ���� ����������, �� �� ���� � ���� ������.
    public Image LocalPlayerImage;
    public Text PlayerNameText;
    public Text ScoreText;

    public NetworkIdentity PlayerIdentity;

    public void Start()
    {
        Redraw();
    }

    /// <summary>
    /// ��������� ��������� ���������� ��������.
    /// </summary>
    public void Redraw()
    {
        PlayerNameText.text = PlayerName;
        ScoreText.text = Score.ToString();
        LocalPlayerImage.enabled = NetworkClient.localPlayer == PlayerIdentity; //����� ������� ������� ��������������.
    }
}
