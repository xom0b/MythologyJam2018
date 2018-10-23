using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Debug")]
    public bool showDebug;
    public GameObject debugSphere1;
    public GameObject debugSphere2;

    // private variables
    private PlayerPlayerCollisionInfo playerPlayerCollisionThisFrame = null;
    private List<PlayerChaliceCollisionInfo> playerChaliceCollisionsThisFrame = new List<PlayerChaliceCollisionInfo>();

    // private debug
    private Vector3 debugPosition;
    private Vector3 debugDirection;

    #region Classes
    private class PlayerPlayerCollisionInfo
    {
        public GameObject registeredCollision;
        public GameObject collidedWith;

        public PlayerPlayerCollisionInfo(GameObject registeredCollision, GameObject collidedWith)
        {
            this.registeredCollision = registeredCollision;
            this.collidedWith = collidedWith;
        }

        public override string ToString()
        {
            return registeredCollision.name + " " + collidedWith.name;
        }
    }

    private class PlayerChaliceCollisionInfo
    {
        public GameObject chalice;
        public GameObject player;

        public PlayerChaliceCollisionInfo(GameObject chalice, GameObject player)
        {
            this.chalice = chalice;
            this.player = player;
        }

        public override string ToString()
        {
            return chalice.name + " " + player.name;
        }
    }
    #endregion

    #region Singleton
    private static PlayerManager instance;

    private void Awake()
    {
        instance = this;
    }

    public static bool TryGetInstance(out PlayerManager manager)
    {
        manager = instance;
        return manager != null;
    }
    #endregion

    #region MoboBehaviour

    private void Start()
    {
        if (showDebug)
        {
            debugSphere1.SetActive(true);
            debugSphere2.SetActive(true);
        }
        else
        {
            debugSphere1.SetActive(false);
            debugSphere2.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (playerPlayerCollisionThisFrame != null)
        {
            ProcessPlayerPlayerCollision(playerPlayerCollisionThisFrame);
        }

        if (playerChaliceCollisionsThisFrame.Count > 0)
        {
            foreach(PlayerChaliceCollisionInfo playerChaliceCollision in playerChaliceCollisionsThisFrame)
            {
                ProcessPlayerChaliceCollision(playerChaliceCollision);
            }
        }

        playerChaliceCollisionsThisFrame.Clear();
        playerPlayerCollisionThisFrame = null;
        Debug.DrawLine(debugPosition, debugPosition + debugDirection.normalized * 2f, Color.cyan);
    }

    #endregion


    #region Public Methods
    public void RegisterPlayerPlayerCollisionThisFrame(GameObject registeredCollision, GameObject collidedWith)
    {
        playerPlayerCollisionThisFrame = new PlayerPlayerCollisionInfo(registeredCollision, collidedWith);
    }

    public void RegisterPlayerChaliceCollisionThisFrame(GameObject chalice, GameObject player)
    {
        playerChaliceCollisionsThisFrame.Add(new PlayerChaliceCollisionInfo(chalice, player));
    }
    #endregion

    #region Collision Processing

    private void ProcessPlayerChaliceCollision(PlayerChaliceCollisionInfo collision)
    {
        PlayerController playerController = collision.player.GetComponent<PlayerController>();

        /*
        double d = Math.sqrt(Math.pow(cx1 - cx2, 2) + Math.pow(cy1 - cy2, 2));
        double nx = (cx2 - cx1) / d;
        double ny = (cy2 - cy1) / d;
        double p = 2 * (circle1.vx * nx + circle1.vy * n_y - circle2.vx * nx - circle2.vy * n_y) /
                (circle1.mass + circle2.mass);
        vx1 = circle1.vx - p * circle1.mass * n_x;
        vy1 = circle1.vy - p * circle1.mass * n_y;
        vx2 = circle2.vx + p * circle2.mass * n_x;
        vy2 = circle2.vy + p * circle2.mass * n_y;
        */

        float distance = Vector3.Distance(playerController.transform.position, collision.chalice.transform.position);

        float newX = (collision.chalice.transform.position.x - playerController.transform.position.x) / distance;
        float newZ = (collision.chalice.transform.position.z - playerController.transform.position.z) / distance;

        float p = playerController.velocity.x * newX + playerController.velocity.z * newZ;

        Vector3 newRegisteredDirection = new Vector3(playerController.velocity.x - p * newX, 0f, playerController.velocity.z - p * newZ).normalized;
        
        if (playerController.GetMovementState() == PlayerController.MovementState.HitByRam)
        {
            playerController.UpdateHitByRamDirection(newRegisteredDirection);
        }
        else if (playerController.GetMovementState() == PlayerController.MovementState.Ramming)
        {
            playerController.UpdateRamDirection(newRegisteredDirection);
        }
    }

    private void ProcessPlayerPlayerCollision(PlayerPlayerCollisionInfo collision)
    {
        PlayerController registeredPlayerController = collision.registeredCollision.GetComponent<PlayerController>();
        PlayerController collidedPlayerController = collision.collidedWith.GetComponent<PlayerController>();
        Rigidbody registeredRigidbody = null;
        Rigidbody collidedRigidbody = null;

        if (registeredPlayerController && collidedPlayerController)
        {
            registeredRigidbody = registeredPlayerController.characterController.attachedRigidbody;
            collidedRigidbody = collidedPlayerController.characterController.attachedRigidbody;
        }

        if (registeredRigidbody != null && collidedRigidbody != null && registeredPlayerController != null && collidedRigidbody != null)
        {
            // circle math: https://ericleong.me/research/circle-circle/#dynamic-circle-circle-collision
            Vector3 registeredVelocity = new Vector3(registeredPlayerController.velocity.x, 0f, registeredPlayerController.velocity.z);
            Vector3 collidedVelocity = new Vector3(collidedPlayerController.velocity.x, 0f, collidedPlayerController.velocity.z);

            /*
            READ THIS CAREFULLY

            double d = Math.sqrt(Math.pow(cx1 - cx2, 2) + Math.pow(cy1 - cy2, 2)); 
            double nx = (cx2 - cx1) / d; 
            double ny = (cy2 - cy1) / d; 
            double p = 2 * (circle1.vx * nx + circle1.vy * n_y - circle2.vx * nx - circle2.vy * n_y) / 
                    (circle1.mass + circle2.mass); 
            vx1 = circle1.vx - p * circle1.mass * n_x; 
            vy1 = circle1.vy - p * circle1.mass * n_y; 
            vx2 = circle2.vx + p * circle2.mass * n_x; 
            vy2 = circle2.vy + p * circle2.mass * n_y;
            */

            float distance = Vector3.Distance(registeredPlayerController.transform.position, collidedPlayerController.transform.position);

            float newX = (collidedPlayerController.transform.position.x - registeredPlayerController.transform.position.x) / distance;
            float newZ = (collidedPlayerController.transform.position.z - registeredPlayerController.transform.position.z) / distance;
            
            float p = registeredVelocity.x * newX + registeredVelocity.z * newZ - collidedVelocity.x * newX - collidedVelocity.z * newZ;

            Vector3 newRegisteredDirection = new Vector3(registeredVelocity.x - p * newX, 0f, registeredVelocity.z - p * newZ).normalized;
            Vector3 newCollidedDirection = new Vector3(collidedVelocity.x + p * newX, 0f, collidedVelocity.z + p * newZ).normalized;

            float newRegisteredDistance = registeredPlayerController.HitByRamDistance;
            float newCollidedDistance = registeredPlayerController.HitByRamDistance;

            float newRegisteredSpeed = 0f;
            float newCollidedSpeed = 0f;

            // RAM V RAM COLLISION
            if (registeredPlayerController.GetMovementState() == PlayerController.MovementState.Ramming && collidedPlayerController.GetMovementState() == PlayerController.MovementState.Ramming)
            {
                newRegisteredSpeed = collidedPlayerController.RamSpeed / registeredPlayerController.RamSpeed;
                newCollidedSpeed = registeredPlayerController.RamSpeed / collidedPlayerController.RamSpeed;

                newRegisteredDistance = collidedPlayerController.HitByRamDistance / registeredPlayerController.HitByRamDistance;
                newCollidedDistance = registeredPlayerController.HitByRamDistance / collidedPlayerController.HitByRamDistance;

                registeredPlayerController.RegisterCollision(newRegisteredDirection, newRegisteredSpeed, newRegisteredDistance);
                collidedPlayerController.RegisterCollision(newCollidedDirection, newCollidedSpeed, newCollidedDistance);
            }
            else
            {
                // RAM V HIT BY RAM
                if (registeredPlayerController.GetMovementState() == PlayerController.MovementState.Ramming && collidedPlayerController.GetMovementState() == PlayerController.MovementState.HitByRam)
                {
                    collidedPlayerController.UpdateHitByRamDirection(newCollidedDirection.normalized);
                }
                else if (collidedPlayerController.GetMovementState() == PlayerController.MovementState.Ramming && registeredPlayerController.GetMovementState() == PlayerController.MovementState.HitByRam)
                {
                    registeredPlayerController.UpdateHitByRamDirection(newRegisteredDirection.normalized);
                }
                // RAM V MOVING NORMALLY
                else if (registeredPlayerController.GetMovementState() == PlayerController.MovementState.Ramming)
                {
                    newCollidedSpeed = registeredPlayerController.HitByRamSpeed;
                    collidedPlayerController.RegisterCollision(newCollidedDirection.normalized, newCollidedSpeed, newCollidedDistance);
                    registeredPlayerController.EndRam(); // this feels better, but is subject to change
                }
                else if (collidedPlayerController.GetMovementState() == PlayerController.MovementState.Ramming)
                {
                    newRegisteredSpeed = collidedPlayerController.HitByRamSpeed; 
                    registeredPlayerController.RegisterCollision(newRegisteredDirection.normalized, newRegisteredSpeed, newRegisteredDistance);
                    collidedPlayerController.EndRam(); // this feels better, but is subject to change
                }
            }
        }
    }
    #endregion
}
