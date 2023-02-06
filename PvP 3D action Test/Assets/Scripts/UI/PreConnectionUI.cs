using UnityEngine;
using UnityEngine.UI;

public class PreConnectionUI : MonoBehaviour
{
    /// <summary> [Синглтон] Ссылка на единсвенный экземпляр скрипта. </summary>
    public static PreConnectionUI main;
    MyNetworkManager manager;

    public InputField InputPlayerName;
    public InputField InputIPAddress;
    [Space(5)]
    public Button B_Client;
    public Button B_Host;
    public Button B_Server;
    [Space(5)]
    public Text T_StartedServer;

    void Awake()
    {
        manager = Mirror.NetworkManager.singleton as MyNetworkManager;
        main = this;
    }

    /// <summary> Включает/Выключает этот UI. </summary>
    public void SetActive(bool value)
    {
        gameObject.SetActive(value);

        B_Client.interactable = true;
        T_StartedServer.enabled = false;
    }

    public void ButtonHost()
    {
        manager.StartHost();
    }
    public void ButtonServer()
    {
        manager.StartServer();

        if (Mirror.NetworkServer.active)
        {
            T_StartedServer.enabled = true;
            EscToExitUI.main.SetActive(true);
        }
    }
    public void ButtonClient()
    {
        manager.networkAddress = InputIPAddress.text;
        manager.StartClient();

        B_Client.interactable = false; //Избегаем повторного нажатия на кнопку.
    }
}
