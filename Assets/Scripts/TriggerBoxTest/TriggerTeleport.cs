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
            //put play sound script here

            _linkedObject.position = _teleportDestination.position;

            if (_isOneTime)
            {
                DestroyAfter(1);
               
            }
        }

    }
    private IEnumerator DestroyAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
