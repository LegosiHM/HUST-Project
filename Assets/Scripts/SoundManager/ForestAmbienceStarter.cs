using UnityEngine;

public class ForestAmbienceStarter : MonoBehaviour
{
    [SerializeField] private string ambienceId = "ambience_nightforest";

    void Start()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayAmbience(ambienceId, true);
        }
    }
}
