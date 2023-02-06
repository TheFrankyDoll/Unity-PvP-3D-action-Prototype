using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Этот класс отвечает за основное управление персонажа. 
/// В данном случае управление полностью доверенно клиенту.
/// </summary>
public class PlayerMovementController : NetworkBehaviour
{
    /// <summary> Ссылка на PlayerObject этого игрока. </summary>
    public PlayerObject main;

    /// <summary> Колайдер этого Объекта. Используется для изменения трения в полёте. </summary>
    public Collider PlCollider;
    /// <summary> Rigidbody этого персонажа. </summary>
    [HideInInspector]public Rigidbody rb;
    /// <summary> PlayerVisuals этого персонажа. </summary>
    public PlayerVisuals Visuals;

    [Header("Movement")]
    public Transform MovementDirection;
    public float DragOnGround = 5;
    [Range(0, 90f)] public float MaxSlopeAngle = 35f;

    [Space(5)]
    [Tooltip("Определяет ускорение персонажа при передвижении по поверхностям.")]
    public float MovespeedAcceleration = 450;
    public float DefaultMovespeedCap = 7;

    [Tooltip("Ограничевает скорость движения до указанного значения (0 = без ограничения). Может быть переопределена способностями. Не учитывает вертикальную скорость.")]
    public float CurrentMovespeedCap;
    public bool IgnoreSpeedCap = false;

    [Header("GroundCheck")]
    public LayerMask GroundLayers;
    [Tooltip("Определяет растояние от поверхности, на котором персонаж будет считаться приземлённым. Обычно (Высота_игрока / 2 + ~0.2f)")]
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


        //todo: Сейчас игрок может висеть на стене, двигаясь в её сторону. Поменяв параметры PhysicMaterial это можно избежать.
    }

    // Не ипользуется в ТЗ, но может потребоваться если нужно провести GroundCheck в КОНКРЕТНЫЙ момент времени, не опираясь на FixedUpdate.
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
    /// [ТОЛЬКО ЛОКАЛЬНЫЙ ИГРОК] Возвращяет вектор, в направлении которого игрок совершает движение используя положение камеры и WASD.
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
    /// Определяет актуальное ограничение скорости и применяет его к Rigidbody.
    /// </summary>
    public void ApplySpeedCaps()
    {
        //Определяем актуальное ограничение.
        if (main.Abilities.Dash && main.Abilities.Dash.DurationTimer > 0)
        {
            CurrentMovespeedCap = main.Abilities.Dash.DashSpeedCap;
        }
        else CurrentMovespeedCap = DefaultMovespeedCap;

        //Применяем ограничение.
        if (CurrentMovespeedCap == 0) return;

        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVel.magnitude > CurrentMovespeedCap)
        {
            Vector3 limitedVel = flatVel.normalized * CurrentMovespeedCap;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }
}
