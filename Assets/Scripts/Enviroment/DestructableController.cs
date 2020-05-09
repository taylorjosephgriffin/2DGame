using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DestructableController : MonoBehaviour
{
    public int health;
    public GameObject destroyedBush;
    // Start is called before the first frame update
    [Serializable]
    public class DestroyEvent : UnityEvent {}
    public DestroyEvent destroyEvent = new DestroyEvent();
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            GameObject fallingLeaves = Instantiate(destroyedBush, transform);
            fallingLeaves.transform.SetParent(null);
            Destroy(transform.gameObject);
            destroyEvent.Invoke();
        }
    }

    void TakeDamage(int Damage, Collider2D collision)
    {
        if (health > collision.GetComponent<Bolt>().damage)
        {
            collision.transform.parent = transform;
            collision.GetComponent<Bolt>().currentBoltState = BoltState.STUCK;
        }
        health -= Damage;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // if (collision.gameObject.tag == "Projectile" && collision.GetComponent<Bolt>().currentBoltState == BoltState.FIRED)
        // {
        //     TakeDamage(collision.GetComponent<Bolt>().damage, collision.GetComponent<Collider2D>());
        // }
    }
}
 