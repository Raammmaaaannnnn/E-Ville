using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Manager : MonoBehaviour
{
   public float health = 10;

   public Animator Enemy_animator;

   private bool isHit = false;

    public float Health
    {
        set
        {
            health = value;

            if(health <= 0)
            {
               defeated();
            }
        }
        get
        {
            return health;
        }
    }

    private void Start()
    {
        Enemy_animator = GetComponent<Animator>();
    }

    public void TakeDamage(float amount)
    {
        if (!isHit && health > 0)
        {
            isHit = true;
            Enemy_animator.SetTrigger("isHit");
            Health -= amount;
            StartCoroutine(ResetHit());
        }
    }

    private IEnumerator ResetHit()
    {
        yield return new WaitForSeconds(0.4f); // duration of hit animation
        isHit = false;
    }

    public void defeated()
    {
        Enemy_animator.SetTrigger("Defeated");
    }

    public void removeEnemy()
    {
        Destroy(gameObject);
    }
}
