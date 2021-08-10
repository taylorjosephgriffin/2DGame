using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateEmitter : MonoBehaviour
{
  public int rotationSpeed;
  ParticleSystem particleSystem;
  AudioSource audioSource;

  float timer = 0;
  float burstTime;

  private ShakeBehavior cameraShake;

  public int previousNumberOfParticles = 0;
  public int currentNumberOfParticles = 0;

  // Start is called before the first frame update
  void Start()
  {
    particleSystem = GetComponent<ParticleSystem>();
    audioSource = GetComponent<AudioSource>();
    burstTime = particleSystem.emission.GetBurst(0).repeatInterval;
    cameraShake = GameObject.FindWithTag("MainCamera").GetComponent<ShakeBehavior>();
    timer = burstTime;
  }

  // Update is called once per frame
  void Update()
  {
    ParticleSystem.EmissionModule em = particleSystem.emission;
    timer -= Time.deltaTime;
    transform.Rotate(Vector3.forward * (rotationSpeed * Time.deltaTime));
    if (em.enabled == true && timer < 0)
    {
      audioSource.pitch = Random.Range(.8f, 1.1f);
      audioSource.Play();
      StartCoroutine(cameraShake.Shake(.05f, .05f));
      timer = burstTime;
    }

  }

  private void OnParticleCollision(GameObject other)
  {
    if (other.tag == "Player")
    {
      other.GetComponent<Move>().TakeDamage(5, transform);
    }
  }
}
