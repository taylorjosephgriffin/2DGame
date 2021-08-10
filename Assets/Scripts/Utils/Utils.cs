using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
  public static Texture2D TextureFromSprite(Sprite sprite)
  {
    if (sprite.rect.width != sprite.texture.width)
    {
      Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
      Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                   (int)sprite.textureRect.y,
                                                   (int)sprite.textureRect.width,
                                                   (int)sprite.textureRect.height);
      newText.SetPixels(newColors);
      newText.Apply();
      return newText;
    }
    else
      return sprite.texture;
  }

  public static Vector3 ClampMagnitudeMinMax(Vector3 v, float min, float max)
  {
    double sm = v.sqrMagnitude;
    if (sm > (double)max * (double)max) return v.normalized * max;
    else if (sm < (double)min * (double)min) return v.normalized * min;
    return v;
  }
  public static Transform FindChildWithTag(Transform transform, string tag)
  {
    foreach (Transform child in transform)
    {
      if (child.CompareTag(tag)) return child;
    }
    return null;
  }

  public static Vector3 GetMouseWorldPosition()
  {
    Vector3 vec = GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
    vec.z = 0;
    return vec;
  }

  public static Vector3 GetMouseWorldPositionWithZ()
  {
    return GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
  }

  public static Vector3 GetMouseWorldPositionWithZ(Camera worldCamera)
  {
    return GetMouseWorldPositionWithZ(Input.mousePosition, worldCamera);
  }

  public static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera)
  {
    Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
    return worldPosition;
  }
  public static string CreateRandomString(int stringLength = 10)
  {
    int _stringLength = stringLength - 1;
    string randomString = "";
    string[] characters = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
    for (int i = 0; i <= _stringLength; i++)
    {
      randomString = randomString + characters[Random.Range(0, characters.Length)];
    }
    return randomString;
  }
}
