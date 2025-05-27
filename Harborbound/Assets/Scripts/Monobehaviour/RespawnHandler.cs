using UnityEngine;
using UnityEngine.UI;

public class RespawnHandler : MonoBehaviour
{
    [SerializeField] private Button restartButton;
    
    void Start()
    {
        // Add listener to the button
        restartButton.onClick.AddListener(OnRestartButtonClicked);
    }
    
    // This function runs when the button is clicked
    void OnRestartButtonClicked()
    {
        Debug.Log("Restart button clicked!");
        GameManager.Instance.RespawnPlayer();
        UIManager.Instance.ShowDeathPanel(false);
    }
}
