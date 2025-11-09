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

    [Header("Others")]
    [SerializeField] private SurvivalStats playerSurvivalStats;
    private float currentHP => playerSurvivalStats.currentHP;
    private float maxHP => playerSurvivalStats.maxHP;
    private float currentEnergy => playerSurvivalStats.currentEnergy;
    private float maxEnergy => playerSurvivalStats.maxEnergy;

    void Start()
    {
        count = HealthChangeDelay;
    }
    
    void Update()
    {
        HealthVisualChange();
        EnergyVisualChange();
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
}
