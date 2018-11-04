using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonkWall : MonoBehaviour
{
    public Vector3 wallNormal;

    private void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position, wallNormal, Color.blue);
    }

    private void OnCollisionEnter(Collision collision)
    {
        PlayerController playerHit = collision.gameObject.GetComponent<PlayerController>();

        if (playerHit != null)
        {
            PlayerManager playerManager;
            if (PlayerManager.TryGetInstance(out playerManager))
            {
                playerManager.RegisterPlayerWallCollisionThisFrame(playerHit, this);
            }
        }
    }
}
