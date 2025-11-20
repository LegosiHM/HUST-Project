using UnityEngine;

[CreateAssetMenu]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public StatToChange statToChange =  new StatToChange();
    public int amountToChangeStat;

    public AttributesToChange attributesToChange = new AttributesToChange();
    public int amountToChangeAttribute;

    public bool UseItem()
    {
        if (statToChange == StatToChange.health)
        {
            //PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
            //if (playerHealth != null) { }
            if (amountToChangeStat > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
           
        }
        return false;
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
