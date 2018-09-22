using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SetSpriteParallelToCamera : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite)
        {
            sprite.transform.rotation = Camera.main.transform.parent.transform.rotation;
        }
    }
}
