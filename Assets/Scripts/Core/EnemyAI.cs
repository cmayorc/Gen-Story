using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemy AI - Inteligencia artificial para enemigos del RPG
/// Maneja comportamiento, detección del jugador y lógica de combate
/// </summary>
public class EnemyAI : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHealth = 50;
    public int currentHealth;
    public int attackPower = 8;
    public int defense = 2;
    public int experienceReward = 25;
    public int goldReward = 10;
    
    [Header("AI Behavior")]
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float chaseRange = 15f;
    public float moveSpeed = 3f;
    public float attackCooldown = 2f;
    
    [Header("References")]
    public Transform playerTransform;
    public CombatSystem combatSystem;
    
    private Rigidbody rb;
    private Animator animator;
    private EnemyState currentState = EnemyState.Patrol;
    private float lastAttackTime = 0f;
    private Vector3 patrolPoint;
    private bool isDead = false;
    
    enum EnemyState
    {
        Patrol,
        Chase,
        Attack,
        Dead
    }
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        
        currentHealth = maxHealth;
        patrolPoint = transform.position;
        
        // Encontrar al jugador
        if (playerTransform == null)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
        
        // Obtener referencia al sistema de combate
        if (combatSystem == null)
        {
            combatSystem = FindObjectOfType<CombatSystem>();
        }
    }
    
    void Update()
    {
        if (isDead || GameManager.Instance.currentState != GameState.Playing)
        {
            return;
        }
        
        // Actualizar estado del enemigo
        UpdateState();
        
        // Ejecutar comportamiento según el estado
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Chase:
                ChasePlayer();
                break;
            case EnemyState.Attack:
                AttackPlayer();
                break;
        }
    }
    
    void UpdateState()
    {
        if (playerTransform == null)
        {
            return;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        if (distanceToPlayer <= attackRange)
        {
            currentState = EnemyState.Attack;
        }
        else if (distanceToPlayer <= detectionRange)
        {
            currentState = EnemyState.Chase;
        }
        else
        {
            currentState = EnemyState.Patrol;
        }
    }
    
    void Patrol()
    {
        // Movimiento de patrulla simple
        Vector3 direction = patrolPoint - transform.position;
        
        if (direction.magnitude > 0.5f)
        {
            direction.Normalize();
            rb.velocity = new Vector3(direction.x * moveSpeed * 0.5f, rb.velocity.y, direction.z * moveSpeed * 0.5f);
            
            // Rotar hacia la dirección de movimiento
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
            
            animator.SetFloat("Speed", 0.5f);
        }
        else
        {
            animator.SetFloat("Speed", 0f);
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }
    
    void ChasePlayer()
    {
        if (playerTransform == null)
        {
            return;
        }
        
        Vector3 direction = playerTransform.position - transform.position;
        direction.y = 0; // Mantener en el plano horizontal
        
        if (direction.magnitude > 0.1f)
        {
            direction.Normalize();
            rb.velocity = new Vector3(direction.x * moveSpeed, rb.velocity.y, direction.z * moveSpeed);
            
            // Rotar hacia el jugador
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
            
            animator.SetFloat("Speed", 1f);
        }
    }
    
    void AttackPlayer()
    {
        if (playerTransform == null || combatSystem == null)
        {
            return;
        }
        
        // Verificar cooldown de ataque
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            // Atacar al jugador
            PlayerController player = playerTransform.GetComponent<PlayerController>();
            if (player != null)
            {
                combatSystem.EnemyAttack(this, player);
                lastAttackTime = Time.time;
                
                // Animación de ataque
                animator.SetTrigger("Attack");
            }
        }
        
        // Mantenerse cerca del jugador
        Vector3 direction = playerTransform.position - transform.position;
        direction.y = 0;
        
        if (direction.magnitude > attackRange + 0.5f)
        {
            direction.Normalize();
            rb.velocity = new Vector3(direction.x * moveSpeed * 0.5f, rb.velocity.y, direction.z * moveSpeed * 0.5f);
        }
        else
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }
    
    // Métodos de combate
    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }
        
        int finalDamage = Mathf.Max(1, damage - defense);
        currentHealth -= finalDamage;
        
        // Retroceso visual
        Vector3 knockback = (transform.position - playerTransform.position).normalized;
        rb.AddForce(knockback * 5f, ForceMode.Impulse);
        
        // Animación de daño
        animator.SetTrigger("TakeDamage");
        
        // Verificar si el enemigo murió
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        isDead = true;
        currentState = EnemyState.Dead;
        
        // Animación de muerte
        animator.SetTrigger("Die");
        
        // Desactivar colisiones
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        // Desactivar rigidbody
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        
        // Notificar al sistema de combate
        if (combatSystem != null)
        {
            combatSystem.EnemyDefeated(this);
        }
        
        // Otorgar recompensas
        if (playerTransform != null)
        {
            PlayerController player = playerTransform.GetComponent<PlayerController>();
            if (player != null)
            {
                player.AddExperience(experienceReward);
                player.AddGold(goldReward);
            }
        }
        
        // Destruir el enemigo después de un tiempo
        Destroy(gameObject, 5f);
    }
    
    // Detectar colisiones con ataques del jugador
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerAttack"))
        {
            // Obtener daño del ataque
            AttackCollider attack = other.GetComponent<AttackCollider>();
            if (attack != null)
            {
                TakeDamage(attack.damage);
            }
        }
    }
    
    // Métodos para el editor
    private void OnDrawGizmosSelected()
    {
        // Dibujar rangos de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        // Dibujar punto de patrulla
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(patrolPoint, 0.5f);
    }
}