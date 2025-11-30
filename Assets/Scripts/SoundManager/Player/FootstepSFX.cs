using UnityEngine;

[RequireComponent(typeof(PlayerContext))]
[RequireComponent(typeof(CharacterController))]
public class FootstepSFX : MonoBehaviour
{
    [Header("Sound ID in AudioLibrary")]
    [SerializeField] private string walkSfxId = "sfx_walk";

    [Header("Timings")]
    [SerializeField] private float walkInterval = 0.5f;   // time between steps
    [SerializeField] private float minMoveSpeed = 0.2f;   // ignore tiny sliding

    private PlayerContext ctx;
    private CharacterController cc;
    private AudioSource stepSource;

    private float stepTimer;

    void Awake()
    {
        ctx = GetComponent<PlayerContext>();
        cc = GetComponent<CharacterController>();

        // ONE audio source just for footsteps
        stepSource = gameObject.AddComponent<AudioSource>();
        stepSource.playOnAwake = false;
        stepSource.loop = false;
        stepSource.spatialBlend = 0f; // 2D sound

        // Route through SFX mixer group
        if (SoundManager.Instance != null && SoundManager.Instance.sfxGroup != null)
        {
            stepSource.outputAudioMixerGroup = SoundManager.Instance.sfxGroup;
        }
    }

    void Update()
    {
        if (ctx == null || cc == null) return;

        bool grounded = ctx.IsGrounded;
        bool hasInput = ctx.Move.sqrMagnitude > 0.01f;

        Vector3 horizontalVel = new Vector3(cc.velocity.x, 0f, cc.velocity.z);
        bool hasSpeed = horizontalVel.magnitude > minMoveSpeed;

        bool isWalking = grounded && hasInput && hasSpeed;

        if (!isWalking)
        {
            // IMPORTANT: kill any currently playing step immediately
            stepTimer = 0f;

            if (stepSource != null && stepSource.isPlaying)
                stepSource.Stop();

            return;
        }

        stepTimer += Time.deltaTime;

        if (stepTimer >= walkInterval)
        {
            PlayStep();
            stepTimer = 0f;
        }
    }

    private void PlayStep()
    {
        if (SoundManager.Instance == null || SoundManager.Instance.library == null)
            return;

        var evt = SoundManager.Instance.library.Get(walkSfxId);
        if (evt == null || evt.clip == null)
            return;

        // Restart clip instead of stacking sounds
        stepSource.Stop();
        stepSource.clip = evt.clip;
        stepSource.volume = evt.volume;
        stepSource.pitch = evt.GetPitch();
        stepSource.Play();
    }
}
