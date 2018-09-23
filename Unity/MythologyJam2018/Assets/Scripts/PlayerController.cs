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

        Debug.Log("speed: " + new Vector2(horizontalMovement, verticalMovement).ToString("F8"));

        //Vector3 isoMoveDelta = ConvertToIso(new Vector3(horizontalMovement, verticalMovement));
        characterController.Move(ConvertToIso(new Vector3(horizontalMovement, 0f, verticalMovement)) * Time.deltaTime * speed);
        Debug.DrawRay(transform.position, ConvertToIso(transform.right), Color.blue);
        Debug.DrawRay(transform.position, ConvertToIso(-transform.right), Color.cyan);
        Debug.DrawRay(transform.position, ConvertToIso(transform.forward), Color.green);
        Debug.DrawRay(transform.position, ConvertToIso(-transform.forward), Color.red);
    }

    void GetInput()
    {
        inputThisFrame.leftStick = new Vector2(player.GetAxis("Left Stick Horizontal"), player.GetAxis("Left Stick Vertical"));
        inputThisFrame.aButton = player.GetButton("A Button");
        inputThisFrame.aButtonDown = player.GetButtonDown("A Button");
        inputThisFrame.aButtonUp = player.GetButtonUp("A Button");
    }

    Vector3 ConvertToIso(Vector3 vector)
    {
        return globalOrientationTransform.TransformDirection(vector);
    }
}
