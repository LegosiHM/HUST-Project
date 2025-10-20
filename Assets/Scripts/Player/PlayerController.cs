using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Gravity))]
public class PlayerController : MonoBehaviour
{
    [Header("Speeds")]
    public float walkSpeed = 4.0f;
    public float sprintMultiplier = 1.6f;
    public float crouchMoveSpeed = 2.0f;

    [Header("Capsule / Crouch")]
    public float standHeight = 1.8f;
    public float crouchHeight = 1.1f;
    public float heightChangeSpeed = 6f;
    public Vector3 controllerCenterStand = new Vector3(0, 0.9f, 0);
    public Vector3 controllerCenterCrouch = new Vector3(0, 0.55f, 0);

    [Header("Camera Target (Cinemachine)")]
    public Transform cameraTarget;
    public float cameraLerpSpeed = 8f;

    [Header("Camera Heights")]
    public float eyeHeightStand = 1.65f;
    public float eyeHeightCrouch = 1.0f;

    [Header("Jump")]
    public float jumpSpeed = 6.0f;

    [HideInInspector] public bool wantSprint;
    [HideInInspector] public bool wantCrouch;
    [HideInInspector] public bool wantJumpPulse;
    [HideInInspector] public Vector2 moveInput;

    CharacterController cc;
    Gravity gravity;

    float currentTargetHeight;
    Vector3 bodyLocalScaleStand = Vector3.one;
    Vector3 bodyLocalPosStand = Vector3.zero;

    // Cache movement direction for FixedUpdate
    Vector3 wishDir = Vector3.zero;
    float smoothedHeight;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        gravity = GetComponent<Gravity>();

        cc.height = standHeight;
        cc.center = controllerCenterStand;
        currentTargetHeight = standHeight;
        smoothedHeight = standHeight;
    }

    void Update()
    {
        // --- Handle camera and visuals per frame (not physics) ---
        float t = Mathf.InverseLerp(standHeight, crouchHeight, smoothedHeight);
    }

    void FixedUpdate()
    {
        // --- Handle movement & physics in fixed time steps ---

        // --- Calculate movement direction relative to the camera --- //
        var move = moveInput;
        Transform cam = Camera.main.transform;

        // Use camera forward/right but ignore vertical tilt
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // Build movement vector relative to camera
        wishDir = (move.y * camForward) + (move.x * camRight);
        wishDir = Vector3.ClampMagnitude(wishDir, 1f);

        bool crouching = wantCrouch;
        bool sprinting = wantSprint && !crouching;

        float speed = crouching ? crouchMoveSpeed :
                      sprinting ? walkSpeed * sprintMultiplier :
                                  walkSpeed;

        Vector3 horizontal = wishDir * speed;
        Vector3 velocity = new Vector3(horizontal.x, gravity.VerticalVelocity, horizontal.z);

        // Move character
        cc.Move(velocity * Time.fixedDeltaTime);

        // Adjust capsule height and center smoothly
        if (crouching)
        {
            currentTargetHeight = crouchHeight;
            cc.center = Vector3.Lerp(cc.center, controllerCenterCrouch, Time.fixedDeltaTime * heightChangeSpeed);
        }
        else
        {
            currentTargetHeight = HasHeadroomToStand() ? standHeight : crouchHeight;
            cc.center = Vector3.Lerp(cc.center,
                Mathf.Approximately(currentTargetHeight, standHeight) ? controllerCenterStand : controllerCenterCrouch,
                Time.fixedDeltaTime * heightChangeSpeed);
        }

        float newHeight = Mathf.MoveTowards(cc.height, currentTargetHeight, heightChangeSpeed * Time.fixedDeltaTime);
        if (!Mathf.Approximately(newHeight, cc.height))
            cc.height = newHeight;

        // Apply jump pulse if requested
        ConsumeJumpPulseIfAny(s => gravity.Jump(s), jumpSpeed);

        // Store smoothed height for visual update
        smoothedHeight = cc.height;

        // --- Update Cinemachine camera target height ---
        if (cameraTarget != null)
        {
            float t = Mathf.InverseLerp(standHeight, crouchHeight, cc.height);
            float targetY = Mathf.Lerp(eyeHeightStand, eyeHeightCrouch, t);
            Vector3 targetPos = new Vector3(0, targetY, 0);
            cameraTarget.localPosition = Vector3.Lerp(cameraTarget.localPosition, targetPos, cameraLerpSpeed * Time.fixedDeltaTime);
        }

    }

    public void ConsumeJumpPulseIfAny(System.Action<float> onJump, float jumpSpeedValue)
    {
        if (!wantJumpPulse) return;
        onJump?.Invoke(jumpSpeedValue);
        wantJumpPulse = false;
    }

    bool HasHeadroomToStand()
    {
        float radius = Mathf.Max(0.1f, cc.radius * 0.95f);
        Vector3 head = transform.position + cc.center + Vector3.up * (cc.height * 0.5f - radius);
        return !Physics.SphereCast(head, radius * 0.95f, Vector3.up, out _, 0.15f,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
    }
}
