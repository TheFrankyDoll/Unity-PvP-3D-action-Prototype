using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// ���� ����� ������ ��� ������ � ������������ ������.
/// ��� ��������� ������ ���� ����������������� ����� ��������� (����� SyncVar ��� Rpc).
/// </summary>
public class NetworkPlayer : NetworkBehaviour
{
    /// <summary> "NetworkIdentity", ������� ��������� �� ������� ������ �� �����. ���������������� ����� "GameLogic.main.RpcSyncScenePlayerVar" </summary>
    public NetworkIdentity PlayerIdentity;

    public GameObject PlayerPrefab; //������ ������, ������� ����� ������ �� �����. ������������ � Editor.


    //����� ��������, ��� ������������� ���������� ��� ������������� ����� SyncVar:

    /// <summary> [SyncVar] ��� ����� ������. </summary>
    [field: SerializeField] public string PlayerName { get; private set; }                   //<- 1. ��������, �� �������� ������� ����� �������� ��������.
    [SyncVar(hook = nameof(SyncPlayerName))] string _PlayerName;                             //<- 2. ����������, ������� ��������� ������ ������.
    [Client] void SyncPlayerName(string oldValue, string newValue)                           //<- 3. ��� �� �������� ������ ����� ����� ��������. ���������� � ������� �������.
    { 
        PlayerName = newValue;
        //���������� ������� ��� ����� ����������� ������� => �������� � ������� �� �������� ��������� �� ��������� ����� � ��� ������ �� ������������.
        ScoreBoardUI.main.UpdateScoreboardData(this); 
    } 
    [Server] public void SetPlayerName_Server(string newValue) => _PlayerName = newValue;    //<- 4. ������ ������ ����� �������� ���������� ��� ���� ��������.

    //��� ���������� ���� ����� �������������� ������ �������.

    /// <summary> [SyncVar] ������� ���������� ����� � ����� ������. </summary>
    [field: SerializeField] public int Score { get; private set; }
    [SyncVar(hook = nameof(SyncScore))] int _Score;
    [Client] void SyncScore(int oldValue, int newValue) => Score = newValue;
    [Server] public void SetScore_Server(int newValue)  // <- ����� ����� ��������������, ���� �� ����������.
    {
        _Score = newValue;
        if(_Score >= GameLogic.main.HitsToWin) 
        {
            GameLogic.main.RpcShowWinnerUI(PlayerName);
            GameLogic.main.NextMatchTimer = GameLogic.main.NextMatchTime;
            GameLogic.main.NextMatchCountdown = true;
        }
    }

}