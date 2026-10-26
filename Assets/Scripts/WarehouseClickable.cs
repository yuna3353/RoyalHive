
using UnityEngine;
using UnityEngine.EventSystems;

public class WarehouseClickable : MonoBehaviour
{
    [SerializeField] private WarehouseUI warehouseUI;

    private void OnMouseDown()
    {
        // 마우스가 UI 위에 있으면 건물을 클릭하지 않음
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (warehouseUI == null)
        {
            Debug.LogWarning("WarehouseUI is not assigned.");
            return;
        }

        warehouseUI.OpenPanel();
    }
}
