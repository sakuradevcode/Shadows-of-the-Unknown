using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    // Loads the main game scene
    public void StartGame()
    {
        // Make sure to replace "Game" with your actual game scene name
        SceneManager.LoadScene("Game"); 
    }

    // Returns to the main menu (useful for Game Over or Victory screens)
    public void LoadMainMenu()
    {
        // Make sure to replace "MainMenu" with your actual menu scene name
        SceneManager.LoadScene("MainMenu"); 
    }

    // Quits the application (works in the final build, not in the editor)
    public void QuitGame()
    {
        Debug.Log("The player has quit the game.");
        Application.Quit();
    }
}