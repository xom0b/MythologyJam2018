using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDebugDrunkMeter : MonoBehaviour
{
    public float fillSpeed;
    public float fillDamp;
    public Image fillImage;
    public PlayerController playerController;

    private float currentFillSpeed;
    private PlayerData.DrunkLevel drunkLevelLastFrame;
    private PlayerData playerData;
    private float targetFill;
    private float currentFill;

    // Use this for initialization
    void Start()
    {
        drunkLevelLastFrame = playerController.DrunkLevel();
        currentFill = fillImage.fillAmount;
        targetFill = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (drunkLevelLastFrame != playerController.DrunkLevel())
        {
            PlayerData playerData;
            if (PlayerData.TryGetInstance(out playerData))
            {
                targetFill = (float)playerController.DrunkLevel() / (float)playerData.maxDrunkLevel;
            }
        }

        currentFill = Mathf.SmoothDamp(currentFill, targetFill, ref currentFillSpeed, fillDamp, fillSpeed, Time.deltaTime);
        fillImage.fillAmount = currentFill;
        drunkLevelLastFrame = playerController.DrunkLevel();
    }
}
