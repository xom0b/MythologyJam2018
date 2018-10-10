﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public Rigidbody attachedRigidbody;

    private Vector3 deltaMovement;
    private Vector3 deltaForce;

    private void FixedUpdate()
    {
        if (deltaMovement != Vector3.zero)
        {
            attachedRigidbody.MovePosition(transform.position + deltaMovement * Time.fixedDeltaTime);
            deltaMovement = Vector3.zero;
        }

        if (deltaForce != Vector3.zero)
        {
            attachedRigidbody.AddForce(deltaForce);
            deltaMovement = Vector3.zero;
        }
    }
    
    /// <summary>
    /// Do not normalize this with Time.deltaTime. 
    /// </summary>
    /// <param name="deltaMovement"></param>
    public void Move(Vector3 deltaMovement)
    {
        this.deltaMovement = deltaMovement;
    }

    public void AddForce(Vector3 deltaForce)
    {
        this.deltaForce = deltaForce;
    }
}
