using System.Collections;
using UnityEngine;

public class TriggerTeleport : MonoBehaviour
{
    [SerializeField] private Transform _linkedObject;
    [SerializeField] private Transform _teleportDestination;

    [SerializeField] private bool _isOneTime = true;

    [SerializeField] private string _sound;


    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerContext>() != null)
        {
            GetComponent<TriggerSoundPlayer>()?.PlaySound();

            _linkedObject.position = _teleportDestination.position;

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
