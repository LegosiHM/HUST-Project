using System.Diagnostics.Tracing;
using System.Security;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;


public class SurvivalStats : MonoBehaviour
{
    private UIVisibilityManager visibilityManager;

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

    [Header("Brainwave Visual")]
    [SerializeField] private Volume postProcessVolume;

    [SerializeField] private float minVignetteIntensity = 0.3f;
    [SerializeField] private float maxVignetteIntensity = 0.9f;
    private Vignette playerVignette;

    [SerializeField] private float maxFilmGrainIntensity = 1.0f;
    [SerializeField] private float maxChromaticIntensity = 1.0f;
    private FilmGrain playerFilmGrain;
    private ChromaticAberration playerChromatic;

    [Header("HP")]
    [SerializeField] private float _maxHP = 100f;
    public float maxHP => _maxHP;
    private float _currentHP;
    public float currentHP => _currentHP;

    [Header("Energy")]
    [SerializeField] private float _maxEnergy = 100f;
    public float maxEnergy => _maxEnergy;
    [SerializeField] private float _recoveryEnergyOnIdle = 0.1f;
    [SerializeField] private float _baseEnergyRestoreCooldownAfterAction = 3f;

    public float currentEnergy => _currentEnergy;

    [Header("Base Energy Consumption Per Action")]
    [SerializeField] private float _walkingEnergy = 0f; //per second
    [SerializeField] private float _crouchingEnergy = 0.5f; //per second
    [SerializeField] private float _runningEnergy = 1f; //per second
    //[SerializeField] private float _jumpingEnergy = 0.5f; //per use
    [SerializeField] private float _primaryAttackEnergy = 1f; //per use - light attack
    public float primaryAttackEnergy => _primaryAttackEnergy; //for test only. will change to delegate function later
    [SerializeField] private float _secondaryAttackEnergy = 2f; //per use - heavy attack

    [Header("Speed Adjustment Per Brainwave")]
    [SerializeField] private float baseSpeedPoint = 12f; //at this brainwave point, the speed will be base speed
    [SerializeField] private float softCapSpeedPoint = 60f; //after this point, speed will increase a lot slower
    [SerializeField] private float maxSpeedMultiplier = 2f; //hard cap. Speed will not go over this.
    [SerializeField] private float steepness = 0.05f; //more steepness = value change faster


    [Header("Brainwave")]
    [SerializeField] private float _maxBrainwave = 100f;
    public float maxBrainwave => _maxBrainwave;
    [SerializeField] private float _baseBrainwaveChangeRate = 0.3f;
    [SerializeField] private float _baseWaveDecreaseCooldownAfterGettingHit = 3f;

    public float currentBrainwave => _currentBrainwave;

    [Header("Brainwave Audio (Whispers)")]
    [SerializeField] private string whisperSfxId = "sfx_whisper";
    [SerializeField] private float whisperMinBrainwave = 10f;    // start being audible after this
    [SerializeField] private float whisperMaxBrainwave = 100f;   // full volume near this
    [SerializeField][Range(0f, 1f)] private float whisperMinVolume = 0.02f;
    [SerializeField][Range(0f, 1f)] private float whisperMaxVolume = 0.6f;

    [Header("Energy Audio (Tired)")]
    [SerializeField] private string tiredSfxId = "sfx_playertired";
    [SerializeField] private float tiredStartEnergy = 0f;   // start tired SFX at or below this
    [SerializeField] private float tiredStopEnergy = 15f;  // stop tired SFX when energy recovers above this
    [SerializeField][Range(0f, 1f)] private float tiredVolume = 1f;

    private bool isTiredSfxPlaying = false;


    private float currentWhisperVolume = 0f;

    private PlayerStateMachine fsm;
    private PlayerContext ctx;
    private PlayerController motor;

    private float baseSpeed;

    [Header("Test Only Inspector")]
    [SerializeField] private float _currentBrainwaveCooldown;
    [SerializeField] private float _currentBrainwave;
    [SerializeField] private int _brainWaveLevel; //Delta (1)-> Theta (2)-> Alpha (3)-> Beta (4)-> Gamma (5)
    public int brainWaveLevel => _brainWaveLevel;
    [SerializeField] private float _currentBrainwaveChangeRate;

    [SerializeField] private float _currentEnergy;
    [SerializeField] private float _brainwaveEnergyMuliplier; //lower brainwave = less multiplier = less energy consumption
    [SerializeField] private float _currentEnergyConsumption;
    [SerializeField] private float _currentEnergyCooldown;

    [SerializeField] private float _brainwaveSpeedMultiplier;
    [SerializeField] private float _anxiousCrosshairMultiplier;

