using UnityEngine;
using UnityEngine.InputSystem;   

public class Interactor : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Transform interactorSource;  
    [SerializeField] private float interactRange = 3f;

    [Header("UI")]
    [SerializeField] private GameObject interactIcon;     
    [SerializeField] private GameObject pickupIcon;       

    [Header("References")]
    public InventoryManager inventoryManager;              

    private IInteractable currentInteractable;
    private bool lookingAtItem;

    private void Awake()
    {
        if (interactorSource == null)
            interactorSource = transform;

        if (inventoryManager == null)
            inventoryManager = FindAnyObjectByType<InventoryManager>();

        if (interactIcon != null) interactIcon.SetActive(false);
        if (pickupIcon != null) pickupIcon.SetActive(false);
    }

    private void Update()
    {
        UpdateCurrentInteractable();
        HandleInteractInput();
    }

    private void UpdateCurrentInteractable()
    {
        currentInteractable = null;
        lookingAtItem = false;

        if (Physics.Raycast(interactorSource.position,
                            interactorSource.forward,
                            out RaycastHit hitInfo,
                            interactRange))
        {
            if (hitInfo.collider.TryGetComponent<IInteractable>(out var interactObj))
            {
                currentInteractable = interactObj;
                // If this interactable is an Item, we use the pickup UI
                if (interactObj is Item)
                    lookingAtItem = true;
            }
        }

        // Toggle icons
        bool hasTarget = currentInteractable != null;

        if (pickupIcon != null)
            pickupIcon.SetActive(hasTarget && lookingAtItem);

        if (interactIcon != null)
            interactIcon.SetActive(hasTarget && !lookingAtItem);
    }

    private void HandleInteractInput()
    {
        if (currentInteractable == null)
            return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            currentInteractable.Interact(this);
        }
    }
}
