using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// ���� ����� �������� �� ���������� ����������� ������ (��� ���� �� �������). 
/// ��� �������� ������� ���� ������ ������� �� � �� ��� �������� � ��.
/// </summary>
public class PlayerVisuals : NetworkBehaviour
{
    /// <summary> [������ ������] ������ ����������. �� ���������� 0 ������ ������� �������� "Invulnerable", ��������� ��������. </summary>
    public float ServerInvulTimer;

    public Material PlMaterial;
    public GameObject VisualGO;
    public bool Invulnerable;

    public Color DefaultColor;
    public Color InvulColor;

    [SyncVar(hook = nameof(SyncInvulnerable))] bool _Invulnerable;

    // ����� �� ����������, ���� ������ �������� ����� ������.
    void SyncInvulnerable(bool oldValue, bool newValue)
    {
        Invulnerable = newValue;
        CmdUpdateColor();
    }

    /// <summary>
    /// ������ ������� ����� �������� ���� ������ � ���� ��������.
    /// </summary>
    [Command(requiresAuthority = false)]
    public void CmdUpdateColor()
    {
        //�������� �� ���� �� ���������, ��� ������ ������ ������ � ����.

        PlMaterial.SetColor("_Color", _Invulnerable ? InvulColor : DefaultColor); //������ �������� � �������
        RpcUpdateColor(); //� � ���� ���������
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
    /// ������ ������ ��������, ������� ������� ����� ������ �� ����� �������.
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
    /// ������ ������ ��������, ������� ������� ����� ������ �� ����� �������.
    /// </summary>
    [Server]
    public void ServerSetInvulnerable(bool newValue)
    {
        _Invulnerable = newValue;
    }
}
