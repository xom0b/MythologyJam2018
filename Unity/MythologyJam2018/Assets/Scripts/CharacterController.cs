using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public Rigidbody rigidbody;

    private Vector3 deltaMovement;

    private void FixedUpdate()
    {
        if (deltaMovement != Vector3.zero)
        {
            rigidbody.MovePosition(transform.position + deltaMovement * Time.fixedDeltaTime);
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
}
