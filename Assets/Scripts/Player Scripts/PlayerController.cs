using System.Collections;
//using System.Collections.Generic;
//using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    public float moveSpeed = 0.7f;
    private bool playingFootsteps = false;
    public float footstepsSpeed = 0.5f;
    
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Blink Effect Settings")]
    public float blinkDuration = 0.1f;
    
    [Header("Attack Settings")]
    public float attackRange = 0.35f;           // how far the linecast reaches
    public LayerMask attackLayer;                 // layer mask for enemies
    public int attackDamage = 10;
    public float attackKnockbackForce = 0.5f;
    public float enemyBlinkDuration = 0.8f;// force applied to enemy on hit
    //only specific tag check as well:
    public string enemyTag = "Enemy";

    // Runtime
    private bool EnemyInAttackRange = false;
    private Collider2D detectedEnemyCollider = null; // last hit collider (if any)

    
    // Start is called before the first frame update
    void Start()
    {

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        currentHealth = maxHealth;
        
    }

    // Update is called once per frame
    void Update()
    {
        if(PauseController.IsGamePaused)
        {
            rb.velocity = Vector2.zero;
            animator.SetBool("isMoving?", false);
            StopFootsteps();
            return;
        }

        AdjustPlayerFacingDirection();
        // perform short-range linecast every frame 
        DetectEnemyForAttack();
        rb.velocity = moveInput * moveSpeed;

        animator.SetBool("isMoving?", rb.velocity.magnitude > 0);
        
        if (rb.velocity.magnitude > 0 && !playingFootsteps)
        {
            StartFootsteps();
        }
        else if(rb.velocity.magnitude == 0)
        {
            StopFootsteps();
        }
            
    }


    public void Move(InputAction.CallbackContext context)
    {
        if(context.canceled)
        {
            animator.SetBool("isMoving?", false);
            
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }
        moveInput = context.ReadValue<Vector2>();
        animator.SetFloat("InputXCurrent",moveInput.x);
        animator.SetFloat("InputYCurrent",moveInput.y);
    }

    void StartFootsteps()
    {
        playingFootsteps = true;
        InvokeRepeating(nameof(PlayFootsteps), 0f, footstepsSpeed);
        
    }


    void StopFootsteps()
    {
        playingFootsteps = false;
        CancelInvoke(nameof(PlayFootsteps));
    }


    void PlayFootsteps()
    {
        SoundEffectManager.Play("Footsteps", true);
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (!context.performed) return; // only trigger once per press

        if (EnemyInAttackRange && detectedEnemyCollider != null)
        {
            // Trigger attack animation
            animator.SetTrigger("Attack");

            // Deal damage and knockback to enemy
            Enemy enemyScript = detectedEnemyCollider.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(attackDamage);//, gameObject);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            // death or respawn logic can go here
            Debug.Log("Player died");
            return;
        }

        // Blink effect
        StartCoroutine(BlinkEffect(0.15f, 5));
    }

    private IEnumerator BlinkEffect(float duration, int flashCount)
    {
        if (spriteRenderer == null) yield break;
        float flashDuration = duration / (flashCount * 2); // time per half-flash
                                                           // Store original color
        Color originalColor = spriteRenderer.color;

        // Create faded version (half-transparent)
        Color fadedColor = originalColor;
        fadedColor.a = 0.5f; // 50% transparent

        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = fadedColor; // fade out
            yield return new WaitForSeconds(flashDuration);

            spriteRenderer.color = originalColor; // fade back
            yield return new WaitForSeconds(flashDuration);
        }

        // ensure we end on normal color
        spriteRenderer.color = originalColor;
    }

    private void AdjustPlayerFacingDirection()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(transform.position); 

        if(mousePos.x < playerScreenPoint.x)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }


    // -------------------- Step 1: Attack detection --------------------

    /// <summary>
    /// Performs a directional Linecast from the player towards the mouse world direction.
    /// Sets enemyInAttackRange = true if an enemy (on enemyLayer and optionally with tag) was hit within attackRange.
    /// </summary>
    void DetectEnemyForAttack()
    {
        detectedEnemyCollider = null;
        EnemyInAttackRange = false;

        // get mouse world position and direction from player to mouse
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 dir = (mouseWorld - transform.position);
        dir.z = 0f;

        if (dir.sqrMagnitude <= 0.0001f)
            return; // guard

        Vector2 dirNormalized = dir.normalized;
        Vector2 startPos = transform.position;
        Vector2 endPos = startPos + dirNormalized * attackRange;

        // Linecast against the enemyLayer
        RaycastHit2D hit = Physics2D.Linecast(startPos, endPos, attackLayer);

        if (hit.collider != null)
        {
            // If you also want to ensure a specific tag:
            if (string.IsNullOrEmpty(enemyTag) || hit.collider.CompareTag(enemyTag))
            {
                EnemyInAttackRange = true;
                detectedEnemyCollider = hit.collider;
                Debug.Log("Enemy detected for attack: " + hit.collider.name);
            }
            else
            {
                EnemyInAttackRange = false;
            }
        }
        else
        {
            EnemyInAttackRange = false;
        }
    }

    // Draw gizmos to visualize the attack check in the Scene view
    private void OnDrawGizmosSelected()
    {
        // Draw direction and range toward the mouse
        if (!Application.isPlaying)
        {
            // Show a default forward line in editor when not playing using the sprite facing direction
            Gizmos.color = Color.gray;
            Gizmos.DrawLine(transform.position, transform.position + (spriteRenderer != null && spriteRenderer.flipX ? Vector3.left : Vector3.right) * attackRange);
            return;
        }

        // When playing, draw actual detection line toward mouse
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mouseWorld - transform.position);
        if (dir.sqrMagnitude <= 0.0001f)
            return;

        Vector2 dirNormalized = dir.normalized;
        Vector3 endPos = transform.position + (Vector3)(dirNormalized * attackRange);

        // color indicates if enemy is in range
        Gizmos.color = EnemyInAttackRange ? Color.yellow : Color.gray;
        Gizmos.DrawLine(transform.position, endPos);

        // draw small sphere at the end of the check
        Gizmos.DrawSphere(endPos, 0.05f);

        // if we hit an enemy, highlight it
        if (detectedEnemyCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(detectedEnemyCollider.transform.position, 0.3f);
        }
    }



}
