using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterCreation : MonoBehaviour
{
    CharacterCreation instance;
    public enum CharacterDataTypes { FACIAL_HAIR, FACIAL_HAIR_COLOR, HAIR_STYLE, HAIR_COLOR, SKIN, EYE_COLOR };
    public CharacterDataTypes currentCharacterDataType;
    public Image characterImage;
    Color32 defaultColor;
    public Sprite selectionSprite;
    Texture2D selectionSpriteTexture;
    public Button defaultSelection;

    public CharacterCreationData characterCreationData;

    public string stepName;

    public void Init()
    {
        if (instance == null) instance = this;
        if (currentCharacterDataType == CharacterDataTypes.HAIR_COLOR || currentCharacterDataType == CharacterDataTypes.HAIR_STYLE ) {
            defaultColor = characterCreationData.hairColor;
        } else if (currentCharacterDataType == CharacterDataTypes.SKIN) {
            defaultColor = characterCreationData.skinTone;
        } else if (currentCharacterDataType == CharacterDataTypes.FACIAL_HAIR || currentCharacterDataType == CharacterDataTypes.FACIAL_HAIR_COLOR) {
            defaultColor = characterCreationData.facialHairColor;
        } else if (currentCharacterDataType == CharacterDataTypes.EYE_COLOR) {
            defaultColor = characterCreationData.eyeColor;
        }
        selectionSpriteTexture = Utils.TextureFromSprite(selectionSprite);
        UpdateCharacterTexture(defaultSelection);
    }

    public void SetButtonInteractable() {
        if (characterCreationData.hairStyle == null && currentCharacterDataType == CharacterDataTypes.HAIR_COLOR) {
            Button[] creatorButtons = GameObject.Find("Presets").GetComponentsInChildren<Button>();
            foreach (Button button in creatorButtons) {
                button.interactable = false;
            }
        }
    }

    byte GetValidByteColor(byte color, int colorChange) {
        if (color + colorChange < 0) return 0;
        if (color + colorChange > 255) return 255;
        return (byte)(color + colorChange);
    }

    public Texture2D CopyTexture2D(Texture2D copiedTexture, Image skinTonePallette)
    {
        Texture2D texture = new Texture2D((int)copiedTexture.width, (int)copiedTexture.height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color32 skinToneColor = skinTonePallette.color;
        if (currentCharacterDataType == CharacterDataTypes.HAIR_COLOR) characterCreationData.hairColor = skinTonePallette.color;
        else if (currentCharacterDataType == CharacterDataTypes.SKIN) characterCreationData.skinTone = skinTonePallette.color;
        else if (currentCharacterDataType == CharacterDataTypes.FACIAL_HAIR_COLOR) characterCreationData.facialHairColor = skinTonePallette.color;
        else if (currentCharacterDataType == CharacterDataTypes.EYE_COLOR) characterCreationData.eyeColor = skinTonePallette.color;

        
        int y = 0;
        while (y < texture.height)
        {
            int x = 0;
            while (x < texture.width)
            {
                if(copiedTexture.GetPixel(x,y) == Color.red) texture.SetPixel(x, y, skinToneColor);
                else if (copiedTexture.GetPixel(x,y) == Color.green) texture.SetPixel(x, y, new Color32(GetValidByteColor(skinToneColor.r, -15), GetValidByteColor(skinToneColor.g, -15), GetValidByteColor(skinToneColor.b, -15), 255));
                else if (copiedTexture.GetPixel(x,y) == Color.blue) texture.SetPixel(x, y, new Color32(GetValidByteColor(skinToneColor.r, 15), GetValidByteColor(skinToneColor.g, 15), GetValidByteColor(skinToneColor.b, 15), 255));
                else if (copiedTexture.GetPixel(x, y) == new Color32(255, 0, 255, 255)) texture.SetPixel(x, y, new Color32(GetValidByteColor(characterCreationData.skinTone.r, -15), GetValidByteColor(characterCreationData.skinTone.g, -15), GetValidByteColor(characterCreationData.skinTone.b, -15), 255));
                else texture.SetPixel(x, y, copiedTexture.GetPixel(x,y));
                ++x;
            }
            ++y;
        }
        texture.name = ("skintone_SpriteSheet");
 
        texture.Apply();
   
        return texture;
    }
   
    public void UpdateCharacterTexture(Button selection)
    {
        Image skinTonePallette = selection.GetComponent<Image>();
        defaultSelection = selection;
        selectionSpriteTexture = CopyTexture2D(selectionSprite.texture, skinTonePallette);
        string tempName = characterImage.sprite.name;
        characterImage.sprite = Sprite.Create(selectionSpriteTexture, characterImage.sprite.rect, new Vector2(0,1));
        characterImage.sprite.name = tempName;

    }

    public void SetSelectedHairStyle(Sprite hairStyle) {
        selectionSprite = hairStyle;
    }

    public void SetSelectedFacialHair(Sprite facialHairStyle) {
        selectionSprite = facialHairStyle;
    }
}
