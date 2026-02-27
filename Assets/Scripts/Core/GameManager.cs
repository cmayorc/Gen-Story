using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Game Manager - Controlador principal del juego RPG
/// Gestiona el flujo del juego, estados y sistemas principales
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game State")]
    public GameState currentState = GameState.Menu;
    
    [Header("Player References")]
    public PlayerController player;
    public Transform playerSpawnPoint;
    
    [Header("UI References")]
    public UIManager uiManager;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        InitializeGame();
    }
    
    private void InitializeGame()
    {
        // Inicializar sistemas
        if (uiManager != null)
        {
            uiManager.Initialize();
        }
        
        // Cargar escena principal
        ChangeState(GameState.Menu);
    }
    
    public void ChangeState(GameState newState)
    {
        currentState = newState;
        
        switch (newState)
        {
            case GameState.Menu:
                HandleMenuState();
                break;
            case GameState.Playing:
                HandlePlayingState();
                break;
            case GameState.Combat:
                HandleCombatState();
                break;
            case GameState.Paused:
                HandlePausedState();
                break;
        }
    }
    
    private void HandleMenuState()
    {
        if (uiManager != null)
        {
            uiManager.ShowMainMenu();
        }
    }
    
    private void HandlePlayingState()
    {
        if (uiManager != null)
        {
            uiManager.HideMainMenu();
            uiManager.ShowGameUI();
        }
        
        if (player != null && playerSpawnPoint != null)
        {
            player.transform.position = playerSpawnPoint.position;
        }
    }
    
    private void HandleCombatState()
    {
        if (uiManager != null)
        {
            uiManager.ShowCombatUI();
        }
    }
    
    private void HandlePausedState()
    {
        if (uiManager != null)
        {
            uiManager.ShowPauseMenu();
        }
    }
    
    public void StartGame()
    {
        ChangeState(GameState.Playing);
    }
    
    public void StartCombat()
    {
        ChangeState(GameState.Combat);
    }
    
    public void PauseGame()
    {
        ChangeState(GameState.Paused);
    }
    
    public void ResumeGame()
    {
        ChangeState(GameState.Playing);
    }
}

public enum GameState
{
    Menu,
    Playing,
    Combat,
    Paused
}