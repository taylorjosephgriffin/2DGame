using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantGrowthManager : MonoBehaviour
{
    public Growable currentlyPlantedGrowable;
    public GameObject plantContainer;
    GameObject currentPlant;
    public int minutesSincePlanted = 0;
    public WorldTime worldTime;
    public int worldDayPlanted;
    public int currentTotalWorldMinute;
    public int daysSincePlanted = 0;
    public bool isHarvestable = false;
    public int minutesPerStage;
    public int currentStage;
    public Material plantMaterial;
    public GameObject dropItemComponent;

    private void Start()
    {
    }
    private void Update()
    {
        if (currentlyPlantedGrowable != null) {
            bool isFullGrown = daysSincePlanted == currentlyPlantedGrowable.daysTillFullGrown;

            if (currentTotalWorldMinute != worldTime.currentTotalWorldMinutes) minutesSincePlanted += 1;

            if (isFullGrown) isHarvestable = true;

            if (isHarvestable) currentStage = currentlyPlantedGrowable.stages.Length - 1;
            else if (
                minutesSincePlanted % minutesPerStage == 0 
                && minutesSincePlanted != 0 
                && isHarvestable == false) {
                    currentStage++;
                }
            
            currentTotalWorldMinute = worldTime.currentTotalWorldMinutes;
            daysSincePlanted = minutesSincePlanted / worldTime.minInDay;
            
            currentPlant.GetComponent<SpriteRenderer>().sprite = currentlyPlantedGrowable.stages[currentStage];
            worldDayPlanted = worldTime.dayCount;
        }
    }

    public void AddPlant(Growable plant) {
        currentlyPlantedGrowable = plant;
        currentTotalWorldMinute = worldTime.currentTotalWorldMinutes;
        InitializePlant();
        minutesPerStage = ((worldTime.minInDay * currentlyPlantedGrowable.daysTillFullGrown) - 1) / (currentlyPlantedGrowable.stages.Length - 1); 
    }

    public void Harvest() {
        int harvestYield = Random.Range(currentlyPlantedGrowable.minHarvestQuantity, currentlyPlantedGrowable.maxHarvestQuantity);
        for (int i = 0; i < harvestYield; i++) {
            GameObject droppedItem = Instantiate(dropItemComponent, transform);
            droppedItem.GetComponent<ItemPickup>().item = currentlyPlantedGrowable;
            droppedItem.transform.SetParent(null);
        }
        Reset();
    }

    void Reset() {
        minutesSincePlanted = 0;
        worldDayPlanted = 0;
        currentlyPlantedGrowable = null;
        currentTotalWorldMinute = 0;
        daysSincePlanted = 0;
        isHarvestable = false;
        currentStage = 0;
        Destroy(currentPlant);
    }

    void InitializePlant() {
        currentPlant = Instantiate(new GameObject(), plantContainer.transform);
        currentPlant.name = "Plant";
        currentPlant.transform.SetParent(plantContainer.transform);
        currentPlant.AddComponent<SpriteRenderer>();
        currentPlant.GetComponent<SpriteRenderer>().sprite = currentlyPlantedGrowable.stages[daysSincePlanted];
        currentPlant.GetComponent<Renderer>().material = plantMaterial;
        currentPlant.AddComponent<PositionRendererSorter>();
        GameObject currentPlantSorter = Instantiate(new GameObject(), plantContainer.transform);
        currentPlantSorter.transform.SetParent(currentPlant.transform);
        currentPlantSorter.transform.position = Vector3.zero;
        currentPlantSorter.name = "SortAnchor";
        currentPlantSorter.transform.localPosition = new Vector3(0, -1.2f, 1);
        PositionRendererSorter sorter = currentPlant.GetComponent<PositionRendererSorter>();
        sorter.sortAnchor = currentPlantSorter;
        sorter.myRenderer = currentPlant.GetComponent<SpriteRenderer>();
    }
}

