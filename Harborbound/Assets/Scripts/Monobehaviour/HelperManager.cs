using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class HelperManager : MonoBehaviour
{
    public static HelperManager Instance { get; private set; }
    public UIManager UIManager;

    // Dictionary to track which interactions have been seen
    private Dictionary<string, bool> interactionHistory = new Dictionary<string, bool>();

    // Dictionary to track which items have been equipped
    private Dictionary<string, bool> equippedItemHistory = new Dictionary<string, bool>();

    // Dictionary to track which tooltips have been shown

    private void Awake()
    {
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

    // debug log history on game start
    private void Start()
    {
        // Load saved interaction history from PlayerPrefs
        LoadInteractionHistory();

        // Debug log the interaction history
        Debug.Log("Interaction History: " + string.Join(", ", interactionHistory));
        Debug.Log("Equipped Item History: " + string.Join(", ", equippedItemHistory));
    }

    // Call this when player interacts with an object that has a tooltip
    public void ShowTooltipIfFirstTime(
        string interactId,
        string interactMessage,
        float duration = 5f
    )
    {
        // Check if this tooltip has been shown before
        if (!interactionHistory.ContainsKey(interactId) || !interactionHistory[interactId])
        {
            // Show the tooltip
            UIManager.ShowInteractPanel(interactMessage, duration);

            // Mark as shown
            interactionHistory[interactId] = true;

            // Save to PlayerPrefs for persistence between sessions (optional)
            //PlayerPrefs.SetInt("Tooltip_" + interactId, 1);
            //PlayerPrefs.Save();
        }
    }

    // Call this when player equips an item for the first time
    public void HandleEquip(GameObject item, float duration = 5f)
    {
        string itemName = item.name;
        string interactMessage;

        switch (itemName)
        {
            case "Item_Basic rod":
                interactMessage = "Press space to fish!";
                break;
            case "Item_Pistol":
                interactMessage = "Press mouse button to shoot!";
                break;
            case "Item_Shotgun":
                interactMessage = "Press mouse button to shoot!";
                break;
            case "Item_Kalashnikov":
                interactMessage = "Press mouse button to shoot!";
                break;
            default:
                interactMessage = "You equipped " + itemName + "!";
                break;
        }

        // Show the equipment tooltip
        ShowTooltipIfFirstTime(itemName, interactMessage, duration);

        // Mark as equipped
        equippedItemHistory[itemName] = true;
    }

    // Load saved interaction history from PlayerPrefs (call this in Start)
    private void LoadInteractionHistory()
    {
        // This would load all saved tooltips and equipment statuses from PlayerPrefs
        // You would call this in Start() method
    }

    // Reset all tooltips (for debugging or new game)
    public void ResetAllTooltips()
    {
        interactionHistory.Clear();
        equippedItemHistory.Clear();

        // Clear PlayerPrefs entries related to tooltips and equipment
        // This is a simple implementation, you might want to be more specific
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    // Optional: add method to handle interaction with objects
    public void HandleInteraction(GameObject interactableObject)
    {
        string objectId = interactableObject.name;

        string interactMessage;

        switch (objectId)
        {
            case "Father":
                interactMessage = "Press E to talk to your father";
                break;
            case "FishingSpot(Clone)":
                interactMessage = "Press SPACE to fish";
                break;
            case "PlayerBoat":
                interactMessage = "Press E to control the boat";
                break;
            default:
                interactMessage = "Press E to interact with " + objectId;
                break;
        }

        // Show tooltip if this is the first interaction
        ShowTooltipIfFirstTime(objectId, interactMessage);

        // Call the object's interaction method
        interactableObject.SendMessage("Interact", SendMessageOptions.DontRequireReceiver);
    }

    // Optional: add method to call based on specific game events
    public void handleSpecificTooltip(string eventMessage)
    {
        // Show the event message
        UIManager.ShowInteractPanel(eventMessage, 10f);

        // Mark as handled
        // Save to PlayerPrefs for persistence between sessions (optional)
        //PlayerPrefs.SetInt("Event_" + eventId, 1);
        //PlayerPrefs.Save();
    }
}
