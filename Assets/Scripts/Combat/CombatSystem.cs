using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Combat System - Sistema de combate del RPG
/// Gestiona turnos, cálculo de daño y estados de combate
/// </summary>
public class CombatSystem : MonoBehaviour
{
    [Header("Combat Settings")]
    public float combatStartTime = 2f;
    public float turnDuration = 1.5f;
    
    [Header("References")]
    public UIManager uiManager;
    
    private bool isInCombat = false;
    private PlayerController currentPlayer;
    private EnemyAI currentEnemy;
    private CombatState currentState = CombatState.PlayerTurn;
    
    enum CombatState
    {
        PlayerTurn,
        EnemyTurn,
        CombatEnd
    }
    
    void Start()
    {
        // Obtener referencia al UI Manager
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }
    }
    
    void Update()
    {
        if (isInCombat && GameManager.Instance.currentState == GameState.Combat)
        {
            HandleCombatTurns();
        }
    }
    
    public void StartCombat(PlayerController player, EnemyAI enemy)
    {
        if (isInCombat)
        {
            return;
        }
        
        isInCombat = true;
        currentPlayer = player;
        currentEnemy = enemy;
        currentState = CombatState.PlayerTurn;
        
        // Cambiar estado del juego
        GameManager.Instance.ChangeState(GameState.Combat);
        
        // Mostrar UI de combate
        if (uiManager != null)
        {
            uiManager.ShowCombatUI();
            uiManager.UpdateCombatUI(currentPlayer, currentEnemy);
        }
        
        // Iniciar combate después de un breve delay
        Invoke("BeginCombat", combatStartTime);
    }
    
    void BeginCombat()
    {
        if (uiManager != null)
        {
            uiManager.ShowCombatMessage("¡Combate iniciado!");
        }
    }
    
    void HandleCombatTurns()
    {
        switch (currentState)
        {
            case CombatState.PlayerTurn:
                HandlePlayerTurn();
                break;
            case CombatState.EnemyTurn:
                HandleEnemyTurn();
                break;
            case CombatState.CombatEnd:
                EndCombat();
                break;
        }
    }
    
    void HandlePlayerTurn()
    {
        // El jugador puede atacar, usar habilidades o huir
        if (Input.GetButtonDown("Fire1"))
        {
            PlayerAttack(currentPlayer);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            UseSkill(currentPlayer, "Habilidad 1");
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            UseItem(currentPlayer, "Poción");
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            TryEscapeCombat();
        }
    }
    
    void HandleEnemyTurn()
    {
        // El enemigo ataca automáticamente después de un delay
        Invoke("EnemyAttack", turnDuration);
    }
    
    public void PlayerAttack(PlayerController player)
    {
        if (currentEnemy == null || !isInCombat)
        {
            return;
        }
        
        // Calcular daño
        int damage = CalculateDamage(player.attackPower, currentEnemy.defense);
        
        // Aplicar daño al enemigo
        currentEnemy.TakeDamage(damage);
        
        // Mostrar mensaje
        if (uiManager != null)
        {
            uiManager.ShowCombatMessage($"¡Golpe crítico! -{damage} de daño");
            uiManager.UpdateCombatUI(player, currentEnemy);
        }
        
        // Cambiar a turno del enemigo
        currentState = CombatState.EnemyTurn;
    }
    
    public void EnemyAttack(EnemyAI enemy, PlayerController player)
    {
        if (currentPlayer == null || !isInCombat)
        {
            return;
        }
        
        // Calcular daño
        int damage = CalculateDamage(enemy.attackPower, currentPlayer.defense);
        
        // Aplicar daño al jugador
        currentPlayer.TakeDamage(damage);
        
        // Mostrar mensaje
        if (uiManager != null)
        {
            uiManager.ShowCombatMessage($"El enemigo te golpea! -{damage} de daño");
            uiManager.UpdateCombatUI(currentPlayer, enemy);
        }
        
        // Cambiar a turno del jugador
        currentState = CombatState.PlayerTurn;
    }
    
    int CalculateDamage(int attackPower, int defense)
    {
        // Fórmula básica de daño
        int baseDamage = attackPower;
        int mitigation = defense / 4; // La defensa reduce el daño
        int finalDamage = Mathf.Max(1, baseDamage - mitigation);
        
        // Probabilidad de golpe crítico (10%)
        if (Random.value < 0.1f)
        {
            finalDamage *= 2;
            if (uiManager != null)
            {
                uiManager.ShowCombatMessage("¡Golpe crítico!");
            }
        }
        
        return finalDamage;
    }
    
    void UseSkill(PlayerController player, string skillName)
    {
        if (currentEnemy == null || player.currentMana <= 0)
        {
            return;
        }
        
        // Costo de maná
        int manaCost = 10;
        if (player.currentMana >= manaCost)
        {
            player.currentMana -= manaCost;
            
            // Efecto de la habilidad (daño mágico)
            int magicDamage = player.magicPower * 2;
            currentEnemy.TakeDamage(magicDamage);
            
            if (uiManager != null)
            {
                uiManager.ShowCombatMessage($"¡{skillName}! -{magicDamage} de daño mágico");
                uiManager.UpdateCombatUI(player, currentEnemy);
            }
            
            currentState = CombatState.EnemyTurn;
        }
    }
    
    void UseItem(PlayerController player, string itemName)
    {
        // Usar ítem (por ejemplo, poción de curación)
        if (itemName == "Poción")
        {
            player.Heal(25);
            if (uiManager != null)
            {
                uiManager.ShowCombatMessage("¡Te curaste 25 de salud!");
                uiManager.UpdateCombatUI(player, currentEnemy);
            }
        }
        
        currentState = CombatState.EnemyTurn;
    }
    
    void TryEscapeCombat()
    {
        // Probabilidad de escape (50%)
        if (Random.value < 0.5f)
        {
            EndCombat();
            if (uiManager != null)
            {
                uiManager.ShowCombatMessage("¡Escapaste del combate!");
            }
        }
        else
        {
            if (uiManager != null)
            {
                uiManager.ShowCombatMessage("¡No pudiste escapar!");
            }
            currentState = CombatState.EnemyTurn;
        }
    }
    
    public void EnemyDefeated(EnemyAI enemy)
    {
        if (enemy != currentEnemy)
        {
            return;
        }
        
        currentState = CombatState.CombatEnd;
        
        // Mostrar mensaje de victoria
        if (uiManager != null)
        {
            uiManager.ShowCombatMessage("¡Enemigo derrotado!");
        }
        
        // Otorgar recompensas (ya se otorgaron en EnemyAI.Die())
        Invoke("EndCombat", 2f);
    }
    
    void EndCombat()
    {
        isInCombat = false;
        currentState = CombatState.CombatEnd;
        
        // Volver al estado de juego normal
        GameManager.Instance.ChangeState(GameState.Playing);
        
        // Ocultar UI de combate
        if (uiManager != null)
        {
            uiManager.HideCombatUI();
        }
        
        // Limpiar referencias
        currentPlayer = null;
        currentEnemy = null;
    }
    
    // Métodos auxiliares
    public bool IsInCombat()
    {
        return isInCombat;
    }
    
    public PlayerController GetCurrentPlayer()
    {
        return currentPlayer;
    }
    
    public EnemyAI GetCurrentEnemy()
    {
        return currentEnemy;
    }
}