using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCreationUI : MonoBehaviour
{
    CharacterCreation[] characterCreators;
    CharacterCreationData characterCreationData;
    public int currentStepIndex;
    public CharacterCreation currentCharacterCreator;
    public Text headerText;
    public Text headerTextShadow; 
    public Sprite currentHairStyle;
    public Sprite currentFacialHairStyle;
    public Image characterPreviewFacialHair;
    public Image characterPreviewHair;
    public GameObject helmet;
    public Vector3 helmetOffLocation;
    public Vector2 helmetOffSize;
    public GameObject helmetEquippedLayer;
    public GameObject helmetUnequippedLayer;
    bool helmetEquipped = false;
    public AudioClip helmetSound;
    AudioSource helmetAudioSource;
    public GameObject[] tabs;
    
    CharacterCreation hairColorCreator;
    CharacterCreation facialHairColorCreator;

    // Start is called before the first frame update
    void Start()
    {
        characterCreationData = GetComponent<CharacterCreationData>();
        characterCreators = characterCreationData.characterCreators;
        characterCreators[currentStepIndex].gameObject.SetActive(true);
        hairColorCreator = characterCreators[2];
        facialHairColorCreator = characterCreators[5];
        helmetAudioSource = GetComponent<AudioSource>();
        SetActiveTab();
    }

    public void SetSelectedHairStyle(Sprite selectedStyle) {
        currentHairStyle = selectedStyle;
        characterPreviewHair.gameObject.GetComponent<Image>().enabled = true;
        characterPreviewHair.sprite = selectedStyle;
        hairColorCreator.selectionSprite = selectedStyle;
    }

    public void ResetSelectedHairStyle() {
        currentHairStyle = null;
        characterPreviewHair.sprite = null;
        characterPreviewHair.gameObject.GetComponent<Image>().enabled = false;
        hairColorCreator.selectionSprite = null;
    }

    public void SetSelectedFacialHair(Sprite selectedStyle) {
        currentFacialHairStyle = selectedStyle;
        characterPreviewFacialHair.gameObject.GetComponent<Image>().enabled = true;
        characterPreviewFacialHair.sprite = selectedStyle;
        facialHairColorCreator.selectionSprite = selectedStyle;
    }

    public void ResetSelectedFacialHair() {
        currentFacialHairStyle = null;
        characterPreviewFacialHair.sprite = null;
        characterPreviewFacialHair.gameObject.GetComponent<Image>().enabled = false;
        facialHairColorCreator.selectionSprite = null;
    }

    byte GetValidByteColor(byte color, int colorChange) {
        if (color + colorChange < 0) return 0;
        if (color + colorChange > 255) return 255;
        return (byte)(color + colorChange);
    }

    public Texture2D CopyTexture2D(Texture2D copiedTexture)
    {
        Texture2D texture = new Texture2D((int)copiedTexture.width, (int)copiedTexture.height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        
        int y = 0;
        while (y < texture.height)
        {
            int x = 0;
            while (x < texture.width)
            {
                if(copiedTexture.GetPixel(x,y) == Color.red) texture.SetPixel(x, y, characterCreationData.hairColor);
                else if (copiedTexture.GetPixel(x,y) == Color.green) texture.SetPixel(x, y, new Color32(GetValidByteColor(characterCreationData.hairColor.r, -15), GetValidByteColor(characterCreationData.hairColor.g, -15), GetValidByteColor(characterCreationData.hairColor.b, -15), 255));
                else if (copiedTexture.GetPixel(x,y) == Color.blue) texture.SetPixel(x, y, new Color32(GetValidByteColor(characterCreationData.hairColor.r, 15), GetValidByteColor(characterCreationData.hairColor.g, 15), GetValidByteColor(characterCreationData.hairColor.b, 15), 255));
                else if (copiedTexture.GetPixel(x, y) == new Color32(255, 0, 255, 255)) texture.SetPixel(x, y, new Color32(GetValidByteColor(characterCreationData.skinTone.r, -15), GetValidByteColor(characterCreationData.skinTone.g, -15), GetValidByteColor(characterCreationData.skinTone.b, -15), 0));
                else texture.SetPixel(x, y, copiedTexture.GetPixel(x,y));
                ++x;
            }
            ++y;
        }
        texture.name = ("skintone_SpriteSheet");
 
        texture.Apply();
   
        return texture;
    }
   
    public void UpdateCharacterTexture(Sprite blankSprite)
    {
        Texture2D selectionSpriteTexture = Utils.TextureFromSprite(blankSprite);
        selectionSpriteTexture = CopyTexture2D(selectionSpriteTexture);
        string tempName = characterPreviewHair.sprite.name;
        characterPreviewHair.sprite = Sprite.Create(selectionSpriteTexture, characterPreviewHair.sprite.rect, new Vector2(0,1));
        characterPreviewHair.sprite.name = tempName;
        
    }

    public void UpdateCharacterFacialHairTexture(Sprite blankSprite)
    {
        Texture2D selectionSpriteTexture = Utils.TextureFromSprite(blankSprite);
        selectionSpriteTexture = CopyTexture2D(selectionSpriteTexture);
        string tempName = characterPreviewFacialHair.sprite.name;
        characterPreviewFacialHair.sprite = Sprite.Create(selectionSpriteTexture, characterPreviewFacialHair.sprite.rect, new Vector2(0,1));
        characterPreviewFacialHair.sprite.name = tempName;
        
    }

    public void SetActiveSlide(int incrementBy) {
        if ((currentStepIndex == characterCreators.Length - 1 && incrementBy > 0) || (currentStepIndex == 0 && incrementBy < 0)) return;
        characterCreators[currentStepIndex].gameObject.SetActive(false);
        characterCreators[currentStepIndex + incrementBy].gameObject.SetActive(true);
        currentStepIndex += incrementBy;
        currentCharacterCreator = characterCreators[currentStepIndex];
        headerText.text = currentCharacterCreator.stepName;
        headerTextShadow.text = currentCharacterCreator.stepName;
    }

    public void SetActiveTab() {
        for (int i = 0; i < tabs.Length; i++) {
            if (i != currentStepIndex) {
                tabs[i].GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                tabs[i].transform.Find("TabShadow").gameObject.SetActive(true);
                tabs[i].GetComponent<Canvas>().sortingOrder = -i;

            } else {
                tabs[i].GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                tabs[i].transform.Find("TabShadow").gameObject.SetActive(false);
                tabs[i].GetComponent<Canvas>().sortingOrder = 10;
            }
        }
    }

    public void SetHelmetStatus() {
        helmetEquipped = !helmetEquipped;
        helmetAudioSource.clip = helmetSound;
        helmetAudioSource.Play();
        if (helmetEquipped) {
            helmet.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            helmet.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 500);
            helmet.transform.SetParent(helmetEquippedLayer.transform);
        } else {
            helmet.GetComponent<RectTransform>().localPosition = helmetOffLocation;
            helmet.GetComponent<RectTransform>().sizeDelta = helmetOffSize;
            helmet.transform.SetParent(helmetUnequippedLayer.transform);
        }
    }

}
