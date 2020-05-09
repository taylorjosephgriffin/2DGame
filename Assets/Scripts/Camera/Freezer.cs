using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Freezer : MonoBehaviour
{
    public float duration = .05f;

    bool isFrozen = false;

    float pendingFreezeDuration = 0f;

    public void Freeze()
    {
        pendingFreezeDuration = duration;

        if (pendingFreezeDuration > 0 && !isFrozen)
        {
            StartCoroutine(DoFreeze());
        }
    }

    IEnumerator DoFreeze()
    {
        isFrozen = true;
        float original = Time.timeScale;
        Time.timeScale = 0;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = original;
        isFrozen = false;
        pendingFreezeDuration = 0;
    }
}
