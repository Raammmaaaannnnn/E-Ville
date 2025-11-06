using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KnockBack : MonoBehaviour
{
    public Rigidbody2D rb;

    public float strength = 16, delay = 0.15f;

    public UnityEvent OnBegin, OnDone;

    public void PlayFeedBack(GameObject sender)
    {
        StopAllCoroutines();
        OnBegin?.Invoke();
        Vector2 dir = (transform.position - sender.transform.position).normalized;
        rb.AddForce(dir * strength, ForceMode2D.Impulse);
        StartCoroutine(Reset());
    }


    private IEnumerator Reset()
    {
        yield return new WaitForSeconds(delay); 
        rb.velocity = Vector3.zero;
        OnDone?.Invoke();
    }
}
