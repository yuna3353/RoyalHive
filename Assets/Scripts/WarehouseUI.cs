using System.Text;
using TMPro;
using UnityEngine;

public class WarehouseUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private WarehouseController warehouse;

    [SerializeField]
    private GameObject panel;

    [Header("UI")]
    [SerializeField]
    private TMP_Text capacityText;

    [SerializeField]
    private TMP_Text honeyListText;

    private void Awake()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private void Update()
    {
        if (panel != null &&
            panel.activeSelf)
        {
            RefreshUI();
        }
    }

    public void OpenPanel()
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }

        RefreshUI();
    }

    public void ClosePanel()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private void RefreshUI()
    {
        if (warehouse == null)
        {
            return;
        }

        if (capacityText != null)
        {
            capacityText.text =
                $"Storage: " +
                $"{warehouse.CurrentAmount} / " +
                $"{warehouse.MaxCapacity} H";
        }

        if (honeyListText == null)
        {
            return;
        }

        if (warehouse.StoredHoney.Count == 0)
        {
            honeyListText.text =
                "No honey stored.";

            return;
        }

        StringBuilder builder =
            new();

        foreach (
            var entry
            in warehouse.StoredHoney)
        {
            builder.AppendLine(
                $"{entry.Key}: {entry.Value} H"
            );
        }

        honeyListText.text =
            builder.ToString();
    }
}