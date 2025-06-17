using UnityEngine;

public class ShelfItemInteraction : MonoBehaviour
{
    [Header("Настройки взаимодействия")]
    [SerializeField] private float _interactionRange = 2f;
    [SerializeField] private bool _showOutline = true;
    [SerializeField] private Color _outlineColor = Color.yellow;
    
    private ShelfItemVisual _shelfItem;
    private Renderer _renderer;
    private Material _originalMaterial;
    private Material _outlineMaterial;
    private bool _isPlayerInRange = false;
    
    public void Initialize(ShelfItemVisual shelfItem)
    {
        _shelfItem = shelfItem;
        _renderer = GetComponent<Renderer>();
        
        if (_renderer != null)
        {
            _originalMaterial = _renderer.material;
            CreateOutlineMaterial();
        }
    }
    
    private void CreateOutlineMaterial()
    {
        if (_outlineMaterial == null)
        {
            _outlineMaterial = new Material(_originalMaterial);
            _outlineMaterial.color = _outlineColor;
        }
    }
    
    private void Update()
    {
        CheckPlayerDistance();
        HandleInput();
    }
    
    private void CheckPlayerDistance()
    {
        Player player = FindObjectOfType<Player>();
        if (player == null) return;
        
        float distance = Vector3.Distance(transform.position, player.transform.position);
        bool wasInRange = _isPlayerInRange;
        _isPlayerInRange = distance <= _interactionRange;
        
        if (wasInRange != _isPlayerInRange)
        {
            UpdateVisuals();
        }
    }
    
    private void HandleInput()
    {
        if (!_isPlayerInRange || _shelfItem == null || _shelfItem.IsEmpty) return;
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryPickupItem();
        }
    }
    
    private void TryPickupItem()
    {
        if (_shelfItem == null || _shelfItem.IsEmpty || _shelfItem.IsBeingPickedUp) return;
        
        Player player = FindObjectOfType<Player>();
        if (player == null) return;
        
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        if (inventory == null) return;
        
        Goods goods = _shelfItem.Goods;
        if (goods == null) return;
        
        if (!inventory.CanAddItem(goods))
        {
            Debug.Log("Инвентарь полон или перегружен!");
            return;
        }
        
        if (!CheckStealingSuccess(goods, player))
        {
            Debug.Log($"Не удалось украсть {goods.Label}! Слишком сложно.");
            player.AddCrimeRate(10);
            return;
        }
        
        inventory.TryAddItem(goods);
        player.AddCrimeRate(goods.StealingDifficulty * 5);
        
        _shelfItem.PickupItem();
        
        Debug.Log($"Украден товар: {goods.Label}!");
    }
    
    private bool CheckStealingSuccess(Goods item, Player player)
    {
        float baseChance = 1f - (item.StealingDifficulty * 0.1f);
        
        baseChance += player.GetStealthBonus();
        baseChance += player.GetPickpocketChance() * 0.5f;
        
        baseChance -= item.GetStealthPenalty();
        
        baseChance = Mathf.Clamp01(baseChance);
        
        return Random.value <= baseChance;
    }
    
    private void UpdateVisuals()
    {
        if (_renderer == null || !_showOutline) return;
        
        if (_isPlayerInRange && !_shelfItem.IsEmpty)
        {
            _renderer.material = _outlineMaterial;
        }
        else
        {
            _renderer.material = _originalMaterial;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _interactionRange);
    }
    
    private void OnDestroy()
    {
        if (_outlineMaterial != null)
        {
            DestroyImmediate(_outlineMaterial);
        }
    }
} 