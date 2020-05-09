using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ItemObject : ScriptableObject
{
    public string name;
    public string description;

    public Sprite sprite;

    public int attack;
    public int durability;

}
