using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeBehavior : MonoBehaviour
{
    Vector3 initialPosition;

    public IEnumerator Shake (float duration, float magnitude)
    {
        if (!PauseManager.isPaused)
        {
            initialPosition = transform.position;
            float elapsed = 0.0f;
            while (elapsed < duration)
            {
                float x = Random.Range(initialPosition.x - (1f * magnitude), initialPosition.x + (1f * magnitude));
                float y = Random.Range(initialPosition.y - (1f * magnitude), initialPosition.y + (1f * magnitude));


                transform.localPosition = new Vector3(x, y, initialPosition.z);

                elapsed += Time.deltaTime;

                yield return null;
            }

            transform.localPosition = initialPosition;
        }
    }
}   

