using System;
using TMPro;
using UnityEngine;

public class MarketManager : MonoBehaviour
{
    [Serializable]
    public class MarketItem
    {
        [Header("Honey Type")]
        public string honeyType;

        [Header("Price Per 1 Honey")]
        [Min(1)]
        public int pricePerHoney = 10;
    }

    [Header("References")]
    [SerializeField]
    private WarehouseController warehouse;

    [SerializeField]
    private BuildManager buildManager;

    [SerializeField]
    private GameObject marketPanel;

    [Header("Market Items")]
    [SerializeField]
    private MarketItem[] marketItems;

    [Header("Selected Item UI")]
    [SerializeField]
    private TMP_Text honeyTypeText;

    [SerializeField]
    private TMP_Text warehouseAmountText;

    [SerializeField]
    private TMP_Text priceText;

    [SerializeField]
    private TMP_Text statusText;

    private int selectedItemIndex = 0;

    private MarketItem CurrentItem
    {
        get
        {
            if (marketItems == null ||
                marketItems.Length == 0 ||
                selectedItemIndex < 0 ||
                selectedItemIndex >= marketItems.Length)
            {
                return null;
            }

            return marketItems[selectedItemIndex];
        }
    }

    private void Awake()
    {
        if (marketPanel != null)
        {
            marketPanel.SetActive(false);
        }
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

    public void SelectMarketItem(int index)
    {
        if (marketItems == null ||
            index < 0 ||
            index >= marketItems.Length)
        {
            Debug.LogWarning(
                $"Invalid Market Item Index: {index}"
            );

            return;
        }

        selectedItemIndex = index;

        if (statusText != null)
        {
            statusText.text = "";
        }

        RefreshUI();
    }

    public void Sell10()
    {
        SellHoney(10);
    }

    public void SellAll()
    {
        MarketItem item = CurrentItem;

        if (item == null ||
            warehouse == null)
        {
            return;
        }

        int available =
            warehouse.GetHoneyAmount(
                item.honeyType
            );

        if (available <= 0)
        {
            SetStatus("No honey to sell.");
            return;
        }

        SellHoney(available);
    }

    private void SellHoney(int requestedAmount)
    {
        MarketItem item = CurrentItem;

        if (item == null)
        {
            SetStatus(
                "No market item selected."
            );

            return;
        }

        if (warehouse == null)
        {
            Debug.LogWarning(
                "WarehouseController is not assigned."
            );

            return;
        }

        if (buildManager == null)
        {
            Debug.LogWarning(
                "BuildManager is not assigned."
            );

            return;
        }

        int available =
            warehouse.GetHoneyAmount(
                item.honeyType
            );

        if (available <= 0)
        {
            SetStatus(
                $"No {item.honeyType} in warehouse."
            );

            return;
        }

        int amountToSell =
            Mathf.Min(
                requestedAmount,
                available
            );

        int removed =
            warehouse.RemoveHoney(
                item.honeyType,
                amountToSell
            );

        if (removed <= 0)
        {
            SetStatus(
                "Sale failed."
            );

            return;
        }

        int earnedGold =
            removed *
            item.pricePerHoney;

        buildManager.AddMoney(
            earnedGold
        );

        SetStatus(
            $"Sold {removed}H for {earnedGold}G!"
        );

        RefreshUI();
    }

    private void RefreshUI()
    {
        MarketItem item = CurrentItem;

        if (item == null)
        {
            if (honeyTypeText != null)
            {
                honeyTypeText.text =
                    "No Market Item";
            }

            if (warehouseAmountText != null)
            {
                warehouseAmountText.text =
                    "Warehouse: -";
            }

            if (priceText != null)
            {
                priceText.text =
                    "Price: -";
            }

            return;
        }

        if (honeyTypeText != null)
        {
            honeyTypeText.text =
                item.honeyType;
        }

        if (warehouseAmountText != null)
        {
            int amount = 0;

            if (warehouse != null)
            {
                amount =
                    warehouse.GetHoneyAmount(
                        item.honeyType
                    );
            }

            warehouseAmountText.text =
                $"Warehouse: {amount} H";
        }

        if (priceText != null)
        {
            priceText.text =
                $"Price: {item.pricePerHoney}G / H";
        }
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }

        Debug.Log(message);
    }
}