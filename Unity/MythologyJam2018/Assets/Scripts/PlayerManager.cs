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
        // TODO: LOOK AT https://ericleong.me/research/circle-circle/#dynamic-circle-circle-collision
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

            float distance = Vector3.Distance(registeredPlayerController.transform.position, collidedPlayerController.transform.position);

            /*
            double nx = (cx2 - cx1) / d; 
            double ny = (cy2 - cy1) / d; 
            */

            float newX = (collidedPlayerController.transform.position.x - registeredPlayerController.transform.position.x) / distance;
            float newZ = (registeredPlayerController.transform.position.z - collidedPlayerController.transform.position.z) / distance;

            // (circle1.vx * nx + circle1.vy * n_y - circle2.vx * nx - circle2.vy * n_y)
            float p = registeredVelocity.x * newX + registeredVelocity.z * newZ - collidedVelocity.x * newX - collidedVelocity.z * newZ;

            /*
            vx1 = circle1.vx - p * circle1.mass * n_x; 
            vy1 = circle1.vy - p * circle1.mass * n_y; 
            vx2 = circle2.vx + p * circle2.mass * n_x; 
            vy2 = circle2.vy + p * circle2.mass * n_y;
            */

            Vector3 newRegisteredDirection = new Vector3(registeredVelocity.x - p * newX, 0f, registeredVelocity.z - p * newZ);
            Vector3 newCollidedDirection = new Vector3(collidedVelocity.x - p * newX, 0f, collidedVelocity.z - p * newZ);

            float newRegisteredSpeed = newRegisteredDirection.magnitude;
            float newCollidedSpeed = newRegisteredDirection.magnitude;

            newRegisteredDirection = newRegisteredDirection.normalized;
            newCollidedDirection = newCollidedDirection.normalized;

            if (registeredPlayerController.GetMovementState() == PlayerController.MovementState.Ramming && collidedPlayerController.GetMovementState() == PlayerController.MovementState.Ramming)
            {
                newRegisteredSpeed = collidedPlayerController.RamSpeed / registeredPlayerController.RamSpeed;
                newCollidedSpeed = registeredPlayerController.RamSpeed / collidedPlayerController.RamSpeed;

                registeredPlayerController.RegisterCollision(newRegisteredDirection, newRegisteredSpeed);
                collidedPlayerController.RegisterCollision(newCollidedDirection, newCollidedSpeed);
            }
            else
            {
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
