using System.Collections;
using UnityEngine;

public class TriggerSoundOnly : MonoBehaviour
{
    [SerializeField] private string _sound;
    [SerializeField] private bool _isOneTime = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerContext>() != null)
        {
            GetComponent<TriggerSoundPlayer>()?.PlaySound();

            if (_isOneTime)
                StartCoroutine(DestroyAfter(1));
        }
    }



    private IEnumerator DestroyAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}

