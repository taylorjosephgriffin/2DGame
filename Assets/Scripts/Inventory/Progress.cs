 using UnityEngine;
 using UnityEngine.UI;
 using System.Collections;
 
 public class Progress : MonoBehaviour {
    RectTransform rectTransform;

    public Text currentHealthUI;

    GameObject player;
    float playerHealth;

    float playerMaxHealth;
    public float barWidth;

    private void Start()
    {
        rectTransform = transform.GetComponent<RectTransform>();
    }

    public void UpdateHealthUI(float health) {
        float widthFromHealth = (health * .01f) * barWidth;
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, widthFromHealth);
        currentHealthUI.text = health.ToString() + "/100";
    }
 }