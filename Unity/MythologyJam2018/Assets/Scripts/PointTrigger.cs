using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerManager playerManager;
        PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
        if (playerController && PlayerManager.TryGetInstance(out playerManager))
        {
            playerManager.RegisterPointTrigger(playerController);   
        }
    }
}
