using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Vector3 forward;
    private Vector3 right;

    void Start()
    {
        forward = Camera.main.transform.forward;
        forward = new Vector3(Camera.main.transform.forward.y, 0f, Camera.main.transform.forward.z);
    }

    void Update()
    {

    }
}
