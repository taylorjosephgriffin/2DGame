using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAlphaReducer : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    public Color32 spriteColorNormal;
    public Color32 spriteColorFade;
    bool shouldFade = false;

    bool isBehindSprite = false;

    // Start is called before the first frame update
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (shouldFade) spriteRenderer.color = Color32.Lerp(spriteRenderer.color, spriteColorFade, Time.deltaTime * 7);
        else spriteRenderer.color =  Color32.Lerp(spriteRenderer.color, spriteColorNormal, Time.deltaTime * 7);;
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        shouldFade = true;
        isBehindSprite = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        shouldFade = false;
        isBehindSprite = false;
    }
}
