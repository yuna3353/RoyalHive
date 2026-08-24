using UnityEngine;
using UnityEngine.EventSystems;

public class MerchantClickable : MonoBehaviour
{
    [SerializeField]
    private MerchantManager merchantManager;

    private void OnMouseDown()
    {
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (merchantManager == null)
        {
            Debug.LogWarning(
                "MerchantManager is not assigned."
            );

            return;
        }

        merchantManager.OpenPanel();
    }
}