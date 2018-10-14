using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaliceTriggerZone : MonoBehaviour
{
    public ChaliceController chaliceManager;

    private void OnTriggerEnter(Collider collider)
    {
        PlayerController playerHit = collider.gameObject.GetComponent<PlayerController>();
        if (playerHit)
        {
            chaliceManager.OnEnterTriggerZone(playerHit);
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        PlayerController playerHit = collider.gameObject.GetComponent<PlayerController>();
        if (playerHit)
        {
            chaliceManager.OnExitTriggerZone(playerHit);
        }
    }
}
