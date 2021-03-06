﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class RigidbodyPlayerController : MonoBehaviour
{
    [Header("Rewired")]
    public int playerId = 0;

    [Header("Inspector References")]
    public CharacterController characterController;
    public CapsuleCollider capsuleCollider;

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

    private Vector3 debugRay = new Vector3();

    public enum MovementState
    {
        Idle,
        Moving,
        Ramming,
        HitByRam,
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

        Debug.DrawLine(hitByRamAt, currentHitDirection * hitByRamDistance);
    }

    public MovementState GetMovementState()
    {
        return movementState;
    }

    public PlayerData.DrunkLevel DrunkLevel()
    {
        return drunkLevel;
    }

    private Vector3 collisionDirection = Vector3.zero;
    private Vector3 collisionPoint = Vector3.zero;

    private bool collidedLastFrame = false;
    private bool collidedThisFrame = false;

    private void CheckPlayerCollision()
    {
        collidedThisFrame = false;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, capsuleCollider.radius, playerData.playerLayer);
        if (hitColliders.Length > 0)
        {
            foreach (Collider collider in hitColliders)
            {
                if (collider.gameObject != gameObject)
                {
                    collidedThisFrame = true;
                    if (collidedThisFrame && !collidedLastFrame)
                    {
                        OnCollision(collider);
                    }
                }
            }
        }

        collidedLastFrame = collidedThisFrame;
    }

    private void OnCollision(Collider collider)
    {
        PlayerManager playerManager;
        if (PlayerManager.TryGetInstance(out playerManager))
        {
            playerManager.RegisterPlayerPlayerCollisionThisFrame(gameObject, collider.gameObject);
        }
    }

    void HandleMovement()
    {
        // check if grounded
        groundedThisFrame = false;
        Debug.DrawLine(transform.position, transform.position - transform.up * (capsuleCollider.radius + 0.1f), Color.green);
        if (Physics.Raycast(transform.position, -transform.up, capsuleCollider.radius + 0.1f, playerData.groundLayer))
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
            case MovementState.HitByRam:
                HitByRamHandler();
                break;
        }

        groundedLastFrame = groundedThisFrame;
    }

    #region State Handlers

    private void IdleHandler()
    {
        if (HandleStickMovement() != Vector3.zero)
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
            movingTowards = IsoUtils.InverseTransformVectorToScreenSpace(ramDirection.normalized);
        }
        else
        {
            Move(ramDirection * RamSpeed);
        }
    }

    private void MovingHandler()
    {
        if (groundedThisFrame)
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
    }

    private void FallHandler()
    {
        if (groundedThisFrame)
        {
            movementState = MovementState.Idle;
        }
        else
        {
            //movingTowards = Vector3.MoveTowards(movingTowards, Vector3.zero, movingTowards.magnitude * Time.deltaTime);
            Move(new Vector3(characterController.attachedRigidbody.velocity.x, transform.position.y, characterController.attachedRigidbody.velocity.z));
        }
    }

    private void HitByRamHandler()
    {
        collisionTimer += Time.deltaTime;
        float currentDistance = Vector3.Distance(hitByRamAt, transform.position);
        if (currentDistance >= hitByRamDistance)
        {
            movementState = MovementState.Moving;
            movingTowards = IsoUtils.InverseTransformVectorToScreenSpace(currentHitDirection * hitSpeed);
            collisionTimer = 0f;
        }
        else
        {
            Move(currentHitDirection * hitSpeed);
        }
    }

    private Vector3 currentHitDirection;
    private Vector3 hitByRamAt;
    private float hitSpeed;
    private float hitByRamDistance;

    private float collisionTimer;

    public void RegisterCollision(Vector3 newDirection, float newSpeed)
    {
        if (collisionTimer < playerData.collisionTimeout)
        {
            hitByRamAt = transform.position;
            movementState = MovementState.HitByRam;
            collisionTimer = 0f;
            currentHitDirection = newDirection;
            hitSpeed = newSpeed;
            Vector3 endPoint = transform.position + currentHitDirection * hitSpeed;
            hitByRamDistance = Vector3.Distance(transform.position, endPoint);
        }
    }

    public void SetMovingTowards(Vector3 mt)
    {
        movingTowards += mt;
    }

    #endregion

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
        Vector3 stickMovement = Vector3.MoveTowards(movingTowards, stickDirection, MoveSpeed * Time.deltaTime);

        Move(IsoUtils.TransformVectorToScreenSpace(stickMovement) * MoveSpeed);

        movingTowards = stickMovement;
        return stickMovement;
    }

    private void Move(Vector3 deltaMovement)
    {
        characterController.Move(deltaMovement);
    }

    private void GetInput()
    {
        inputThisFrame.leftStick = new Vector2(player.GetAxis("Left Stick Horizontal"), player.GetAxis("Left Stick Vertical"));
        inputThisFrame.aButton = player.GetButton("A Button");
        inputThisFrame.aButtonDown = player.GetButtonDown("A Button");
        inputThisFrame.aButtonUp = player.GetButtonUp("A Button");
    }

    private Vector3 Vector2ToVector3(Vector2 vector, float y = 0f)
    {
        return new Vector3(vector.x, 0f, vector.y);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject != gameObject)
        {
            PlayerManager playerManager;
            if(PlayerManager.TryGetInstance(out playerManager))
            {
                playerManager.RegisterPlayerPlayerCollisionThisFrame(gameObject, collision.gameObject);
            }
        }
    }

    #region Properties
    private float MoveSpeed
    {
        get
        {
            if (playerData)
            {
                return playerData.drunkenMovementVariables[(int)drunkLevel].movementSpeed;
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
                return playerData.drunkenMovementVariables[(int)drunkLevel].smoothMoveSpeed;
            }
            else
            {
                Debug.LogWarning("Tried getting SmoothMoveSpeed with no PlayerData", gameObject);
                return 0;
            }
        }
    }

    public float RamSpeed
    {
        get
        {
            if (playerData)
            {
                return playerData.drunkenMovementVariables[(int)drunkLevel].ramSpeed;
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
                return playerData.drunkenMovementVariables[(int)drunkLevel].ramDistance;
            }
            else
            {
                Debug.LogWarning("Tried getting RamDistance with no PlayerData", gameObject);
                return 0;
            }
        }
    }
    #endregion
}
