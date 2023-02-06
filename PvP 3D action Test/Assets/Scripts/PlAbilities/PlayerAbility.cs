using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Надкласс, который наследуют все способности персонажа. (В целях ТЗ этот класс не так важен, но при дальнейшем развитии игры эта иерархия упростит дальнейшую работу)
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

    /// <summary> По умолчанию здесь просто вычитаются все активные таймеры. </summary>
    public virtual void Update()
    {
        if (CooldownTimer > 0) CooldownTimer -= Time.deltaTime;
        if (DurationTimer > 0) DurationTimer -= Time.deltaTime;
    }

    /// <summary> Основной метод активации способности. </summary>
    /// <returns> Была ли активация способности успешной? </returns>
    public abstract bool UseAbility();

    /// <summary> Выполняет проверку, возможно ли активировать способность в данный момент. Вернёт false, если нет. </summary>
    public abstract bool CanAbilityBeUsed { get; }

    /// <summary> Вернёт true, если на данный момент способность активна/находится в процессе действия. </summary>
    public abstract bool IsActive { get; } //Чаще всего это просто "DurationTimer > 0", но иногда может потребоваться специфическая логика.
}
