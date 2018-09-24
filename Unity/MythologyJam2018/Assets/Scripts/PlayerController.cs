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

    private Player player;
    private Vector3 forward;
    private Vector3 right;
    private Vector3 movingTowards;
    private Vector3 movingVelocity;

    private Input inputThisFrame;
    private PlayerData playerData;

    private struct Input
    {
        public Vector2 leftStick;
        public bool aButton;
        public bool aButtonDown;
        public bool aButtonUp;
    }

    void Start()
    {
        PlayerData.TryGetInstance(out playerData);
        player = ReInput.players.GetPlayer(playerId);
        forward = new Vector3(playerData.isoOrientation.forward.y, 0f, playerData.isoOrientation.forward.z);
        right = new Vector3(playerData.isoOrientation.right.y, 0f, playerData.isoOrientation.right.z);
    }

    void Update()
    {
        GetInput();
        HandleMovement();
    }

    

    void HandleMovement()
    {
        bool isGrounded = false;

        // gravity
        if (Physics.CheckBox(transform.position, playerData.boxCheckHalfExtents, transform.rotation, playerData.groundLayer, QueryTriggerInteraction.Ignore))
        {
            isGrounded = true;
        }

        // input movement
        Vector3 stickDirection = new Vector3(inputThisFrame.leftStick.x, 0f, inputThisFrame.leftStick.y).normalized;
        Vector3 newDirection = Vector3.MoveTowards(movingTowards, stickDirection, playerData.smoothMoveSpeed * Time.deltaTime);

        if (isGrounded)
        {
            characterController.Move(IsoUtils.TransformVectorToScreenSpace(newDirection) * playerData.moveSpeed);
        }
        else
        {
            characterController.Move(IsoUtils.TransformVectorToScreenSpace(newDirection) * playerData.moveSpeed + playerData.gravity);
        }
         
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
