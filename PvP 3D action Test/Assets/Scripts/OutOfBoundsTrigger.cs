using UnityEngine;
using Mirror;

/// <summary>
/// ���������� ����� ������� ������ � NetworkTransform �� ��������� �����.
/// </summary>
public class OutOfBoundsTrigger : NetworkBehaviour
{
    [ServerCallback]
    private void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.GetComponent<NetworkTransformBase>()) MyNetworkManager.singletonModed.TeleportToFreeSpawnPoint(coll.gameObject);
    }
}
