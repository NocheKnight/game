using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Player))]
public class GameOver : MonoBehaviour
{
    private PlayerInventory _inventory;

    void Awake()
    {
        _inventory = GetComponent<PlayerInventory>();
    }

    void Start() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UpdateGame();
    }

    void UpdateGame()
    {
        _inventory.Goods.Clear();
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}