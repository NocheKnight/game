using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    void Awake() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void onDestroy() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}