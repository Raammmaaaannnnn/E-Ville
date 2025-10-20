using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _Attack : MonoBehaviour
{

    Collider2D Attackbox;

    Vector2 attackOffsetright;

    private void Start()
    {
        Attackbox = GetComponent<Collider2D>();
        attackOffsetright = transform.position;
    }

    public void AttackLeft()
    {
        Debug.Log("Attacking Left");
        Attackbox.enabled = true;
        transform.position = new Vector3(attackOffsetright.x * -1, attackOffsetright.y);
    }

    public void AttackRight()
    {
        Debug.Log("Attacking Right");
        Attackbox.enabled = true;
        transform.position = attackOffsetright; 

    }

    public void StopAttack()
    {
        Attackbox.enabled=false;    
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Enemy")
        {

        }
    }
}
