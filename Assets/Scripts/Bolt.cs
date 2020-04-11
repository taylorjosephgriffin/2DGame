using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoltState { LOADED, FIRED, STUCK };

public class Bolt : MonoBehaviour
{
    public BoltState currentBoltState = BoltState.LOADED;
    public Vector2 stuckPosition;
    public GameObject stuckAnchor;
    public float boltSpeed = 12;
    public int damage = 5;
    Vector3 mousePosition;
    Vector3 aimDirection;
    // Start is called before the first frame update
    void Start()
    {
        CalculateMousePosition();
    }

    private void CalculateMousePosition()
    {
        mousePosition = Utils.GetMouseWorldPosition();
        aimDirection = (mousePosition - transform.position);
        aimDirection.z = 0;
        aimDirection.Normalize();
    }

    public void FireBolt()
    {
        transform.SetParent(null);
        currentBoltState = BoltState.FIRED;
        CalculateMousePosition();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentBoltState == BoltState.FIRED)
        {
            GetComponent<Rigidbody2D>().velocity = aimDirection * boltSpeed;
            if (boltSpeed > 0)
            {
                boltSpeed -= +Time.deltaTime * 2;
            }
            transform.SetParent(null);
        }
        if (currentBoltState == BoltState.STUCK)
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(stuckPosition.x, stuckPosition.y) * 0;
        }
    }
}
