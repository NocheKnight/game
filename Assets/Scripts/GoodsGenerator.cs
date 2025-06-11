using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoodsGenerator : MonoBehaviour
{
    [SerializeField] private List<Goods> _goods;
    private void Start()
    {
        foreach (var good in _goods)
        {
            good.ShowInfo();
        }
    }

}
