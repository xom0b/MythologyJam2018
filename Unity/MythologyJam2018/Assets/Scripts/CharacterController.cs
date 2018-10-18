using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public PlayerController playerController;
    public Rigidbody attachedRigidbody;

    private Vector3 deltaMovement;
    private Vector3 deltaForce;
    private Vector3 newPosition;

    private bool setPositionNextFrame = false;

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

        if (setPositionNextFrame)
        {
            setPositionNextFrame = false;
            attachedRigidbody.isKinematic = true;
            attachedRigidbody.position = newPosition;
            attachedRigidbody.isKinematic = false;
            playerController.FinishedResettingPosition();
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

    public void SetPosition(Vector3 newPosition)
    {
        this.newPosition = newPosition;
        setPositionNextFrame = true;
    }

    public void AddForce(Vector3 deltaForce)
    {
        this.deltaForce = deltaForce;
    }
}
