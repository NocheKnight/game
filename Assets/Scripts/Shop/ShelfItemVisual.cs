using UnityEngine;

public class ShelfItemVisual : MonoBehaviour
{
    [Header("Настройки визуального товара")]
    [SerializeField] private Goods _goods;
    [SerializeField] private GameObject _itemModel;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private Vector3 _spawnOffset = Vector3.zero;
    [SerializeField] private float _spawnDelay = 0.1f;
    
    [Header("Анимация")]
    [SerializeField] private float _pickupAnimationDuration = 0.5f;
    [SerializeField] private AnimationCurve _pickupCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private GameObject _currentItem;
    private bool _isBeingPickedUp = false;
    
    public Goods Goods => _goods;
    public bool IsEmpty => _currentItem == null;
    public bool IsBeingPickedUp => _isBeingPickedUp;
    
    public void Initialize(Goods goods)
    {
        _goods = goods;
        SpawnItem();
    }
    
    public void SpawnItem()
    {
        if (_goods == null || _goods.Prefab == null) return;
        
        Vector3 spawnPosition = _spawnPoint != null ? _spawnPoint.position : transform.position;
        spawnPosition += _spawnOffset;
        
        _currentItem = Instantiate(_goods.Prefab, spawnPosition, Quaternion.identity, transform);
        
        var itemInteraction = _currentItem.AddComponent<ShelfItemInteraction>();
        itemInteraction.Initialize(this);
        
        Debug.Log($"Создан визуальный товар: {_goods.Label} на позиции {spawnPosition}");
    }
    
    public void PickupItem()
    {
        if (_currentItem == null || _isBeingPickedUp) return;
        
        _isBeingPickedUp = true;
        StartCoroutine(PickupAnimation());
    }
    
    private System.Collections.IEnumerator PickupAnimation()
    {
        if (_currentItem == null) yield break;
        
        Vector3 startPosition = _currentItem.transform.position;
        Vector3 endPosition = startPosition + Vector3.up * 2f;
        float elapsed = 0f;
        
        while (elapsed < _pickupAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / _pickupAnimationDuration;
            float curveValue = _pickupCurve.Evaluate(progress);
            
            _currentItem.transform.position = Vector3.Lerp(startPosition, endPosition, curveValue);
            _currentItem.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, curveValue);
            
            yield return null;
        }
        
        if (_currentItem != null)
        {
            Destroy(_currentItem);
            _currentItem = null;
        }
        
        _isBeingPickedUp = false;
    }
    
    public void RemoveItem()
    {
        if (_currentItem != null)
        {
            Destroy(_currentItem);
            _currentItem = null;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Vector3 spawnPos = _spawnPoint != null ? _spawnPoint.position : transform.position;
        spawnPos += _spawnOffset;
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(spawnPos, 0.1f);
        Gizmos.DrawLine(transform.position, spawnPos);
    }
} 