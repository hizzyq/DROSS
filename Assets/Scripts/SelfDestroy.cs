using System;
using System.Collections;
using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    public float timeForDestruction;

    void Start()
    {
        StartCoroutine(DestroySelf(timeForDestruction));
    }

    private IEnumerator DestroySelf(float timeForDestruction)
    {
        yield return new WaitForSeconds(timeForDestruction);

        Destroy(gameObject);
    }
}
