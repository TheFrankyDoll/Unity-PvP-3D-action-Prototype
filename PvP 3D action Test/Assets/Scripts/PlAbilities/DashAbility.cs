using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DashAbility : PlayerAbility
{
    [Header("����� �����")]
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

        //���������� ���� � ����������� ������.
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
            //���� ����������� �������, ��������� ����������� �������� �� ��� Y (����� ����� �� ������ � ������, ��������� ��� �� ������).
            if (DashSpeedCapY != 0 && plMover.rb.velocity.y > DashSpeedCapY) // 0 ���������� �����������, �� ������ ���� �� �����, ����� ����� ����� � ������.
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
            //����������� ������ ������������, ����:
            if (CooldownTimer > 0 ||                           //�� �����������.
                plMover.InputDirection.magnitude < 0.3f ||     //����� �� �������� � �����-���� �����������.
                (!AllowDashInMidair && !plMover.IsGrounded))   //����� �� �� ����� � ����� � ����� ��������.
                return false;
            else return true;

            //(����� �������� ����� ��������, �� ����� ���������� �������������)
        }
    }
}
