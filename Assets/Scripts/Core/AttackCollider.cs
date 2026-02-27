using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack Collider - Componente para detectar golpes en combate
/// Se adjunta a los colliders de ataque del jugador
/// </summary>
public class AttackCollider : MonoBehaviour
{
    [Header("Attack Settings")]
    public int damage = 10;
    public float attackRange = 2f;
    public LayerMask enemyLayer;
    
    [Header("Visual Effects")]
    public ParticleSystem hitEffect;
    public AudioClip hitSound;
    
    private bool isAttackActive = false;
    private PlayerController player;
    
    void Start()
    {
        // Desactivar el collider inicialmente
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        // Obtener referencia al jugador
        player = GetComponentInParent<PlayerController>();
    }
    
    /// <summary>
    /// Activar el collider de ataque
    /// </summary>
    public void ActivateAttack()
    {
        isAttackActive = true;
        
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }
        
        // Desactivar después de un corto tiempo
        Invoke("DeactivateAttack", 0.5f);
    }
    
    /// <summary>
    /// Desactivar el collider de ataque
    /// </summary>
    void DeactivateAttack()
    {
        isAttackActive = false;
        
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
    }
    
    /// <summary>
    /// Detectar colisiones con enemigos
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!isAttackActive)
        {
            return;
        }
        
        // Verificar si el objeto colisionado es un enemigo
        if (other.CompareTag("Enemy"))
        {
            EnemyAI enemy = other.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                // Aplicar daño al enemigo
                enemy.TakeDamage(damage);
                
                // Crear efectos visuales y de sonido
                CreateHitEffects(other.transform.position);
                
                // Notificar al sistema de combate
                CombatSystem combatSystem = FindObjectOfType<CombatSystem>();
                if (combatSystem != null)
                {
                    combatSystem.PlayerAttack(player);
                }
            }
        }
    }
    
    /// <summary>
    /// Crear efectos de golpe
    /// </summary>
    void CreateHitEffects(Vector3 hitPosition)
    {
        // Efecto de partículas
        if (hitEffect != null)
        {
            ParticleSystem effect = Instantiate(hitEffect, hitPosition, Quaternion.identity);
            Destroy(effect.gameObject, 2f);
        }
        
        // Sonido de golpe
        if (hitSound != null && player != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, hitPosition);
        }
    }
    
    // Métodos para el editor
    private void OnDrawGizmosSelected()
    {
        // Dibujar rango de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}