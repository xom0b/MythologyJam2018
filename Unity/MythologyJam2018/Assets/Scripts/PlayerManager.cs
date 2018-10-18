using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private static PlayerManager instance;

    private class CollisionInfo
    {
        public GameObject registeredCollision;
        public GameObject collidedWith;

        public CollisionInfo(GameObject registeredCollision, GameObject collidedWith)
        {
            this.registeredCollision = registeredCollision;
            this.collidedWith = collidedWith;
        }

        public override string ToString()
        {
            return registeredCollision.name + " " + collidedWith.name;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    public static bool TryGetInstance(out PlayerManager manager)
    {
        manager = instance;
        return manager != null;
    }

    private CollisionInfo collisionThisFrame = null;
    
    public void RegisterCollisionThisFrame(GameObject registeredCollision, GameObject collidedWith)
    {
        collisionThisFrame = new CollisionInfo(registeredCollision, collidedWith);
    }

    Vector3 debugRay1 = new Vector3();
    Vector3 debugRay2 = new Vector3();

    private void LateUpdate()
    {
        if (collisionThisFrame != null)
        {
            ProcessCollision(collisionThisFrame);
        }

        collisionThisFrame = null;
    }

    private void ProcessCollision(CollisionInfo collision)
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

            float distance = Vector3.Distance(registeredPlayerController.transform.position, collidedPlayerController.transform.position);

            float newX = (collidedPlayerController.transform.position.x - registeredPlayerController.transform.position.x) / distance;
            float newZ = (registeredPlayerController.transform.position.z - collidedPlayerController.transform.position.z) / distance;
            
            float p = registeredVelocity.x * newX + registeredVelocity.z * newZ - collidedVelocity.x * newX - collidedVelocity.z * newZ;

            Vector3 newRegisteredDirection = new Vector3(collidedVelocity.x + p * newX, 0f, collidedVelocity.z + p * newZ).normalized;
            Vector3 newCollidedDirection = new Vector3(registeredVelocity.x - p * newX, 0f, registeredVelocity.z - p * newZ).normalized;

            float newRegisteredDistance = registeredPlayerController.HitByRamDistance;
            float newCollidedDistance = registeredPlayerController.HitByRamDistance;

            float newRegisteredSpeed = 0f;
            float newCollidedSpeed = 0f;

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
                if (registeredPlayerController.GetMovementState() == PlayerController.MovementState.Ramming && collidedPlayerController.GetMovementState() == PlayerController.MovementState.HitByRam)
                {
                    Debug.Log("updating collision: " + newCollidedDirection);
                    collidedPlayerController.UpdateCollisionDirection(newCollidedDirection.normalized);
                }
                else if (collidedPlayerController.GetMovementState() == PlayerController.MovementState.Ramming && registeredPlayerController.GetMovementState() == PlayerController.MovementState.HitByRam)
                {
                    Debug.Log("updating collision 2: " + newRegisteredDirection);
                    registeredPlayerController.UpdateCollisionDirection(newRegisteredDirection.normalized);
                }
                else if (registeredPlayerController.GetMovementState() == PlayerController.MovementState.Ramming)
                {
                    newCollidedSpeed = registeredPlayerController.RamSpeed;
                    collidedPlayerController.RegisterCollision(newCollidedDirection.normalized, newCollidedSpeed, newCollidedDistance);
                }
                else if (collidedPlayerController.GetMovementState() == PlayerController.MovementState.Ramming)
                {
                    newRegisteredSpeed = collidedPlayerController.RamSpeed;
                    registeredPlayerController.RegisterCollision(newRegisteredDirection.normalized, newRegisteredSpeed, newRegisteredDistance);
                }
            }
        }
    }
}
