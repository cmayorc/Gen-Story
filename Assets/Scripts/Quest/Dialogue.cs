using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dialogue - Sistema de diálogos para NPCs en el RPG
/// </summary>
public class Dialogue : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public string npcName = "NPC";
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
    
    [Header("Quest Settings")]
    public Quest questToGive;
    public string questDialogue = "Tengo una misión para ti.";
    
    [Header("References")]
    public UIManager uiManager;
    
    private bool isPlayerInRange = false;
    private int currentLineIndex = 0;
    
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
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            StartDialogue();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            ShowInteractionPrompt();
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            HideInteractionPrompt();
        }
    }
    
    void ShowInteractionPrompt()
    {
        if (uiManager != null)
        {
            uiManager.ShowCombatMessage($"Hablar con {npcName}");
        }
    }
    
    void HideInteractionPrompt()
    {
        // Limpiar mensaje de interacción
    }
    
    /// <summary>
    /// Iniciar diálogo con el NPC
    /// </summary>
    public void StartDialogue()
    {
        if (dialogueLines.Count == 0)
        {
            return;
        }
        
        currentLineIndex = 0;
        ShowDialogueLine(dialogueLines[0]);
    }
    
    /// <summary>
    /// Mostrar una línea de diálogo
    /// </summary>
    /// <param name="line">Línea de diálogo a mostrar</param>
    void ShowDialogueLine(DialogueLine line)
    {
        if (uiManager != null)
        {
            uiManager.ShowCombatMessage($"{npcName}: {line.text}");
        }
        
        // Manejar opciones de diálogo
        if (line.hasOptions)
        {
            ShowDialogueOptions(line.options);
        }
        
        // Manejar recompensas
        if (line.hasReward)
        {
            GiveReward(line.rewardItem);
        }
        
        // Manejar misiones
        if (line.hasQuest && questToGive != null)
        {
            GiveQuest();
        }
    }
    
    /// <summary>
    /// Mostrar opciones de diálogo
    /// </summary>
    /// <param name="options">Opciones disponibles</param>
    void ShowDialogueOptions(List<DialogueOption> options)
    {
        // En una implementación completa, esto mostraría botones en la UI
        // Por ahora, simplemente mostramos las opciones en consola
        Debug.Log($"Opciones de diálogo con {npcName}:");
        for (int i = 0; i < options.Count; i++)
        {
            Debug.Log($"{i + 1}. {options[i].text}");
        }
    }
    
    /// <summary>
    /// Otorgar recompensa al jugador
    /// </summary>
    /// <param name="rewardItem">Item a otorgar</param>
    void GiveReward(Item rewardItem)
    {
        InventoryManager inventory = FindObjectOfType<InventoryManager>();
        if (inventory != null && rewardItem != null)
        {
            inventory.AddItem(rewardItem);
            
            if (uiManager != null)
            {
                uiManager.ShowCombatMessage($"Recibiste: {rewardItem.itemName}");
            }
        }
    }
    
    /// <summary>
    /// Otorgar misión al jugador
    /// </summary>
    void GiveQuest()
    {
        QuestManager questManager = FindObjectOfType<QuestManager>();
        if (questManager != null && questToGive != null)
        {
            // Verificar si el jugador ya tiene la misión
            if (!questManager.HasActiveQuest(questToGive.id))
            {
                questManager.AcceptQuest(questToGive);
                
                if (uiManager != null)
                {
                    uiManager.ShowCombatMessage($"Misión: {questToGive.title}");
                }
            }
            else
            {
                if (uiManager != null)
                {
                    uiManager.ShowCombatMessage("Ya tienes esta misión.");
                }
            }
        }
    }
    
    /// <summary>
    /// Avanzar a la siguiente línea de diálogo
    /// </summary>
    public void NextDialogueLine()
    {
        currentLineIndex++;
        
        if (currentLineIndex < dialogueLines.Count)
        {
            ShowDialogueLine(dialogueLines[currentLineIndex]);
        }
        else
        {
            EndDialogue();
        }
    }
    
    /// <summary>
    /// Finalizar diálogo
    /// </summary>
    void EndDialogue()
    {
        if (uiManager != null)
        {
            uiManager.ShowCombatMessage("Diálogo terminado.");
        }
    }
    
    // Métodos para el editor
    public void AddTestDialogue()
    {
        dialogueLines.Clear();
        
        DialogueLine line1 = new DialogueLine
        {
            text = "¡Hola aventurero! El reino está en peligro.",
            hasOptions = true,
            options = new List<DialogueOption>
            {
                new DialogueOption { text = "¿Qué sucede?", nextLineIndex = 1 },
                new DialogueOption { text = "No me interesa.", nextLineIndex = 2 }
            }
        };
        
        DialogueLine line2 = new DialogueLine
        {
            text = "Los goblins han atacado el pueblo. Necesito que los derrotes.",
            hasQuest = true,
            hasOptions = true,
            options = new List<DialogueOption>
            {
                new DialogueOption { text = "Aceptar misión", nextLineIndex = 3 },
                new DialogueOption { text = "Rechazar", nextLineIndex = 4 }
            }
        };
        
        DialogueLine line3 = new DialogueLine
        {
            text = "¡Gracias! Vuelve cuando hayas completado la misión.",
            hasReward = true,
            rewardItem = new Consumable
            {
                itemName = "Poción de Salud",
                description = "Restaura 25 de salud",
                itemType = ItemType.Consumable,
                value = 10,
                consumableType = ConsumableType.HealthPotion,
                healAmount = 25
            }
        };
        
        dialogueLines.Add(line1);
        dialogueLines.Add(line2);
        dialogueLines.Add(line3);
    }
}

// Clases de datos para diálogos
[System.Serializable]
public class DialogueLine
{
    public string text;
    public bool hasOptions = false;
    public List<DialogueOption> options = new List<DialogueOption>();
    public bool hasReward = false;
    public Item rewardItem;
    public bool hasQuest = false;
}

[System.Serializable]
public class DialogueOption
{
    public string text;
    public int nextLineIndex;
}