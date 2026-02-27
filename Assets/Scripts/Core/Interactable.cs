using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interactable - Clase base para objetos interactuables en el RPG
/// </summary>
public abstract class Interactable : MonoBehaviour
{
    [Header("Interaction Settings")]
    public string interactionText = "Presiona E para interactuar";
    public float interactionRange = 2f;
    
    [Header("Visual Feedback")]
    public Color highlightColor = Color.yellow;
    public float highlightIntensity = 1.5f;
    
    protected bool isPlayerInRange = false;
    protected Renderer objectRenderer;
    protected Color originalColor;
    
    protected virtual void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
        }
    }
    
    protected virtual void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Interact(null); // El jugador será pasado desde el PlayerController
        }
    }
    
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            ShowInteractionPrompt();
        }
    }
    
    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            HideInteractionPrompt();
        }
    }
    
    protected virtual void ShowInteractionPrompt()
    {
        // Resaltar el objeto
        if (objectRenderer != null)
        {
            objectRenderer.material.color = highlightColor * highlightIntensity;
        }
        
        // Mostrar mensaje en UI (implementación opcional)
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.ShowCombatMessage(interactionText);
        }
    }
    
    protected virtual void HideInteractionPrompt()
    {
        // Restaurar color original
        if (objectRenderer != null)
        {
            objectRenderer.material.color = originalColor;
        }
    }
    
    /// <summary>
    /// Método abstracto que debe ser implementado por las clases hijas
    /// </summary>
    /// <param name="player">Referencia al jugador que interactúa</param>
    public abstract void Interact(PlayerController player);
}