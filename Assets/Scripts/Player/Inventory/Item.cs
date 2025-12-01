// Item.cs
using UnityEngine;

public class Item : MonoBehaviour, IInteractable
{
    [SerializeField] private string itemName;
    [SerializeField] private int quantity;
    [SerializeField] private Sprite sprite;
    [TextArea]
    [SerializeField] private string itemDescription;

    private InventoryManager inventoryManager;
    [Header("Audio")]
    [SerializeField] private string pickupSfxId = "sfx_itempickup";

    [Header("Material")]
    [SerializeField] private Material baseMaterial;
    [SerializeField] private Material interactableMaterial;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        inventoryManager = GameObject.FindAnyObjectByType<InventoryManager>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    // Used when spawning items from other scripts if you need it
    public void Initialize(string itemName, int quantity, Sprite sprite, string itemDescription)
    {
        this.itemName = itemName;
        this.quantity = quantity;
        this.sprite = sprite;
        this.itemDescription = itemDescription;
    }

    private void Update()
    {
        //ChangeMaterialToNormal();
    }

    // NEW: interact pickup (no more walk-to-pickup)
    public void Interact(Interactor interactor)
    {
        // make sure we have an inventory reference
        if (inventoryManager == null)
            inventoryManager = GameObject.FindAnyObjectByType<InventoryManager>();

        if (inventoryManager == null)
        {
            Debug.LogWarning("Item: No InventoryManager found in scene.");
            return;
        }

        int leftOverItems = inventoryManager.AddItem(itemName, quantity, sprite, itemDescription);

        if (leftOverItems < quantity)
        {
            SoundManager.Instance.PlaySFX(pickupSfxId);
        }

        if (leftOverItems <= 0)
        {
            // all picked up
            Destroy(gameObject);
        }
        else
        {
            // stack not full, keep leftover on ground
            quantity = leftOverItems;
        }
    }

    
    /*
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            int leftOverItems = inventoryManager.AddItem(itemName, quantity, sprite, itemDescription);
            ...
        }
    }
    */

    public void ChangeMaterialToInteractable()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.material = interactableMaterial;
    }

    public void ChangeMaterialToNormal()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.material = baseMaterial;
    }
}
