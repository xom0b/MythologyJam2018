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

    private Player player;
    private Vector3 forward;
    private Vector3 right;

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
        float horizontalMovement = 0f;
        float verticalMovement = 0f;

        if (inputThisFrame.leftStick.x != 0)
        {
            horizontalMovement = inputThisFrame.leftStick.x;
        }

        if (inputThisFrame.leftStick.y != 0)
        {
            verticalMovement = inputThisFrame.leftStick.y;
        }

        Debug.Log(IsoUtils.TransformVectorToScreenSpace(new Vector3(horizontalMovement, 0f, verticalMovement)) * Time.deltaTime * speed);

        characterController.Move(IsoUtils.TransformVectorToScreenSpace(new Vector3(horizontalMovement, 0f, verticalMovement)) * Time.deltaTime * speed);

        Debug.DrawRay(transform.position, IsoUtils.TransformVectorToScreenSpace(transform.right + transform.forward) * 2, Color.blue);
        Debug.Log("upper right mag: " + IsoUtils.TransformVectorToScreenSpace(transform.right + transform.forward).magnitude);
        Debug.DrawRay(transform.position, IsoUtils.TransformVectorToScreenSpace(-transform.right + transform.forward) * 2, Color.cyan);
        Debug.Log("upper left mag: " + IsoUtils.TransformVectorToScreenSpace(-transform.right + transform.forward).magnitude);
        Debug.DrawRay(transform.position, IsoUtils.TransformVectorToScreenSpace(-transform.forward + transform.right) * 2, Color.green);
        Debug.Log("lower left mag: " + IsoUtils.TransformVectorToScreenSpace(-transform.forward + transform.right).magnitude);
        Debug.DrawRay(transform.position, IsoUtils.TransformVectorToScreenSpace(-transform.forward - transform.right) * 2, Color.red);
        Debug.Log("lower right mag: " + IsoUtils.TransformVectorToScreenSpace(-transform.forward -transform.right).magnitude);
    }

    void GetInput()
    {
        inputThisFrame.leftStick = new Vector2(player.GetAxis("Left Stick Horizontal"), player.GetAxis("Left Stick Vertical"));
        inputThisFrame.aButton = player.GetButton("A Button");
        inputThisFrame.aButtonDown = player.GetButtonDown("A Button");
        inputThisFrame.aButtonUp = player.GetButtonUp("A Button");
    }
}
