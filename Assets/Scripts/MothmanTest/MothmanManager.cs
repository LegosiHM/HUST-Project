using UnityEngine;

public class MothmanManager : MonoBehaviour
{
    [SerializeField] private float brainwaveIncreaseAfterDisappear = 60f;
    private SurvivalStats playerSurvivalStats;

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
        Destroy(gameObject);
    }
}
