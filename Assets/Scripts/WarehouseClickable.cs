using UnityEngine;

public class WarehouseClickable : MonoBehaviour
{
    [SerializeField]
    private WarehouseUI warehouseUI;

    private void OnMouseDown()
    {
        if (warehouseUI == null)
        {
            Debug.LogWarning(
                "WarehouseUI is not assigned."
            );

            return;
        }

        warehouseUI.OpenPanel();
    }
}