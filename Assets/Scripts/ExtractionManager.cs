using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ExtractionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private WarehouseController warehouse;

    [SerializeField]
    private GameObject extractionPanel;

    [Header("UI")]
    [SerializeField]
    private TMP_Text hiveNameText;

    [SerializeField]
    private TMP_Text honeySummaryText;

    [SerializeField]
    private TMP_Text statusText;

    private HiveController selectedHive;

    private void Awake()
    {
        if (extractionPanel != null)
        {
            extractionPanel.SetActive(false);
        }
    }

    public void OpenPanel()
    {
        if (extractionPanel != null)
        {
            extractionPanel.SetActive(true);
        }

        RefreshUI();
    }

    public void StartExtraction(
        HiveController hive)
    {
        if (hive == null)
        {
            return;
        }

        if (!hive.CanHarvest)
        {
            Debug.Log(
                $"{hive.HiveName}: Harvest is not ready."
            );

            return;
        }

        selectedHive =
            hive;

        OpenPanel();
    }

    public void ExtractHoney()
    {
        if (selectedHive == null)
        {
            if (statusText != null)
            {
                statusText.text =
                    "Select a ready hive first.";
            }

            return;
        }

        if (warehouse == null)
        {
            Debug.LogWarning(
                "WarehouseController is not assigned."
            );

            return;
        }

        Dictionary<string, int> honeyCopy =
            new(selectedHive.HoneyByType);

        int totalMoved = 0;

        foreach (var entry in honeyCopy)
        {
            int movedAmount =
                warehouse.AddHoney(
                    entry.Key,
                    entry.Value
                );

            if (movedAmount <= 0)
            {
                continue;
            }

            selectedHive.RemoveHoney(
                entry.Key,
                movedAmount
            );

            totalMoved +=
                movedAmount;
        }

        if (totalMoved > 0)
        {
            Debug.Log(
                $"Extraction complete: {totalMoved}H moved to warehouse."
            );
        }

        if (selectedHive.CurrentHoney <= 0)
        {
            selectedHive = null;
        }

        RefreshUI();
    }

    public void ClosePanel()
    {
        if (extractionPanel != null)
        {
            extractionPanel.SetActive(false);
        }
    }

    private void RefreshUI()
    {
        if (selectedHive == null)
        {
            if (hiveNameText != null)
            {
                hiveNameText.text =
                    "No Hive Selected";
            }

            if (honeySummaryText != null)
            {
                honeySummaryText.text =
                    "Use Start Harvest on a ready hive.";
            }

            if (statusText != null)
            {
                statusText.text =
                    "Waiting for hive...";
            }

            return;
        }

        if (hiveNameText != null)
        {
            hiveNameText.text =
                selectedHive.HiveName;
        }

        if (honeySummaryText != null)
        {
            string summary = "";

            foreach (
                var entry
                in selectedHive.HoneyByType)
            {
                summary +=
                    $"{entry.Key}: {entry.Value} H\n";
            }

            honeySummaryText.text =
                summary;
        }

        if (statusText != null)
        {
            statusText.text =
                $"Total: {selectedHive.CurrentHoney} H";
        }
    }
}