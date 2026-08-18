using System;
using TMPro;
using UnityEngine;

public class MarketManager : MonoBehaviour
{
    [Serializable]
    public class BillOption
    {
        public string honeyType;

        [Min(1)]
        public int requiredAmount = 10;

        [Min(1)]
        public int rewardGold = 100;
    }

    [Header("References")]
    [SerializeField]
    private WarehouseController warehouse;

    [SerializeField]
    private BuildManager buildManager;

    [SerializeField]
    private GameObject marketPanel;

    [Header("Bills")]
    [SerializeField]
    private BillOption[] billOptions;

    [Header("UI")]
    [SerializeField]
    private TMP_Text billText;

    [SerializeField]
    private TMP_Text warehouseAmountText;

    [SerializeField]
    private TMP_Text rewardText;

    [SerializeField]
    private TMP_Text statusText;

    private BillOption currentBill;

    private void Awake()
    {
        if (marketPanel != null)
        {
            marketPanel.SetActive(false);
        }

        GenerateNewBill();
    }

    private void Update()
    {
        if (marketPanel != null &&
            marketPanel.activeSelf)
        {
            RefreshUI();
        }
    }

    public void OpenPanel()
    {
        if (marketPanel != null)
        {
            marketPanel.SetActive(true);
        }

        RefreshUI();
    }

    public void ClosePanel()
    {
        if (marketPanel != null)
        {
            marketPanel.SetActive(false);
        }
    }

    public void GenerateNewBill()
    {
        if (billOptions == null ||
            billOptions.Length == 0)
        {
            currentBill = null;

            return;
        }

        int index =
            UnityEngine.Random.Range(
                0,
                billOptions.Length
            );

        currentBill =
            billOptions[index];

        if (statusText != null)
        {
            statusText.text =
                "New request received.";
        }

        RefreshUI();
    }

    public void SubmitBill()
    {
        if (currentBill == null ||
            warehouse == null ||
            buildManager == null)
        {
            return;
        }

        int availableHoney =
            warehouse.GetHoneyAmount(
                currentBill.honeyType
            );

        if (availableHoney <
            currentBill.requiredAmount)
        {
            if (statusText != null)
            {
                statusText.text =
                    "Not enough honey.";
            }

            return;
        }

        int removed =
            warehouse.RemoveHoney(
                currentBill.honeyType,
                currentBill.requiredAmount
            );

        if (removed <
            currentBill.requiredAmount)
        {
            return;
        }

        buildManager.AddMoney(
            currentBill.rewardGold
        );

        if (statusText != null)
        {
            statusText.text =
                $"Sold {removed}H for " +
                $"{currentBill.rewardGold}G!";
        }

        GenerateNewBill();
    }

    private void RefreshUI()
    {
        if (currentBill == null)
        {
            if (billText != null)
            {
                billText.text =
                    "No active request.";
            }

            return;
        }

        if (billText != null)
        {
            billText.text =
                $"{currentBill.honeyType}\n" +
                $"Required: " +
                $"{currentBill.requiredAmount} H";
        }

        if (warehouseAmountText != null &&
            warehouse != null)
        {
            int amount =
                warehouse.GetHoneyAmount(
                    currentBill.honeyType
                );

            warehouseAmountText.text =
                $"Warehouse: {amount} H";
        }

        if (rewardText != null)
        {
            rewardText.text =
                $"Reward: " +
                $"{currentBill.rewardGold} G";
        }
    }
}