using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


/// <summary>
/// Этот скрипт хранит данные о состоянии и правилах игры для клиентов.
/// </summary>
public class GameLogic : NetworkBehaviour
{
    /// <summary> [Синглтон] Ссылка на единсвенный экземпляр скрипта. </summary>
    public static GameLogic main;

    [Header("Правила игры (Учитываются только значения на Сервере/Хосте)")]
    public int HitsToWin = 3;
    public float InvulnerabilityTime = 3f;
    public float NextMatchTime = 5f;

    [Space(5)]
    public bool NextMatchCountdown = false;
    public float NextMatchTimer = 0f;

    [Header("Список активных Игроков")] //Придётся синхронизировать вручную т.к. SyncList не поддерживает SyncVar внутри своих элементов.
    public List<NetworkPlayer> Players = new List<NetworkPlayer>(); //Синхронизируется через "GameLogic.main.RpcSyncPlayerLists()"

    private void Awake()
    {
        if (!main) main = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Таймер вызывается только на сервере.
    /// </summary>
    [ServerCallback]
    public void FixedUpdate()
    {
        if (!NextMatchCountdown) return;
        NextMatchTimer -= Time.fixedDeltaTime;
        if (NextMatchTimer <= 0)
        {
            NextMatchCountdown = false;
            NextMatchTimer = NextMatchTime;
            MyNetworkManager.singletonModed.BeginNextRound();
        }
    }


    /// <summary> Позволяет получить данные о игроке по его GameObject-у. Вернёт null, если этого игрока нет в списке. 
    /// <para> Только сервер может изменять выданное значение, остальным доступ только для чтения. </para></summary>
    public NetworkPlayer GetDataOf(GameObject PlayerGO)
    {
        if (!Players.Exists(x => x.PlayerIdentity.gameObject == PlayerGO)) return null;
        return Players.Find(x => x.PlayerIdentity.gameObject == PlayerGO);
    }



    /// <summary> Cервер выдаcт +1 очко игроку PlayerGO. </summary>
    [Server] public void AddPoint(GameObject PlayerGO)
    {
        if (!Players.Exists(x => x.PlayerIdentity.gameObject == PlayerGO)) throw new System.NullReferenceException($"{PlayerGO} не является объектом ни одного игрока в списке.");

        var playerInList = Players.Find(x => x.PlayerIdentity.gameObject == PlayerGO);
        playerInList.SetScore_Server(playerInList.Score+1);

        RpcUpdateScoreboards();
    }
    /// <summary> Cервер обнулит счёт игрока PlayerGO. </summary>
    [Server] public void ClearScore(GameObject PlayerGO)
    {
        if (!Players.Exists(x => x.PlayerIdentity.gameObject == PlayerGO)) throw new System.NullReferenceException($"{PlayerGO} не является объектом ни одного игрока в списке.");

        Players.Find(x => x.PlayerIdentity.gameObject == PlayerGO).SetScore_Server(0);

        RpcUpdateScoreboards();
    }
    /// <summary> Cервер обнулит счёта КАЖДОГО игрока. </summary>
    [Server] public void ClearAllScores()
    {
        foreach(var i in Players)
        {
            i.SetScore_Server(0);
        }
        RpcUpdateScoreboards();
    }

    /// <summary> Обновляет счёт для каждого клиента. </summary>
    [ClientRpc] public void RpcUpdateScoreboards()
    {
        ScoreBoardUI.main.RedrawScoreboard();
    }

    /// <summary> Показать каждому игроку победителя. </summary>
    [ClientRpc]
    public void RpcShowWinnerUI(string winnerName)
    {
        WinnerUI.main.T_WinnerName.text = winnerName;
        WinnerUI.main.SetActive(true);
    }
    /// <summary> Скрыть WinnerUI для каждого клиента. </summary>
    [ClientRpc]
    public void RpcHideWinnerUI()
    {
        WinnerUI.main.SetActive(false);
    }

    // >Приходится переводить в массив потому что Rpc не поддерживает передачу списков.
    /// <summary> Отчищает у игроков список GameLogic.Players, и меняет на новый. Обычно это нужно ТОЛЬКО при подключении/отключении игрока к серверу. </summary>
    [ClientRpc] public void RpcSyncPlayerLists(NetworkPlayer[] newList)
    {
        main.Players.Clear();
        main.Players = newList.ToList();
    }

    /// <summary>
    /// Этот метод присваивает значение нового NetworkPlayer.PlayerIdentity на "playerScene" для каждого клиента.
    /// </summary>
    /// <param name="player">Объект с данными игрока на сервере и скриптом "NetworkPlayer"</param>
    /// <param name="playerScene">NetworkIdentity, который пренадлежит уже созданному игроку на сцене.</param>
    [ClientRpc]
    public void RpcSyncScenePlayerVar(NetworkPlayer player, NetworkIdentity playerScene)
    {
        player.PlayerIdentity = playerScene;
    }
}
