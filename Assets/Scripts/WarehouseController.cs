using System.Collections.Generic;
using UnityEngine;

public class WarehouseController : MonoBehaviour
{
    [Header("Warehouse")]
    [Min(1)]
    [SerializeField]
    private int maxCapacity = 5000;

    private readonly Dictionary<string, int> storedHoney =
        new();

    public int MaxCapacity =>
        maxCapacity;

    public int CurrentAmount
    {
        get
        {
            int total = 0;

            foreach (var entry in storedHoney)
            {
                total += entry.Value;
            }

            return total;
        }
    }

    public float StorageRatio =>
        maxCapacity <= 0
            ? 0f
            : (float)CurrentAmount / maxCapacity;

    public IReadOnlyDictionary<string, int> StoredHoney =>
        storedHoney;

    public int AddHoney(
        string honeyType,
        int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        if (string.IsNullOrWhiteSpace(honeyType))
        {
            honeyType =
                "Unknown Honey";
        }

        int availableSpace =
            maxCapacity - CurrentAmount;

        if (availableSpace <= 0)
        {
            return 0;
        }

        int storedAmount =
            Mathf.Min(
                amount,
                availableSpace
            );

        if (!storedHoney.ContainsKey(honeyType))
        {
            storedHoney.Add(
                honeyType,
                0
            );
        }

        storedHoney[honeyType] +=
            storedAmount;

        Debug.Log(
            $"Warehouse: Stored {storedAmount}H of {honeyType}. " +
            $"Total: {CurrentAmount}/{maxCapacity}H"
        );

        return storedAmount;
    }

    public int GetHoneyAmount(
        string honeyType)
    {
        if (string.IsNullOrWhiteSpace(honeyType))
        {
            return 0;
        }

        if (!storedHoney.TryGetValue(
                honeyType,
                out int amount))
        {
            return 0;
        }

        return amount;
    }

    public int RemoveHoney(
        string honeyType,
        int amount)
    {
        if (amount <= 0 ||
            string.IsNullOrWhiteSpace(honeyType))
        {
            return 0;
        }

        if (!storedHoney.TryGetValue(
                honeyType,
                out int storedAmount))
        {
            return 0;
        }

        int removedAmount =
            Mathf.Min(
                amount,
                storedAmount
            );

        storedHoney[honeyType] -=
            removedAmount;

        if (storedHoney[honeyType] <= 0)
        {
            storedHoney.Remove(
                honeyType
            );
        }

        return removedAmount;
    }
}