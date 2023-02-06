using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

/// <summary>
/// Это ячейка просто отображает информацию, её параметры меняет [...]
/// </summary>
public class ScoreboardCell : MonoBehaviour
{
    public string PlayerName;
    public int Score;

    // Можно сделать переменные ниже приватными, но не вижу в этом смысла.
    public Image LocalPlayerImage;
    public Text PlayerNameText;
    public Text ScoreText;

    public NetworkIdentity PlayerIdentity;

    public void Start()
    {
        Redraw();
    }

    /// <summary>
    /// Визуально применяет полученные значения.
    /// </summary>
    public void Redraw()
    {
        PlayerNameText.text = PlayerName;
        ScoreText.text = Score.ToString();
        LocalPlayerImage.enabled = NetworkClient.localPlayer == PlayerIdentity; //Игрок клиента немного подсвечивается.
    }
}
