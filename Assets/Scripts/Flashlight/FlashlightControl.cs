using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightControl : MonoBehaviour
{
    [SerializeField] private float minRandomFlashlightDelay = 0.1f;
    [SerializeField] private float maxRandomFlashlightDelay = 10f;
    [SerializeField] private float maxAnxiousRandomFlashlightMultiplier = 0.25f;
    private float currentMaxRandomFlashlightDelay;

    [SerializeField] private float secondBeforeToggleAgain = 0.05f;

    [SerializeField] private SurvivalStats playerSurvivalStats;

    [Header("Debug")]
    [SerializeField] private float currentSecondBeforeToggleCount;
    [SerializeField] private float randomDelay;
    [SerializeField] private float randomDelayCount;
    [SerializeField] private float brainwaveFlashlightMultiplier;
    private float currentBrainwave => playerSurvivalStats.currentBrainwave;

    private Light flashlightComponent => GetComponent<Light>();
    private bool isFlashlightOn;
    private bool canRandomToggle;

    private bool isRandomToggleOnce = false;


    private void Start()
    {
        randomDelay = Random.Range(minRandomFlashlightDelay, maxRandomFlashlightDelay);
        randomDelayCount = randomDelay;

        currentSecondBeforeToggleCount = secondBeforeToggleAgain;
        CheckBrainwaveFlashlightMultiplier();
        CheckIfCanBeToggle();
    }

    void Update()
    {
        isFlashlightOn = flashlightComponent.enabled;

        RandomFlashlightToggle();

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            ToggleFlashlight();

            CheckIfCanBeToggle();
        }
    }

    private void ToggleFlashlight()
    {

        if (isFlashlightOn)
        {
           flashlightComponent.enabled = false;
        }
        else
        {
           flashlightComponent.enabled = true;
        }
    }

    private void CheckIfCanBeToggle()
    {
        if (flashlightComponent.enabled == false)
        {
            canRandomToggle = false;
        }
        else if (flashlightComponent.enabled == true)
        {
            canRandomToggle = true;
        }
    }

    private void RandomFlashlightToggle()
    {
        CheckBrainwaveFlashlightMultiplier();

        if (canRandomToggle)
        {
            if (randomDelayCount > 0)
            {
                randomDelayCount -= Time.deltaTime;
            }
            else
            {
                if (!isRandomToggleOnce)
                {
                    ToggleFlashlight();
                    isRandomToggleOnce = true;
                }

                if (currentSecondBeforeToggleCount > 0)
                {
                    currentSecondBeforeToggleCount -= Time.deltaTime;
                }
                else
                {
                    ToggleFlashlight();

                    randomDelay = Random.Range(minRandomFlashlightDelay, maxRandomFlashlightDelay * brainwaveFlashlightMultiplier);
                    randomDelayCount = randomDelay;

                    currentSecondBeforeToggleCount = secondBeforeToggleAgain;
                    isRandomToggleOnce = false;
                }
            }

        }
    }

    private void CheckBrainwaveFlashlightMultiplier()
    {
        //check multiplier
        if(currentBrainwave > 0) //avoid divide by 0 problem
        {
            brainwaveFlashlightMultiplier = (100 / currentBrainwave) * maxAnxiousRandomFlashlightMultiplier; //at 100 brainwave, maxRandomDelay should be 0.25x (maxAnxiousRandomFlashlightMultiplier) of normal
        }

        brainwaveFlashlightMultiplier = Mathf.Clamp(brainwaveFlashlightMultiplier, maxAnxiousRandomFlashlightMultiplier, 1f);
        
    }
}
