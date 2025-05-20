using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Player player;

    [Header("UI Elements")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject fishingPanel;
    [SerializeField] private GameObject shoppingPanel;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] public GameObject tooltipPanel;
    private GameManager.GameState previousState;

    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private Slider fuelSlider;
    [SerializeField] private TextMeshProUGUI fuelText;

    // Dictionary to track active UI states
    private Dictionary<GameManager.GameState, List<GameObject>> statePanels;

    private void Awake()
    {
        statePanels = new Dictionary<GameManager.GameState, List<GameObject>>
    {
        { GameManager.GameState.DIALOGUE, new List<GameObject> { dialoguePanel } },
        { GameManager.GameState.FISHING, new List<GameObject> { fishingPanel } },
        { GameManager.GameState.MENU, new List<GameObject> { menuPanel } },
        { GameManager.GameState.INVENTORY, new List<GameObject> { inventoryPanel } },
        { GameManager.GameState.SHOPPING, new List<GameObject> { inventoryPanel, shoppingPanel } }
    };
    }

    private void Start()
    {
        // Find references if not assigned
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (player == null && gameManager != null)
            player = gameManager.player;

        // Subscribe to game state changes
        if (gameManager != null)
            gameManager.OnGameStateChanged += UpdateUI;

        // Initial UI update
        if (player != null)
            UpdateHealthDisplay(player.currentHealth, player.maxHealth);

        // Hide all panels initially
        HideAllPanels();

        // Show the appropriate panel for the current state
        if (gameManager != null)
            UpdateUI(gameManager.state);
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (gameManager != null)
            gameManager.OnGameStateChanged -= UpdateUI;
    }

    private void Update()
    {
        // Update health display if player reference exists
        if (player != null && healthText != null)
            UpdateHealthDisplay(player.currentHealth, player.maxHealth);

        // Update money display if game manager reference exists
        if (gameManager != null && moneyText != null)
            UpdateMoneyDisplay(gameManager.money);

        // Update fuel display if player boat reference exists
        // if (gameManager?.currentPlayerBoat is PlayerBoat playerBoat && fuelText != null)
        //     UpdateFuelDisplay(playerBoat.fuel, playerBoat.maxFuel);
    }

    // Method to update UI based on game state
    public void UpdateUI(GameManager.GameState newState)
    {
        HideAllPanels();

        // Show all panels for the new state
        if (statePanels.TryGetValue(newState, out List<GameObject> panels))
        {
            foreach (var panel in panels)
            {
                if (panel != null)
                    panel.SetActive(true);
            }
        }

        // Special case for DRIVING state, which uses the HUD
        if (newState == GameManager.GameState.DRIVING && hudPanel != null)
        {
            hudPanel.SetActive(true);
        }

        if (newState != GameManager.GameState.INVENTORY)
        {
            if (Tooltip.Instance != null)
            {
                Tooltip.Instance.HideTooltip();
            }
        }

        Debug.Log($"UI updated for game state: {newState}");
    }

    // Helper method to hide all UI panels
    private void HideAllPanels()
    {
        var uniquePanels = new HashSet<GameObject>();
        foreach (var panelList in statePanels.Values)
        {
            foreach (var panel in panelList)
            {
                if (panel != null && panel != tooltipPanel)
                    uniquePanels.Add(panel);
            }
        }

        foreach (var panel in uniquePanels)
        {
            panel.SetActive(false);
        }

        if (pauseMenu != null)
            pauseMenu.SetActive(false);
    }

    #region UI Update Methods

    // Update the health display with current values
    public void UpdateHealthDisplay(int currentHealth, int maxHealth)
    {
        if (healthText != null)
            healthText.text = $"HP: {currentHealth}/{maxHealth}";

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    // Update the money display
    public void UpdateMoneyDisplay(int amount)
    {
        if (moneyText != null)
            moneyText.text = $"${amount}";
    }

    // Update the fuel display for the boat
    public void UpdateFuelDisplay(float currentFuel, float maxFuel)
    {
        if (fuelText != null)
            fuelText.text = $"Fuel: {Mathf.RoundToInt(currentFuel)}/{Mathf.RoundToInt(maxFuel)}";

        if (fuelSlider != null)
        {
            fuelSlider.maxValue = maxFuel;
            fuelSlider.value = currentFuel;
        }
    }

    #endregion

    #region Panel Toggle Methods

    // Methods to show/hide specific UI elements
    public void ShowFishingUI(bool show) => SetPanelActive(fishingPanel, show);

    // public void ShowDialogueUI(Dialogue dialogue = null)
    // {
    //     SetPanelActive(dialoguePanel, dialogue != null);
    //
    //     // If dialogue parameter is provided, update dialogue UI content
    //     if (dialogue != null)
    //     {
    //         // Implementation would update text fields, speaker name, etc.
    //     }
    // }

    public void UpdateDialogueText(string text, string speaker)
    {
        // Implementation depends on your dialogue UI structure
        // This would update the text elements in your dialogue panel
    }

    // public void ShowInventoryUI(Inventory inventory = null)
    // {
    //     SetPanelActive(inventoryPanel, inventory != null);
    //
    //     if (inventory != null)
    //     {
    //         UpdateInventoryDisplay();
    //     }
    // }

    public void UpdateInventoryDisplay()
    {
        // Implementation depends on your inventory UI structure
    }

    public void ShowPauseMenu(bool show) => SetPanelActive(pauseMenu, show);

    // Helper method to safely toggle panel visibility
    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }

    #endregion
}
