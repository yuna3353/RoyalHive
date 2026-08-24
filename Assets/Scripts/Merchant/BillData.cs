using System;
using UnityEngine;

[Serializable]
public class BillData
{
    [Header("Bill Information")]
    public string billName = "Honey Request";

    [Header("Required Honey")]
    public string honeyType = "Flower Honey";

    [Min(1)]
    public int requiredAmount = 10;

    [Header("Reward")]
    [Min(0)]
    public int rewardGold = 100;

    [Header("Deadline")]
    [Min(1)]
    public int durationDays = 2;
}