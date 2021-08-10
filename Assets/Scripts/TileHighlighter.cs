using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileHighlighter : MonoBehaviour
{
    public MouseCursor cursor;
    public GameObject overlayContainer;
    public Sprite activeOverlaySprite;
    public Sprite inactiveOverlaySprite;
    public GameObject player;
    Ray2D ray;
    RaycastHit2D hit;
    public GameObject currentTile;
    public bool isInteractable;
    PlayerControls controls;
    public EquippedItem equippedItem;
    public LayerMask layerMask;
    public float interactableDistance = 5f;
    private void Start()
    {
        controls = new PlayerControls();
        controls.Gameplay.Enable();
        controls.Gameplay.Fire1.performed += ctx => OnClick();
    }
     
     void Update()
     {
         hit = Physics2D.Raycast(cursor.cursorPosition, cursor.transform.forward, 20, layerMask);
         if(hit)
         {
            if (hit.collider.tag == "Soil") {
                currentTile = hit.collider.gameObject;
                overlayContainer.SetActive(true);
                overlayContainer.transform.position = currentTile.transform.position;
                PlantGrowthManager growthManager = currentTile.GetComponent<PlantGrowthManager>();
                bool outOfRange = Vector3.Distance(player.transform.position, currentTile.transform.position) > interactableDistance;
                if (outOfRange) {
                    isInteractable = false;
                }
                else if (!outOfRange) {
                    isInteractable = true;
                }
            } else {
                overlayContainer.SetActive(false);
                currentTile = null;
            }
            if (isInteractable) overlayContainer.GetComponent<SpriteRenderer>().sprite = activeOverlaySprite;
            else overlayContainer.GetComponent<SpriteRenderer>().sprite = inactiveOverlaySprite;
         } else {
            overlayContainer.SetActive(false);
            currentTile = null;
         }
        
     }

    void OnClick() {
        if (currentTile != null && isInteractable) {
            PlantGrowthManager plantGrowthManager = currentTile.GetComponent<PlantGrowthManager>();
            if (plantGrowthManager.currentlyPlantedGrowable != null) {
                if (plantGrowthManager.isHarvestable) {
                    plantGrowthManager.Harvest();
                }
            } else {
                plantGrowthManager.AddPlant(equippedItem.equippedItem.growable);
            }
        }
    }

    // // Update is called once per frame
    // void Update()
    // {
    //     Vector3Int cellPosition = gridLayout.WorldToCell(cursor.cursorPosition);
    //     TileBase tile = tileMap.GetTile(cellPosition);
    //     overlay.transform.position = cellPosition;
    //     Debug.Log(tile);
    // }
}
