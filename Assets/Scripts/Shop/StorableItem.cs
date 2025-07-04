using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class StorableItem : MonoBehaviour
{
    private Goods _goods;
    private Shelf _parentShelf;

    private Renderer _renderer;
    private Material _originalMaterial;
    private Material _highlightMaterial;

    public Goods Goods => _goods;
    public Shelf ParentShelf => _parentShelf;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _originalMaterial = _renderer.material;
    }

    public void Initialize(Goods goods, Shelf parentShelf, Material highlightMaterial)
    {
        _goods = goods;
        _parentShelf = parentShelf;
        _highlightMaterial = highlightMaterial;
    }

    public void Highlight()
    {
        if (_highlightMaterial != null)
        {
            _renderer.material = _highlightMaterial;
        }
    }

    public void Unhighlight()
    {
        if (this == null || _renderer == null) return;
        _renderer.material = _originalMaterial;
    }
    public void OnStolen()
    {
        if (_goods == null) return;

        // Количество подозрений зависит от сложности кражи и цены товара
        float suspicionAmount = 15f + (_goods.Price / 5f) + (_goods.StealingDifficulty * 5f);

        var theftEvent = new SuspicionEvent(transform.position, suspicionAmount, SuspicionType.Theft);
        
        // Отправляем событие в "эфир"
        SuspicionEvents.Raise(theftEvent);
        Debug.Log($"Событие кражи {_goods.Label} создано в точке {transform.position} с силой {suspicionAmount}");
    }
}