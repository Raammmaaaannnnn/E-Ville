using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _Attack : MonoBehaviour
{
    [SerializeField] private float damage = 5;
    public Collider2D Attackbox;
    private Vector2 defaultPos;

    private void Start()
    {
        
        defaultPos = transform.localPosition;
        Attackbox.enabled = false;
        

        //if (Attackbox == null)
        //    Attackbox = GetComponent<Collider2D>();

        //Attackbox.enabled = false;
        //defaultPos = transform.localPosition;
    }


    public void AttackUp()
    {
        Attackbox.enabled = true;
        transform.localPosition = defaultPos;//offsetUp;
        Debug.Log("Attacking Up");
    }

    public void AttackDown()
    {
        Attackbox.enabled = true;
        transform.localPosition = defaultPos;//offsetDown;
        Debug.Log("Attacking Down");
       
    }


    public void AttackLeft()
    {
        Debug.Log("Attacking Left");
        Attackbox.enabled = true;
        transform.localPosition = new Vector3(defaultPos.x * -1, defaultPos.y);
    }

    public void AttackRight()
    {
        Debug.Log("Attacking Right");
        Attackbox.enabled = true;
        transform.localPosition = defaultPos; 

    }

    public void StopAttack()
    {
        Attackbox.enabled=false;    
    }

    

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Enemy")
        {
            Enemy_Manager enemy = other.GetComponent<Enemy_Manager>();

            if (enemy != null )
            {
                enemy.TakeDamage(damage);
            }

            
        }

        
    }
}
