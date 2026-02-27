using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player Controller - Controlador del personaje del jugador
/// Maneja movimiento, estadísticas y acciones del personaje
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float rotationSpeed = 10f;
    
    [Header("Player Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public int maxMana = 50;
    public int currentMana;
    public int level = 1;
    public int experience = 0;
    public int gold = 0;
    
    [Header("Combat Stats")]
    public int attackPower = 10;
    public int defense = 5;
    public int magicPower = 8;
    
    [Header("Equipment")]
    public Equipment currentWeapon;
    public Equipment currentArmor;
    
    private Rigidbody rb;
    private Animator animator;
    private bool isRunning = false;
    private bool isAttacking = false;
    
    // Referencias a sistemas
    private CombatSystem combatSystem;
    private InventoryManager inventoryManager;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        
        // Inicializar estadísticas
        currentHealth = maxHealth;
        currentMana = maxMana;
        
        // Obtener referencias a sistemas
        combatSystem = FindObjectOfType<CombatSystem>();
        inventoryManager = FindObjectOfType<InventoryManager>();
    }
    
    void Update()
    {
        if (GameManager.Instance.currentState == GameState.Playing)
        {
            HandleMovement();
            HandleInput();
        }
    }
    
    void HandleMovement()
    {
        // Input de movimiento
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 movement = new Vector3(horizontal, 0, vertical).normalized;
        
        if (movement.magnitude > 0.1f)
        {
            // Determinar velocidad
            float currentSpeed = isRunning ? runSpeed : moveSpeed;
            
            // Mover al jugador
            Vector3 moveDirection = transform.TransformDirection(movement);
            rb.velocity = new Vector3(moveDirection.x * currentSpeed, rb.velocity.y, moveDirection.z * currentSpeed);
            
            // Rotar hacia la dirección de movimiento
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            
            // Animaciones
            animator.SetFloat("Speed", movement.magnitude);
        }
        else
        {
            animator.SetFloat("Speed", 0);
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
        
        // Correr
        if (Input.GetKey(KeyCode.LeftShift))
        {
            isRunning = true;
            animator.SetBool("IsRunning", true);
        }
        else
        {
            isRunning = false;
            animator.SetBool("IsRunning", false);
        }
    }
    
    void HandleInput()
    {
        // Ataque
        if (Input.GetButtonDown("Fire1") && !isAttacking)
        {
            Attack();
        }
        
        // Interacción
        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
        
        // Abrir inventario
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (inventoryManager != null)
            {
                inventoryManager.ToggleInventory();
            }
        }
        
        // Abrir menú de pausa
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameManager.Instance.PauseGame();
        }
    }
    
    void Attack()
    {
        if (combatSystem != null)
        {
            combatSystem.PlayerAttack(this);
        }
    }
    
    void Interact()
    {
        // Raycast para detectar objetos interactuables
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 2f))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.Interact(this);
            }
        }
    }
    
    // Métodos de combate
    public void TakeDamage(int damage)
    {
        int finalDamage = Mathf.Max(1, damage - defense);
        currentHealth -= finalDamage;
        
        // Actualizar UI
        if (GameManager.Instance.uiManager != null)
        {
            GameManager.Instance.uiManager.UpdateHealthBar(currentHealth, maxHealth);
        }
        
        // Verificar si el jugador murió
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        if (GameManager.Instance.uiManager != null)
        {
            GameManager.Instance.uiManager.UpdateHealthBar(currentHealth, maxHealth);
        }
    }
    
    public void RestoreMana(int amount)
    {
        currentMana = Mathf.Min(maxMana, currentMana + amount);
    }
    
    // Métodos de experiencia y nivel
    public void AddExperience(int amount)
    {
        experience += amount;
        
        // Verificar subida de nivel
        int requiredExp = level * 100;
        if (experience >= requiredExp)
        {
            LevelUp();
            experience -= requiredExp;
        }
        
        // Actualizar UI
        if (GameManager.Instance.uiManager != null)
        {
            GameManager.Instance.uiManager.UpdateExperienceBar(experience, requiredExp);
        }
    }
    
    void LevelUp()
    {
        level++;
        maxHealth += 20;
        maxMana += 10;
        attackPower += 2;
        defense += 1;
        magicPower += 2;
        
        currentHealth = maxHealth;
        currentMana = maxMana;
        
        // Notificar al jugador
        if (GameManager.Instance.uiManager != null)
        {
            GameManager.Instance.uiManager.ShowLevelUpMessage(level);
        }
    }
    
    // Métodos de economía
    public void AddGold(int amount)
    {
        gold += amount;
        if (GameManager.Instance.uiManager != null)
        {
            GameManager.Instance.uiManager.UpdateGoldDisplay(gold);
        }
    }
    
    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            if (GameManager.Instance.uiManager != null)
            {
                GameManager.Instance.uiManager.UpdateGoldDisplay(gold);
            }
            return true;
        }
        return false;
    }
    
    // Métodos de equipo
    public void EquipWeapon(Equipment weapon)
    {
        currentWeapon = weapon;
        attackPower = weapon.baseAttack;
    }
    
    public void EquipArmor(Equipment armor)
    {
        currentArmor = armor;
        defense = armor.baseDefense;
    }
    
    void Die()
    {
        // Mostrar pantalla de muerte
        if (GameManager.Instance.uiManager != null)
        {
            GameManager.Instance.uiManager.ShowGameOver();
        }
        
        // Reiniciar el juego después de un tiempo
        Invoke("RestartGame", 3f);
    }
    
    void RestartGame()
    {
        // Reiniciar estadísticas
        currentHealth = maxHealth;
        currentMana = maxMana;
        level = 1;
        experience = 0;
        gold = 0;
        
        // Volver al menú
        GameManager.Instance.ChangeState(GameState.Menu);
    }
}