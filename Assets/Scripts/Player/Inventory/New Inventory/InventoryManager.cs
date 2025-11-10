using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private GameObject inventoryMenu;
    private bool menuActive = false;

    public ItemSlot[] itemSlot;


    void Update()
    {
        // Make sure keyboard is connected
        if (Keyboard.current == null) return;

        // Toggle with Tab key
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            menuActive = !menuActive;
            inventoryMenu.SetActive(menuActive);

            // Cursor control and input lock
            if (menuActive)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0f; // optional pause
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1f; // resume game
            }
        }
    }

    public int AddItem(string itemName, int quantity, Sprite itemSprite , string itemDescription)
    {
        for (int i = 0; i < itemSlot.Length; i++)
        {
            if (itemSlot[i].isFull == false && itemSlot[i].name == name || itemSlot[i].quantity == 0)
            {
                int leftOverItems = itemSlot[i].AddItem(itemName, quantity, itemSprite, itemDescription);
                if (leftOverItems>0)
                {
                    leftOverItems = AddItem(itemName, leftOverItems, itemSprite, itemDescription);
                    return leftOverItems;
                }

                
            }
        }
        return quantity;
    }

    public void DeselectAllSlots()
    {
        for (int i = 0; i < itemSlot.Length; i++)
        {
            itemSlot[i].selectedShader.SetActive(false);
            itemSlot[i].isSelected = false;
        }
    }
}
