using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Этот класс хранит все данные о подключенном игроке.
/// Все параметры должны быть синхронизированны между клиентами (через SyncVar или Rpc).
/// </summary>
public class NetworkPlayer : NetworkBehaviour
{
    /// <summary> "NetworkIdentity", который находится на объекте играка на сцене. Синхронизируется через "GameLogic.main.RpcSyncScenePlayerVar" </summary>
    public NetworkIdentity PlayerIdentity;

    public GameObject PlayerPrefab; //Префаб игрока, который будет создан на сцене. Записываятся в Editor.


    //Здесь показано, как сериализуются переменные для синхронизации через SyncVar:

    /// <summary> [SyncVar] Имя этого игрока. </summary>
    [field: SerializeField] public string PlayerName { get; private set; }                   //<- 1. Свойство, из которого клиенты могут получать значение.
    [SyncVar(hook = nameof(SyncPlayerName))] string _PlayerName;                             //<- 2. Переменная, которую обновляет только сервер.
    [Client] void SyncPlayerName(string oldValue, string newValue)                           //<- 3. Хук по которому сервер выдаёт новое значение. Вызывается у каждого клиента.
    { 
        PlayerName = newValue;
        //Приходится вызвать при новом подключении клиента => значения с сервера не успевают поступать до отрисовки счёта и имя игрока не отображается.
        ScoreBoardUI.main.UpdateScoreboardData(this); 
    } 
    [Server] public void SetPlayerName_Server(string newValue) => _PlayerName = newValue;    //<- 4. Отсюда сервер может изменить переменную для всех клиентов.

    //Все переменные ниже будут сериализованны схожим образом.

    /// <summary> [SyncVar] Текущее количество очков у этого игрока. </summary>
    [field: SerializeField] public int Score { get; private set; }
    [SyncVar(hook = nameof(SyncScore))] int _Score;
    [Client] void SyncScore(int oldValue, int newValue) => Score = newValue;
    [Server] public void SetScore_Server(int newValue)  // <- Здесь также просчитывается, есть ли победитель.
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