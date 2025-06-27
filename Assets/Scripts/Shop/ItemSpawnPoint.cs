using UnityEngine;

public class ItemSpawnPoint : MonoBehaviour
{
    [SerializeField] private Goods _goods;
    public Goods Goods => _goods;
} 