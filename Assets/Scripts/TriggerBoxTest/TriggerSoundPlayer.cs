using UnityEngine;

public class TriggerSoundPlayer : MonoBehaviour
{
    [Header("Pick Audio Event (no string typing needed)")]
    public AudioEvent audioEvent;      

    [Header("Optional Override")]
    [Range(0f, 2f)]
    public float customVolume = 1f;    

    public bool useCustomVolume = false;   

    public void PlaySound()
    {
        if (audioEvent == null)
        {
            Debug.LogWarning($"TriggerSoundPlayer on {name} has no AudioEvent assigned.");
            return;
        }

        if (!useCustomVolume)
        {
            SoundManager.Instance.PlaySFX(audioEvent.id);
            return;
        }

        float pitch = audioEvent.GetPitch();
        float volume = customVolume;

        AudioSource source = SoundManager.Instance.GetSFXSource();
        if (source == null) return;

        source.pitch = pitch;
        source.PlayOneShot(audioEvent.clip, volume);
    }
}
