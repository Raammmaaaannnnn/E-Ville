using System.Collections;
//using System.Collections.Generic;
//using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{

    public float moveSpeed = 0.7f;
    private bool playingFootsteps = false;
    public float footstepsSpeed = 0.5f;
    
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    public Image healthBarFill; // assign in Inspector
    public float healthBarSpeed = 0.15f; // speed of fill animation

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
    public string enemy2Tag = "AttackerNPC";

    // Runtime
    private bool Enemy2InAttackRange = false;
    private Collider2D detectedEnemy2Collider = null; // last hit collider (if any)

    // Add runtime flag for NPC detection
    private bool NPCInAttackRange = false;
    private Collider2D detectedNPCCollider = null;

    private Collider2D detectedDestructibleCollider = null;
    private bool destructibleInRange = false;

    public Transform policeStationRespawn;
    public Transform hospitalRespawn;
    public Transform defaultRespawn;
    

    // runtime
    private PlayerIntoxication playerIntox;
    private PlayerInput playerInput;
    public static PlayerController instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            DontDestroyOnLoad(gameObject);
            DDOLTracker.Register(gameObject); // <- Track this DDOL object
        }
        else
        {
            Destroy(gameObject);
        }
        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = true; // ensure input is active
        }
    }
    // Start is called before the first frame update
    void Start()
    {

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        currentHealth = maxHealth;
        if (healthBarFill != null)
            healthBarFill.fillAmount = currentHealth / maxHealth;

        // cache player intoxication (optional null fallback)
        playerIntox = FindObjectOfType<PlayerIntoxication>();
        if (playerIntox == null)
        {
            Debug.LogWarning("PlayerIntoxication not found in scene. NPC attack detection will default to always-off for NPCs until PlayerIntoxication exists.");
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (PauseController.IsGamePaused)
        {
            // Stop movement
            rb.velocity = Vector2.zero;

            // Stop animations
            animator.SetBool("isMoving?", false);

            // Stop footsteps
            StopFootsteps();

            // Disable player input completely
            if (playerInput != null && playerInput.enabled)
                playerInput.enabled = false;

            return;
        }
        else
        {
            // Re-enable input after unpausing
            if (playerInput != null && !playerInput.enabled)
                playerInput.enabled = true;
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


        // Prevent attack if player is not intoxicated
        if (playerIntox == null || playerIntox.currentLevel < PlayerIntoxication.IntoxicationLevel.Orange)
        {
            Debug.Log("Cannot attack: Player is not intoxicated enough!");
            return;
        }
        // -----------------------------------
        // 1. Destructible Object Attack
        // -----------------------------------
        if (destructibleInRange && detectedDestructibleCollider != null)
        {
            Destructible destructible = detectedDestructibleCollider.GetComponentInParent<Destructible>();
            if (destructible != null)
            {

                SoundEffectManager.Play("Punch");
                animator.SetTrigger("Attack");
                destructible.Hit();
                return; // stop here so we don't double-hit enemies
            }
        }

        if (EnemyInAttackRange && detectedEnemyCollider != null)
        {

            SoundEffectManager.Play("Punch");
            // Trigger attack animation
            animator.SetTrigger("Attack");

            // Deal damage and knockback to enemy
            Enemy enemyScript = detectedEnemyCollider.GetComponent<Enemy>();
            
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(attackDamage);//, gameObject);
            }

            

        }

        if (Enemy2InAttackRange && detectedEnemy2Collider != null)
        {

            SoundEffectManager.Play("Punch");
            // Trigger attack animation
            animator.SetTrigger("Attack");

            // Deal damage to AttackerNPC
            AttackerNPC attackerScript = detectedEnemy2Collider.GetComponent<AttackerNPC>();
            if (attackerScript != null)
            {
                attackerScript.TakeDamage(attackDamage); // spawned attackers
            }

        }

        // ------------------- NPC attack -------------------
        if (NPCInAttackRange && detectedNPCCollider != null)
        {
            NPC npcScript = detectedNPCCollider.GetComponent<NPC>();
            if (npcScript != null)
            {

                SoundEffectManager.Play("Punch");
                animator.SetTrigger("Attack");
                npcScript.TakeDamage(attackDamage);
            }
        }


    }

    public void TakeDamage(int damage, GameObject attacker = null)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthBarFill != null)
            StartCoroutine(UpdateHealthBar(currentHealth / maxHealth));

        StartCoroutine(BlinkRed(0.15f, 5));
        SoundEffectManager.Play("Hurt");

        if (currentHealth <= 0)
        {
            Debug.Log("Player died");

            animator.SetTrigger("Died");
            
            StartCoroutine(RespawnPlayer(attacker));
            return;
        }

        
    }

    private IEnumerator RespawnPlayer(GameObject attacker)
    {
       
        // Disable player controls (implement your control disabling here)
        PlayerController playerMovement = GetComponent<PlayerController>();
        if (playerMovement != null) playerMovement.enabled = false;

        // Hide player visually
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.enabled = false;

        
        // Wait briefly (death animation, etc.)
        yield return new WaitForSeconds(0.9f);

        // Determine respawn point based on attacker
        Transform respawnPoint = defaultRespawn; // default

        if (attacker != null)
        {
            if (attacker.CompareTag("AttackerNPC") && hospitalRespawn != null)
            {
                respawnPoint = hospitalRespawn;
            }
            else if (attacker.CompareTag("Enemy") && policeStationRespawn != null)
            {
                respawnPoint = policeStationRespawn;
            }
        }

        // **Move player to respawn point**
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
        }

        animator.SetBool("Respawn", true);

        currentHealth = maxHealth;

        ResetAfterSleep();
        // Show player visually again
        sr.enabled = true;

        // Blink to indicate respawn invincibility
        int blinkTimes = 5;
        float blinkDuration = 0.15f;
        for (int i = 0; i < blinkTimes; i++)
        {
            sr.enabled = false;
            yield return new WaitForSeconds(blinkDuration);
            sr.enabled = true;
            yield return new WaitForSeconds(blinkDuration);
        }

        // Re-enable controls
        if (playerMovement != null) playerMovement.enabled = true;

        Debug.Log($"Player respawned at {respawnPoint.name}");
    }


    public bool AddHealth(int amount)
    {
        if (amount <= 0 || currentHealth >= maxHealth)
            return false; // nothing to add

        float previousHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        if (healthBarFill != null)
            StartCoroutine(UpdateHealthBar(currentHealth / maxHealth));

        Debug.Log($"Player healed: {currentHealth - previousHealth} | Current Health: {currentHealth}/{maxHealth}");
        return currentHealth > previousHealth; // true if health increased
    }

    private IEnumerator UpdateHealthBar(float targetFill)
    {
        float startFill = healthBarFill.fillAmount;
        float elapsed = 0f;

        while (elapsed < healthBarSpeed)
        {
            elapsed += Time.deltaTime;
            healthBarFill.fillAmount = Mathf.Lerp(startFill, targetFill, elapsed / healthBarSpeed);
            yield return null;
        }

        healthBarFill.fillAmount = targetFill;
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

        detectedNPCCollider = null; 
        NPCInAttackRange = false;

        destructibleInRange = false;
        detectedDestructibleCollider = null;
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
        RaycastHit2D[] hits = Physics2D.RaycastAll(startPos, dirNormalized, attackRange, attackLayer);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null) continue;



            // Enemy detection
            if (hit.collider.CompareTag(enemyTag))
            {
                EnemyInAttackRange = true;
                detectedEnemyCollider = hit.collider;
            }

            // Enemy detection
            if (hit.collider.CompareTag(enemy2Tag))
            {
                Enemy2InAttackRange = true;
                detectedEnemy2Collider = hit.collider;
            }

            // NPC detection
            NPC npcScript = hit.collider.GetComponent<NPC>();
            if (npcScript != null)
            {
                // Update runtime detection
                NPCInAttackRange = true;
                detectedNPCCollider = hit.collider;
            }

            // Destructible detection
            Destructible destructible = hit.collider.GetComponentInParent<Destructible>();
            if (destructible != null)
            {
                destructibleInRange = true;
                detectedDestructibleCollider = hit.collider;
            }

        }
        
    }

    public void ResetAfterSleep()
    {
        // 1. Restore Health
        currentHealth = maxHealth;
        if (healthBarFill != null)
            healthBarFill.fillAmount = 1f;

        // 2. Reset Wanted/Star Level
        IntoxicationStarsManager.Instance?.ResetStars();

        // 3. Remove Drunk Effect
        DrunkEffectController.Instance?.ResetEffects();

        // Stop attacker spawning immediately
        HouseSpawnManager.Instance?.StopChaosMode();

        Debug.Log("Player reset after sleep.");
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


        // draw small sphere at the end of the check
        Gizmos.DrawSphere(endPos, 0.05f);

        // if we hit an enemy, highlight it
        if (detectedEnemyCollider != null)
        {
            // color indicates if enemy is in range
            Gizmos.color = EnemyInAttackRange ? Color.yellow : Color.gray;
            Gizmos.DrawLine(transform.position, endPos);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(detectedEnemyCollider.transform.position, 0.3f);
        }

        // NPC: cyan
        if (NPCInAttackRange)
        {
            Gizmos.color = NPCInAttackRange ? Color.magenta : Color.gray;
            Gizmos.DrawLine(transform.position, endPos);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(detectedNPCCollider.transform.position, 0.2f);
        }

        
        if (detectedDestructibleCollider != null)
        {
            Gizmos.color = destructibleInRange ? Color.black : Color.gray;
            Gizmos.DrawLine(transform.position, endPos);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(detectedDestructibleCollider.transform.position, 0.2f);
        }

        //// Small sphere at the end of the check
        //Gizmos.color = Color.white;
        //Gizmos.DrawSphere(endPos, 0.05f);


    }


}
