using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Inventory Item UI - Componente para mostrar un item en la UI del inventario
/// </summary>
public class InventoryItemUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Text itemNameText;
    public Text itemDescriptionText;
    public Text itemValueText;
    public Image itemIcon;
    public Button useButton;
    public Button equipButton;
    public Button unequipButton;
    
    private Item currentItem;
    
    void Start()
    {
        // Configurar botones
        if (useButton != null)
        {
            useButton.onClick.AddListener(OnUseButtonClicked);
        }
        
        if (equipButton != null)
        {
            equipButton.onClick.AddListener(OnEquipButtonClicked);
        }
        
        if (unequipButton != null)
        {
            unequipButton.onClick.AddListener(OnUnequipButtonClicked);
        }
    }
    
    /// <summary>
    /// Establecer el item a mostrar
    /// </summary>
    /// <param name="item">Item a mostrar</param>
    public void SetItem(Item item)
    {
        currentItem = item;
        
        if (itemNameText != null && item != null)
        {
            itemNameText.text = item.itemName;
        }
        
        if (itemDescriptionText != null && item != null)
        {
            itemDescriptionText.text = item.description;
        }
        
        if (itemValueText != null && item != null)
        {
            itemValueText.text = $"Valor: {item.value}";
        }
        
        // Actualizar visibilidad de botones según el tipo de item
        UpdateButtonVisibility();
    }
    
    /// <summary>
    /// Actualizar la visibilidad de los botones según el tipo de item
    /// </summary>
    void UpdateButtonVisibility()
    {
        if (currentItem == null)
        {
            return;
        }
        
        // Mostrar/Ocultar botones según el tipo de item
        if (useButton != null)
        {
            useButton.gameObject.SetActive(currentItem.itemType == ItemType.Consumable);
        }
        
        if (equipButton != null)
        {
            equipButton.gameObject.SetActive(currentItem.itemType == ItemType.Equipment);
        }
        
        if (unequipButton != null)
        {
            unequipButton.gameObject.SetActive(currentItem.itemType == ItemType.Equipment);
        }
    }
    
    /// <summary>
    /// Manejar clic en botón de usar
    /// </summary>
    void OnUseButtonClicked()
    {
        InventoryManager inventory = FindObjectOfType<InventoryManager>();
        if (inventory != null && currentItem != null)
        {
            inventory.UseItem(currentItem);
        }
    }
    
    /// <summary>
    /// Manejar clic en botón de equipar
    /// </summary>
    void OnEquipButtonClicked()
    {
        InventoryManager inventory = FindObjectOfType<InventoryManager>();
        if (inventory != null && currentItem != null)
        {
            Equipment equipment = currentItem as Equipment;
            if (equipment != null)
            {
                inventory.EquipItem(equipment);
            }
        }
    }
    
    /// <summary>
    /// Manejar clic en botón de desequipar
    /// </summary>
    void OnUnequipButtonClicked()
    {
        InventoryManager inventory = FindObjectOfType<InventoryManager>();
        if (inventory != null && currentItem != null)
        {
            Equipment equipment = currentItem as Equipment;
            if (equipment != null)
            {
                // Obtener el slot del equipo
                EquipmentSlot slot = equipment.equipmentSlot;
                
                // Desequipar el item
                inventory.UnequipItem(slot);
            }
        }
    }
    
    /// <summary>
    /// Obtener el item actual
    /// </summary>
    /// <returns>Item actual</returns>
    public Item GetItem()
    {
        return currentItem;
    }
}