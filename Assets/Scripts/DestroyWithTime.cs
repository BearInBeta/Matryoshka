using System.Collections;
using UnityEngine;

public class DestroyWithTime : MonoBehaviour
{
    [SerializeField] float waitTime = 2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(DestroyMe());
    }

    IEnumerator DestroyMe()
    {
        yield return new WaitForSeconds(waitTime);
        Destroy(gameObject);
    }
}
