﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public Transform isoOrientation;
    public Vector3 gravity; 
    public Vector3 boxCheckHalfExtents;
    public LayerMask groundLayer;
    public List<DrunkenMovementVariables> drunkenMovementVariabels = new List<DrunkenMovementVariables>();

    [System.Serializable]
    public struct DrunkenMovementVariables
    {
        public DrunkLevel drunkLevel;
        public float movementSpeed;
        public float smoothMoveSpeed;
        public float ramSpeed;
    }

    public enum DrunkLevel
    {
        NotDrunk = 0,
        LevelOne = 1,
        LevelTwo = 2,
        LevelThree = 3
    }

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
