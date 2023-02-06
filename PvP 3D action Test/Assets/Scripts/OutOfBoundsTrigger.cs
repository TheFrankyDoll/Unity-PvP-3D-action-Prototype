using UnityEngine;
using Mirror;

/// <summary>
/// Отправляет любой упавший объект с NetworkTransform на случайную точку.
/// </summary>
public class OutOfBoundsTrigger : NetworkBehaviour
{
    [ServerCallback]
    private void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.GetComponent<NetworkTransformBase>()) MyNetworkManager.singletonModed.TeleportToFreeSpawnPoint(coll.gameObject);
    }
}
