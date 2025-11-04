using UnityEngine;

public class MothmanManager : MonoBehaviour
{
    [SerializeField] private float brainwaveIncreaseAfterDisappear = 60f;
    private SurvivalStats playerSurvivalStats;

    [SerializeField] private string EnterAnimationName = "LV1_Enter";
    [SerializeField] private string ExitAnimationName = "LV1_Exit";
    [SerializeField] private Animator mothmanScareAnimator;

    private void Start()
    {
        playerSurvivalStats = FindAnyObjectByType<SurvivalStats>();
    }

    public void IncreasePlayerBrainwave()
    {
        playerSurvivalStats.TakeDMG(0f, 60f);
    }

    public void DestroySelf()
    {
        playerSurvivalStats.ResetBrainwaveAreaValue();
        Destroy(gameObject);
    }

    public void PlayEnterScareAnimation()
    {
        if (EnterAnimationName != null)
        {
            mothmanScareAnimator.Play(EnterAnimationName);
        }
    }
    public void PlayExitScareAnimation()
    {
        if (ExitAnimationName != null)
        {
            //mothmanScareAnimator.Play(EnterAnimationName);

            mothmanScareAnimator.Play(ExitAnimationName);
        }
    }
}
