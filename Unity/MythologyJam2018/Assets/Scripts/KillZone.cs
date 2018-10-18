using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillZOne : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Kill Screen " + collision.gameObject.name);
    }
}
