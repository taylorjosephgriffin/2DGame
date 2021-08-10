using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Growable", menuName = "Items/Growable")]
public class Growable : Item
{
    public Sprite[] stages;
    public int daySinceCreation;
    public int daysTillFullGrown;
    public Item growItem;
    public int minHarvestQuantity;
    public int maxHarvestQuantity;
    public bool isHarvestable;
    public int size = 1;
}
