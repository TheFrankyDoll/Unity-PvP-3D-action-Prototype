using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class EscToExitUI : MonoBehaviour
{
    public static EscToExitUI main;
    public float HoldTime = 2f;
    public float Timer;
    public Image Fill;

    void Awake()
    {
        main = this;
        SetActive(false);
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.Escape)) Timer += Time.deltaTime;
        else Timer -= Time.deltaTime * 2f;

        Timer = Mathf.Clamp(Timer, 0, HoldTime);

        Fill.fillAmount = Timer / HoldTime;
        if(Timer == HoldTime)
        {
            // stop host if host mode
            if (NetworkServer.active && NetworkClient.isConnected)
            {
                MyNetworkManager.singletonModed.StopHost();
            }
            else if (NetworkClient.isConnected)
            {
                MyNetworkManager.singletonModed.StopClient();
            }
            // stop server if server-only
            else if (NetworkServer.active)
            {
                MyNetworkManager.singletonModed.StopServer();
                PreConnectionUI.main.T_StartedServer.enabled = false;
            }

            
            SetActive(false);
        }
    }

    /// <summary> Включает/Выключает этот UI. </summary>
    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
        Timer = 0;
        Fill.fillAmount = 0;
    }
}
