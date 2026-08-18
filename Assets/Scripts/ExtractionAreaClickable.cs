using UnityEngine;

public class ExtractionAreaClickable : MonoBehaviour
{
    [SerializeField]
    private ExtractionManager extractionManager;

    private void OnMouseDown()
    {
        if (extractionManager == null)
        {
            Debug.LogWarning(
                "ExtractionManager is not assigned."
            );

            return;
        }

        extractionManager.OpenPanel();
    }
}