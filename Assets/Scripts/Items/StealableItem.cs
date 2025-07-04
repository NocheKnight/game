// D:/unity/projects/Kradylechka/Assets/Scripts/Items/StealableItem.cs
using UnityEngine;

public class StealableItem : MonoBehaviour
{
    [Tooltip("Насколько подозрительной является кража этого предмета")]
    [SerializeField] private float _suspicionAmount = 30f;

    private bool _isStolen = false;

    // Этот метод должен вызываться, когда игрок успешно крадет предмет
    public void OnStolen()
    {
        if (_isStolen) return;
        _isStolen = true;

        Debug.Log($"Предмет {gameObject.name} был украден!");

        // 1. Создаем данные о событии
        var theftEvent = new SuspicionEvent(transform.position, _suspicionAmount, SuspicionType.Theft);

        // 2. Отправляем событие в общий канал. Все, кто слушает, получат его.
        SuspicionEvents.Raise(theftEvent);

        // 3. Уничтожаем или деактивируем предмет
        Destroy(gameObject);
    }

    // Пример, как это может быть вызвано
    private void OnMouseDown()
    {
        // Для теста: клик мыши симулирует кражу
        OnStolen();
    }
}