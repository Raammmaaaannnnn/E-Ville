using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEditor.Experimental.GraphView;

public class Enemy : MonoBehaviour
{
    [Header("References")]
    public Transform target; // player
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D rb;
    public Animator animator;

    [Header("Detection Settings")]
    public float detectionDistance = 1f; // linecast range
    public LayerMask playerLayer;

    [Header("Movement Settings")]
    public float speed = 2f;

    [Header("Patrol Settings")]
    public Transform patrolParent;
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;
    public bool loopPatrolPoints = true;
    private bool isWaiting;

    [Header("Chase Settings")]
    public float chaseDuration = 10f; // seconds to chase even if player is detected
    public float idleCheckDuration = 1.3f; // idle before deciding to resume chase

    [Header("Attack Settings")]
    public float attackDistance = 0.2f;
    //public float attackCooldown = 0.9f;
    public int attackDamage = 10;
    private bool playerInAttackRange = false;
    private bool isAttacking = false;
    //private float attackTimer = 0f;
    private bool inAttackStance = false;

    // Pathfinding
    private Path path;
    private int currentWaypoint = 0;
    private Seeker seeker;
    public float nextWaypointDistance = 0.1f;

    // Internal
    private bool playerDetected = false;
    private bool isChasing = false;
    private float chaseTimer = 0f;
    private bool isIdleChecking = false;

    [Header("Enemy Combat")]
    private KnockBack knockback;
    public int health = 50;
    


    private void Start()
    {
        patrolPoints = new Transform[patrolParent.childCount];

        for (int i = 0; i < patrolParent.childCount; i++)
        {
            patrolPoints[i] = patrolParent.GetChild(i);
        }

        /////
        knockback = GetComponent<KnockBack>();
        seeker = GetComponent<Seeker>();
        InvokeRepeating("UpdatePath", 0f, 0.5f); // recalc path every 0.5s
    }
    private void Update()
    {
        DetectPlayer();
        DetectPlayerForAttack();

        // Update animator parameter
        animator.SetBool("InAttackRange", playerInAttackRange);

        HandleAttack();

        

        if (isWaiting)
        {
            return;
        }
        if (isIdleChecking || isAttacking) return;


        if (!inAttackStance)
        {
            if (playerDetected)
            {
                if (!isChasing)
                {
                    isChasing = true;
                    chaseTimer = 0f;
                }
                ChasePlayer();
                chaseTimer += Time.deltaTime;

                if (chaseTimer >= chaseDuration)
                    StartCoroutine(IdleCheckCoroutine());


            }
            else
            {
                if (!isChasing)
                {
                    Patrol();
                }
                else
                {
                    ChasePlayer();
                    chaseTimer += Time.deltaTime;
                    if (chaseTimer >= chaseDuration)
                    {
                        StartCoroutine(IdleCheckCoroutine());
                    }
                }
            }
        }
            
        
    }

    void DetectPlayer()
    {
        Vector2 direction = (target.position - transform.position).normalized;
        Vector2 startPos = transform.position;
        Vector2 endPos = startPos + direction * detectionDistance;


        RaycastHit2D hit = Physics2D.Linecast(startPos, endPos, playerLayer);

        Debug.Log("Enemy persists: " + gameObject.name + " at Z=" + transform.position.z);
        playerDetected = (hit.collider != null && hit.collider.CompareTag("Player"));
        if (playerDetected && !isChasing)
        {

            chaseTimer = 0f;
            Debug.Log("Player Detected");

        }
        else
        {
            Debug.Log("Player NOT Detected");
        }

    }


