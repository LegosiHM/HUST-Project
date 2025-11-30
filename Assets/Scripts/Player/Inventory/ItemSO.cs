using UnityEngine;

[CreateAssetMenu]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public StatToChange statToChange = new StatToChange();
    public int amountToChangeStat;

    public AttributesToChange attributesToChange = new AttributesToChange();
    public int amountToChangeAttribute;

    [Header("Audio")]
    public string useItemSfxId = "sfx_itemused"; // can override this per item in Inspector



    public bool UseItem()
    {
        bool canUse = false;

        if (statToChange == StatToChange.health)
        {
            //PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
            //if (playerHealth != null) { }

            if (amountToChangeStat > 0)
            {
                // item can be used
                canUse = true;

                // playerHealth.Heal(amountToChangeStat);
            }
            else
            {
                canUse = false;
            }
        }


        if (canUse && SoundManager.Instance != null && !string.IsNullOrEmpty(useItemSfxId))
        {
            SoundManager.Instance.PlaySFX(useItemSfxId);
        }

        return canUse;
    }



    public enum StatToChange
    {
        none,
        health,
        mana,
        stamina,
    };

    public enum AttributesToChange
    {
        none,
        strength,
        defense,
        intelligence,
        agility
    };
}
