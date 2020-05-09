using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<ItemObject> items = new List<ItemObject>();

    public void Add(ItemObject item)
    {
        items.Add(item);
    }

    public void Remove(ItemObject item)
    {
        items.Remove(item);
    }
}
