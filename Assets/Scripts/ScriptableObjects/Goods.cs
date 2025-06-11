using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Good", menuName = "Shop/Good", order = 51)]
public class Goods : ScriptableObject
{
    [SerializeField] private String _label;
    [SerializeField] private String _description;
    [SerializeField] private int _price;
    [SerializeField] private GameObject _prefab;
    
    public int Price => _price;
    public String Label => _label;
    public void ShowInfo()
    {
        Debug.Log(_label);
    }
}