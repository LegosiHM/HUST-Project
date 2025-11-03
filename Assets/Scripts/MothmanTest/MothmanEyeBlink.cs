using System.Collections.Generic;
using UnityEngine;

public class MothmanEyeBlink : MonoBehaviour
{
    [SerializeField] private List<MeshRenderer> mothmanEyes = new List<MeshRenderer>();
    [SerializeField] private float minEyeBlinkInterval = 1f;
    [SerializeField] private float maxEyeBlinkInterval = 5f;

    [SerializeField] private float secondBeforeToggleAgain = 0.05f;
    private float currentRandomInterval;
    private float currentRandomIntervalCount;
    private float secondBeforeToggleCount;
    private bool isToggleOnce;


    void Start()
    {
        currentRandomInterval = Random.Range(minEyeBlinkInterval, maxEyeBlinkInterval);
        currentRandomIntervalCount = currentRandomInterval;
        secondBeforeToggleCount = secondBeforeToggleAgain;
    }

    private void Update()
    {
        EyeBlink();
    }

    private void EyeBlink()
    {
        if(currentRandomIntervalCount > 0)
        {
            currentRandomIntervalCount -= Time.deltaTime;
        }
        else
        {
            if (!isToggleOnce)
            {
                foreach (MeshRenderer eyes in mothmanEyes)
                {
                    eyes.enabled = false;
                }

                isToggleOnce = true;
            }
            else
            {
                if(secondBeforeToggleCount > 0)
                {
                    secondBeforeToggleCount -= Time.deltaTime;
                }
                else
                {
                    foreach (MeshRenderer eyes in mothmanEyes)
                    {
                        eyes.enabled = true;
                    }

                    currentRandomInterval = Random.Range(minEyeBlinkInterval, maxEyeBlinkInterval);
                    currentRandomIntervalCount = currentRandomInterval;
                    secondBeforeToggleCount = secondBeforeToggleAgain;
                    isToggleOnce = false;
                }
            }

        }
    }
}
