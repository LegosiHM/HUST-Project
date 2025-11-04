using UnityEngine;

public class MothmanTriggerboxTest : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string EnterAnimationName;
    [SerializeField] private string ExitAnimationName;
    [SerializeField] private string IdleAnimationName;
    [SerializeField] private Animator mothmanScareAnimator;
    [SerializeField] private float AreaBrainwaveValue = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if(EnterAnimationName != null)
            {
                //mothmanScareAnimator.Play(EnterAnimationName);

                mothmanScareAnimator.Play(IdleAnimationName);
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
            if (EnterAnimationName != null)
            {
                mothmanScareAnimator.Play(ExitAnimationName);
            }
            other.GetComponent<SurvivalStats>().ResetBrainwaveAreaValue();
        }
        else
        {
            return;
        }
    }   


}
