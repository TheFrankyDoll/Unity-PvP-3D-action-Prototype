using UnityEngine;
using Mirror;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Надстройка NetworkManager-а.
/// </summary>
public class MyNetworkManager : NetworkManager
{
    public static MyNetworkManager singletonModed;

    /// <summary>
    /// Список ещё не занятах стартовых позиций. Заполняется заново как только всем места использованны или в начале раунда.
    /// </summary>
    public List<Transform> FreeSpawnPoints = new List<Transform>();

    public override void Awake()
    {
        base.Awake();
        singletonModed = this;
    }

    [Server]
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // 1. Создаём глобальный скрипт игрока.
        NetworkPlayer netPlayer = Instantiate(playerPrefab).GetComponent<NetworkPlayer>();
        netPlayer.gameObject.name = $"{netPlayer.PlayerName} [connId={conn.connectionId}]";

        // 2. Создаём игрока на сцене (это новый игрок, так что точка спауна выбирается случайно и без проверок как при старте нового раунда)
        Transform spPoint = startPositions[Random.Range(0, startPositions.Count)];

        GameObject ScenePlayer = Instantiate(netPlayer.PlayerPrefab, spPoint.position, spPoint.rotation);
        netPlayer.PlayerIdentity = ScenePlayer.GetComponent<NetworkIdentity>();


        // 3. Инициализируем игрока для сервера.

        //Может быть неочевидно, но "основным" считается игрок на сцене, который создаётся через netPlayer.PlayerPrefab.
        //Так он получит "авторитет клиента" и сможет ловить OnLocalPlayer и т.п.
        //NetworkPlayer же является просто хранилищем данных, которое обновляет сервер. Это хранилище так же удаляется при отключении игрока от сервера.
        NetworkServer.Spawn(netPlayer.gameObject);                //<- Принадлежит серверу
        NetworkServer.AddPlayerForConnection(conn, ScenePlayer);  //<- Принадлежит клиенту

        // 4. Добавляем мгрока в список (список у сервера!) и передаём новый список всем клиентам через Rpc.
        GameLogic.main.Players.Add(netPlayer);
        GameLogic.main.RpcSyncPlayerLists(GameLogic.main.Players.ToArray());

        foreach (var i in GameLogic.main.Players)
        {
            GameLogic.main.RpcSyncScenePlayerVar(i, i.PlayerIdentity);
        }
    }  

    [Server]
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        NetworkPlayer target = GameLogic.main.Players.First(x => x.PlayerIdentity == conn.identity);
        NetworkServer.Destroy(target.gameObject);
        GameLogic.main.Players.Remove(target);
        

        GameLogic.main.RpcSyncPlayerLists(GameLogic.main.Players.ToArray());
        NetworkServer.DestroyPlayerForConnection(conn);

        GameLogic.main.RpcUpdateScoreboards();
    }

    public override void OnClientDisconnect()
    {
        foreach(var i in GameLogic.main.Players) { Destroy(i.gameObject); }
        GameLogic.main.Players.Clear();
        ScoreBoardUI.main.WipeScoreboard();

        CameraController.main.PreConnection = true;
        PreConnectionUI.main.SetActive(true);
        AbilitiesUI.main.SetActive(false);
        Cursor.visible = true;
    }


    /// <summary>
    /// Модифицированный метод вернёт случайную точку из тех, которые ещё никто не занимал.
    /// </summary>
    /// <returns></returns>
    public override Transform GetStartPosition()
    {
        if (startPositions.Count == 0) return null;

        if (FreeSpawnPoints.Count == 0)
        {
            //Перезаполняем список из "startPositions"

            startPositions.RemoveAll(t => t == null); //чистим пустышки

            foreach (var i in startPositions) FreeSpawnPoints.Add(i);
        }
        Transform spwnPoint = FreeSpawnPoints[Random.Range(0, FreeSpawnPoints.Count)]; //Выбираем случайную точку.

        FreeSpawnPoints.Remove(spwnPoint); // Удаляем её из списка свободных.

        return spwnPoint;
    }

    /// <summary>
    /// Сервер отправит указанный объект на случайную стартовую позицию, которую ещё никто не занимал.
    /// </summary>
    /// <param name="Target">Объект для телепортации.</param>
    [Server] public void TeleportToFreeSpawnPoint(GameObject Target)
    {
        Transform spawnPoint = GetStartPosition();

        Target.GetComponent<NetworkTransform>().RpcTeleport(spawnPoint.position, spawnPoint.rotation);
    }

    /// <summary>
    /// Сервер начнёт следующий раунд, обнулит счёт каждого игрока и отправит его на новые координаты.
    /// </summary>
    [Server]
    public void BeginNextRound()
    {
        //Перезаполняем свободных точек список из "startPositions"
        FreeSpawnPoints.Clear();
        startPositions.RemoveAll(t => t == null); //чистим пустышки
        foreach (var i in startPositions) FreeSpawnPoints.Add(i);

        foreach(var player in GameLogic.main.Players)
        {
            player.SetScore_Server(0);

            TeleportToFreeSpawnPoint(player.PlayerIdentity.gameObject);
        }

        GameLogic.main.RpcHideWinnerUI();
        GameLogic.main.RpcUpdateScoreboards();
    }



    //
    //
    //     ░░░░░░░░░░░▄▀▄▀▀▀▀▄▀▄░░░░░░░░░░░░░░░░░░ 
    //     ░░░░░░░░░░░█░░░░░░░░▀▄░░░░░░▄░░░░░░░░░░ 
    //     ░░░░░░░░░░█░░▀░░▀░░░░░▀▄▄░░█░█░░░░░░░░░ 
    //     ░░░░░░░░░░█░▄░█▀░▄░░░░░░░▀▀░░█░░░░░░░░░ 
    //     ░░░░░░░░░░█░░▀▀▀▀░░░░░░░░░░░░█░░░░░░░░░ 
    //     ░░░░░░░░░░█░░░░░░░░░░░░░░░░░░█░░░░░░░░░ 
    //     ░░░░░░░░░░█░░░░░░░░░░░░░░░░░░█░░░░░░░░░ 
    //     ░░░░░░░░░░░█░░▄▄░░▄▄▄▄░░▄▄░░█░░░░░░░░░░ 
    //     ░░░░░░░░░░░█░▄▀█░▄▀░░█░▄▀█░▄▀░░░░░░░░░░ 
    //     ░░░░░░░░░░░░▀░░░▀░░░░░▀░░░▀░░░░░░░░░░░░ 
    //     ╔═════════════════════════════════════╗
    //     ║ * Однажды здесь будет находится     ║
    //     ║   полезный код.                     ║
    //     ║ * Но пока тут есть только пёс.      ║
    //     ╚═════════════════════════════════════╝
    //
    //
}
