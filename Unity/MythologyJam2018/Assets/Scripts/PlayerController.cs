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

    // rewired
    private Player player;
    private PlayerData playerData;

    // player state & input
    private Input inputThisFrame;
    private PlayerData.DrunkLevel drunkLevel = PlayerData.DrunkLevel.NotDrunk;
    private MovementState movementState = MovementState.Idle;
    private bool groundedLastFrame = false;
    private bool groundedThisFrame = false;

    // movement 
    private Vector3 movingTowards;
    private Vector3 movingVelocity;

    // ram variables
    private Vector3 ramDirection;
    private Vector3 startedRamAt = new Vector3();

    private enum MovementState
    {
        Idle,
        Moving,
        Ramming,
        Falling
    }

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
    }

    void Update()
    {
        GetInput();
        HandleMovement();
        playerData.debugText.text = movementState.ToString();
    }

    void HandleMovement()
    {
        // check if grounded
        groundedThisFrame = false;
        if (Physics.CheckBox(transform.position, playerData.boxCheckHalfExtents, transform.rotation, playerData.groundLayer, QueryTriggerInteraction.Ignore))
        {
            groundedThisFrame = true;
        }

        switch (movementState)
        {
            case MovementState.Idle:
                IdleHandler();
                break;
            case MovementState.Moving:
                MovingHandler();
                break;
            case MovementState.Ramming:
                RamHandler();
                break;
            case MovementState.Falling:
                FallHandler();
                break;
        }

        // check for falling
        if (groundedLastFrame && !groundedThisFrame)
        {
            OnFall();
        }

        groundedLastFrame = groundedThisFrame;
    }

    #region State Handlers

    private void IdleHandler()
    {
        if (!groundedThisFrame)
        {
            // Move handles gravity so call move with
            Move(movingTowards);
        }
        // moved this frame
        else if (groundedThisFrame && HandleStickMovement() != Vector3.zero)
        {
            movementState = MovementState.Moving;
        }
    }

    private void RamHandler()
    {
        float currentRamDistance = Vector3.Distance(startedRamAt, transform.position);

        if (currentRamDistance >= RamDistance)
        {
            movementState = MovementState.Moving;
            movingTowards = IsoUtils.InverseTransformVectorToScreenSpace(ramDirection);
        }
        else
        {
            Move(ramDirection * RamSpeed);
        }
    }

    private void MovingHandler()
    {
        // check for ram
        if (CheckRam())
        {
            movementState = MovementState.Ramming;
        }
        // didn't moved this frame
        else if (HandleStickMovement() == Vector3.zero)
        {
            movementState = MovementState.Idle;
        }
    }

    private void FallHandler()
    {
        if (groundedThisFrame)
        {
            movementState = MovementState.Idle;
        }
        else
        {
            Move(movingTowards * MoveSpeed);
        }
    }

    #endregion

    float fallingMoveSpeed;
    private void OnFall()
    {
        movementState = MovementState.Falling;
        movingTowards = new Vector3(characterController.rigidbody.velocity.x, 0f, characterController.rigidbody.velocity.z).normalized;
        fallingMoveSpeed = characterController.rigidbody.velocity.magnitude;
    }

    private bool CheckRam()
    {
        bool pressedRam = false;

        if (inputThisFrame.aButtonDown && groundedThisFrame)
        {
            ramDirection = IsoUtils.TransformVectorToScreenSpace(Vector2ToVector3(inputThisFrame.leftStick).normalized);
            startedRamAt = transform.position;
            pressedRam = true;
        }

        return pressedRam;
    }

    private Vector3 HandleStickMovement()
    {
        // input movement
        Vector3 stickDirection = Vector2ToVector3(inputThisFrame.leftStick).normalized;
        Vector3 stickMovement = Vector3.MoveTowards(movingTowards, stickDirection, SmoothMoveSpeed * Time.deltaTime);

        Move(IsoUtils.TransformVectorToScreenSpace(stickMovement) * MoveSpeed);

        movingTowards = stickMovement;
        return stickMovement;
    }

    private void Move(Vector3 deltaMovement)
    {
        if (groundedThisFrame || movementState == MovementState.Ramming)
        {
            characterController.Move(deltaMovement);
        }
        else
        {
            characterController.Move(deltaMovement + playerData.gravity);
        }
    }

    private void GetInput()
    {
        inputThisFrame.leftStick = new Vector2(player.GetAxis("Left Stick Horizontal"), player.GetAxis("Left Stick Vertical"));
        inputThisFrame.aButton = player.GetButton("A Button");
        inputThisFrame.aButtonDown = player.GetButtonDown("A Button");
        inputThisFrame.aButtonUp = player.GetButtonUp("A Button");
    }

    private Vector3 Vector2ToVector3(Vector2 vector)
    {
        return new Vector3(vector.x, 0f, vector.y);
    }

    private float MoveSpeed
    {
        get
        {
            if (playerData)
            {
                return playerData.drunkenMovementVariabels[(int)drunkLevel].movementSpeed;
            }
            else
            {
                Debug.LogWarning("Tried getting MoveSpeed with no PlayerData", gameObject);
                return 0;
            }
        }
    }

    private float SmoothMoveSpeed
    {
        get
        {
            if (playerData)
            {
                return playerData.drunkenMovementVariabels[(int)drunkLevel].smoothMoveSpeed;
            }
            else
            {
                Debug.LogWarning("Tried getting SmoothMoveSpeed with no PlayerData", gameObject);
                return 0;
            }
        }
    }

    private float RamSpeed
    {
        get
        {
            if (playerData)
            {
                return playerData.drunkenMovementVariabels[(int)drunkLevel].ramSpeed;
            }
            else
            {
                Debug.LogWarning("Tried getting RamSpeed with no PlayerData", gameObject);
                return 0;
            }
        }
    }

    private float RamDistance
    {
        get
        {
            if (playerData)
            {
                return playerData.drunkenMovementVariabels[(int)drunkLevel].ramDistance;
            }
            else
            {
                Debug.LogWarning("Tried getting RamDistance with no PlayerData", gameObject);
                return 0;
            }
        }
    }
}
