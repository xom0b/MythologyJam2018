using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SetTransformParallelToCamera : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        transform.rotation = Camera.main.transform.parent.transform.rotation;
    }
}
