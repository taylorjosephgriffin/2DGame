using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCreationData : MonoBehaviour
{
    public CharacterCreation[] characterCreators;
    public Color32 skinTone;
    public Color32 hairColor;
    public Color32 eyeColor;
    public Sprite hairStyle;

    public Color32 facialHairColor;

    public Sprite facialHair;

    // Start is called before the first frame update
    private void Start()
    {
        CreationInit();
    }

    public void CreationInit()
    {
        foreach (CharacterCreation creator in characterCreators) {
            creator.Init();
        }
    }

    public void SetHairStyle(Sprite selectedStyle) {
        hairStyle = selectedStyle;
        SetButtonInteractableStatus(true, characterCreators[2].gameObject);
    }

    public void ResetHairStyle() {
        hairStyle = null;
        if (hairStyle == null) SetButtonInteractableStatus(false, characterCreators[2].gameObject);
    }

    public void SetFacialHair(Sprite selectedStyle) {
        facialHair = selectedStyle;
        SetButtonInteractableStatus(true, characterCreators[5].gameObject);
    }

    public void ResetFacialHair() {
        facialHair = null;
        if (facialHair == null) SetButtonInteractableStatus(false, characterCreators[5].gameObject);
    }

    void SetButtonInteractableStatus(bool isInteractable, GameObject creator) {
        Transform presets = creator.transform.GetChild(0);
        Button[] creatorButtons = presets.GetComponentsInChildren<Button>();
        foreach (Button button in creatorButtons) {
            button.interactable = isInteractable;
        }
    }
}
