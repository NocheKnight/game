using UnityEngine;

public class SimpleItemPrefab : MonoBehaviour
{
    [Header("Настройки товара")]
    [SerializeField] private string _itemName = "Товар";
    [SerializeField] private Color _itemColor = Color.white;
    [SerializeField] private Vector3 _itemSize = Vector3.one;
    
    private void Awake()
    {
        CreateSimpleMesh();
    }
    
    private void CreateSimpleMesh()
    {
        string lowerName = _itemName.ToLower();
        
        if (lowerName.Contains("хлеб") || lowerName.Contains("сыр"))
        {
            CreateCubeMesh();
        }
        else if (lowerName.Contains("молоко") || lowerName.Contains("бутылка"))
        {
            CreateCylinderMesh();
        }
        else if (lowerName.Contains("яйцо"))
        {
            CreateSphereMesh();
        }
        else
        {
            CreateCubeMesh();
        }
    }
    
    private void CreateCubeMesh()
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.SetParent(transform);
        cube.transform.localPosition = Vector3.zero;
        cube.transform.localScale = _itemSize;
        
        Renderer renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = _itemColor;
            renderer.material = material;
        }
        
        DestroyImmediate(cube.GetComponent<Collider>());
    }
    
    private void CreateCylinderMesh()
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.SetParent(transform);
        cylinder.transform.localPosition = Vector3.zero;
        cylinder.transform.localScale = _itemSize;
        
        Renderer renderer = cylinder.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = _itemColor;
            renderer.material = material;
        }
        
        DestroyImmediate(cylinder.GetComponent<Collider>());
    }
    
    private void CreateSphereMesh()
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.SetParent(transform);
        sphere.transform.localPosition = Vector3.zero;
        sphere.transform.localScale = _itemSize;
        
        Renderer renderer = sphere.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = _itemColor;
            renderer.material = material;
        }
        
        DestroyImmediate(sphere.GetComponent<Collider>());
    }
} 