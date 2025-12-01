using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class WeaponControl : MonoBehaviour
{ // Animator component
    [SerializeField] private Image weaponImage;
    [SerializeField] private List<Sprite> weaponDefaultSprites;

    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource1; // Audiosource component
    [SerializeField] private AudioClip pistolShoot; // Pistol shot audio clip

    [SerializeField] private string hotbarSwitchSFX = "ui_switchhotbaritem";

    [SerializeField] private List<AnimationClip> testItemHotbarAnimation;
    private AnimationClip currentItemAnimation;

    private AnimatorOverrideController overrideController;

    private bool allowSwap = true;
    public bool _allowSwap => allowSwap;
    private int itemIndex = 0;
    private int visualIndex = 1;

    private bool usingQuacker = false;
    public bool _usingQuacker => usingQuacker;

    private SurvivalStats playerSurvivalStats;
    private UIVisibilityManager visibilityManager;


    private void Start()
    {
        playerSurvivalStats = GetComponent<SurvivalStats>();
        visibilityManager = GetComponent<UIVisibilityManager>();
        //currentItemAnimation = testItemHotbarAnimation[0];
        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = overrideController;

        currentItemAnimation = testItemHotbarAnimation[itemIndex];
        overrideController["DefaultAttack"] = currentItemAnimation;
        weaponImage.sprite = weaponDefaultSprites[itemIndex];
    }

    private void Update()
    {

        weaponImage.sprite = weaponDefaultSprites[visualIndex]; //bad coding

        if (playerSurvivalStats != null && visibilityManager != null)
        {
            if (playerSurvivalStats.canUseEnergyAction && visibilityManager.isUnsheath)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame) // Left mouse button down for new input system
                {
                    animator.SetTrigger("AnimPlay"); // Sets the trigger "AnimPlay" on the pistol animator
                    audioSource1.clip = pistolShoot; // Sets the audio clip of audioSource1 to the pistol shot sound effect
                    audioSource1.Play(); // Plays the clip

                    playerSurvivalStats.DecreaseEnergy(playerSurvivalStats.primaryAttackEnergy);
                }
            }
            else if(!playerSurvivalStats.canUseEnergyAction && visibilityManager.isUnsheath)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame) // Left mouse button down for new input system
                {
                    animator.SetTrigger("AnimPlay"); // Sets the trigger "AnimPlay" on the pistol animator
                    audioSource1.clip = pistolShoot; // Sets the audio clip of audioSource1 to the pistol shot sound effect
                    audioSource1.Play(); // Plays the clip

                    playerSurvivalStats.DecreaseHP(playerSurvivalStats.primaryAttackEnergy);
                }
            }


            else if (!visibilityManager.isUnsheath)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame) // Left mouse button down for new input system
                {
                    visibilityManager.UnsheathWeapon();
                }
            }
        }
        else
        {
            if (Mouse.current.leftButton.wasPressedThisFrame) // Left mouse button down for new input system
            {
                //animator.SetTrigger("AnimPlay"); // Sets the trigger "AnimPlay" on the pistol animator
                audioSource1.clip = pistolShoot; // Sets the audio clip of audioSource1 to the pistol shot sound effect
                audioSource1.Play(); // Plays the clip
            }
        }

        SwapItem();

        //hard code - not meant for final product
        if(currentItemAnimation.name == "TestItem_QuackerAnim")
        {
            usingQuacker = true;
        }
        else
        {
            usingQuacker = false;
        }
    }

    private void SwapItem()
    {
        if (allowSwap)
        {

            if (Mouse.current.scroll.y.ReadValue() > 0)
            {
                SoundManager.Instance.PlaySFX(hotbarSwitchSFX);
                itemIndex++;
                visualIndex++;

                if (itemIndex > testItemHotbarAnimation.Count - 1)
                {
                    itemIndex = 0;
                }
                if (visualIndex > testItemHotbarAnimation.Count - 1)
                {
                    visualIndex = 0;
                }

                currentItemAnimation = testItemHotbarAnimation[itemIndex];
                overrideController["DefaultAttack"] = currentItemAnimation;
            }
            /*
             if (Mouse.current.scroll.y.ReadValue() < 0)
            {
                itemIndex--;

                if (itemIndex < 0)
                {
                    itemIndex = testItemHotbarAnimation.Count - 1;
                }

                currentItemAnimation = testItemHotbarAnimation[itemIndex];
                overrideController["DefaultAttack"] = currentItemAnimation;
            }*/
        }

    }

    public void ChangeAllowSwap(bool result)
    {
        allowSwap = result;
    }
}

