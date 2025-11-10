using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

public class UIVisibilityManager: MonoBehaviour
{
    [Header("Weapon Visibility")]
    [SerializeField] private float weaponSheathDelay = 5f;
    [SerializeField] private Transform weaponHand;
    [SerializeField] private float sheathSpeed = 3f;
    [SerializeField] private Vector3 sheathBy = new Vector3(0,150,0);
    private Vector3 newPosition;
    private float sheathDelayCount;

    private bool _isUnsheath = true; //is unsheath = is using weapon
    public bool isUnsheath => _isUnsheath;

    [Header("UI Visibility")]
    [SerializeField] private CanvasGroup uiCanvas;
    [SerializeField] private float hideUIDelay = 5f;
    private float hideUIDelayCount;
    
    private void Start()
    {
        sheathDelayCount = weaponSheathDelay;
        hideUIDelayCount = hideUIDelay;
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


        IdleSheathWeapon();
        IdleHideUI();
    }

    public void ToggleSheathWeapon()
    {
        _isUnsheath = !isUnsheath;
    }

    public void UnsheathWeapon()
    {
        _isUnsheath = true ;
    }

    public void SheathWeapon()
    {
        _isUnsheath = false ;
    }

    private void IdleSheathWeapon()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame) // reset sheat delay
        {
            sheathDelayCount = weaponSheathDelay;
        }

        if (!_isUnsheath)
        {
            return;
        }

        if(sheathDelayCount <= 0)
        {
            SheathWeapon();
        }
        else
        {
            sheathDelayCount-= Time.deltaTime;
        }
    }
    
    private void IdleHideUI()
    {
        if (!_isUnsheath) //if hide weapon
        {
            if (hideUIDelayCount > 0)
            {
                hideUIDelayCount -= Time.deltaTime;
            }
            else
            {
                if (uiCanvas.alpha > 0)
                {
                    uiCanvas.alpha -= Time.deltaTime;
                }
            }
        }
        else //if use weapon => these should count if there's change in HP or Energy instead
        {
            if (uiCanvas.alpha <1)
            {
                uiCanvas.alpha += Time.deltaTime;
            }

            hideUIDelayCount = hideUIDelay;

        }
    }
}
