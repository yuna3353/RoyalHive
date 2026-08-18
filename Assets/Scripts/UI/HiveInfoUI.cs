using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HiveInfoUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField]
    private GameObject panel;

    [Header("Texts")]
    [SerializeField]
    private TMP_Text titleText;

    [SerializeField]
    private TMP_Text honeyText;

    [SerializeField]
    private TMP_Text honeyTypeText;

    [SerializeField]
    private TMP_Text beeCountText;

    [SerializeField]
    private TMP_Text harvestStatusText;

    [Header("Honey Gauge")]
    [SerializeField]
    private Image honeyFill;

    [Header("Extraction")]
    [SerializeField]
    private ExtractionManager extractionManager;

    private HiveController selectedHive;
    private BeeSpawner selectedSpawner;

    private void Awake()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private void Update()
    {
        if (selectedHive == null)
        {
            return;
        }

        RefreshUI();
    }

    public void OpenHive(
        HiveController hive)
    {
        if (hive == null)
        {
            return;
        }

        selectedHive =
            hive;

        selectedSpawner =
            hive.GetComponent<BeeSpawner>();

        if (panel != null)
        {
            panel.SetActive(true);
        }

        RefreshUI();
    }

    public void StartHarvest()
    {
        if (selectedHive == null)
        {
            return;
        }

        if (!selectedHive.CanHarvest)
        {
            Debug.Log(
                "Selected hive is not ready for harvest."
            );

            return;
        }

        if (extractionManager == null)
        {
            Debug.LogWarning(
                "ExtractionManager is not assigned."
            );

            return;
        }

        extractionManager.StartExtraction(
            selectedHive
        );

        Close();
    }

    public void Close()
    {
        selectedHive = null;
        selectedSpawner = null;

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private void RefreshUI()
    {
        if (selectedHive == null)
        {
            return;
        }

        if (titleText != null)
        {
            titleText.text =
                selectedHive.HiveName;
        }

        if (honeyText != null)
        {
            honeyText.text =
                $"Honey: " +
                $"{selectedHive.CurrentHoney} / " +
                $"{selectedHive.MaxHoney} H";
        }

        UpdateHoneyTypeText();

        if (beeCountText != null)
        {
            if (selectedSpawner != null)
            {
                beeCountText.text =
                    $"Bees: " +
                    $"{selectedSpawner.CurrentBeeCount} / " +
                    $"{selectedSpawner.MaxBeeCount}";
            }
            else
            {
                beeCountText.text =
                    "Bees: -";
            }
        }

        if (harvestStatusText != null)
        {
            harvestStatusText.text =
                selectedHive.CanHarvest
                    ? "Harvest: READY"
                    : "Harvest: Not Ready";
        }

        if (honeyFill != null)
        {
            honeyFill.fillAmount =
                selectedHive.HoneyRatio;
        }
    }

    private void UpdateHoneyTypeText()
    {
        if (honeyTypeText == null ||
            selectedHive == null)
        {
            return;
        }

        if (selectedHive.HoneyByType.Count == 0)
        {
            honeyTypeText.text =
                "No honey collected.";

            return;
        }

        StringBuilder builder =
            new();

        foreach (
            var honeyEntry
            in selectedHive.HoneyByType)
        {
            builder.AppendLine(
                $"{honeyEntry.Key}: " +
                $"{honeyEntry.Value} H"
            );
        }

        honeyTypeText.text =
            builder.ToString();
    }
}