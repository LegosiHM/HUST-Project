using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private string itemName;
    [SerializeField] private int quantity;
    [SerializeField] private Sprite sprite;
    [TextArea]
    [SerializeField] private string itemDescription;

    private InventoryManager inventoryManager;

    void Start()
    {
        inventoryManager = GameObject.FindAnyObjectByType<InventoryManager>();
    }

    public void Initialize(string itemName, int quantity, Sprite sprite, string itemDescription)
    {
        this.itemName = itemName;
        this.quantity = quantity;
        this.sprite = sprite;
        this.itemDescription = itemDescription;
    }

    private void OnTriggerEnter(Collider collision)
    {
    Debug.Log("Collided with " + collision.gameObject.name + "Tag = " + collision.gameObject.tag);
        if (collision.gameObject.tag == "Player")
        {
            int leftOverItems = inventoryManager.AddItem(itemName, quantity, sprite, itemDescription);
            if (leftOverItems <= 0)
            {
                Destroy(gameObject);
            }
            else
            {
                quantity = leftOverItems;
            }
        }
    }

}
