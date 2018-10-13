using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject debugSphere1;
    public GameObject debugSphere2;

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

    private void Update()
    { 
        Debug.DrawRay(debugSphere1.transform.position, debugRay1, Color.red);
        Debug.DrawRay(debugSphere2.transform.position, debugRay2, Color.green);
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
        debugSphere1.transform.position = collision.registeredCollision.transform.position;
        debugSphere2.transform.position = collision.collidedWith.transform.position;
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
            Vector3 registeredVelocity = new Vector3(registeredPlayerController.velocity.x, 0f, registeredPlayerController.velocity.z);
            Vector3 collidedVelocity = new Vector3(collidedPlayerController.velocity.x, 0f, collidedPlayerController.velocity.z);

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

            Vector3 newRegisteredDirection = new Vector3(collidedVelocity.x + p * newX, 0f, collidedVelocity.z + p * newZ);
            Vector3 newCollidedDirection = new Vector3(registeredVelocity.x - p * newX, 0f, registeredVelocity.z - p * newZ);

            debugRay1 = newRegisteredDirection;
            debugRay2 = newCollidedDirection;

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
                    registeredPlayerController.RegisterCollision(IsoUtils.InverseTransformVectorToScreenSpace(newRegisteredDirection), newRegisteredSpeed);
                }
                else
                {
                    //registeredPlayerController.SetMovingTowards(IsoUtils.InverseTransformVectorToScreenSpace(newRegisteredDirection.normalized) * collidedVelocity.magnitude);
                    //collidedPlayerController.SetMovingTowards(IsoUtils.InverseTransformVectorToScreenSpace(newCollidedDirection.normalized) * registeredVelocity.magnitude);
                }
            }
        }
    }
}
