using UnityEngine;

public class MothmanFinalTriggerBox : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Animator mothmanAnimator;
    [SerializeField] private string finalAnimationName = "Mothman";
    [SerializeField] private float AreaBrainwaveValue = 0f;

    [Header("Audio")]
    [SerializeField] private MothmanAudioController mothmanAudio; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            mothmanAnimator.Play(finalAnimationName);

            if (mothmanAudio != null)
            {
                mothmanAudio.TriggerJumpscare();
            }
        }
        else
        {
            return;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            other.GetComponent<SurvivalStats>().AdjustBrainwaveAreaValue(AreaBrainwaveValue);
        }
        else
        {
            return;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("Exit: " + gameObject.name);
        }
        else
        {
            return;
        }
    }
}
