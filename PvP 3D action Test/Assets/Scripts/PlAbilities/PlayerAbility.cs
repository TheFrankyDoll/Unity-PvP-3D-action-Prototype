using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// ��������, ������� ��������� ��� ����������� ���������. (� ����� �� ���� ����� �� ��� �����, �� ��� ���������� �������� ���� ��� �������� �������� ���������� ������)
/// </summary>
//[RequireComponent(typeof(PlayerController1))]
public abstract class PlayerAbility : NetworkBehaviour
{
    [HideInInspector]public PlayerObject plMain;
    [HideInInspector]public PlayerMovementController plMover;

    public Sprite UIAbilitySprite;
    public KeyCode ActivationKey;
    [Space(5)]
    public float Cooldown = 1;
    public float CooldownTimer;
    [Space(5)]
    public float Duration;
    public float DurationTimer;

    public virtual void Awake()
    {
        if (!plMover) plMover = GetComponent<PlayerMovementController>();
        if (!plMain) plMain = GetComponent<PlayerObject>();
    }
    public override void OnStartLocalPlayer()
    {
        enabled = true;
    }

    /// <summary> �� ��������� ����� ������ ���������� ��� �������� �������. </summary>
    public virtual void Update()
    {
        if (CooldownTimer > 0) CooldownTimer -= Time.deltaTime;
        if (DurationTimer > 0) DurationTimer -= Time.deltaTime;
    }

    /// <summary> �������� ����� ��������� �����������. </summary>
    /// <returns> ���� �� ��������� ����������� ��������? </returns>
    public abstract bool UseAbility();

    /// <summary> ��������� ��������, �������� �� ������������ ����������� � ������ ������. ����� false, ���� ���. </summary>
    public abstract bool CanAbilityBeUsed { get; }

    /// <summary> ����� true, ���� �� ������ ������ ����������� �������/��������� � �������� ��������. </summary>
    public abstract bool IsActive { get; } //���� ����� ��� ������ "DurationTimer > 0", �� ������ ����� ������������� ������������� ������.
}
