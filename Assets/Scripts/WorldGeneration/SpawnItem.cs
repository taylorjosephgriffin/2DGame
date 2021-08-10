using UnityEngine;

[CreateAssetMenu(fileName = "SpawnItem", menuName = "World Generation/Spawn Item")]
public class SpawnItem : ScriptableObject
{
  public GameObject item;
  public int chance;
  public int max;
  public int minRadius;
}