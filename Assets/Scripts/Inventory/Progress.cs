 using UnityEngine;
 using UnityEngine.UI;
 using System.Collections;
 
 public class Progress : MonoBehaviour {
    RectTransform rectTransform;
    public int health = 100;
    public int previousHealth = 100;

    public Text currentHealthUI;

    GameObject player;
    float playerHealth;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        rectTransform = transform.GetComponent<RectTransform>();
    }

    private void Update()
    {
        playerHealth = player.GetComponent<Move>().health;
        float widthFromHealth = (playerHealth * .01f) * 340;
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, widthFromHealth);
        currentHealthUI.text = playerHealth.ToString() + "/100";
    }
 }