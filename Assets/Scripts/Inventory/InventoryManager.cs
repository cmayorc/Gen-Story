using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Inventory Manager - Sistema de inventario del RPG
/// Gestiona items, equipo y slots del inventario
/// </summary>
public class InventoryManager : MonoBehaviour
{
    [Header("Inventory Settings")]
    public int maxInventorySlots = 20;
    public int maxEquipmentSlots = 5;
    
    [Header("References")]
    public UIManager uiManager;
    
    private List<Item> inventoryItems = new List<Item>();
    private Dictionary<EquipmentSlot, Equipment> equipment = new Dictionary<EquipmentSlot, Equipment>();
    private bool isInventoryOpen = false;
    
    // Referencias a sistemas
    private PlayerController player;
    
    void Start()
    {
        // Inicializar equipo vacío
        InitializeEquipment();
        
        // Obtener referencia al jugador
        player = FindObjectOfType<PlayerController>();
        
        // Obtener referencia al UI Manager
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }
    }
    
    void InitializeEquipment()
    {
        equipment[EquipmentSlot.Weapon] = null;
        equipment[EquipmentSlot.Armor] = null;
        equipment[EquipmentSlot.Helmet] = null;
        equipment[EquipmentSlot.Gloves] = null;
        equipment[EquipmentSlot.Boots] = null;
    }
    
    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        
        if (isInventoryOpen)
        {
            OpenInventory();
        }
        else
        {
            CloseInventory();
        }
    }
    
    void OpenInventory()
    {
        if (uiManager != null)
        {
            uiManager.ShowInventory();
            uiManager.UpdateInventoryUI(inventoryItems, equipment);
        }
        
        // Pausar el juego
        Time.timeScale = 0f;
    }
    
    void CloseInventory()
    {
        if (uiManager != null)
        {
            uiManager.HideInventory();
        }
        
        // Reanudar el juego
        Time.timeScale = 1f;
    }
    
    // Métodos de gestión de inventario
    public bool AddItem(Item item)
    {
        if (inventoryItems.Count >= maxInventorySlots)
        {
            Debug.Log("Inventario lleno");
            return false;
        }
        
        inventoryItems.Add(item);
        
        // Actualizar UI
        if (uiManager != null && isInventoryOpen)
        {
            uiManager.UpdateInventoryUI(inventoryItems, equipment);
        }
        
        return true;
    }
    
    public bool RemoveItem(Item item)
    {
        if (inventoryItems.Contains(item))
        {
            inventoryItems.Remove(item);
            
            // Actualizar UI
            if (uiManager != null && isInventoryOpen)
            {
                uiManager.UpdateInventoryUI(inventoryItems, equipment);
            }
            
            return true;
        }
        
        return false;
    }
    
    public bool UseItem(Item item)
    {
        if (item == null)
        {
            return false;
        }
        
        switch (item.itemType)
        {
            case ItemType.Consumable:
                return UseConsumable(item);
            case ItemType.Equipment:
                return EquipItem((Equipment)item);
            default:
                return false;
        }
    }
    
    bool UseConsumable(Item item)
    {
        if (player == null)
        {
            return false;
        }
        
        Consumable consumable = (Consumable)item;
        
        switch (consumable.consumableType)
        {
            case ConsumableType.HealthPotion:
                player.Heal(consumable.healAmount);
                break;
            case ConsumableType.ManaPotion:
                player.RestoreMana(consumable.manaAmount);
                break;
            case ConsumableType.Elixir:
                player.Heal(consumable.healAmount);
                player.RestoreMana(consumable.manaAmount);
                break;
        }
        
        // Remover el ítem usado
        RemoveItem(item);
        
        return true;
    }
    
    bool EquipItem(Equipment equipmentItem)
    {
        if (equipmentItem == null)
        {
            return false;
        }
        
        EquipmentSlot slot = equipmentItem.equipmentSlot;
        
        // Verificar si el jugador cumple con los requisitos
        if (player.level < equipmentItem.requiredLevel)
        {
            Debug.Log($"Necesitas nivel {equipmentItem.requiredLevel} para equipar esto");
            return false;
        }
        
        // Desequipar item actual del mismo slot
        if (equipment.ContainsKey(slot) && equipment[slot] != null)
        {
            UnequipItem(slot);
        }
        
        // Equipar nuevo item
        equipment[slot] = equipmentItem;
        
        // Aplicar efectos del equipo
        ApplyEquipmentEffects(equipmentItem);
        
        // Remover de inventario
        RemoveItem(equipmentItem);
        
        // Actualizar UI
        if (uiManager != null && isInventoryOpen)
        {
            uiManager.UpdateInventoryUI(inventoryItems, equipment);
        }
        
        return true;
    }
    
    void UnequipItem(EquipmentSlot slot)
    {
        if (equipment.ContainsKey(slot) && equipment[slot] != null)
        {
            Equipment oldItem = equipment[slot];
            
            // Remover efectos
            RemoveEquipmentEffects(oldItem);
            
            // Agregar al inventario
            AddItem(oldItem);
            
            // Limpiar slot
            equipment[slot] = null;
        }
    }
    
    void ApplyEquipmentEffects(Equipment equipmentItem)
    {
        if (player == null)
        {
            return;
        }
        
        player.attackPower += equipmentItem.baseAttack;
        player.defense += equipmentItem.baseDefense;
        player.maxHealth += equipmentItem.healthBonus;
        player.maxMana += equipmentItem.manaBonus;
        
        // Actualizar salud y maná actual si aumentó el máximo
        player.currentHealth = Mathf.Min(player.currentHealth, player.maxHealth);
        player.currentMana = Mathf.Min(player.currentMana, player.maxMana);
    }
    
    void RemoveEquipmentEffects(Equipment equipmentItem)
    {
        if (player == null)
        {
            return;
        }
        
        player.attackPower -= equipmentItem.baseAttack;
        player.defense -= equipmentItem.baseDefense;
        player.maxHealth -= equipmentItem.healthBonus;
        player.maxMana -= equipmentItem.manaBonus;
        
        // Ajustar salud y maná actual si disminuyó el máximo
        player.currentHealth = Mathf.Min(player.currentHealth, player.maxHealth);
        player.currentMana = Mathf.Min(player.currentMana, player.maxMana);
    }
    
    // Métodos de búsqueda
    public List<Item> GetItemsByType(ItemType type)
    {
        List<Item> items = new List<Item>();
        
        foreach (Item item in inventoryItems)
        {
            if (item.itemType == type)
            {
                items.Add(item);
            }
        }
        
        return items;
    }
    
    public Equipment GetEquippedItem(EquipmentSlot slot)
    {
        if (equipment.ContainsKey(slot))
        {
            return equipment[slot];
        }
        return null;
    }
    
    public int GetInventoryCount()
    {
        return inventoryItems.Count;
    }
    
    public int GetFreeSlots()
    {
        return maxInventorySlots - inventoryItems.Count;
    }
    
    // Métodos para el editor
    public void AddTestItems()
    {
        // Crear items de prueba
        Consumable healthPotion = new Consumable
        {
            itemName = "Poción de Salud",
            description = "Restaura 25 de salud",
            itemType = ItemType.Consumable,
            value = 10,
            consumableType = ConsumableType.HealthPotion,
            healAmount = 25
        };
        
        Equipment sword = new Equipment
        {
            itemName = "Espada de Hierro",
            description = "Espada básica",
            itemType = ItemType.Equipment,
            value = 50,
            equipmentSlot = EquipmentSlot.Weapon,
            baseAttack = 15,
            baseDefense = 0,
            requiredLevel = 1
        };
        
        // Agregar al inventario
        AddItem(healthPotion);
        AddItem(sword);
    }
}

// Clases de datos para items
[System.Serializable]
public abstract class Item
{
    public string itemName;
    public string description;
    public ItemType itemType;
    public int value;
}

[System.Serializable]
public class Consumable : Item
{
    public ConsumableType consumableType;
    public int healAmount;
    public int manaAmount;
}

[System.Serializable]
public class Equipment : Item
{
    public EquipmentSlot equipmentSlot;
    public int baseAttack;
    public int baseDefense;
    public int healthBonus;
    public int manaBonus;
    public int requiredLevel;
}

public enum ItemType
{
    Consumable,
    Equipment,
    Quest,
    Material
}

public enum ConsumableType
{
    HealthPotion,
    ManaPotion,
    Elixir
}

public enum EquipmentSlot
{
    Weapon,
    Armor,
    Helmet,
    Gloves,
    Boots
}