    [SerializeField] private float currentBrainwaveAreaValue;

    [SerializeField] private bool _canUseEnergyAction;
    public bool canUseEnergyAction => _canUseEnergyAction;

    void Awake()
    {
        fsm = GetComponent<PlayerStateMachine>();
        ctx = GetComponent<PlayerContext>();
        motor = GetComponent<PlayerController>();
        visibilityManager = GetComponent<UIVisibilityManager>();
    }

    void Start()
    {
        originalCrosshairPosition = crosshair.transform.localPosition;
        originalHandspritePosition = handSprite.transform.localPosition;

        if (!postProcessVolume.profile.TryGet(out playerVignette))
        {
            Debug.Log("Vignette not found!");
        }
        if (!postProcessVolume.profile.TryGet(out playerFilmGrain))
        {
            Debug.Log("FilmGrain not found!");
        }
        if (!postProcessVolume.profile.TryGet(out playerChromatic))
        {
            Debug.Log("Chromatic not found!");
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
        currentBrainwaveAreaValue = 0f;
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
        /*
        if (Keyboard.current.qKey.wasPressedThisFrame) // test only - decrease Brainwave
        {
            IncreaseBrainwave(-3f);
        }
        if (Keyboard.current.eKey.wasPressedThisFrame) // test only - increase Brainwave
        {
            IncreaseBrainwave(3f);
        }
        if (Keyboard.current.iKey.wasPressedThisFrame) // test only - decrease HP
        {
            DecreaseHP(5f);
        }
        if (Keyboard.current.oKey.wasPressedThisFrame) // test only - increase HP
        {
            DecreaseHP(-5f);
        }
        if (Keyboard.current.kKey.wasPressedThisFrame) // test only - decrease Energy
        {
            DecreaseEnergy(5f);
        }
        if (Keyboard.current.lKey.wasPressedThisFrame) // test only - increase Energy
        {
            DecreaseEnergy(-5f);
        }
        */
        CheckIfHaveEnergyLeft();

        CheckBrainwaveLevel();
        IdleBrainwaveDecreased();

        CheckGroundMovementEnergyConsumption();
        MovementEnergyConsumption();

        IncreaseBrainwaveInArea();
        IdleEnergyIncrease();

        UpdateWhisperAudio();
        UpdateTiredAudio();
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
            if (visibilityManager.isUnsheath)
            {
                handSprite.transform.localPosition = Vector3.MoveTowards(handSprite.transform.localPosition, originalHandspritePosition, 10f);
            }
            playerFilmGrain.intensity.value = 0f;
            playerChromatic.intensity.value = 0f;
        }
        else if (_currentBrainwave >= 30)
        {
            _brainWaveLevel = 5; //Gamma
            RandomlyChangeCrosshairPositionWithinRange();
            GraduallyChangeFilmGrain();
            GraduallyChangeChromatic();
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

        _currentEnergy = Mathf.Clamp(_currentEnergy, 0f, _maxEnergy);
    }


    private void IdleEnergyIncrease()
    {
        if (!motor.wantSprint || !motor.wantCrouch) //if not sprint or crouch
        {
            _currentEnergy += _recoveryEnergyOnIdle * _brainwaveEnergyMuliplier * Time.deltaTime;
            _currentEnergy = Mathf.Clamp(_currentEnergy, 0, _maxEnergy);
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

    //Stats Consumption
    private void CheckIfHaveEnergyLeft()
    {
        if (_currentEnergy <= 0)
        {
            _canUseEnergyAction = false;

        }

        else if (_currentEnergy >= 10)
        {
            _canUseEnergyAction = true;
        }
    }

    private void MovementEnergyConsumption()
    {
        CheckBrainwaveSpeedMultiplier();
        CheckBrainwaveEnergyMultiplier();
        _currentEnergy -= _currentEnergyConsumption * _brainwaveEnergyMuliplier * Time.deltaTime;
        if (_canUseEnergyAction)
        {
            motor.walkSpeed = baseSpeed * _brainwaveSpeedMultiplier;
        }
    }

    public void DecreaseHP(float damage)
    {
        _currentHP -= damage;
        _currentHP = Mathf.Clamp(_currentHP, 0f, _maxHP);
    }

    public void IncreaseBrainwave(float amount)
    {
        _currentBrainwave += amount;
        _currentBrainwaveCooldown = _baseWaveDecreaseCooldownAfterGettingHit;
        _currentBrainwave = Mathf.Clamp(_currentBrainwave, 0f, _maxBrainwave);

    }

    public void DecreaseEnergy(float amount)
    {
        CheckBrainwaveEnergyMultiplier();
        _currentEnergy -= amount * _brainwaveEnergyMuliplier/2;
        _currentEnergy = Mathf.Clamp(_currentEnergy, 0f, _maxEnergy);
    }

    public void IncreaseEnergy(float amount)
    {
        CheckBrainwaveEnergyMultiplier();
        _currentEnergy += amount;
        _currentEnergy = Mathf.Clamp(_currentEnergy, 0f, _maxEnergy);
    }

    //Anxious State
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
                                                Random.Range(anxiousHandspriteRangeY, originalHandspritePosition.y),
                                                originalHandspritePosition.z);

        handSprite.transform.localPosition = Vector3.MoveTowards(handSprite.transform.localPosition, newHandspritePosition, anxiousHandspriteMoveSpeed * _anxiousCrosshairMultiplier * Time.deltaTime);

    }

    //visual
    private void GraduallyChangeVignette() //0 = maxIntensity, 12 = minIntensity
    {
        playerVignette.intensity.value = minVignetteIntensity + ((maxVignetteIntensity - minVignetteIntensity) - (_currentBrainwave / (12 / maxVignetteIntensity - minVignetteIntensity)));
        playerVignette.intensity.value = Mathf.Clamp(playerVignette.intensity.value, minVignetteIntensity, maxVignetteIntensity);
    }
    private void GraduallyChangeFilmGrain() //100 = maxIntensity, 30 = 0
    {
        playerFilmGrain.intensity.value = ((_currentBrainwave - 30) / 70) * maxFilmGrainIntensity;
        playerFilmGrain.intensity.value = Mathf.Clamp(playerFilmGrain.intensity.value, 0, maxFilmGrainIntensity);
    }
    private void GraduallyChangeChromatic() //100 = maxIntensity, 30 = 0
    {
        playerChromatic.intensity.value = ((_currentBrainwave - 30) / 70) * maxChromaticIntensity;
        playerChromatic.intensity.value = Mathf.Clamp(playerChromatic.intensity.value, 0, maxChromaticIntensity);
    }

    //brainwave area
    public void AdjustBrainwaveAreaValue(float value)
    {
        if (value > currentBrainwaveAreaValue)
        {
            currentBrainwaveAreaValue = value;
        }
        else
        {
            return;
        }
    }
    public void ResetBrainwaveAreaValue()
    {
        currentBrainwaveAreaValue = 0f;
    }

    private void IncreaseBrainwaveInArea()
    {
        if (currentBrainwaveAreaValue > 0)
        {
            _currentBrainwaveCooldown = _baseWaveDecreaseCooldownAfterGettingHit;
            _currentBrainwave += currentBrainwaveAreaValue * Time.deltaTime;
        }
    }
    private void UpdateWhisperAudio()
    {
        if (SoundManager.Instance == null)
            return;

        float brain = _currentBrainwave;

        // Below threshold → almost or completely silent
        if (brain <= whisperMinBrainwave)
        {
            currentWhisperVolume = 0f;

            // Option A: keep the loop alive but silent:
            SoundManager.Instance.PlayContinuous(whisperSfxId, currentWhisperVolume);

            // Option B (if you want it truly off when calm) instead:
            // SoundManager.Instance.StopContinuous(whisperSfxId);

            return;
        }

        // Map brainwave from [whisperMinBrainwave .. whisperMaxBrainwave] to [0..1]
        float t = Mathf.InverseLerp(whisperMinBrainwave, whisperMaxBrainwave, brain);
        t = Mathf.Clamp01(t);

        // Remap [0..1] to [whisperMinVolume .. whisperMaxVolume]
        currentWhisperVolume = Mathf.Lerp(whisperMinVolume, whisperMaxVolume, t);

        // This will start the continuous sound if not playing, or update volume if already playing
        SoundManager.Instance.PlayContinuous(whisperSfxId, currentWhisperVolume);
    }
    private void UpdateTiredAudio()
    {
        if (SoundManager.Instance == null)
            return;

        // Start tired SFX when energy is drained
        if (!isTiredSfxPlaying && _currentEnergy <= tiredStartEnergy)
        {
            isTiredSfxPlaying = true;
            SoundManager.Instance.PlayContinuous(tiredSfxId, tiredVolume);
        }
        // Stop tired SFX when energy has recovered enough
        else if (isTiredSfxPlaying && _currentEnergy >= tiredStopEnergy)
        {
            isTiredSfxPlaying = false;
            SoundManager.Instance.StopContinuous(tiredSfxId);
        }
    }

}
