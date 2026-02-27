using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Manager - Sistema de interfaz de usuario del RPG
/// Gestiona todos los elementos de UI del juego
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Main UI Elements")]
    public GameObject mainMenuPanel;
    public GameObject gameUIPanel;
    public GameObject combatUIPanel;
    public GameObject inventoryPanel;
    public GameObject pauseMenuPanel;
    public GameObject gameOverPanel;
    
    [Header("Game UI Components")]
    public Slider healthBar;
    public Slider manaBar;
    public Slider experienceBar;
    public Text healthText;
    public Text manaText;
    public Text levelText;
    public Text goldText;
    public Text experienceText;
    
    [Header("Combat UI Components")]
    public Text combatMessageText;
    public Text playerStatsText;
    public Text enemyStatsText;
    
    [Header("Inventory UI Components")]
    public Transform inventoryGrid;
    public Transform equipmentGrid;
    public GameObject inventoryItemPrefab;
    
    [Header("Animation Settings")]
    public float fadeDuration = 0.5f;
    
    private bool isInventoryVisible = false;
    private bool isCombatUIVisible = false;
    
    void Start()
    {
        InitializeUI();
    }
    
    void InitializeUI()
    {
        // Ocultar todos los paneles inicialmente
        HideAllPanels();
        
        // Mostrar menú principal por defecto
        ShowMainMenu();
    }
    
    void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameUIPanel != null) gameUIPanel.SetActive(false);
        if (combatUIPanel != null) combatUIPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }
    
    // Métodos para mostrar/ocultar paneles principales
    public void ShowMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }
    
    public void HideMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
    }
    
    public void ShowGameUI()
    {
        if (gameUIPanel != null) gameUIPanel.SetActive(true);
    }
    
    public void HideGameUI()
    {
        if (gameUIPanel != null) gameUIPanel.SetActive(false);
    }
    
    public void ShowCombatUI()
    {
        if (combatUIPanel != null) combatUIPanel.SetActive(true);
        isCombatUIVisible = true;
    }
    
    public void HideCombatUI()
    {
        if (combatUIPanel != null) combatUIPanel.SetActive(false);
        isCombatUIVisible = false;
    }
    
    public void ShowInventory()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(true);
        isInventoryVisible = true;
    }
    
    public void HideInventory()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        isInventoryVisible = false;
    }
    
    public void ShowPauseMenu()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
    }
    
    public void HidePauseMenu()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
    }
    
    public void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }
    
    // Métodos para actualizar estadísticas del jugador
    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
        
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }
    
    public void UpdateManaBar(int currentMana, int maxMana)
    {
        if (manaBar != null)
        {
            manaBar.maxValue = maxMana;
            manaBar.value = currentMana;
        }
        
        if (manaText != null)
        {
            manaText.text = $"{currentMana}/{maxMana}";
        }
    }
    
    public void UpdateExperienceBar(int currentExp, int maxExp)
    {
        if (experienceBar != null)
        {
            experienceBar.maxValue = maxExp;
            experienceBar.value = currentExp;
        }
        
        if (experienceText != null)
        {
            experienceText.text = $"{currentExp}/{maxExp}";
        }
    }
    
    public void UpdateLevelDisplay(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Nivel {level}";
        }
    }
    
    public void UpdateGoldDisplay(int gold)
    {
        if (goldText != null)
        {
            goldText.text = $"Oro: {gold}";
        }
    }
    
    // Métodos para UI de combate
    public void ShowCombatMessage(string message)
    {
        if (combatMessageText != null)
        {
            combatMessageText.text = message;
            StartCoroutine(FadeOutText(combatMessageText));
        }
    }
    
    public void UpdateCombatUI(PlayerController player, EnemyAI enemy)
    {
        if (playerStatsText != null && player != null)
        {
            playerStatsText.text = $"Jugador: {player.currentHealth}/{player.maxHealth} HP";
        }
        
        if (enemyStatsText != null && enemy != null)
        {
            enemyStatsText.text = $"Enemigo: {enemy.currentHealth}/{enemy.maxHealth} HP";
        }
    }
    
    // Métodos para UI de inventario
    public void UpdateInventoryUI(List<Item> items, Dictionary<EquipmentSlot, Equipment> equipment)
    {
        // Limpiar inventario actual
        ClearInventoryGrid();
        
        // Mostrar items del inventario
        foreach (Item item in items)
        {
            CreateInventoryItem(item);
        }
        
        // Mostrar equipo
        UpdateEquipmentUI(equipment);
    }
    
    void ClearInventoryGrid()
    {
        if (inventoryGrid != null)
        {
            foreach (Transform child in inventoryGrid)
            {
                Destroy(child.gameObject);
            }
        }
    }
    
    void CreateInventoryItem(Item item)
    {
        if (inventoryItemPrefab != null && inventoryGrid != null)
        {
            GameObject itemObject = Instantiate(inventoryItemPrefab, inventoryGrid);
            InventoryItemUI itemUI = itemObject.GetComponent<InventoryItemUI>();
            
            if (itemUI != null)
            {
                itemUI.SetItem(item);
            }
        }
    }
    
    void UpdateEquipmentUI(Dictionary<EquipmentSlot, Equipment> equipment)
    {
        // Limpiar equipo actual
        if (equipmentGrid != null)
        {
            foreach (Transform child in equipmentGrid)
            {
                Destroy(child.gameObject);
            }
        }
        
        // Mostrar equipo actual
        foreach (var equipmentPair in equipment)
        {
            if (equipmentPair.Value != null)
            {
                CreateInventoryItem(equipmentPair.Value);
            }
        }
    }
    
    // Métodos para mensajes y notificaciones
    public void ShowLevelUpMessage(int newLevel)
    {
        ShowCombatMessage($"¡Subiste de nivel! Nivel {newLevel}");
    }
    
    // Métodos auxiliares
    IEnumerator FadeOutText(Text text)
    {
        if (text == null) yield break;
        
        Color originalColor = text.color;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            text.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        
        text.color = originalColor;
    }
    
    // Métodos para el editor
    public void Initialize()
    {
        // Inicializar valores por defecto
        if (healthBar != null)
        {
            healthBar.maxValue = 100;
            healthBar.value = 100;
        }
        
        if (manaBar != null)
        {
            manaBar.maxValue = 50;
            manaBar.value = 50;
        }
        
        if (experienceBar != null)
        {
            experienceBar.maxValue = 100;
            experienceBar.value = 0;
        }
        
        UpdateLevelDisplay(1);
        UpdateGoldDisplay(0);
    }
    
    // Métodos de depuración
    public void AddDebugItems()
    {
        InventoryManager inventory = FindObjectOfType<InventoryManager>();
        if (inventory != null)
        {
            inventory.AddTestItems();
        }
    }
}