using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Практичней было бы не изобретать велосипед и использовать Cinemachine, но это против правил ТЗ - так что держите самодельную версию :)
/// </summary>
public class CameraController : MonoBehaviour
{
    /// <summary> [Синглтон] Ссылка на единственную камеру с этим скриптом. </summary>
    public static CameraController main;
    public Camera Camera;
    public LayerMask CollisionCheckMask;


    /// <summary>Состояние камеры перед подключением к игре. Просто будет висеть в воздухе, смотря в небо. </summary>
    public bool PreConnection = true; //Только в рамках ТЗ, в игре лобби логичнее делать на отдельной сцене.
    Vector3 prePosition;
    Quaternion preRotation;

    [Space(5)]
    public float HeightOffset = 0.12f;
    public Vector3 TopOrbit;
    public Vector3 MidOrbit;
    public Vector3 LowOrbit;
    public bool FlipOrbitX;

    [Space(5)]
    public Transform FollowBy;
    public float Damping = 5f;

    [Space(5)]
    public Vector3 FollowByVelocity;
    public float CompensateVelocity = 2;

    Vector3 followByLastFramePos;

    [Space(5)]
    public float SensitivityX = 400;
    public float SensitivityY = 300;

    float yRotation;
    float xRotation;

    void Awake()
    {
        if (!main) main = this;
        else
        {
            Debug.LogWarning("Было созданно более одной камеры 3-его лица. Такого не должно происходить.");
            Destroy(gameObject);
        }

        //
        prePosition = transform.position;
        preRotation = transform.rotation;
        //
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }

    void Update()
    {
        if(PreConnection)
        {
            transform.position = Vector3.Lerp(transform.position, prePosition, Time.deltaTime * Damping);
            transform.rotation = Quaternion.Lerp(transform.rotation, preRotation, Time.deltaTime * Damping);

            return;
        }

        //
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * SensitivityX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * SensitivityY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        

        if (!FollowBy)
        {
            //Debug.LogWarning("У камеры нет цели для следования."); // <-- Если необходимо.
            return;
        }


        Vector3 finalPosition = new Vector3(FollowBy.position.x, FollowBy.position.y + HeightOffset, FollowBy.position.z);        
        transform.position = Vector3.Lerp(transform.position, finalPosition, Time.deltaTime * Damping);

        //Мы можем определить вектор направления движения любого объекта основываясь на его положении в прошлом кадре.
        FollowByVelocity = FollowBy.transform.position - followByLastFramePos;
        if (CompensateVelocity > 0)
        {
            transform.position += FollowByVelocity * CompensateVelocity;
        }

        followByLastFramePos = FollowBy.transform.position;

        //Связываем высоту камеры и подгоняем её под "орбиту".

        Vector3 _TopOrbit = TopOrbit;
        Vector3 _MidOrbit = MidOrbit;
        Vector3 _LowOrbit = LowOrbit;
        if(FlipOrbitX)
        {
            _TopOrbit.x *= -1;
            _MidOrbit.x *= -1;
            _LowOrbit *= -1;
        }

        float deltaHeight;
        float angleWithNegative = (transform.eulerAngles.x > 180) ? transform.eulerAngles.x - 360 : transform.eulerAngles.x;
        Vector3 CameraLocalPos;
        if (angleWithNegative >= 0)
        {
            deltaHeight = angleWithNegative / 90f;
            CameraLocalPos = Vector3.Slerp(_MidOrbit, _TopOrbit, deltaHeight);
        }
        else
        {
            deltaHeight = 1f - (angleWithNegative * -1 / 90f);
            CameraLocalPos = Vector3.Slerp(_LowOrbit, _MidOrbit, deltaHeight);
        }
        //Проверяем, не проходит ли камера сквозь стену.
        RaycastHit wallCheck;
        if (Physics.Linecast(transform.position, Camera.transform.TransformPoint(CameraLocalPos), out wallCheck, CollisionCheckMask))
        {
            CameraLocalPos = Camera.transform.InverseTransformPoint(wallCheck.point);
            //Camera.transform.localPosition = Camera.transform.InverseTransformPoint(wallCheck.point);
        }
        Camera.transform.localPosition = Vector3.Lerp(Camera.transform.localPosition, CameraLocalPos, Time.deltaTime * Damping);
    }
}
