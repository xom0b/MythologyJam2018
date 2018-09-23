using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using System;

public class PlayerController : MonoBehaviour
{
    [Header("Rewired")]
    public int playerId = 0;

    [Header("Inspector References")]
    public CharacterController characterController;
    public Transform globalOrientationTransform;

    [Header("Movement")]
    public float speed;
    public float smoothSpeed;

    private Player player;
    private Vector3 forward;
    private Vector3 right;
    private Vector3 movingTowards;
    private Vector3 movingVelocity;

    private Input inputThisFrame;

    private struct Input
    {
        public Vector2 leftStick;
        public bool aButton;
        public bool aButtonDown;
        public bool aButtonUp;
    }

    void Start()
    {
        player = ReInput.players.GetPlayer(playerId);
        forward = new Vector3(globalOrientationTransform.forward.y, 0f, globalOrientationTransform.forward.z);
        right = new Vector3(globalOrientationTransform.right.y, 0f, globalOrientationTransform.right.z);
    }

    void Update()
    {
        GetInput();
        HandleMovement();
    }

    void HandleMovement()
    {
        Vector3 stickDirection = new Vector3(inputThisFrame.leftStick.x, 0f, inputThisFrame.leftStick.y).normalized;
        Debug.Log("stickDirection: " + stickDirection);
        Vector3 newDirection = Vector3.MoveTowards(movingTowards, stickDirection, smoothSpeed * Time.deltaTime);
        Debug.Log("newDirection: " + newDirection.ToString("F8"));
        characterController.Move(IsoUtils.TransformVectorToScreenSpace(newDirection) * Time.deltaTime * speed);
        movingTowards = newDirection;
    }

    void GetInput()
    {
        inputThisFrame.leftStick = new Vector2(player.GetAxis("Left Stick Horizontal"), player.GetAxis("Left Stick Vertical"));
        inputThisFrame.aButton = player.GetButton("A Button");
        inputThisFrame.aButtonDown = player.GetButtonDown("A Button");
        inputThisFrame.aButtonUp = player.GetButtonUp("A Button");
    }
}
