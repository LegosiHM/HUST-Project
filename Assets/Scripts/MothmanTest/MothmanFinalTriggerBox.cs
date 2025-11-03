using UnityEngine;

public class MothmanFinalTriggerBox : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Animator mothmanAnimator;
    [SerializeField] private string finalAnimationName = "Mothman";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            mothmanAnimator.Play(finalAnimationName);
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
