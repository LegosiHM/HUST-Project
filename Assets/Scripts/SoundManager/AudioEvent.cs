using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Audio Event")]
public class AudioEvent : ScriptableObject
{
    public string id;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;

    public float pitch = 1f;

    public bool randomizePitch = false;
    public float randomPitchMin = 0.95f;
    public float randomPitchMax = 1.05f;

    public float GetPitch()
    {
        return randomizePitch ? Random.Range(randomPitchMin, randomPitchMax) : pitch;
    }
}
