using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : ScriptableObject
{
   public string itemName;
   public string itemDescription;
   public Sprite itemIcon;
   public float inventoryIconScale = 1f;
   public int stackLimit;
   public Sprite itemDropShadow;
   public Sprite itemDropIcon;

   public Color itemDropLightColor;
}
