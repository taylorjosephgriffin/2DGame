using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Plantable", menuName = "Items/Plantable")]
public class Plantable : Item
{
    public Growable growable;
    public Ingredient ingredient;

}
