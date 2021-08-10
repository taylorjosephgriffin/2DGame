using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootTable : MonoBehaviour
{
    [System.Serializable]
    public class Drop {
        public Item item;
        public int dropFrequency;
        public int minQuantity = 0;
        public int maxQuantity = 1;
        public int Quantity { get; set; }
    }
    public GameObject dropItemPrefab;

    public List <Drop> itemTable = new List<Drop>();

    public Drop[] currentDrop;

    public int dropChance;

    public Vector3 enemyCollisionPosition;
    GameObject player;

    
    void Start() {

        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void GenerateAndInstaniateLoot() {
        ArrayList dropList = CalculateLoot();
        InstantiateLoot(dropList);
    }

    public ArrayList CalculateLoot() {
        ArrayList dropList = new ArrayList();
        float randomChance = Random.Range(0f, 100f);
 
        foreach (Drop drop in itemTable) {
            if (randomChance < drop.dropFrequency) {
                drop.Quantity = RandomQuantity(drop);
                dropList.Add(drop);
            }
        }
        return dropList;
    }
    void InstantiateLoot(ArrayList dropList) {
        float calculatedDropChance = Random.Range(0f, 100f);
        
        if (calculatedDropChance < dropChance) {
            foreach (Drop drop in dropList) {    
                for (int i = 0; i < drop.Quantity; i++) {
                GameObject newDrop = Instantiate(dropItemPrefab, transform);
                newDrop.GetComponent<ItemPickup>().item = drop.item;
                newDrop.transform.SetParent(null);
                continue;
           }
        }
        };
    }


    int RandomQuantity(Drop drop) {
        return Random.Range(drop.minQuantity, drop.maxQuantity);
    }
}
