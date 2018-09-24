using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public Transform isoOrientation;
    public Vector3 gravity;
    public float moveSpeed;
    public float smoothMoveSpeed;
    public Vector3 boxCheckHalfExtents;
    public LayerMask groundLayer;

    private static PlayerData instance = null;

    private void Awake()
    {
        instance = this;
    }

    public static bool TryGetInstance(out PlayerData playerData)
    {
        playerData = instance;
        return (playerData != null);
    }
}
