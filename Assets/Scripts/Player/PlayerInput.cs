using UnityEngine;

[RequireComponent(typeof(PlayerMover))]
[RequireComponent(typeof(PlayerInteraction))]
[RequireComponent(typeof(PlayerDistractions))]
public class PlayerInput : MonoBehaviour
{
    [Header("Назначение клавиш")]
    [SerializeField] private KeyCode _interactKey = KeyCode.E;
    [SerializeField] private KeyCode _throwMoneyKey = KeyCode.G;
    [SerializeField] private KeyCode _shoutKey = KeyCode.Q;
    [SerializeField] private KeyCode _runKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode _stealthKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode _cursorKey = KeyCode.Escape;

    private PlayerMover _mover;
    private PlayerInteraction _interaction;
    private PlayerDistractions _distractions;

    private void Awake()
    {
        _mover = GetComponent<PlayerMover>();
        _interaction = GetComponent<PlayerInteraction>();
        _distractions = GetComponent<PlayerDistractions>();
    }

    private void Update()
    {
        // Управление движением
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        _mover.SetMoveDirection(new Vector2(horizontal, vertical));

        if (Input.GetKeyDown(_runKey))
        {
            _mover.ToggleRun();
        }

        if (Input.GetKeyDown(_stealthKey))
        {
            _mover.ToggleStealthMode();
        }

        // Управление взаимодействием
        if (Input.GetKeyDown(_interactKey))
        {
            _interaction.TryInteract();
        }

        // Управление отвлечениями
        if (Input.GetKeyDown(_throwMoneyKey))
        {
            _distractions.ThrowMoney();
        }

        if (Input.GetKeyDown(_shoutKey))
        {
            _distractions.ShoutAboutSale();
        }
        
        // Управление курсором
        if (Input.GetKeyDown(_cursorKey))
        {
            ToggleCursorLock();
        }
    }
    
    private void FixedUpdate()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        _mover.HandleMouseLook(mouseX, mouseY);
    }

    private void ToggleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
} 