using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Player))]
public class Upgrades : MonoBehaviour
{
    private Player _player;

    void Awake() {
        _player = GetComponent<Player>();
        _player.AddMoney(1000);
    }

    void Start() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UpdateUI();
    }

    void UpdateUI() {
        GameObject.Find("Money").GetComponent<TMPro.TextMeshProUGUI>().text = _player.Money.ToString();

        GameObject.Find("StealthLevel").GetComponent<TMPro.TextMeshProUGUI>().text = _player.StealthLevel.ToString();
        GameObject.Find("StealthCost").GetComponent<TMPro.TextMeshProUGUI>().text = _player.GetStealthUpdateCost().ToString();

        GameObject.Find("PickpocketLevel").GetComponent<TMPro.TextMeshProUGUI>().text = _player.PickpocketLevel.ToString();
        GameObject.Find("PickpocketCost").GetComponent<TMPro.TextMeshProUGUI>().text = _player.GetPickpocketUpdateCost().ToString();

        GameObject.Find("DistractionLevel").GetComponent<TMPro.TextMeshProUGUI>().text = _player.DistractionLevel.ToString();
        GameObject.Find("DistractionCost").GetComponent<TMPro.TextMeshProUGUI>().text = _player.GetDistractionUpdateCost().ToString();
    }

    public void BuyUpgrade(string name) {
        if (name == "Stealth") _player.TryUpgradeStealth();
        else if (name == "Pickpocket") _player.TryUpgradePickpocket();
        else if (name == "Distraction") _player.TryUpgradeDistraction();

        UpdateUI();
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}