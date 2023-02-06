using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// ���� ����� �������� �� �������� ���������� ���������. 
/// � ������ ������ ���������� ��������� ��������� �������.
/// </summary>
public class PlayerMovementController : NetworkBehaviour
{
    /// <summary> ������ �� PlayerObject ����� ������. </summary>
    public PlayerObject main;

    /// <summary> �������� ����� �������. ������������ ��� ��������� ������ � �����. </summary>
    public Collider PlCollider;
    /// <summary> Rigidbody ����� ���������. </summary>
    [HideInInspector]public Rigidbody rb;
    /// <summary> PlayerVisuals ����� ���������. </summary>
    public PlayerVisuals Visuals;

    [Header("Movement")]
    public Transform MovementDirection;
    public float DragOnGround = 5;
    [Range(0, 90f)] public float MaxSlopeAngle = 35f;

    [Space(5)]
    [Tooltip("���������� ��������� ��������� ��� ������������ �� ������������.")]
    public float MovespeedAcceleration = 450;
    public float DefaultMovespeedCap = 7;

    [Tooltip("������������ �������� �������� �� ���������� �������� (0 = ��� �����������). ����� ���� �������������� �������������. �� ��������� ������������ ��������.")]
    public float CurrentMovespeedCap;
    public bool IgnoreSpeedCap = false;

    [Header("GroundCheck")]
    public LayerMask GroundLayers;
    [Tooltip("���������� ��������� �� �����������, �� ������� �������� ����� ��������� �����������. ������ (������_������ / 2 + ~0.2f)")]
    public float GroundCheckLength = 1.2f;

    public bool IsGrounded;
    public RaycastHit GroundCheckHit;



    float horizontalInput;
    float verticalInput;

    public override void OnStartLocalPlayer()
    {
        enabled = true;
    }

    void Start()
    {
        main = GetComponent<PlayerObject>();
        rb = GetComponent<Rigidbody>(); 
        if (!Visuals) Visuals = transform.GetChild(0).GetComponent<PlayerVisuals>();
        if (!PlCollider) PlCollider = GetComponent<Collider>();
    }

    public void FixedUpdate()
    {
        ApplyInput();
        if (!IgnoreSpeedCap) ApplySpeedCaps();

        //GroundCheck
        IsGrounded = Physics.Raycast(transform.position, Vector3.down, out GroundCheckHit, GroundCheckLength);

        if (IsGrounded) rb.drag = DragOnGround;
        else rb.drag = 0f;


        //todo: ������ ����� ����� ������ �� �����, �������� � � �������. ������� ��������� PhysicMaterial ��� ����� ��������.
    }

    // �� ����������� � ��, �� ����� ������������� ���� ����� �������� GroundCheck � ���������� ������ �������, �� �������� �� FixedUpdate.
    public bool DoGroundCheck()
    {
        IsGrounded = Physics.Raycast(transform.position, Vector3.down, out GroundCheckHit, GroundCheckLength);
        return IsGrounded;
    }

    public void Update()
    {
        MovementDirection.rotation = CameraController.main.transform.rotation;
        MovementDirection.rotation = Quaternion.Euler(0, MovementDirection.eulerAngles.y, MovementDirection.eulerAngles.z);
    }

    public bool OnSlope
    {
        get
        {
            if (IsGrounded)
            {
                float angle = Vector3.Angle(Vector3.up, GroundCheckHit.normal);
                return angle < MaxSlopeAngle && angle != 0;
            }
            return false;
        }
    }

    /// <summary>
    /// [������ ��������� �����] ���������� ������, � ����������� �������� ����� ��������� �������� ��������� ��������� ������ � WASD.
    /// </summary>
    public Vector3 InputDirection
    {
        get
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput = Input.GetAxisRaw("Vertical");

            return (MovementDirection.forward * verticalInput + MovementDirection.right * horizontalInput).normalized;
        }
    }

    void ApplyInput()
    {
        Vector3 forceDir = InputDirection;
        if (OnSlope)
        {
            forceDir = Vector3.ProjectOnPlane(forceDir, GroundCheckHit.normal);
        }

        rb.AddForce(forceDir.normalized * MovespeedAcceleration, ForceMode.Force);
    }

    /// <summary>
    /// ���������� ���������� ����������� �������� � ��������� ��� � Rigidbody.
    /// </summary>
    public void ApplySpeedCaps()
    {
        //���������� ���������� �����������.
        if (main.Abilities.Dash && main.Abilities.Dash.DurationTimer > 0)
        {
            CurrentMovespeedCap = main.Abilities.Dash.DashSpeedCap;
        }
        else CurrentMovespeedCap = DefaultMovespeedCap;

        //��������� �����������.
        if (CurrentMovespeedCap == 0) return;

        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVel.magnitude > CurrentMovespeedCap)
        {
            Vector3 limitedVel = flatVel.normalized * CurrentMovespeedCap;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }
}
