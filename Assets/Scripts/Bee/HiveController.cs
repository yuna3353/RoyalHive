using UnityEngine;

public class HiveController : MonoBehaviour
{
    [Header("벌통 기본 정보")]
    [SerializeField]
    private string hiveName =
        "Basic Hive";

    [Header("꿀 저장")]
    [Min(1)]
    [SerializeField] private int maxHoney = 800;

    [Min(0)]
    [SerializeField] private int currentHoney;

    [Header("벌의 밀원 선택 능력")]
    [Tooltip("효율적인 밀원을 선택할 확률. 0.8은 80%")]
    [Range(0f, 1f)]
    [SerializeField]
    private float smartChoiceChance =
        0.8f;

    [Tooltip("가까운 밀원을 얼마나 중요하게 판단할지 설정")]
    [Min(0f)]
    [SerializeField]
    private float distanceImportance =
        1f;

    [Tooltip("한 번에 많은 꿀을 주는 밀원을 얼마나 중요하게 판단할지 설정")]
    [Min(0f)]
    [SerializeField]
    private float honeyImportance =
        2f;

    public string HiveName => hiveName;
    public int CurrentHoney => currentHoney;
    public int MaxHoney => maxHoney;
    public bool IsFull => currentHoney >= maxHoney;

    public float SmartChoiceChance =>
        smartChoiceChance;

    public float DistanceImportance =>
        distanceImportance;

    public float HoneyImportance =>
        honeyImportance;

    public int StoreHoney(int amount)
    {
        if (amount <= 0 || IsFull)
        {
            return 0;
        }

        int availableSpace =
            maxHoney - currentHoney;

        int storedAmount =
            Mathf.Min(amount, availableSpace);

        currentHoney += storedAmount;

        Debug.Log(
            $"{hiveName}: 꿀 {storedAmount}H 저장 " +
            $"({currentHoney}/{maxHoney}H)",
            gameObject
        );

        return storedAmount;
    }

    public int RemoveHoney(int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        int removedAmount =
            Mathf.Min(amount, currentHoney);

        currentHoney -= removedAmount;

        return removedAmount;
    }

    public int RemoveAllHoney()
    {
        int removedAmount = currentHoney;
        currentHoney = 0;

        return removedAmount;
    }

    public void SetSmartChoiceChance(
        float newChance)
    {
        smartChoiceChance =
            Mathf.Clamp01(newChance);
    }
}