    void DetectPlayerForAttack()
    {
        if (target == null)
        {
            playerInAttackRange = false;
            return;
        }
        
        Vector2 directionToPlayer = (target.position - transform.position).normalized;
        Vector2 startPos = transform.position;
        Vector2 endPos = startPos + directionToPlayer * attackDistance;

        // Do a linecast only against the attackLayer (likely includes Player and obstacles)
        RaycastHit2D hit = Physics2D.Linecast(startPos, endPos, playerLayer);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            playerInAttackRange = true;
            // Useful for debug logging:
            Debug.Log("Player within attack range");
        }
        else
        {
            playerInAttackRange = false;
        }
    }

    /// <summary>
    /// Attack
    /// </summary>


    void HandleAttack()
    {
        // If player is not in range, cancel stance
        if (!playerInAttackRange)
        {
            inAttackStance = false;
            
            return;
        }

        // Stop moving completely while in range
        inAttackStance = true;
        rb.velocity = Vector2.zero;
        

        // Face player direction
        Vector2 dirToPlayer = (target.position - transform.position).normalized;
        spriteRenderer.flipX = dirToPlayer.x < 0;

        // If ready and not already attacking
        if (!isAttacking)
        {
            StartCoroutine(AttackCoroutine());
        }
    }

    IEnumerator AttackCoroutine()
    {
        isAttacking = true;

        // Stop movement
        rb.velocity = Vector2.zero;

        // Play wind-up / attack stance
        animator.SetTrigger("AttackStance");

        // Wait for the wind-up animation duration
        yield return new WaitForSeconds(0.7f);

        // Face player
        Vector2 dirToPlayer = (target.position - transform.position).normalized;
        spriteRenderer.flipX = dirToPlayer.x < 0;

        // Play actual attack animation
        animator.SetTrigger("EnemyAttacking");

        // Wait for hit frame / attack animation
        yield return new WaitForSeconds(0.5f);

        // Deal damage if still in range
        if (playerInAttackRange && target != null)
        {
            PlayerController player = target.GetComponent<PlayerController>();
            if(player != null)
            {
                player.TakeDamage(attackDamage);
            }
        }

        // Optional: small pause after attack
        yield return new WaitForSeconds(0.4f);

        isAttacking = false; // allows next attack or stance
    }


    ////
    void Patrol()
    {
        if (patrolPoints.Length == 0) return;
        Transform patrolTarget = patrolPoints[currentPatrolIndex];
        transform.position = Vector2.MoveTowards(transform.position, patrolTarget.position, speed * Time.deltaTime);
        
        Vector2 moveDir = (patrolTarget.position - transform.position).normalized;


        // Flip sprite based on x-direction
        if (moveDir.x > 0.01f)
            spriteRenderer.flipX = false;
        else if (moveDir.x < -0.01f)
            spriteRenderer.flipX = true;

        // Animation
        animator.SetBool("EnemyMoving", true);

        // Check if reached patrol point
        if (Vector2.Distance(transform.position, patrolTarget.position) < 0.1f)
        {
            isWaiting = true;
            currentPatrolIndex = loopPatrolPoints ? (currentPatrolIndex + 1) % patrolPoints.Length : Mathf.Min(currentPatrolIndex + 1, patrolPoints.Length - 1);
            isWaiting = false;// loop patrol
        }

    }

    void UpdatePath()
    {
        if (isChasing && seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    void ChasePlayer()
    {
        if (path == null) return;

        if (currentWaypoint >= path.vectorPath.Count)
        {
            return;
        }

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        rb.velocity = direction * speed;

        // Flip sprite based on direction
        if (direction.x >= 0.01f) spriteRenderer.flipX = false;
        else if (direction.x <= -0.01f) spriteRenderer.flipX = true;

        animator.SetBool("EnemyMoving", true);

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }
    }

    

    private IEnumerator IdleCheckCoroutine()
    {
        isIdleChecking = true;
        isChasing = false;
        rb.velocity = Vector2.zero;
        
        animator.SetBool("EnemyMoving", false);
        
        // Wait for idle duration
        yield return new WaitForSeconds(idleCheckDuration);


        /// After idle, check again
        DetectPlayer();
        if (playerDetected)
        {
            isChasing = true;
            chaseTimer = 0f;
        }

        isIdleChecking = false;

    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = playerDetected ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);

        Gizmos.color = playerInAttackRange ? Color.blue : Color.black;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

    }

    ////////////////////////////////////////////////////
    ///
    public void TakeDamage(int damage)//, GameObject attacker)
    {
        health -= damage;
        
        StartCoroutine(BlinkRed(0.15f, 5));

        if (health <= 0)
        {
            Die();
        }
    }

    public void ApplyKnockback(Vector2 force)
    {
        
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    public IEnumerator BlinkRed(float duration, int flashCount)
    {
        if (spriteRenderer == null) yield break;
        float flashDuration = duration / (flashCount * 2); // time per half-flash
        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = Color.red; // red on
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = Color.white; // back to normal
            yield return new WaitForSeconds(flashDuration);
        }
    }

    private void Die()
    {
        // simple destroy for now
        Destroy(gameObject);
    }
}
