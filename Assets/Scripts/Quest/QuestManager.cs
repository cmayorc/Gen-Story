using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quest Manager - Sistema de misiones del RPG
/// Gestiona el progreso, recompensas y estado de las misiones
/// </summary>
public class QuestManager : MonoBehaviour
{
    [Header("Quest Settings")]
    public List<Quest> activeQuests = new List<Quest>();
    public List<Quest> completedQuests = new List<Quest>();
    
    [Header("References")]
    public UIManager uiManager;
    
    private static QuestManager instance;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Obtener referencia al UI Manager
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }
    }
    
    /// <summary>
    /// Aceptar una misión
    /// </summary>
    /// <param name="quest">Misión a aceptar</param>
    public void AcceptQuest(Quest quest)
    {
        if (quest == null || activeQuests.Contains(quest))
        {
            return;
        }
        
        activeQuests.Add(quest);
        quest.isActive = true;
        
        // Notificar al jugador
        if (uiManager != null)
        {
            uiManager.ShowCombatMessage($"Misión aceptada: {quest.title}");
        }
        
        Debug.Log($"Misión aceptada: {quest.title}");
    }
    
    /// <summary>
    /// Completar una misión
    /// </summary>
    /// <param name="quest">Misión a completar</param>
    public void CompleteQuest(Quest quest)
    {
        if (quest == null || !activeQuests.Contains(quest))
        {
            return;
        }
        
        // Otorgar recompensas
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.AddExperience(quest.experienceReward);
            player.AddGold(quest.goldReward);
            
            // Otorgar items de recompensa
            foreach (Item rewardItem in quest.rewardItems)
            {
                InventoryManager inventory = FindObjectOfType<InventoryManager>();
                if (inventory != null)
                {
                    inventory.AddItem(rewardItem);
                }
            }
        }
        
        // Mover a misiones completadas
        quest.isActive = false;
        quest.isCompleted = true;
        activeQuests.Remove(quest);
        completedQuests.Add(quest);
        
        // Notificar al jugador
        if (uiManager != null)
        {
            uiManager.ShowCombatMessage($"Misión completada: {quest.title}");
        }
        
        Debug.Log($"Misión completada: {quest.title}");
    }
    
    /// <summary>
    /// Actualizar progreso de una misión
    /// </summary>
    /// <param name="quest">Misión a actualizar</param>
    /// <param name="progress">Progreso actual</param>
    public void UpdateQuestProgress(Quest quest, int progress)
    {
        if (quest == null || !activeQuests.Contains(quest))
        {
            return;
        }
        
        quest.currentProgress = progress;
        
        // Verificar si la misión está completa
        if (quest.currentProgress >= quest.requiredProgress)
        {
            CompleteQuest(quest);
        }
        
        Debug.Log($"Progreso de misión {quest.title}: {quest.currentProgress}/{quest.requiredProgress}");
    }
    
    /// <summary>
    /// Obtener misiones activas
    /// </summary>
    /// <returns>Lista de misiones activas</returns>
    public List<Quest> GetActiveQuests()
    {
        return activeQuests;
    }
    
    /// <summary>
    /// Obtener misiones completadas
    /// </summary>
    /// <returns>Lista de misiones completadas</returns>
    public List<Quest> GetCompletedQuests()
    {
        return completedQuests;
    }
    
    /// <summary>
    /// Buscar una misión por ID
    /// </summary>
    /// <param name="questId">ID de la misión</param>
    /// <returns>Misión encontrada o null</returns>
    public Quest GetQuestById(string questId)
    {
        // Buscar en misiones activas
        foreach (Quest quest in activeQuests)
        {
            if (quest.id == questId)
            {
                return quest;
            }
        }
        
        // Buscar en misiones completadas
        foreach (Quest quest in completedQuests)
        {
            if (quest.id == questId)
            {
                return quest;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Verificar si el jugador tiene una misión activa
    /// </summary>
    /// <param name="questId">ID de la misión</param>
    /// <returns>True si tiene la misión activa</returns>
    public bool HasActiveQuest(string questId)
    {
        Quest quest = GetQuestById(questId);
        return quest != null && quest.isActive && !quest.isCompleted;
    }
    
    /// <summary>
    /// Verificar si el jugador completó una misión
    /// </summary>
    /// <param name="questId">ID de la misión</param>
    /// <returns>True si completó la misión</returns>
    public bool HasCompletedQuest(string questId)
    {
        Quest quest = GetQuestById(questId);
        return quest != null && quest.isCompleted;
    }
    
    // Métodos para el editor
    public void AddTestQuest()
    {
        Quest testQuest = new Quest
        {
            id = "test_quest_001",
            title = "Prueba de Misiones",
            description = "Esta es una misión de prueba para el sistema de misiones",
            questType = QuestType.Kill,
            targetEnemyType = "Goblin",
            requiredProgress = 5,
            currentProgress = 0,
            experienceReward = 100,
            goldReward = 50
        };
        
        AcceptQuest(testQuest);
    }
}

// Clase de datos para misiones
[System.Serializable]
public class Quest
{
    public string id;
    public string title;
    public string description;
    public QuestType questType;
    public string targetEnemyType;
    public int requiredProgress;
    public int currentProgress;
    public int experienceReward;
    public int goldReward;
    public List<Item> rewardItems = new List<Item>();
    
    public bool isActive = false;
    public bool isCompleted = false;
}

public enum QuestType
{
    Kill,
    Collect,
    TalkTo,
    Explore
}