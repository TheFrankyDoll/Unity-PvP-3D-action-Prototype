using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


/// <summary>
/// ���� ������ ������ ������ � ��������� � �������� ���� ��� ��������.
/// </summary>
public class GameLogic : NetworkBehaviour
{
    /// <summary> [��������] ������ �� ����������� ��������� �������. </summary>
    public static GameLogic main;

    [Header("������� ���� (����������� ������ �������� �� �������/�����)")]
    public int HitsToWin = 3;
    public float InvulnerabilityTime = 3f;
    public float NextMatchTime = 5f;

    [Space(5)]
    public bool NextMatchCountdown = false;
    public float NextMatchTimer = 0f;

    [Header("������ �������� �������")] //������� ���������������� ������� �.�. SyncList �� ������������ SyncVar ������ ����� ���������.
    public List<NetworkPlayer> Players = new List<NetworkPlayer>(); //���������������� ����� "GameLogic.main.RpcSyncPlayerLists()"

    private void Awake()
    {
        if (!main) main = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// ������ ���������� ������ �� �������.
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


    /// <summary> ��������� �������� ������ � ������ �� ��� GameObject-�. ����� null, ���� ����� ������ ��� � ������. 
    /// <para> ������ ������ ����� �������� �������� ��������, ��������� ������ ������ ��� ������. </para></summary>
    public NetworkPlayer GetDataOf(GameObject PlayerGO)
    {
        if (!Players.Exists(x => x.PlayerIdentity.gameObject == PlayerGO)) return null;
        return Players.Find(x => x.PlayerIdentity.gameObject == PlayerGO);
    }



    /// <summary> C����� ����c� +1 ���� ������ PlayerGO. </summary>
    [Server] public void AddPoint(GameObject PlayerGO)
    {
        if (!Players.Exists(x => x.PlayerIdentity.gameObject == PlayerGO)) throw new System.NullReferenceException($"{PlayerGO} �� �������� �������� �� ������ ������ � ������.");

        var playerInList = Players.Find(x => x.PlayerIdentity.gameObject == PlayerGO);
        playerInList.SetScore_Server(playerInList.Score+1);

        RpcUpdateScoreboards();
    }
    /// <summary> C����� ������� ���� ������ PlayerGO. </summary>
    [Server] public void ClearScore(GameObject PlayerGO)
    {
        if (!Players.Exists(x => x.PlayerIdentity.gameObject == PlayerGO)) throw new System.NullReferenceException($"{PlayerGO} �� �������� �������� �� ������ ������ � ������.");

        Players.Find(x => x.PlayerIdentity.gameObject == PlayerGO).SetScore_Server(0);

        RpcUpdateScoreboards();
    }
    /// <summary> C����� ������� ����� ������� ������. </summary>
    [Server] public void ClearAllScores()
    {
        foreach(var i in Players)
        {
            i.SetScore_Server(0);
        }
        RpcUpdateScoreboards();
    }

    /// <summary> ��������� ���� ��� ������� �������. </summary>
    [ClientRpc] public void RpcUpdateScoreboards()
    {
        ScoreBoardUI.main.RedrawScoreboard();
    }

    /// <summary> �������� ������� ������ ����������. </summary>
    [ClientRpc]
    public void RpcShowWinnerUI(string winnerName)
    {
        WinnerUI.main.T_WinnerName.text = winnerName;
        WinnerUI.main.SetActive(true);
    }
    /// <summary> ������ WinnerUI ��� ������� �������. </summary>
    [ClientRpc]
    public void RpcHideWinnerUI()
    {
        WinnerUI.main.SetActive(false);
    }

    // >���������� ���������� � ������ ������ ��� Rpc �� ������������ �������� �������.
    /// <summary> �������� � ������� ������ GameLogic.Players, � ������ �� �����. ������ ��� ����� ������ ��� �����������/���������� ������ � �������. </summary>
    [ClientRpc] public void RpcSyncPlayerLists(NetworkPlayer[] newList)
    {
        main.Players.Clear();
        main.Players = newList.ToList();
    }

    /// <summary>
    /// ���� ����� ����������� �������� ������ NetworkPlayer.PlayerIdentity �� "playerScene" ��� ������� �������.
    /// </summary>
    /// <param name="player">������ � ������� ������ �� ������� � �������� "NetworkPlayer"</param>
    /// <param name="playerScene">NetworkIdentity, ������� ����������� ��� ���������� ������ �� �����.</param>
    [ClientRpc]
    public void RpcSyncScenePlayerVar(NetworkPlayer player, NetworkIdentity playerScene)
    {
        player.PlayerIdentity = playerScene;
    }
}
