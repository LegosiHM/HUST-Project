using UnityEngine;

public class TriggerSoundPlayer : MonoBehaviour
{
    [Header("Pick Audio Event (no string typing needed)")]
    public AudioEvent audioEvent;   // Direct reference to SFX asset

    public void PlaySound()
    {
        if (audioEvent == null)
        {
            Debug.LogWarning($"TriggerSoundPlayer on {name} has no AudioEvent assigned.");
            return;
        }

        SoundManager.Instance.PlaySFX(audioEvent.id);
    }
}
