using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class AttackerNPC : MonoBehaviour
{
    [Header("References")]
    private GameObject target; // Player
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D rb;
    public Animator animator;
    private bool isDead = false;

    [Header("Separation/Avoidance")]
    public float separationRadius = 0.3f; // distance to maintain from other attackers
    public float separationForce = 0.3f;  // force to push away
    public LayerMask attackerLayer;       // layer for other AttackerNPCs

    [Header("Movement Settings")]
    public float speed = 2f;

    [Header("Attack Settings")]
    public float attackDistance = 0.2f;
    public int attackDamage = 10;
    public float attackCooldown = 1f; // Time between attacks

    private bool playerInAttackRange = false;
    private bool isAttacking = false;

    // Pathfinding
    private Path path;
    private int currentWaypoint = 0;
    private Seeker seeker;
    public float nextWaypointDistance = 0.1f;

    private void Start()
    {
        seeker = GetComponent<Seeker>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        if(target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player");
        }
        // Recalculate path every 0.5s
        InvokeRepeating(nameof(UpdatePath), 0f, 0.5f);
    }

    private void Update()
    {
        if (target == null) return;

        CheckAttackRange();

        if (!isAttacking)
        {
            ChasePlayer();
        }
    }

    void CheckAttackRange()
    {
        if (target == null) return;

        float distance = Vector2.Distance(rb.position, target.transform.position);
        playerInAttackRange = distance <= attackDistance;

        if (playerInAttackRange && !isAttacking)
        {
            StartCoroutine(AttackCoroutine());
        }
    }

    Vector2 GetSeparationForce()
    {
        Vector2 force = Vector2.zero;
        float separationDistance = 0.5f; // minimum distance between attackers
        float separationStrength = 0.5f; // how strong the repulsion is

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, separationDistance);
        foreach (var hit in hits)
        {
            if (hit.gameObject != gameObject && hit.CompareTag("AttackerNPC"))
            {
                Vector2 away = (Vector2)(transform.position - hit.transform.position);
                float distance = away.magnitude;
                if (distance > 0.01f)
                {
                    force += away.normalized * (separationDistance - distance) * separationStrength;
                }
            }
        }
        return force;
    }


    void UpdatePath()
    {
        if (seeker.IsDone() && target != null)
        {
            seeker.StartPath(rb.position, target.transform.position, OnPathComplete);
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
        if (path == null || currentWaypoint >= path.vectorPath.Count) return;

        // Direction toward next waypoint
        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;

        // Separation force from other attackers
        Vector2 separation = GetSeparationForce();

        // Combine movement and separation smoothly
        Vector2 desiredVelocity = (direction * speed) + separation;
        desiredVelocity = Vector2.ClampMagnitude(desiredVelocity, speed); // limit speed

        // Smoothly apply the velocity to avoid jitter
        rb.velocity = Vector2.Lerp(rb.velocity, desiredVelocity, 0.1f); // adjust 0.1f for responsiveness

        // Flip sprite based on direction
        if (rb.velocity.x >= 0.01f) spriteRenderer.flipX = false;
        else if (rb.velocity.x <= -0.01f) spriteRenderer.flipX = true;

        animator.SetBool("EnemyMoving", true);

        // Check if reached the current waypoint
        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
            currentWaypoint++;
    }

    IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;

        // Play attack animation if available
        if (animator != null)
            animator.SetTrigger("EnemyAttacking");

        // Wait for attack cooldown
        yield return new WaitForSeconds(attackCooldown);

        // Deal damage
        if (playerInAttackRange && target != null)
        {
            PlayerController player = target.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(attackDamage, this.gameObject);
            }
        }

        isAttacking = false;
    }

    // Damage system
    public int health = 50;

    public void TakeDamage(int damage)
    {
        health -= damage;
        StartCoroutine(BlinkRed(0.15f, 3));

        if (health <= 0)
            Die();

        // Increment kill counter
        KillCounter.instance.AddKill();
    }

    public IEnumerator BlinkRed(float duration, int flashCount)
    {
        if (spriteRenderer == null) yield break;
        float flashDuration = duration / (flashCount * 2);
        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(flashDuration);
        }
    }

    void Die()
    {
        if (isDead) return; // prevent multiple triggers
        isDead = true;

        // Stop movement and attacks
        rb.velocity = Vector2.zero;
        StopAllCoroutines(); // optional: stops ongoing attack/provoke coroutines

        // Trigger death animation
        animator.SetTrigger("Died");

        // Disable collider so it doesn't interfere with player or enemies
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Destroy after animation finishes
        StartCoroutine(DestroyAfterAnimation());
    }

    private IEnumerator DestroyAfterAnimation()
    {
        // Wait for the length of the death animation
        float deathAnimLength = 1f; // default 1 second
        if (animator != null)
        {
            AnimatorClipInfo[] clips = animator.GetCurrentAnimatorClipInfo(0);
            if (clips.Length > 0)
                deathAnimLength = clips[0].clip.length;
        }

        yield return new WaitForSeconds(deathAnimLength);

        Destroy(gameObject, 1.9f);
    }
}
