using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DashAbility : PlayerAbility
{
    [Header("Опции Рывка")]
    public float DashForce;
    public float DashUpwardForce;
    [Space(5)]
    public bool AllowDashInMidair = true;
    [Space(5)]
    public float DashSpeedCap = 25;
    public float DashSpeedCapY = 7;

    public override void Awake()
    {
        base.Awake();

        //Записываем себя в способности игрока.
        if (!plMain.Abilities.Dash) plMain.Abilities.Dash = this;
        else Destroy(this);
    }

    public override bool IsActive => DurationTimer > 0;

    public override void Update()
    {
        if (!isLocalPlayer) return;
        base.Update();

        if (Input.GetKeyDown(ActivationKey)) UseAbility();
        if(DurationTimer > 0)
        {
            //Пока способность активна, применяем ограничение скорости по оси Y (чтобы игрок не улетел в космос, используя дэш на рампах).
            if (DashSpeedCapY != 0 && plMover.rb.velocity.y > DashSpeedCapY) // 0 игнорирует ограничение, на случай если мы ХОТИМ, чтобы игрок летел в космос.
            {
                plMover.rb.velocity = new Vector3(plMover.rb.velocity.x, DashSpeedCapY, plMover.rb.velocity.z);
            }
        }
    }

    public override bool UseAbility()
    {
        if (!CanAbilityBeUsed) return false;

        Vector3 forceDir = plMover.InputDirection * DashForce + plMover.transform.up * DashUpwardForce;
        plMover.rb.AddForce(forceDir, ForceMode.Impulse);


        CooldownTimer = Cooldown;
        DurationTimer = Duration;
        return true;
    }

    public void OnCollisionEnter(Collision collision)
    {      
        if (!IsActive) return; 
        if (collision.gameObject.GetComponent<PlayerMovementController>())
        {
            CmdCountPlayerHit(collision.gameObject);
        }
    }

    [Command]
    public void CmdCountPlayerHit(GameObject Target)
    {
        if (Target.GetComponent<PlayerMovementController>().Visuals.ServerInvulTimer > 0) return;

        Target.GetComponent<PlayerMovementController>().Visuals.ServerInvulTimer = GameLogic.main.InvulnerabilityTime;
        Target.GetComponent<PlayerMovementController>().Visuals.ServerSetInvulnerable(true);

        GameLogic.main.AddPoint(gameObject);

        GameLogic.main.RpcUpdateScoreboards();
    }

    public override bool CanAbilityBeUsed
    {   
        get
        {
            //Способность НЕЛЬЗЯ активировать, если:
            if (CooldownTimer > 0 ||                           //На перезарядке.
                plMover.InputDirection.magnitude < 0.3f ||     //Игрок не движется в каком-либо направлении.
                (!AllowDashInMidair && !plMover.IsGrounded))   //Игрок не на земле и рывок в полёте запрещён.
                return false;
            else return true;

            //(Можно записать одной строчкой, но тогда пострадает читабельность)
        }
    }
}
