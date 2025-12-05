using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class Enemy : MonoBehaviour
{
    [Header("References")]
    public Transform target; // player
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D rb;
    public Animator animator;
    private bool isDead = false;

    [Header("Detection Settings")]
    public float detectionDistance = 1f; // linecast range
    public LayerMask playerLayer;

    [Header("Provocation Settings")]
    public float provokeTime = 2f;
    public float provokeChaseColorTime = 5f;
    public GameObject provokeIcon;
    private bool forceChaseFromProvoke = false;

    private bool provokeTimerRunning = false;
    private float provokeTimer = 0f;

    private List<RaycastHit2D> resultsList = new List<RaycastHit2D>();
    private ContactFilter2D contactFilter;
    private List<RaycastHit2D> attackResults = new List<RaycastHit2D>();
    private ContactFilter2D attackFilter;

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
    public int attackDamage = 10;
    private bool playerInAttackRange = false;
    private bool isAttacking = false;
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

    // Stars
    private bool playerHasStars = false;
    private static bool globalAlert = false;

    private void Start()
    {
        patrolPoints = new Transform[patrolParent.childCount];
        for (int i = 0; i < patrolParent.childCount; i++)
            patrolPoints[i] = patrolParent.GetChild(i);

        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(playerLayer);
        contactFilter.useTriggers = false;

        attackFilter = new ContactFilter2D();
        attackFilter.SetLayerMask(playerLayer);
        attackFilter.useTriggers = false;

        knockback = GetComponent<KnockBack>();
        seeker = GetComponent<Seeker>();
        InvokeRepeating("UpdatePath", 0f, 0.5f);

        // Subscribe to stars
        if (IntoxicationStarsManager.Instance != null)
            IntoxicationStarsManager.Instance.OnStarsChanged += OnStarsChanged;
    }

    private void OnDisable()
    {
        if (IntoxicationStarsManager.Instance != null)
            IntoxicationStarsManager.Instance.OnStarsChanged -= OnStarsChanged;
    }

    private void OnStarsChanged(int stars)
    {
        playerHasStars = stars > 0;

        if (stars >= 2) globalAlert = true;
        else globalAlert = false;
    }

    private void Update()
    {
        DetectPlayer();
        DetectPlayerForAttack();

        animator.SetBool("InAttackRange", playerInAttackRange);

        HandleAttack();
        HandleProvokeTimer();

        if (isWaiting) return;
        if (isIdleChecking || isAttacking) return;

        if (!inAttackStance)
        {
            if (playerHasStars || globalAlert || forceChaseFromProvoke)
            {
                if (playerDetected || globalAlert || forceChaseFromProvoke)
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
                else Patrol();
            }
            else Patrol();
        }
    }

    #region Detection
    void DetectPlayer()
    {
        if (target == null)
        {
            playerDetected = false;
            return;
        }

        if (!playerHasStars && !globalAlert && !forceChaseFromProvoke)
        {
            playerDetected = false;
            return;
        }

        Vector2 dir = (target.position - transform.position).normalized;
        resultsList.Clear();
        int count = Physics2D.Raycast(transform.position, dir, contactFilter, resultsList, detectionDistance);

        playerDetected = false;
        for (int i = 0; i < count; i++)
        {
            if (resultsList[i].collider.CompareTag("Player"))
            {
                playerDetected = true;
                break;
            }
        }
    }

    void DetectPlayerForAttack()
    {
        if (target == null)
        {
            playerInAttackRange = false;
            return;
        }

        Vector2 dir = (target.position - transform.position).normalized;
        attackResults.Clear();

        int count = Physics2D.Raycast(transform.position, dir, attackFilter, attackResults, attackDistance);

        playerInAttackRange = false;
        for (int i = 0; i < count; i++)
        {
            if (attackResults[i].collider.CompareTag("Player"))
            {
                playerInAttackRange = true;

                // Provoke only if no stars
                if (!playerHasStars && !provokeTimerRunning)
                {
                    provokeTimerRunning = true;
                    provokeTimer = 0f;
                }
                break;
            }
        }

        if (!playerInAttackRange)
        {
            provokeTimerRunning = false;
            provokeTimer = 0f;
        }
    }
    #endregion

    #region Provoke
    void HandleProvokeTimer()
    {
        if (!provokeTimerRunning) return;

        provokeTimer += Time.deltaTime;

        if (provokeTimer >= provokeTime)
        {
            StartCoroutine(TriggerProvoke());
            provokeTimerRunning = false;
            provokeTimer = 0f;
        }
    }

    IEnumerator TriggerProvoke()
    {
        forceChaseFromProvoke = true;

        if (provokeIcon != null) provokeIcon.SetActive(true);
        SoundEffectManager.Play("Provoke", true);

        if (IntoxicationStarsManager.Instance != null)
            IntoxicationStarsManager.Instance.AddStars(1);

        spriteRenderer.color = Color.red;

        float t = 0f;
        while (t < provokeChaseColorTime)
        {
            t += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = Color.white;
        if (provokeIcon != null) provokeIcon.SetActive(false);

        forceChaseFromProvoke = false;
    }
    #endregion

    #region Attack
    void HandleAttack()
    {
        if (!playerInAttackRange || (!playerHasStars && !forceChaseFromProvoke))
        {
            inAttackStance = false;
            return;
        }

        inAttackStance = true;
        rb.velocity = Vector2.zero;

        Vector2 dir = (target.position - transform.position).normalized;
        spriteRenderer.flipX = dir.x < 0;

        if (!isAttacking)
            StartCoroutine(AttackCoroutine());
    }

    IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;

        animator.SetTrigger("AttackStance");
        yield return new WaitForSeconds(0.7f);

        Vector2 dir = (target.position - transform.position).normalized;
        spriteRenderer.flipX = dir.x < 0;

        animator.SetTrigger("EnemyAttacking");
        yield return new WaitForSeconds(0.5f);

        if (playerInAttackRange && target != null)
        {
            PlayerController player = target.GetComponent<PlayerController>();
            if (player != null) player.TakeDamage(attackDamage, this.gameObject);
        }

        yield return new WaitForSeconds(0.4f);
        isAttacking = false;
    }
    #endregion

    #region Movement
    void Patrol()
    {
        if (patrolPoints.Length == 0) return;
        Transform patrolTarget = patrolPoints[currentPatrolIndex];
        transform.position = Vector2.MoveTowards(transform.position, patrolTarget.position, speed * Time.deltaTime);

        Vector2 moveDir = (patrolTarget.position - transform.position).normalized;
        spriteRenderer.flipX = moveDir.x < 0;
        animator.SetBool("EnemyMoving", true);

        if (Vector2.Distance(transform.position, patrolTarget.position) < 0.1f)
        {
            currentPatrolIndex = loopPatrolPoints
                ? (currentPatrolIndex + 1) % patrolPoints.Length
                : Mathf.Min(currentPatrolIndex + 1, patrolPoints.Length - 1);
        }
    }

    void UpdatePath()
    {
        if (isChasing && seeker.IsDone())
            seeker.StartPath(rb.position, target.position, OnPathComplete);
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

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        rb.velocity = direction * speed;

        spriteRenderer.flipX = direction.x < 0;
        animator.SetBool("EnemyMoving", true);

        if (Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]) < nextWaypointDistance)
            currentWaypoint++;
    }

    private IEnumerator IdleCheckCoroutine()
    {
        isIdleChecking = true;
        isChasing = false;
        rb.velocity = Vector2.zero;
        animator.SetBool("EnemyMoving", false);

        yield return new WaitForSeconds(idleCheckDuration);

        DetectPlayer();
        if (playerDetected)
        {
            isChasing = true;
            chaseTimer = 0f;
        }

        isIdleChecking = false;
    }
    #endregion

    #region Combat
    public void TakeDamage(int damage)
    {
        health -= damage;
        StartCoroutine(BlinkRed(0.15f, 5));
        if (health <= 0) Die();
    }

    public void ApplyKnockback(Vector2 force)
    {
        rb.AddForce(force, ForceMode2D.Impulse);
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

    private void Die()
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

        // Grant 3 stars to the player
        if (IntoxicationStarsManager.Instance != null)
        {
            IntoxicationStarsManager.Instance.AddStars(3);
        }

        // Destroy after animation finishes
        StartCoroutine(DestroyAfterAnimation()); ;
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
        // Increment kill counter
        KillCounter.instance.AddKill();
        Destroy(gameObject, 1.9f);
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = playerDetected ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);

        Gizmos.color = playerInAttackRange ? Color.blue : Color.black;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}
