using UnityEngine;

[RequireComponent(typeof(HiveController))]
public class HiveClickable : MonoBehaviour
{
    private HiveController hive;

    private void Awake()
    {
        hive =
            GetComponent<HiveController>();
    }

    private void OnMouseDown()
    {
        HiveInfoUI hiveInfoUI =
            FindFirstObjectByType<HiveInfoUI>();

        if (hiveInfoUI == null)
        {
            Debug.LogWarning(
                "HiveInfoUI was not found."
            );

            return;
        }

        hiveInfoUI.OpenHive(hive);
    }
}