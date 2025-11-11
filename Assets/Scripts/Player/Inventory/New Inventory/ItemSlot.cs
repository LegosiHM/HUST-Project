using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IPointerClickHandler
{

    //=====Item Data=====//
    public string itemName;
    public int quantity;
    public Sprite itemSprite;
    public bool isFull;
    public string itemDescription;
    public Sprite emptySprite;

    [SerializeField] private int maxNumberOfItems;

    //=====Item Slot=====//
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image itemImage;

    //=====Item Description Slot=====//
    public Image itemDescriptionImage;
    public TMP_Text itemDescriptionNameText;
    public TMP_Text itemDescriptionText;

    public GameObject selectedShader;
    public bool isSelected;

    private InventoryManager inventoryManager;

    private void Start()
    {
        //inventoryManager = GameObject.Find("InGameUICanvas").GetComponent<InventoryManager>();
        inventoryManager = FindAnyObjectByType<InventoryManager>();
    }

    public int AddItem(string itemName, int quantity, Sprite itemSprite, string itemDescription)
    {
        //Check to see if slot is already full
        if (isFull)
        {
            return quantity;
        }

        //Update Name
        this.itemName = itemName;


        //Update Sprite
        this.itemSprite = itemSprite;
        itemImage.sprite = itemSprite;

        //Update Description
        this.itemDescription = itemDescription;

        //Update Quantity
        this.quantity += quantity;
        if (this.quantity >= maxNumberOfItems)
        {
            quantityText.text = maxNumberOfItems.ToString();
            quantityText.enabled = true;
            isFull = true;


            //Return leftover items
            int extraItems = this.quantity - maxNumberOfItems;
            this.quantity = maxNumberOfItems;
            return extraItems;
        }

        //Update Quantity Text
        quantityText.text = this.quantity.ToString();
        quantityText.enabled = true;

        return 0;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("OnPointerClick fired on " + gameObject.name + " button: " + eventData.button);

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }
    }

    public void OnLeftClick()
    {
        if (isSelected)
        {
            bool usable = inventoryManager.UseItem(itemName);
            if (usable)
            {
                this.quantity -= 1;
                quantityText.text = this.quantity.ToString();
                if (this.quantity <= 0)
                {
                    EmptySlot();
                }
            }
            
        }
        else
        {
            inventoryManager.DeselectAllSlots();

            selectedShader.SetActive(true);
            isSelected = true;
            itemDescriptionNameText.text = itemName;
            itemDescriptionText.text = itemDescription;
            itemDescriptionImage.sprite = itemSprite;
            if (itemDescriptionImage.sprite == null)
            {
                itemDescriptionImage.sprite = emptySprite;
                itemDescriptionNameText.text = "Empty Slot";
                itemDescriptionText.text = "This slot is empty.";
            }
        }


    }

    private void EmptySlot()
    {
        quantityText.enabled = false;
        itemImage.sprite = emptySprite;

        itemDescriptionNameText.text = "";
        itemDescriptionText.text = "";
        itemDescriptionImage.sprite = emptySprite;
    }

    public void OnRightClick()
    {
        //Create a new Item
        GameObject itemToDrop = new GameObject(itemName);
        Item newItem = itemToDrop.AddComponent<Item>();
        newItem.Initialize(itemName, 1, itemSprite, itemDescription);

        //Create and modify The SR
        SpriteRenderer sr = itemToDrop.AddComponent<SpriteRenderer>();
        sr.sprite = itemSprite;
        sr.sortingLayerName = "Items";

        //Add a collider
        itemToDrop.AddComponent<BoxCollider>();
        itemToDrop.GetComponent<BoxCollider>().isTrigger = true;
        itemToDrop.AddComponent<LookAtPlayer>();

        //set the Location
        itemToDrop.transform.localPosition = GameObject.FindWithTag("Player").transform.position + new Vector3(-0.5f, 0.5f, 0);
        itemToDrop.transform.localScale = new Vector3(0.03f, 0.03f, 1f);

        //Subtract from quantity
        this.quantity -= 1;
        quantityText.text = this.quantity.ToString();
        if (this.quantity <= 0)
        {
            EmptySlot();
        }

    }
}
