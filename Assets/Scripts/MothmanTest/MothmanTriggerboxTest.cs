using UnityEngine;

public class MothmanTriggerboxTest : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("Enter: " + gameObject.name);
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
