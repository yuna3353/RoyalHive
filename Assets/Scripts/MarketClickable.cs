
using UnityEngine;
using UnityEngine.EventSystems;

public class MarketClickable : MonoBehaviour
{
    [SerializeField]
    private MarketManager marketManager;

    private void OnMouseDown()
    {
        // UI 위를 클릭했다면 마켓을 열지 않음
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (marketManager == null)
        {
            Debug.LogWarning(
                "MarketManager is not assigned."
            );

            return;
        }

        marketManager.OpenPanel();
    }
}
