using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Этот класс отвечает за визуальное отображения игрока (для всех на сервере). 
/// При развитии проекта этот скрипт отвечал бы и за его анимации и пр.
/// </summary>
public class PlayerVisuals : NetworkBehaviour
{
    /// <summary> [ТОЛЬКО СЕРВЕР] Таймер бессмертия. По достижению 0 сервер обновит параметр "Invulnerable", доступный клиентам. </summary>
    public float ServerInvulTimer;

    public Material PlMaterial;
    public GameObject VisualGO;
    public bool Invulnerable;

    public Color DefaultColor;
    public Color InvulColor;

    [SyncVar(hook = nameof(SyncInvulnerable))] bool _Invulnerable;

    // Метод не выполнится, если старое значение равно новому.
    void SyncInvulnerable(bool oldValue, bool newValue)
    {
        Invulnerable = newValue;
        CmdUpdateColor();
    }

    /// <summary>
    /// Запрос серверу чтобы обновить цвет игрока у всех клиентов.
    /// </summary>
    [Command(requiresAuthority = false)]
    public void CmdUpdateColor()
    {
        //Проверка на читы не требуется, все данные сервер хранит у себя.

        PlMaterial.SetColor("_Color", _Invulnerable ? InvulColor : DefaultColor); //Меняем параметр у сервера
        RpcUpdateColor(); //И у всех остальных
    }
    [ClientRpc]
    public void RpcUpdateColor()
    {
        PlMaterial.SetColor("_Color", _Invulnerable ? InvulColor : DefaultColor);
    }

    private void OnEnable()
    {
        PlMaterial = VisualGO.GetComponent<MeshRenderer>().material;
    }

    [ServerCallback]
    public void FixedUpdate()
    {
        ServerTickInvulTimer();
    }


    /// <summary>
    /// Сервер должен передать, сколько времени нужно отнять от этого таймера.
    /// </summary>
    [Server]
    public void ServerTickInvulTimer()
    {
        if (ServerInvulTimer <= 0) return;
        ServerInvulTimer -= Time.fixedDeltaTime;
        if (ServerInvulTimer < 0)
        {
            ServerInvulTimer = 0;
            _Invulnerable = false;
        }
    }
    /// <summary>
    /// Сервер должен передать, сколько времени нужно отнять от этого таймера.
    /// </summary>
    [Server]
    public void ServerSetInvulnerable(bool newValue)
    {
        _Invulnerable = newValue;
    }
}
