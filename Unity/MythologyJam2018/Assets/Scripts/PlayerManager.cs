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
    
    public void RegisterCollfisionThisFrame(GameObject registeredCollision, GameObject collidedWith)
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
        Rigidbody registeredRigidbody = collision.registeredCollision.GetComponent<Rigidbody>();
        Rigidbody collidedRigidbody = collision.collidedWith.GetComponent<Rigidbody>();
        PlayerController registeredPlayerController = collision.registeredCollision.GetComponent<PlayerController>();
        PlayerController collidedPlayerController = collision.collidedWith.GetComponent<PlayerController>();

        if (registeredRigidbody != null && collidedRigidbody != null && registeredPlayerController != null && collidedRigidbody != null)
        {
            // check out: https://gamedevelopment.tutsplus.com/tutorials/when-worlds-collide-simulating-circle-circle-collisions--gamedev-769
            Vector3 registeredVelocity = registeredRigidbody.velocity;
            Vector3 registeredPosition = registeredRigidbody.transform.position;
            Vector3 collidedVelocity = collidedRigidbody.velocity;
            Vector3 collidedPosition = collidedRigidbody.transform.position;
        }
    }
}
