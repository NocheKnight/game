using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Player))]
public class GameResults : MonoBehaviour
{
    private PlayerInventory _inventory;
    private Player _player;
    private static string resultTemplate = "{0}: {1} цена товара\n";

    void Awake()
    {
        _inventory = GetComponent<PlayerInventory>();
        _player = GetComponent<Player>();
    }

    void Start() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UpdateGame();
    }

    void UpdateGame()
    {
        string results = "";
        int resultMoney = 0;

        foreach (Goods good in _inventory.Goods)
        {
            results += string.Format(resultTemplate, good.Label, good.Price);
            resultMoney += good.Price;
        }

        results += "\n\nЧистая прибыль: " + resultMoney.ToString();

        GameObject.Find("Results").GetComponent<TMPro.TextMeshProUGUI>().text = results;

        _player.AddMoney(resultMoney);
        _inventory.Goods.Clear();
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}