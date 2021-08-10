using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Utils;

public class InventoryManager : MonoBehaviour
{
    [HideInInspector]
    public static InventoryManager instance;

    public List<Item> inventory = new List<Item>();

    public delegate void OnItemAddCallback(Item item);

    public delegate void OnItemRemoveCallback(Item item);

    public OnItemRemoveCallback onItemRemoveCallback;

    public OnItemAddCallback onItemAddCallback;
    AudioSource inventoryAudioSource;

    public AudioClip addItemSound;
    public AudioClip removeItemSound;
    public AudioClip openInventorySound;
    PlayerControls controls;

    bool inventoryOpen = false;

    public Transform inventoryUI;
    public InventoryUI inventorySlots;
    public Text itemNameTooltip;
    public Text itemDescriptionTooltip; 

    public Weapon currentWeapon; 

    public GameObject inventoryFulltext;

    bool showingInventoryFullText = false;
    public bool inventoryIsFull = false;
    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public bool InventoryIsFull() {
    for (int i = 0; i < inventorySlots.slots.Length; i++) {
        if (inventorySlots.slots[i].item != null && inventorySlots.slots[i].quantity >= inventorySlots.slots[i].item.stackLimit) {
            continue;
        } else return false;
    }
    return true;
    }

    public IEnumerator FadeInInventoryFullText() {
        if (!showingInventoryFullText) {
            showingInventoryFullText = true;
            inventoryFulltext.SetActive(true);
            inventoryFulltext.GetComponent<Animator>().SetBool("IsVisible", true);
            yield return new WaitForSeconds(4);
            inventoryFulltext.GetComponent<Animator>().SetBool("IsVisible", false);
            yield return new WaitForSeconds(2f);
            inventoryFulltext.SetActive(false);
            showingInventoryFullText = false;
        }
    }

    private void Start()
    {
        controls = new PlayerControls();
        controls.Gameplay.Enable();
        inventorySlots.GetComponent<InventoryUI>().Init();
        inventoryAudioSource = GetComponent<AudioSource>();
        inventoryIsFull = InventoryIsFull();
        controls.Gameplay.Inventory.performed += ctx => triggerInventoryStateChange();
    }

    public void AddItem(Item newItem, GameObject drop)
    {
        inventoryIsFull = InventoryIsFull();
        if (!inventoryIsFull) {
            inventory.Add(newItem);
            Destroy(drop);
            inventoryAudioSource.clip = addItemSound;
            inventoryAudioSource.Play();
            if (onItemAddCallback != null) onItemAddCallback.Invoke(newItem);
        } else {
            StartCoroutine(FadeInInventoryFullText());
        }
    }

    public void RemoveItem(Item newItem)
    {
        inventoryIsFull = InventoryIsFull();
        inventory.Remove(newItem);
        inventoryAudioSource.clip = removeItemSound;
        inventoryAudioSource.Play();
        if (onItemRemoveCallback != null) onItemRemoveCallback.Invoke(newItem);
    }

    void triggerInventoryStateChange()
    {
        inventoryAudioSource.clip = openInventorySound;
        if (!inventoryOpen && UIManager.currentUIState == UIManager.UIStates.IN_GAME)
        {
            inventoryAudioSource.Play();
            inventoryOpen = true;
            inventoryUI.gameObject.SetActive(true);
            PauseManager.PauseGame();
            UIManager.currentUIState = UIManager.UIStates.INVENTORY;
        }
        else if (inventoryOpen && UIManager.currentUIState == UIManager.UIStates.INVENTORY)
        {
            inventoryAudioSource.Play();
            inventoryOpen = false;
            inventoryUI.gameObject.SetActive(false);
            PauseManager.PlayGame();
            UIManager.currentUIState = UIManager.UIStates.IN_GAME;
        }
    }
}
