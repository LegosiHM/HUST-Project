using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponControl : MonoBehaviour
{
    [SerializeField] private Animator animator; // Animator component
    [SerializeField] private AudioSource audioSource1; // Audiosource component
    [SerializeField] private AudioClip pistolShoot; // Pistol shot audio clip

    private SurvivalStats playerSurvivalStats;
    private UIVisibilityManager visibilityManager;

    private void Start()
    {
        playerSurvivalStats = GetComponent<SurvivalStats>();
        visibilityManager = GetComponent<UIVisibilityManager>();
    }

    private void Update()
    {
        if(playerSurvivalStats != null && visibilityManager != null)
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
            else if (!visibilityManager.isUnsheath)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame) // Left mouse button down for new input system
                {
                    visibilityManager.ToggleSheathWeapon();
                }
            }
        }
        else
        {
            if (Mouse.current.leftButton.wasPressedThisFrame) // Left mouse button down for new input system
            {
                animator.SetTrigger("AnimPlay"); // Sets the trigger "AnimPlay" on the pistol animator
                audioSource1.clip = pistolShoot; // Sets the audio clip of audioSource1 to the pistol shot sound effect
                audioSource1.Play(); // Plays the clip
            }
        }

    }
}
