using System.Collections.Generic;
using UnityEngine;

public class HiveController : MonoBehaviour
{
    [Header("Hive Information")]
    [SerializeField]
    private string hiveName = "Basic Hive";

    [Header("Honey Storage")]
    [Min(1)]
    [SerializeField]
    private int maxHoney = 800;

    [Min(0)]
    [SerializeField]
    private int currentHoney;

    [Header("Harvest")]
    [Range(0f, 1f)]
    [SerializeField]
    private float harvestThreshold = 0.8f;

    [Header("Forage Selection")]
    [Range(0f, 1f)]
    [SerializeField]
    private float smartChoiceChance = 0.8f;

    [Min(0f)]
    [SerializeField]
    private float distanceImportance = 1f;

    [Min(0f)]
    [SerializeField]
    private float honeyImportance = 2f;

    private readonly Dictionary<string, int> honeyByType =
        new();

    public string HiveName =>
        hiveName;

    public int CurrentHoney =>
        currentHoney;

    public int MaxHoney =>
        maxHoney;

    public bool IsFull =>
        currentHoney >= maxHoney;

    public bool CanHarvest =>
        currentHoney >= maxHoney * harvestThreshold;

    public float HoneyRatio =>
        maxHoney <= 0
            ? 0f
            : (float)currentHoney / maxHoney;

    public float SmartChoiceChance =>
        smartChoiceChance;

    public float DistanceImportance =>
        distanceImportance;

    public float HoneyImportance =>
        honeyImportance;

    public IReadOnlyDictionary<string, int> HoneyByType =>
        honeyByType;

    public int StoreHoney(
        string honeyType,
        int amount)
    {
        if (amount <= 0 || IsFull)
        {
            return 0;
        }

        if (string.IsNullOrWhiteSpace(honeyType))
        {
            honeyType = "Unknown Honey";
        }

        int availableSpace =
            maxHoney - currentHoney;

        int storedAmount =
            Mathf.Min(
                amount,
                availableSpace
            );

        currentHoney += storedAmount;

        if (!honeyByType.ContainsKey(honeyType))
        {
            honeyByType.Add(
                honeyType,
                0
            );
        }

        honeyByType[honeyType] +=
            storedAmount;

        Debug.Log(
            $"{hiveName}: Stored {storedAmount}H of {honeyType}. " +
            $"Total: {currentHoney}/{maxHoney}H",
            gameObject
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

        if (!honeyByType.TryGetValue(
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

        if (!honeyByType.TryGetValue(
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

        honeyByType[honeyType] -=
            removedAmount;

        currentHoney -=
            removedAmount;

        if (honeyByType[honeyType] <= 0)
        {
            honeyByType.Remove(
                honeyType
            );
        }

        return removedAmount;
    }

    public int RemoveAllHoney()
    {
        int removedAmount =
            currentHoney;

        currentHoney = 0;

        honeyByType.Clear();

        return removedAmount;
    }

    public void SetSmartChoiceChance(
        float newChance)
    {
        smartChoiceChance =
            Mathf.Clamp01(newChance);
    }
}