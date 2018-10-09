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
        Debug.Log("processing collision");

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
            // check out: https://gamedevelopment.tutsplus.com/tutorials/when-worlds-collide-simulating-circle-circle-collisions--gamedev-769
            Vector3 registeredVelocity = new Vector3(registeredRigidbody.velocity.x, 0f, registeredRigidbody.velocity.z);
            Vector3 collidedVelocity = new Vector3(collidedRigidbody.velocity.x, 0f, collidedRigidbody.velocity.z);

            // capture new direction, modify speed
            Vector3 newRegisteredDirection = new Vector3(collidedVelocity.x, 0f, collidedVelocity.z).normalized;
            Vector3 newCollidedDirection = new Vector3(registeredVelocity.x, 0f, registeredVelocity.z).normalized;

            float newRegisteredSpeed = 0f;
            float newCollidedSpeed = 0f;
            
            if (registeredPlayerController.GetMovementState() == PlayerController.MovementState.Ramming && collidedPlayerController.GetMovementState() == PlayerController.MovementState.Ramming)
            {
                newRegisteredSpeed = collidedPlayerController.RamSpeed / registeredPlayerController.RamSpeed;
                newCollidedSpeed = registeredPlayerController.RamSpeed / collidedPlayerController.RamSpeed;

                registeredPlayerController.RegisterCollision(newRegisteredDirection, newRegisteredSpeed);
                collidedPlayerController.RegisterCollision(newCollidedDirection, newCollidedSpeed);
            }
            else
            {
                //float registeredVelocityMagnitude = Mathf.Clamp(registeredVelocity.magnitude, 1f, Mathf.Infinity);
                //float collidedVelocityMagnitude = Mathf.Clamp(collidedVelocity.magnitude, 1f, Mathf.Infinity);

                if (registeredPlayerController.GetMovementState() == PlayerController.MovementState.Ramming)
                {
                    newCollidedSpeed = registeredPlayerController.RamSpeed;
                    collidedPlayerController.RegisterCollision(newCollidedDirection, newCollidedSpeed);
                }
                else if (collidedPlayerController.GetMovementState() == PlayerController.MovementState.Ramming)
                {
                    newRegisteredSpeed = collidedPlayerController.RamSpeed;
                    registeredPlayerController.RegisterCollision(newRegisteredDirection, newRegisteredSpeed);
                }
                else
                {
                    registeredPlayerController.SetMovingTowards(IsoUtils.InverseTransformVectorToScreenSpace(newRegisteredDirection.normalized) * collidedVelocity.magnitude);
                    collidedPlayerController.SetMovingTowards(IsoUtils.InverseTransformVectorToScreenSpace(newCollidedDirection.normalized) * registeredVelocity.magnitude);
                }
            }
        }
    }
}
