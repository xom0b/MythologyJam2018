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
        Debug.Log("I: " + transform.parent.gameObject.name + " hit: " + collision.gameObject.name);
    }
}
