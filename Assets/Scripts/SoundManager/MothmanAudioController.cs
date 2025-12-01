using System.Collections;
using UnityEngine;

public class MothmanAudioController : MonoBehaviour
{
    [Header("Audio Event IDs (match AudioLibrary)")]
    [SerializeField] private string staticSfxId = "sfx_statictv";
    [SerializeField] private string jumpscareSfxId = "sfx_jumpscare";

    [Header("Distance → Volume")]
    [SerializeField] private Transform player;
    [SerializeField] private float minDistance = 2f;     // volume max at or closer than this
    [SerializeField] private float maxDistance = 25f;    // beyond this = almost silent
    [SerializeField] private float minStaticVolume = 0.05f;
    [SerializeField] private float maxStaticVolume = 1f;

    [Header("Fade Out After Jumpscare")]
    [SerializeField] private float staticFadeOutTime = 2.5f;

    private bool jumpscareTriggered = false;
    private float currentVolume = 0f;
    private Coroutine fadeOutRoutine;

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        // Start with faint static if player starts inside range
        UpdateStaticVolume();
    }

    private void Update()
    {
        if (jumpscareTriggered) return;

        UpdateStaticVolume();
    }

    private void UpdateStaticVolume()
    {
        if (SoundManager.Instance == null || player == null) return;

        float dist = Vector3.Distance(player.position, transform.position);

        // Outside maxDistance → basically silent
        if (dist >= maxDistance)
        {
            currentVolume = 0f;
            // If you want it completely off when far:
            SoundManager.Instance.StopContinuous(staticSfxId);
            return;
        }

        // Map distance to [0..1] proximity (0 = far, 1 = close)
        float t = Mathf.InverseLerp(maxDistance, minDistance, dist);
        t = Mathf.Clamp01(t);

        // Convert to actual volume between min + max
        currentVolume = Mathf.Lerp(minStaticVolume, maxStaticVolume, t);

        // This will start or update the loop
        SoundManager.Instance.PlayContinuous(staticSfxId, currentVolume);
    }

    public void TriggerJumpscare()
    {
        if (jumpscareTriggered) return;
        jumpscareTriggered = true;

        currentVolume = maxStaticVolume;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayContinuous(staticSfxId, currentVolume);
        }

        if (fadeOutRoutine != null)
            StopCoroutine(fadeOutRoutine);

        fadeOutRoutine = StartCoroutine(FadeOutStatic());
    }


    private IEnumerator FadeOutStatic()
    {
        float startVol = currentVolume;
        float t = 0f;

        while (t < staticFadeOutTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / staticFadeOutTime);

            float v = Mathf.Lerp(startVol, 0f, k);
            currentVolume = v;

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayContinuous(staticSfxId, currentVolume);

            yield return null;
        }

        if (SoundManager.Instance != null)
            SoundManager.Instance.StopContinuous(staticSfxId);

        currentVolume = 0f;
    }
}
