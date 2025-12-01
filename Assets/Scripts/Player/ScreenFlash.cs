using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; 

public class ScreenFlash : MonoBehaviour
{
    [SerializeField] private Volume volume;  

    [SerializeField] private float flashDuration = 0.25f;
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color failColor = Color.yellow;

    private ColorAdjustments colorAdjustments;
    private Coroutine flashRoutine;

    private void Awake()
    {
        if (volume == null)
        {
            volume = GetComponent<Volume>();
        }

        if (volume != null && volume.profile != null)
        {
            if (!volume.profile.TryGet(out colorAdjustments))
            {
                Debug.LogWarning("ScreenFlash: ColorAdjustments override not found on Volume profile.");
            }
        }
        else
        {
            Debug.LogWarning("ScreenFlash: Volume or Volume profile not assigned.");
        }
    }

    public void FlashSuccess()
    {
        StartFlash(successColor);
    }

    public void FlashFail()
    {
        StartFlash(failColor);
    }

    private void StartFlash(Color color)
    {
        if (colorAdjustments == null)
            return;

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashRoutine(color));
    }

    private IEnumerator FlashRoutine(Color flashColor)
    {
        float t = 0f;
        Color baseColor = Color.white; 

        while (t < flashDuration)
        {
            float normalized = t / flashDuration;
            float strength = 1f - Mathf.Abs(2f * normalized - 1f);

            colorAdjustments.colorFilter.value = Color.Lerp(baseColor, flashColor, strength);

            t += Time.deltaTime;
            yield return null;
        }

        colorAdjustments.colorFilter.value = baseColor;
        flashRoutine = null;
    }
}
