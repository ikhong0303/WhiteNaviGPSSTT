using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnerSTT : MonoBehaviour
{
    public AudioSource connerSTT;
    public AudioSource finStt;
    private int delayCount;

    private void Start()
    {
        delayCount = 1;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BoxConner"))
        {
            if (delayCount == 1)
            {
                connerSTT.Play();
                delayCount = 0;
                StartCoroutine("delaySeconds");
            }
        }

        if (other.CompareTag("FinishPoint"))
        {
            if (delayCount == 1)
            {
                finStt.Play();
                delayCount = 0;
                StartCoroutine("delaySeconds");
            }
        }
    }

    IEnumerator delaySeconds()
    {
        yield return new WaitForSeconds(5);
        delayCount = 1;
        yield break;
    }
}
