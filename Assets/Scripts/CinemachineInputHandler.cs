using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonCameraController : MonoBehaviour
{
    [Header("References")]
    public Transform playerBody;         // Player root for yaw rotation
    public Transform cameraTarget;       // Camera target (head position)
    public CinemachineCamera cinemachineCam; // Virtual camera 3.x
    public InputActionAsset inputActions;

    [Header("Settings")]
    public float sensitivityX = 0.15f;
    public float sensitivityY = 0.15f;
    public float minPitch = -85f;
    public float maxPitch = 85f;

    private InputAction lookAction;
    private float pitch;

    void Awake()
    {
        lookAction = inputActions.FindActionMap("Player", true).FindAction("Look", true);
    }

    void OnEnable() => lookAction.Enable();
    void OnDisable() => lookAction.Disable();

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        Vector2 look = lookAction.ReadValue<Vector2>();
        float mouseX = look.x * sensitivityX;
        float mouseY = look.y * sensitivityY;

        // Rotate player body (yaw)
        playerBody.Rotate(Vector3.up * mouseX);

        // Adjust camera pitch (up/down)
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        cameraTarget.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
