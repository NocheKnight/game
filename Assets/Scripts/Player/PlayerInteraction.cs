using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerInteraction : MonoBehaviour
{
    [Header("Настройки взаимодействия")]
    [SerializeField] private float _interactionDistance = 3f;

    private Player _player;
    private Camera _mainCamera;
    private StorableItem _highlightedItem;

    private void Start()
    {
        _player = GetComponent<Player>();
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        HandleHighlighting();
    }

    private void HandleHighlighting()
    {
        Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
        StorableItem newHitItem = null;

        if (Physics.Raycast(ray, out RaycastHit hit, _interactionDistance))
        {
            hit.collider.TryGetComponent<StorableItem>(out newHitItem);
        }

        if (newHitItem != _highlightedItem)
        {
            _highlightedItem?.Unhighlight();
            _highlightedItem = newHitItem;
            _highlightedItem?.Highlight();
        }

        // Если подсвеченный предмет уничтожен, сбрасываем ссылку
        if (_highlightedItem == null)
        {
            _highlightedItem = null;
        }
    }

    public void TryInteract()
    {
        if (_highlightedItem != null)
        {
            Shelf shelf = _highlightedItem.ParentShelf;
            if (shelf != null)
            {
                bool wasStolen = shelf.TryStealItem(_highlightedItem, _player);
                if (wasStolen)
                {
                    _highlightedItem = null;
                }
            }
        }
    }
} 