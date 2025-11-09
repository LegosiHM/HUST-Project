using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

public class UIVisibilityManager: MonoBehaviour
{
    [Header("Weapon Visibility")]
    [SerializeField] private float weaponSheathDelay = 5f;
    [SerializeField] private Transform weaponHand;
    [SerializeField] private float sheathSpeed = 10f;
    [SerializeField] private Vector3 sheathBy = new Vector3(0,150,0);
    private Vector3 newPosition;
    private float sheathDelayCount;

    private bool _isUnsheath = true; //is unsheath = is using weapon
    public bool isUnsheath => _isUnsheath;


    private void Start()
    {
        sheathDelayCount = weaponSheathDelay;
        newPosition = weaponHand.transform.position - sheathBy;
    }

    private void Update()
    {
        if (!_isUnsheath)
        {
            weaponHand.transform.position = Vector3.MoveTowards(weaponHand.transform.position, newPosition, sheathSpeed);
        }

        if (Keyboard.current.rKey.wasPressedThisFrame) // test only - decrease Brainwave
        {
            ToggleSheathWeapon();
        }
    }

    public void ToggleSheathWeapon()
    {
        _isUnsheath = !isUnsheath;
    }
}
