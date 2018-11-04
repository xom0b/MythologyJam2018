using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    // inspector variables
    public PlayerController playerOne;
    public PlayerController playerTwo;
    public float timeAfterHitToScorePoint;

    // private variables
    private PlayerPlayerCollisionInfo playerPlayerCollisionThisFrame = null;
    private List<PlayerChaliceCollisionInfo> playerChaliceCollisionsThisFrame = new List<PlayerChaliceCollisionInfo>();
    private List<PlayerWallCollisionInfo> playerWallCollisionsThisFrame = new List<PlayerWallCollisionInfo>();
    private float playerOnePointCounter;
    private float playerTwoPointCounter;

    #region Classes
    private class PlayerWallCollisionInfo
    {
        public PlayerController playerController;
        public BonkWall bonkWall;

        public PlayerWallCollisionInfo(PlayerController playerController, BonkWall bonkWall)
        {
            this.playerController = playerController;
            this.bonkWall = bonkWall;
        }

        public override string ToString()
        {
            return playerController.gameObject.name + " " + bonkWall.gameObject.name;
        }
    }

    private class PlayerPlayerCollisionInfo
    {
        public GameObject registeredCollision;
        public GameObject collidedWith;

        public PlayerPlayerCollisionInfo(GameObject registeredCollision, GameObject collidedWith)
        {
            this.registeredCollision = registeredCollision;
            this.collidedWith = collidedWith;
        }

        public override string ToString()
        {
            return registeredCollision.name + " " + collidedWith.name;
        }
    }

    private class PlayerChaliceCollisionInfo
    {
        public GameObject chalice;
        public GameObject player;

        public PlayerChaliceCollisionInfo(GameObject chalice, GameObject player)
        {
            this.chalice = chalice;
            this.player = player;
        }

        public override string ToString()
        {
            return chalice.name + " " + player.name;
        }
    }
    #endregion

    #region Singleton
    private static PlayerManager instance;

    private void Awake()
    {
        instance = this;
    }

    public static bool TryGetInstance(out PlayerManager manager)
    {
        manager = instance;
        return manager != null;
    }
    #endregion

    #region MoboBehaviour

    private void Start()
    {
        playerOnePointCounter = timeAfterHitToScorePoint;
        playerTwoPointCounter = timeAfterHitToScorePoint;
    }

    private void Update()
    {
        if (playerOnePointCounter < timeAfterHitToScorePoint)
        {
            playerOnePointCounter += Time.deltaTime;
        }

        if (playerTwoPointCounter < timeAfterHitToScorePoint)
        {
            playerTwoPointCounter += Time.deltaTime;
        }

        debugText.text = "P1: " + playerOnePointCounter + " \nP2: " + playerTwoPointCounter;
    }

    private void LateUpdate()
    {
        if (playerPlayerCollisionThisFrame != null)
        {
            ProcessPlayerPlayerCollision(playerPlayerCollisionThisFrame);
        }

        foreach (PlayerChaliceCollisionInfo playerChaliceCollision in playerChaliceCollisionsThisFrame)
        {
            ProcessPlayerChaliceCollision(playerChaliceCollision);
        }

        foreach (PlayerWallCollisionInfo playerWallCollision in playerWallCollisionsThisFrame)
        {
            ProcessPlayerWallCollision(playerWallCollision);
        }

        playerChaliceCollisionsThisFrame.Clear();
        playerWallCollisionsThisFrame.Clear();
        playerPlayerCollisionThisFrame = null;
    }

    #endregion


    #region Public Methods
    public void ResetPlayers()
    {
        playerOne.ResetPlayer();
        playerTwo.ResetPlayer();
    }

    public void RegisterPlayerPlayerCollisionThisFrame(GameObject registeredCollision, GameObject collidedWith)
    {
        playerPlayerCollisionThisFrame = new PlayerPlayerCollisionInfo(registeredCollision, collidedWith);
    }

    public void RegisterPlayerChaliceCollisionThisFrame(GameObject chalice, GameObject player)
    {
        playerChaliceCollisionsThisFrame.Add(new PlayerChaliceCollisionInfo(chalice, player));
    }

    public void RegisterPlayerWallCollisionThisFrame(PlayerController playerController, BonkWall wall)
    {
        playerWallCollisionsThisFrame.Add(new PlayerWallCollisionInfo(playerController, wall));
    }
    #endregion

    #region Collision Processing
    private void ProcessPlayerWallCollision(PlayerWallCollisionInfo collision)
    {
        // from: https://gamedev.stackexchange.com/questions/23672/determine-resulting-angle-of-wall-collision
        // return vector - 2 * Vector3.Dot(vector, normal) * normal;
        Vector3 ramDirection = collision.playerController.velocity.normalized;
        Vector3 newDirection = ramDirection - 2 * Vector3.Dot(ramDirection, collision.bonkWall.wallNormal.normalized) * collision.bonkWall.wallNormal.normalized;

        if (collision.playerController.GetMovementState() == PlayerController.MovementState.Ramming)
        {
            collision.playerController.UpdateRamDirection(newDirection.normalized);
        }
        else if (collision.playerController.GetMovementState() == PlayerController.MovementState.HitByRam)
        {
            collision.playerController.UpdateHitByRamDirection(newDirection.normalized);
        }
    }

    private void ProcessPlayerChaliceCollision(PlayerChaliceCollisionInfo collision)
    {
        PlayerController playerController = collision.player.GetComponent<PlayerController>();

        /*
        double d = Math.sqrt(Math.pow(cx1 - cx2, 2) + Math.pow(cy1 - cy2, 2));
        double nx = (cx2 - cx1) / d;
        double ny = (cy2 - cy1) / d;
        double p = 2 * (circle1.vx * nx + circle1.vy * n_y - circle2.vx * nx - circle2.vy * n_y) /
                (circle1.mass + circle2.mass);
        vx1 = circle1.vx - p * circle1.mass * n_x;
        vy1 = circle1.vy - p * circle1.mass * n_y;
        vx2 = circle2.vx + p * circle2.mass * n_x;
        vy2 = circle2.vy + p * circle2.mass * n_y;
        */

        float distance = Vector3.Distance(playerController.transform.position, collision.chalice.transform.position);

        float newX = (collision.chalice.transform.position.x - playerController.transform.position.x) / distance;
        float newZ = (collision.chalice.transform.position.z - playerController.transform.position.z) / distance;

        float p = playerController.velocity.x * newX + playerController.velocity.z * newZ;

        Vector3 newRegisteredDirection = new Vector3(playerController.velocity.x - p * newX, 0f, playerController.velocity.z - p * newZ).normalized;

        if (playerController.GetMovementState() == PlayerController.MovementState.HitByRam)
        {
            playerController.UpdateHitByRamDirection(newRegisteredDirection);
        }
        else if (playerController.GetMovementState() == PlayerController.MovementState.Ramming)
        {
            playerController.UpdateRamDirection(newRegisteredDirection);
        }
    }

    private void ProcessPlayerPlayerCollision(PlayerPlayerCollisionInfo collision)
    {
        PlayerController registeredPlayerController = collision.registeredCollision.GetComponent<PlayerController>();
        PlayerController collidedPlayerController = collision.collidedWith.GetComponent<PlayerController>();
        Rigidbody registeredRigidbody = null;
        Rigidbody collidedRigidbody = null;

        if (registeredPlayerController && collidedPlayerController)
        {
            registeredRigidbody = registeredPlayerController.characterController.attachedRigidbody;
            collidedRigidbody = collidedPlayerController.characterController.attachedRigidbody;
        }

        if (registeredRigidbody != null && collidedRigidbody != null && registeredPlayerController != null && collidedRigidbody != null)
        {
            // circle math: https://ericleong.me/research/circle-circle/#dynamic-circle-circle-collision
            Vector3 registeredVelocity = new Vector3(registeredPlayerController.velocity.x, 0f, registeredPlayerController.velocity.z);
            Vector3 collidedVelocity = new Vector3(collidedPlayerController.velocity.x, 0f, collidedPlayerController.velocity.z);

            /*
            READ THIS CAREFULLY

            double d = Math.sqrt(Math.pow(cx1 - cx2, 2) + Math.pow(cy1 - cy2, 2)); 
            double nx = (cx2 - cx1) / d; 
            double ny = (cy2 - cy1) / d; 
            double p = 2 * (circle1.vx * nx + circle1.vy * n_y - circle2.vx * nx - circle2.vy * n_y) / 
                    (circle1.mass + circle2.mass); 
            vx1 = circle1.vx - p * circle1.mass * n_x; 
            vy1 = circle1.vy - p * circle1.mass * n_y; 
            vx2 = circle2.vx + p * circle2.mass * n_x; 
            vy2 = circle2.vy + p * circle2.mass * n_y;
            */

            float distance = Vector3.Distance(registeredPlayerController.transform.position, collidedPlayerController.transform.position);

            float newX = (collidedPlayerController.transform.position.x - registeredPlayerController.transform.position.x) / distance;
            float newZ = (collidedPlayerController.transform.position.z - registeredPlayerController.transform.position.z) / distance;

            float p = registeredVelocity.x * newX + registeredVelocity.z * newZ - collidedVelocity.x * newX - collidedVelocity.z * newZ;

            Vector3 newRegisteredDirection = new Vector3(registeredVelocity.x - p * newX, 0f, registeredVelocity.z - p * newZ).normalized;
            Vector3 newCollidedDirection = new Vector3(collidedVelocity.x + p * newX, 0f, collidedVelocity.z + p * newZ).normalized;

            float newRegisteredDistance = collidedPlayerController.HitByRamDistance;
            float newCollidedDistance = registeredPlayerController.HitByRamDistance;

            float newRegisteredSpeed = 0f;
            float newCollidedSpeed = 0f;

            if (registeredPlayerController.GetMovementState() == PlayerController.MovementState.Ramming)
            {
                ResetPlayerPointCounter(registeredPlayerController);
            }

            if (collidedPlayerController.GetMovementState() == PlayerController.MovementState.Ramming)
            {
                ResetPlayerPointCounter(collidedPlayerController);
            }

            // RAM V RAM COLLISION
            if (registeredPlayerController.GetMovementState() == PlayerController.MovementState.Ramming && collidedPlayerController.GetMovementState() == PlayerController.MovementState.Ramming)
            {
                newRegisteredSpeed = collidedPlayerController.HitByRamSpeed * (collidedPlayerController.HitByRamSpeed / registeredPlayerController.HitByRamSpeed);
                newCollidedSpeed = registeredPlayerController.HitByRamSpeed * (registeredPlayerController.HitByRamSpeed / collidedPlayerController.HitByRamSpeed);

                newRegisteredDistance = collidedPlayerController.HitByRamDistance * (collidedPlayerController.HitByRamDistance / registeredPlayerController.HitByRamDistance);
                newCollidedDistance = registeredPlayerController.HitByRamDistance * (registeredPlayerController.HitByRamDistance / collidedPlayerController.HitByRamDistance);

                registeredPlayerController.RegisterCollision(newRegisteredDirection, newRegisteredSpeed, newRegisteredDistance);
                collidedPlayerController.RegisterCollision(newCollidedDirection, newCollidedSpeed, newCollidedDistance);
            }
            else
            {
                // RAM V HIT BY RAM
                if (registeredPlayerController.GetMovementState() == PlayerController.MovementState.Ramming && collidedPlayerController.GetMovementState() == PlayerController.MovementState.HitByRam)
                {
                    collidedPlayerController.UpdateHitByRamDirection(newCollidedDirection.normalized);
                }
                else if (collidedPlayerController.GetMovementState() == PlayerController.MovementState.Ramming && registeredPlayerController.GetMovementState() == PlayerController.MovementState.HitByRam)
                {
                    registeredPlayerController.UpdateHitByRamDirection(newRegisteredDirection.normalized);
                }
                // RAM V MOVING NORMALLY
                else if (registeredPlayerController.GetMovementState() == PlayerController.MovementState.Ramming)
                {
                    newCollidedSpeed = registeredPlayerController.HitByRamSpeed;
                    collidedPlayerController.RegisterCollision(newCollidedDirection.normalized, newCollidedSpeed, newCollidedDistance);
                    registeredPlayerController.EndRam(); // this feels better, but is subject to change
                }
                else if (collidedPlayerController.GetMovementState() == PlayerController.MovementState.Ramming)
                {
                    newRegisteredSpeed = collidedPlayerController.HitByRamSpeed;
                    registeredPlayerController.RegisterCollision(newRegisteredDirection.normalized, newRegisteredSpeed, newRegisteredDistance);
                    collidedPlayerController.EndRam(); // this feels better, but is subject to change
                }
            }
        }
    }
    #endregion

    private void ResetPlayerPointCounter(PlayerController playerController)
    {
        if (playerController == playerOne)
        {
            playerOnePointCounter = 0f;
        }
        else if (playerController == playerTwo)
        {
            playerTwoPointCounter = 0f;
        }
    }

    public Text debugText;

    public void RegisterPointTrigger(PlayerController playerController)
    {
        if (playerController == playerOne && playerTwoPointCounter < timeAfterHitToScorePoint)
        {
            // PLAYER TWO SCORES POINT
            GameManager gameManager;
            if (GameManager.TryGetInstance(out gameManager))
            {
                gameManager.AddPoint(1);
                playerTwoPointCounter = timeAfterHitToScorePoint;
            }
        }
        else if (playerController == playerTwo && playerOnePointCounter < timeAfterHitToScorePoint)
        {
            // PLAYER ONE SCORES POINT
            GameManager gameManager;
            if (GameManager.TryGetInstance(out gameManager))
            {
                gameManager.AddPoint(0);
                playerOnePointCounter = timeAfterHitToScorePoint;
            }
        }
    }
}
