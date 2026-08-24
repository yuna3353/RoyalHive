using UnityEngine;

[RequireComponent(typeof(HiveController))]
[RequireComponent(typeof(Collider2D))]
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
            FindAnyObjectByType<HiveInfoUI>(
                FindObjectsInactive.Include
            );

        if (hiveInfoUI == null)
        {
            Debug.LogWarning(
                "HiveInfoUI was not found.",
                gameObject
            );

            return;
        }

        hiveInfoUI.OpenHive(
            hive
        );
    }
}