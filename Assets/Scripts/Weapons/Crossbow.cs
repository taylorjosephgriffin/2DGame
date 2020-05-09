using System;
using UnityEngine;
using UnityEngine.Events;

public class Crossbow : MonoBehaviour
{
    public Sprite unloadedCrossbow;
    public Sprite LoadedCrossbow;
    public GameObject Bolt;
    private GameObject Player;
    public Transform BoltAnchor;
    private GameObject newBolt;
    private ShakeBehavior cam;
    [Serializable]
    public class FireEvent : UnityEvent {}
    public FireEvent fireEvent = new FireEvent();
    enum CrossbowState { UNEQUIPED, EMPTY, LOADED };
    CrossbowState currentCrossbowState = CrossbowState.EMPTY;
    private ShakeBehavior cameraShake;

    private void Start()
    {
        Player = GameObject.FindWithTag("Player");
        cameraShake = GameObject.FindWithTag("MainCamera").GetComponent<ShakeBehavior>();
    }
    // Update is called once per frame
    void Update()
    {
        if ((Input.GetButtonDown("Reload") || Input.GetKeyDown(KeyCode.Joystick1Button3)) && currentCrossbowState == CrossbowState.EMPTY)
        {
            CreateBolt();
        }

        if (Input.GetButtonDown("Fire1") && currentCrossbowState == CrossbowState.LOADED)
        {
            FireBolt();
        }
    }

    private void FireBolt()
    {
        newBolt.AddComponent<Bolt>();
        newBolt.GetComponent<Bolt>().FireBolt();
        currentCrossbowState = CrossbowState.EMPTY;
        transform.GetComponent<SpriteRenderer>().sprite = unloadedCrossbow;
        StartCoroutine(cameraShake.Shake(.05f, .05f));
        fireEvent.Invoke();
    }

    private void LateUpdate()
    {
        if (newBolt)
        {
            newBolt.GetComponent<Renderer>().sortingOrder = Player.transform.Find("Sprite").GetComponent<Renderer>().sortingOrder + 50;
        }
    }

    void CreateBolt()
    {
        transform.GetComponent<SpriteRenderer>().sprite = LoadedCrossbow;
        newBolt = Instantiate(Bolt);
        newBolt.transform.position = BoltAnchor.transform.position;
        newBolt.transform.rotation = BoltAnchor.transform.rotation;
        newBolt.transform.SetParent(transform);
        currentCrossbowState = CrossbowState.LOADED;
    }
}
