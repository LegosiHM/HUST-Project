using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    [Header("HP UI")]
    [SerializeField] private Image HealthUI;
    [SerializeField] private Image HealthChangeUI;
    [SerializeField] private float HealthChangeDelay = 2f;
    private float count;

    [Header("Energy UI")]
    [SerializeField] private Image EnergyUI;

    [Header("Brainwave UI")]
    [SerializeField] private Renderer brainwaveUI;

    [Header("Others")]
    [SerializeField] private SurvivalStats playerSurvivalStats;
    private float currentHP => playerSurvivalStats.currentHP;
    private float maxHP => playerSurvivalStats.maxHP;
    private float currentEnergy => playerSurvivalStats.currentEnergy;
    private float maxEnergy => playerSurvivalStats.maxEnergy;
    private float brainWaveLevel => playerSurvivalStats.brainWaveLevel;

    void Start()
    {
        count = HealthChangeDelay;
    }
    
    void Update()
    {
        HealthVisualChange();
        EnergyVisualChange();
        SetBrainwaveState();
    }

    private void HealthVisualChange() //will change to work with delegate function later
    {
        Vector3 healthUIScale = HealthUI.transform.localScale;
        Vector3 healthChangeUIScale = HealthChangeUI.transform.localScale;

        healthUIScale.x = currentHP / maxHP;
        HealthUI.transform.localScale = healthUIScale;

        if (count > 0) //still need to make HealthChangeUI Update immediately if HP Increase => will fix as change to delegate funciton
        {
            count -= Time.deltaTime;
        }
        else
        {
            healthChangeUIScale.x -= Time.deltaTime;
            healthChangeUIScale.x = Mathf.Clamp(healthChangeUIScale.x, healthUIScale.x, 1);

            HealthChangeUI.transform.localScale = healthChangeUIScale;

            if(HealthChangeUI.transform.localScale == HealthUI.transform.localScale)
            {
                count = HealthChangeDelay;
            }
        }
    }
    private void EnergyVisualChange() //will change to work with delegate function later
    {
        Vector3 energyUIScale = EnergyUI.transform.localScale;

        energyUIScale.x = currentEnergy / maxEnergy;
        EnergyUI.transform.localScale = energyUIScale;
    }

    private void SetBrainwaveState()
    {
        if(brainWaveLevel == 1)
        {
            brainwaveUI.material.SetFloat("_WavesAmount", 1.5f);
            brainwaveUI.material.SetFloat("_WavesAmp", 0.23f);
            brainwaveUI.material.SetFloat("_Speed", 0.25f);
            brainwaveUI.material.SetFloat("_NoiseAmp", 7f);
            brainwaveUI.material.SetFloat("_NoiseScale", 1f);
        }
        else if (brainWaveLevel == 2)
        {
            brainwaveUI.material.SetFloat("_WavesAmount", 2f);
            brainwaveUI.material.SetFloat("_WavesAmp", 0.3f);
            brainwaveUI.material.SetFloat("_Speed", 0.3f);
            brainwaveUI.material.SetFloat("_NoiseAmp", 7f);
            brainwaveUI.material.SetFloat("_NoiseScale", 1.2f);
        }
        else if(brainWaveLevel == 3)
        {
            brainwaveUI.material.SetFloat("_WavesAmount", 6f);
            brainwaveUI.material.SetFloat("_WavesAmp", 0.22f);
            brainwaveUI.material.SetFloat("_Speed", 0.5f);
            brainwaveUI.material.SetFloat("_NoiseAmp", 1.2f);
            brainwaveUI.material.SetFloat("_NoiseScale", 2.3f);
        }
        else if (brainWaveLevel == 4)
        {
            brainwaveUI.material.SetFloat("_WavesAmount", 12f);
            brainwaveUI.material.SetFloat("_WavesAmp", 0.1f);
            brainwaveUI.material.SetFloat("_Speed", 0.5f);
            brainwaveUI.material.SetFloat("_NoiseAmp", 2.42f);
            brainwaveUI.material.SetFloat("_NoiseScale", 2.3f);
        }
        else if (brainWaveLevel == 5)
        {
            brainwaveUI.material.SetFloat("_WavesAmount", 20f);
            brainwaveUI.material.SetFloat("_WavesAmp", 0.071f);
            brainwaveUI.material.SetFloat("_Speed", 0.6f);
            brainwaveUI.material.SetFloat("_NoiseAmp", 1.5f);
            brainwaveUI.material.SetFloat("_NoiseScale", 2f);
        }

    }
}
