using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Placeable", menuName = "Items/Placeable")]
public class Placeable : Item
{
    public int placementHeight;
    public int placementWidth;
    public bool hasOutline;

}
