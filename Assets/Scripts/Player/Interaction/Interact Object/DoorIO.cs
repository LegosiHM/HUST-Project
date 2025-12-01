using UnityEngine;

public class DoorIO : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    [SerializeField] private string requiredItemName = "Key01";
    [SerializeField] private bool requiresItem = true;
    [SerializeField] private Animator doorAnimator;

    private bool isOpen = false;

    public void Interact(Interactor interactor)
    {
        if (isOpen)
            return;

        if (requiresItem)
        {
            var inv = interactor.inventoryManager;
            if (inv == null)
            {
                Debug.LogWarning("DoorInteractable: no InventoryManager on Interactor.");
                return;
            }

            if (!inv.HasItem(requiredItemName))
            {
                Debug.Log("Door is locked. Need item: " + requiredItemName);
                
                return;
            }
        }

        OpenDoor();
    }

    private void OpenDoor()
    {
        isOpen = true;

        // Always play door sound
        SoundManager.Instance.PlaySFX("sfx_kickdoor");

        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Open");
        }
        else
        {
            transform.Rotate(0f, 90f, 0f);
        }
    }

}
