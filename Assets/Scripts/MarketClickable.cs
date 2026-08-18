using UnityEngine;

public class MarketClickable : MonoBehaviour
{
    [SerializeField]
    private MarketManager marketManager;

    private void OnMouseDown()
    {
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