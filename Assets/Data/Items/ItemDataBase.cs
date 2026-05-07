using System.Collections.Generic;
using Assets.Data;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItemDatabase", menuName = "Loot/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [Tooltip("Drag every single ItemData  in the game into this list")]
    public List<ItemData> AllItems = new List<ItemData>();

    // The Save System will use this method to turn an ID string back into an item
    public ItemData GetItemByID(string itemID)
    {
        if (string.IsNullOrEmpty(itemID)) return null;

        foreach (ItemData item in AllItems)
        {
            if (item.ItemID == itemID)
            {
                return item;
            }
        }

        Debug.LogWarning($"ItemDatabase could not find an item with the ID: {itemID}");
        return null;
    }
}
