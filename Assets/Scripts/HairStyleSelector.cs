using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HairStyleSelector : MonoBehaviour
{
	[System.Serializable]
	public class HairStyle
	{
		public Image hairStyleImage;
		public Sprite hairStyleBlankSprite;
	}
	public HairStyle[] hairStyles;

	public CharacterCreationData characterCreationData;

	// Start is called before the first frame update
	void OnEnable()
	{
		foreach (HairStyle hairStyle in hairStyles)
		{
			UpdateCharacterTexture(hairStyle.hairStyleBlankSprite, hairStyle.hairStyleImage);
		}
	}

	byte GetValidByteColor(byte color, int colorChange)
	{
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
				if (copiedTexture.GetPixel(x, y) == Color.red) texture.SetPixel(x, y, characterCreationData.hairColor);
				else if (copiedTexture.GetPixel(x, y) == Color.green) texture.SetPixel(x, y, new Color32(GetValidByteColor(characterCreationData.hairColor.r, -15), GetValidByteColor(characterCreationData.hairColor.g, -15), GetValidByteColor(characterCreationData.hairColor.b, -15), 255));
				else if (copiedTexture.GetPixel(x, y) == Color.blue) texture.SetPixel(x, y, new Color32(GetValidByteColor(characterCreationData.hairColor.r, 15), GetValidByteColor(characterCreationData.hairColor.g, 15), GetValidByteColor(characterCreationData.hairColor.b, 15), 255));
				else if (copiedTexture.GetPixel(x, y) == new Color32(255, 0, 255, 255)) texture.SetPixel(x, y, new Color32(GetValidByteColor(characterCreationData.skinTone.r, -15), GetValidByteColor(characterCreationData.skinTone.g, -15), GetValidByteColor(characterCreationData.skinTone.b, -15), 0));
				else texture.SetPixel(x, y, copiedTexture.GetPixel(x, y));
				++x;
			}
			++y;
		}
		texture.name = ("skintone_SpriteSheet");

		texture.Apply();

		return texture;
	}

	public void UpdateCharacterTexture(Sprite blankSprite, Image hairStyleImage)
	{
		Texture2D selectionSpriteTexture = Utils.TextureFromSprite(blankSprite);
		selectionSpriteTexture = CopyTexture2D(selectionSpriteTexture);
		string tempName = hairStyleImage.sprite.name;
		hairStyleImage.sprite = Sprite.Create(selectionSpriteTexture, hairStyleImage.sprite.rect, new Vector2(0, 1));
		hairStyleImage.sprite.name = tempName;

	}
}
