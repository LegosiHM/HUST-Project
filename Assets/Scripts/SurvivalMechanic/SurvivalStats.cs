using System.Security;
using Unity.Mathematics;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class SurvivalStats : MonoBehaviour
{
    [SerializeField] private Transform crosshair;
    [SerializeField] private float anxiousCrosshairRangeX = 0.05f;
    [SerializeField] private float anxiousCrosshairRangeY = 0.05f;
    [SerializeField] private float anxiousCrosshairMoveSpeed = 0.5f;
    private Vector3 originalCrosshairPosition;

    [SerializeField] private RectTransform handSprite;
    [SerializeField] private float anxiousHandspriteRangeX = 20f;
    [SerializeField] private float anxiousHandspriteRangeY = -10f;
    [SerializeField] private float anxiousHandspriteMoveSpeed = 30f;
    private Vector3 originalHandspritePosition;

    [SerializeField] private Volume postProcessVolume;

    [SerializeField] private float minVignetteIntensity = 0.3f;
    [SerializeField] private float maxVignetteIntensity = 0.9f;
    private Vignette playerVignette;

    [Header("HP")]
    [SerializeField] private float _maxHP = 100f;
    private float _currentHP;
    public float currentHP => _currentHP;

    [Header("Energy")]
    [SerializeField] private float _maxEnergy = 100f;
    [SerializeField] private float _recoveryEnergyOnIdle = 0.1f;
    [SerializeField] private float _baseEnergyRestoreCooldownAfterAction = 3f;

    public float currentEnergy => _currentEnergy;

    [Header("Base Energy Consumption Per Action")]
    [SerializeField] private float _walkingEnergy = 0f; //per second
    [SerializeField] private float _crouchingEnergy = 0.5f; //per second
    [SerializeField] private float _runningEnergy = 1f; //per second
    //[SerializeField] private float _jumpingEnergy = 0.5f; //per use
    [SerializeField] private float _lightAttackEnergy = 1f; //per use
    [SerializeField] private float _specialAttackEnergy = 2f; //per use

    [Header("Speed Adjustment Per Brainwave")]
    [SerializeField] private float baseSpeedPoint = 12f; //at this brainwave point, the speed will be base speed
    [SerializeField] private float softCapSpeedPoint = 60f; //after this point, speed will increase a lot slower
    [SerializeField] private float maxSpeedMultiplier = 2f; //hard cap. Speed will not go over this.
    [SerializeField] private float steepness = 0.05f; //more steepness = value change faster


    [Header("Brainwave")]
    [SerializeField] private float _maxBrainwave = 100f;
    [SerializeField] private float _baseBrainwaveChangeRate = 0.3f;
    [SerializeField] private float _baseWaveDecreaseCooldownAfterGettingHit = 3f;

    public float currentBrainwave => _currentBrainwave;

    private PlayerStateMachine fsm;
    private PlayerContext ctx;
    private PlayerController motor;

    private float baseSpeed;

    [Header("Test Only Inspector")]
    [SerializeField] private float _currentBrainwaveCooldown;
    [SerializeField] private float _currentBrainwave;
    [SerializeField] private int _brainWaveLevel; //Delta (1)-> Theta (2)-> Alpha (3)-> Beta (4)-> Gamma (5)
    [SerializeField] private float _currentBrainwaveChangeRate;

    [SerializeField] private float _currentEnergy;
    [SerializeField] private float _brainwaveEnergyMuliplier; //lower brainwave = less multiplier = less energy consumption
    [SerializeField] private float _currentEnergyConsumption;
    [SerializeField] private float _currentEnergyCooldown;

    [SerializeField] private float _brainwaveSpeedMultiplier;
    [SerializeField] private float _anxiousCrosshairMultiplier;
    void Awake()
    {
        fsm = GetComponent<PlayerStateMachine>();
        ctx = GetComponent<PlayerContext>();
        motor = GetComponent<PlayerController>();
    }

    void Start()
    {
        originalCrosshairPosition = crosshair.transform.localPosition;
        originalHandspritePosition = handSprite.transform.localPosition;
        if(!postProcessVolume.profile.TryGet(out playerVignette))
        {
            Debug.Log("Vignette not found!");
        }

        //set HP
        _currentHP = _maxHP;

        //set Energy
        _currentEnergy = _maxEnergy;
        _currentEnergyCooldown = 0f;

        baseSpeed = motor.walkSpeed;

        //set Brainwave
        _currentBrainwave = 20f;
        _currentBrainwaveCooldown = 0f;
        playerVignette.intensity.value = minVignetteIntensity;
        /*
         * 5 Brainwave States
         * Delta(1)  = 0.5-3.9 -> Deep Sleep State -> Decrease 4.5 times slower from Normal Rate  
         * Theta(2) = 4-7.9 -> Dreaming State -> Decrease 4.5 times slower from Normal Rate
         * Alpha(3) = 8-11.9 -> Calm State -> Decrease 4.5 times slower from Normal Rate
         * Beta(4) = 12-29.9 -> Normal/Starting State -> Decrease At Normal Rate
         * Gamma(5) = 30-100 -> Alert State -> -> Decrease 4.5 times faster than Normal Rate
         * 
         * Higher Brainwaves = move faster, more dmg, more energy consumption
         * Lower Brainwaves = move slower, lower dmg, low energy consumption
         * Attack or Taking DMG slightly increase Brainwave
         */
    }

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame) // test only
        {
            TakeDMG(1f, 1f);
        }

        CheckBrainwaveLevel();
        IdleBrainwaveDecreased();

        CheckGroundMovementEnergyConsumption();
        MovementEnergyConsumption();
    }

    private void IdleBrainwaveDecreased()
    {
        if (_currentBrainwaveCooldown <= 0f)
        {
            if (_brainWaveLevel < 4)
            {
                _currentBrainwaveChangeRate = _baseBrainwaveChangeRate / 4.5f;
            }
            else if (_brainWaveLevel > 4)
            {
                _currentBrainwaveChangeRate = _baseBrainwaveChangeRate * 4.5f;
            }
            else
            {
                _currentBrainwaveChangeRate = _baseBrainwaveChangeRate;
            }

            _currentBrainwave -= _currentBrainwaveChangeRate * Time.deltaTime;
            _currentBrainwave = Mathf.Clamp(_currentBrainwave, 0f, 100f);
        }
        else
        {
            _currentBrainwaveCooldown -= Time.deltaTime;
            _currentBrainwave = Mathf.Clamp(_currentBrainwave, 0f, 100f);
        }

    }

    private void CheckBrainwaveLevel()
    {
        if (_currentBrainwave >= 0 && _currentBrainwave < 4)
        {
            _brainWaveLevel = 1; //Delta
            GraduallyChangeVignette();
        }
        else if (_currentBrainwave >= 4 && _currentBrainwave < 8)
        {
            _brainWaveLevel = 2; //Theta
            GraduallyChangeVignette();
        }
        else if (_currentBrainwave >= 8 && _currentBrainwave < 12)
        {
            _brainWaveLevel = 3; //Alpha
            GraduallyChangeVignette();
        }
        else if (_currentBrainwave >= 12 && _currentBrainwave < 30)
        {
            _brainWaveLevel = 4; //Beta
            crosshair.transform.localPosition = originalCrosshairPosition;
            handSprite.transform.localPosition = originalHandspritePosition;
        }
        else if (_currentBrainwave >= 30)
        {
            _brainWaveLevel = 5; //Gamma
            RandomlyChangeCrosshairPositionWithinRange();
        }
    }

    private void CheckGroundMovementEnergyConsumption()
    {
        if (ctx.Move.sqrMagnitude > 0.01f && !motor.wantSprint && !motor.wantCrouch) //isWalking
        {
            _currentEnergyConsumption = _walkingEnergy;
        }
        else if (ctx.Move.sqrMagnitude > 0.01f && motor.wantSprint && !motor.wantCrouch) //isRunning
        {
            _currentEnergyConsumption = _runningEnergy;
        }
        else if (ctx.Move.sqrMagnitude > 0.01f && motor.wantCrouch) //isCrouching
        {
            _currentEnergyConsumption = _crouchingEnergy;
        }
        else
        {
            _currentEnergyConsumption = 0;
        }
    }

    private void CheckBrainwaveEnergyMultiplier()
    {
        _brainwaveEnergyMuliplier = _brainWaveLevel * 0.1f * (_currentBrainwave / 8); //will make 20 (default value) has x1 multiplier
    }
    private void CheckBrainwaveSpeedMultiplier() //12 = 1
    {
        float currentBrainwaveValue = Mathf.Max(_currentBrainwave - baseSpeedPoint, 0f); //shift value using baseSpeedPoint as ref

        //janky formula. will fix later
        float speedDiminishingReturn = currentBrainwaveValue / (currentBrainwaveValue + (softCapSpeedPoint - baseSpeedPoint) / steepness); //diminishing return formula

        _brainwaveSpeedMultiplier = 1f + speedDiminishingReturn * (maxSpeedMultiplier - 1f);

        motor.walkSpeed = baseSpeed * _brainwaveSpeedMultiplier;
    }

    private void MovementEnergyConsumption()
    {
        CheckBrainwaveSpeedMultiplier();
        CheckBrainwaveEnergyMultiplier();
        _currentEnergy -= _currentEnergyConsumption * _brainwaveEnergyMuliplier * Time.deltaTime;
        motor.walkSpeed = baseSpeed * _brainwaveSpeedMultiplier;
    }

    public void TakeDMG(float damage, float increaseBrainwave)
    {
        _currentBrainwave += 3;
        _currentBrainwaveCooldown = _baseWaveDecreaseCooldownAfterGettingHit;
    }
    private void CheckBrainwaveAnxiousCrosshairMultiplier()
    {
        _anxiousCrosshairMultiplier = (_currentBrainwave / 30) - 1; //will be 0 at 30, 1 at 60, 2 at 90+
    }

    public void RandomlyChangeCrosshairPositionWithinRange()
    {
        CheckBrainwaveAnxiousCrosshairMultiplier();

        Vector3 newCrosshairPosition = new Vector3(Random.Range(-anxiousCrosshairRangeX * _anxiousCrosshairMultiplier, anxiousCrosshairRangeX * _anxiousCrosshairMultiplier),
                                                Random.Range(-anxiousCrosshairRangeY * _anxiousCrosshairMultiplier, anxiousCrosshairRangeY * _anxiousCrosshairMultiplier),
                                                originalCrosshairPosition.z);

        crosshair.transform.localPosition = Vector3.MoveTowards(crosshair.transform.localPosition, newCrosshairPosition, anxiousCrosshairMoveSpeed * _anxiousCrosshairMultiplier * Time.deltaTime);

        Vector3 newHandspritePosition = new Vector3(Random.Range(-anxiousHandspriteRangeX * _anxiousCrosshairMultiplier, anxiousHandspriteRangeX * _anxiousCrosshairMultiplier),
                                                Random.Range(anxiousHandspriteRangeY , originalHandspritePosition.y),
                                                originalHandspritePosition.z);

        handSprite.transform.localPosition = Vector3.MoveTowards(handSprite.transform.localPosition, newHandspritePosition, anxiousHandspriteMoveSpeed * _anxiousCrosshairMultiplier * Time.deltaTime);

    }

    private void GraduallyChangeVignette() //0 = maxIntensity, 12 = minIntensity
    {
        playerVignette.intensity.value = minVignetteIntensity + ((maxVignetteIntensity-minVignetteIntensity) - (_currentBrainwave / (12/ maxVignetteIntensity - minVignetteIntensity)));
        playerVignette.intensity.value = Mathf.Clamp(playerVignette.intensity.value, minVignetteIntensity, maxVignetteIntensity);
    }
}
