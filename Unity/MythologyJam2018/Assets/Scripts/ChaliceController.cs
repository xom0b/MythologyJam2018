using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChaliceController : MonoBehaviour
{
    public float chaliceRefillTime;
    public Color onEnterDimColor;
    public HashSet<PlayerController> playerControllersInPickupZone = new HashSet<PlayerController>();
    public Image chaliceFillMeter;
    public MeshRenderer meshRenderer;

    private float currentChaliceRefillTime = 0f;
    private Color startingColor;

    private bool playerIsInTriggerZone = false;

    public void OnEnterTriggerZone(PlayerController playerController)
    {
        if (playerIsInTriggerZone == false)
        {
            meshRenderer.material.color = startingColor * onEnterDimColor;
        }

        playerIsInTriggerZone = true;
        playerControllersInPickupZone.Add(playerController);

        Debug.Log("Chalice: " + gameObject.name + " added: " + playerController.gameObject.name);
    }

    public void OnExitTriggerZone(PlayerController playerController)
    {
        if (playerControllersInPickupZone.Contains(playerController))
        {
            playerControllersInPickupZone.Remove(playerController);
        }

        if (playerControllersInPickupZone.Count == 0)
        {
            meshRenderer.material.color = startingColor;
            playerIsInTriggerZone = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        PlayerManager playerManager;
        if (PlayerManager.TryGetInstance(out playerManager))
        {
            playerManager.RegisterPlayerChaliceCollisionThisFrame(gameObject, collision.gameObject);
        }
    }

    private void Start()
    {
        currentChaliceRefillTime = chaliceRefillTime;
        startingColor = meshRenderer.material.color;
    }

    private void Update()
    {
        UpdateChaliceRefill();
    }

    private void UpdateChaliceRefill()
    {
        if (currentChaliceRefillTime < chaliceRefillTime)
        {
            currentChaliceRefillTime += Time.deltaTime;
            chaliceFillMeter.fillAmount = currentChaliceRefillTime / chaliceRefillTime;
        }
    }

    public bool Drink()
    {
        bool canDrink = false;

        if (!IsRefilling())
        {
            canDrink = true;
            currentChaliceRefillTime = 0f;
        }

        return canDrink;
    }

    public bool IsRefilling()
    {
        return (currentChaliceRefillTime < chaliceRefillTime);
    }
}
