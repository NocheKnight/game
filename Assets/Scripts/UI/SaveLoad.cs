using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;

[RequireComponent(typeof(Player))]
public class SaveLoad : MonoBehaviour
{
    private Player _player;

    void Awake() {
        _player = GetComponent<Player>();
    }

    void Start()
    {
        UpdateUI();
    }

    void UpdateUI() {
        for (int i = 0; i < 3; ++i)
        {
            UpdateSave(i);
        }
    }

    private void UpdateSave(int slotIdx)
    {
        PlayerSave save = LoadSaveSlot(slotIdx);
        GameObject.Find(string.Format("Save{0} Last Update", slotIdx + 1)).GetComponent<TMPro.TextMeshProUGUI>().text =
        "Last update: " + ((DateTime)save.lastUpdateTime).ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        GameObject.Find(string.Format("Save{0} Level", slotIdx + 1)).GetComponent<TMPro.TextMeshProUGUI>().text =
        "Stealth level: " + save.stealthLevel.ToString();
    }

    public void Save(int slotIdx)
    {
        string jsonSave = JsonUtility.ToJson(_player.ToPlayerSave());
        var writer = new StreamWriter("Assets/Resources/Saves/save" + (slotIdx + 1).ToString() + ".json");
        writer.WriteLine(jsonSave);
        writer.Close();

        UpdateSave(slotIdx);
    }

    private PlayerSave LoadSaveSlot(int slotIdx)
    {
        var reader = new StreamReader("Assets/Resources/Saves/save" + (slotIdx + 1).ToString() + ".json");
        var save = JsonUtility.FromJson<PlayerSave>(reader.ReadLine());
        reader.Close();

        return save;
    }

    public void Load(int slotIdx)
    {
        _player.FromPlayerSave(LoadSaveSlot(slotIdx));

        GoToMenu();
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}