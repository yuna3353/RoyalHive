using TMPro;
using UnityEngine;

public class MerchantManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private WarehouseController warehouse;

    [SerializeField]
    private BuildManager buildManager;

    [SerializeField]
    private GameTimeManager gameTimeManager;

    [SerializeField]
    private GameObject merchantPanel;

    [SerializeField]
    private GameObject travelingMerchant;

    [Header("Merchant Visit")]
    [Tooltip("How many days after game start until the first merchant arrives.")]
    [Min(0)]
    [SerializeField]
    private int firstArrivalDelayDays = 1;

    [Tooltip("Days between merchant visits after the previous merchant leaves.")]
    [Min(1)]
    [SerializeField]
    private int revisitIntervalDays = 3;

    [Header("Bill Options")]
    [SerializeField]
    private BillData[] billOptions;

    [Header("UI")]
    [SerializeField]
    private TMP_Text billNameText;

    [SerializeField]
    private TMP_Text requestText;

    [SerializeField]
    private TMP_Text rewardText;

    [SerializeField]
    private TMP_Text deadlineText;

    [SerializeField]
    private TMP_Text statusText;

    private BillData currentBill;

    private int billDeadlineAbsoluteDay = -1;
    private int lastCheckedAbsoluteDay = -1;
    private int nextArrivalAbsoluteDay = -1;

    private bool merchantPresent;
    private bool billExpired;

    private void Awake()
    {
        if (merchantPanel != null)
        {
            merchantPanel.SetActive(false);
        }

        if (travelingMerchant != null)
        {
            travelingMerchant.SetActive(false);
        }
    }

    private void Start()
    {
        if (gameTimeManager == null)
        {
            gameTimeManager =
                GameTimeManager.Instance;
        }

        int currentAbsoluteDay =
            GetCurrentAbsoluteDay();

        lastCheckedAbsoluteDay =
            currentAbsoluteDay;

        nextArrivalAbsoluteDay =
            currentAbsoluteDay +
            Mathf.Max(
                0,
                firstArrivalDelayDays
            );

        // Allows merchant to appear immediately
        // when First Arrival Delay Days is 0.
        CheckMerchantArrival();
    }

    private void Update()
    {
        if (gameTimeManager == null)
        {
            gameTimeManager =
                GameTimeManager.Instance;

            if (gameTimeManager == null)
            {
                return;
            }
        }

        int currentAbsoluteDay =
            GetCurrentAbsoluteDay();

        if (currentAbsoluteDay !=
            lastCheckedAbsoluteDay)
        {
            lastCheckedAbsoluteDay =
                currentAbsoluteDay;

            HandleNewDay();
        }

        if (merchantPanel != null &&
            merchantPanel.activeSelf)
        {
            RefreshUI();
        }
    }

    private void HandleNewDay()
    {
        if (merchantPresent)
        {
            CheckDeadline();

            if (billExpired)
            {
                DepartMerchant(
                    "The merchant left because the request expired."
                );

                return;
            }
        }

        CheckMerchantArrival();
    }

    private void CheckMerchantArrival()
    {
        if (merchantPresent)
        {
            return;
        }

        int currentAbsoluteDay =
            GetCurrentAbsoluteDay();

        if (currentAbsoluteDay <
            nextArrivalAbsoluteDay)
        {
            return;
        }

        ArriveMerchant();
    }

    private void ArriveMerchant()
    {
        merchantPresent = true;
        billExpired = false;

        if (travelingMerchant != null)
        {
            travelingMerchant.SetActive(true);
        }

        GenerateNewBill();

        Debug.Log(
            "Traveling Merchant arrived."
        );
    }

    private void DepartMerchant(
        string message)
    {
        merchantPresent = false;

        if (merchantPanel != null)
        {
            merchantPanel.SetActive(false);
        }

        if (travelingMerchant != null)
        {
            travelingMerchant.SetActive(false);
        }

        currentBill = null;
        billExpired = false;
        billDeadlineAbsoluteDay = -1;

        nextArrivalAbsoluteDay =
            GetCurrentAbsoluteDay() +
            Mathf.Max(
                1,
                revisitIntervalDays
            );

        Debug.Log(message);
    }

    public void OpenPanel()
    {
        if (!merchantPresent)
        {
            return;
        }

        if (merchantPanel == null)
        {
            Debug.LogWarning(
                "Merchant Panel is not assigned."
            );

            return;
        }

        merchantPanel.SetActive(true);

        CheckDeadline();
        RefreshUI();
    }

    public void ClosePanel()
    {
        if (merchantPanel != null)
        {
            merchantPanel.SetActive(false);
        }
    }

    public void GenerateNewBill()
    {
        if (billOptions == null ||
            billOptions.Length == 0)
        {
            currentBill = null;

            Debug.LogWarning(
                "No Merchant Bill Options are configured."
            );

            return;
        }

        int randomIndex =
            Random.Range(
                0,
                billOptions.Length
            );

        currentBill =
            billOptions[randomIndex];

        billExpired = false;

        int currentAbsoluteDay =
            GetCurrentAbsoluteDay();

        billDeadlineAbsoluteDay =
            currentAbsoluteDay +
            Mathf.Max(
                1,
                currentBill.durationDays
            );

        SetStatus(
            "New request available."
        );

        RefreshUI();
    }

    public void SubmitCurrentBill()
    {
        if (!merchantPresent)
        {
            return;
        }

        if (currentBill == null)
        {
            SetStatus(
                "No active request."
            );

            return;
        }

        CheckDeadline();

        if (billExpired)
        {
            SetStatus(
                "This request has expired."
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

        int availableAmount =
            warehouse.GetHoneyAmount(
                currentBill.honeyType
            );

        if (availableAmount <
            currentBill.requiredAmount)
        {
            SetStatus(
                $"Not enough {currentBill.honeyType}."
            );

            return;
        }

        int removedAmount =
            warehouse.RemoveHoney(
                currentBill.honeyType,
                currentBill.requiredAmount
            );

        if (removedAmount <
            currentBill.requiredAmount)
        {
            SetStatus(
                "Delivery failed."
            );

            return;
        }

        int reward =
            currentBill.rewardGold;

        buildManager.AddMoney(
            reward
        );

        Debug.Log(
            $"Merchant delivery complete. +{reward}G"
        );

        DepartMerchant(
            $"Delivery Complete! +{reward}G"
        );
    }

    private void CheckDeadline()
    {
        if (!merchantPresent ||
            currentBill == null ||
            billExpired)
        {
            return;
        }

        int currentAbsoluteDay =
            GetCurrentAbsoluteDay();

        if (currentAbsoluteDay >=
            billDeadlineAbsoluteDay)
        {
            billExpired = true;

            SetStatus(
                "Request Expired."
            );
        }
    }

    private int GetDaysLeft()
    {
        if (currentBill == null)
        {
            return 0;
        }

        return Mathf.Max(
            0,
            billDeadlineAbsoluteDay -
            GetCurrentAbsoluteDay()
        );
    }

    private int GetCurrentAbsoluteDay()
    {
        if (gameTimeManager == null)
        {
            return 0;
        }

        const int daysPerSeason = 10;
        const int seasonsPerYear = 4;

        int seasonIndex =
            (int)gameTimeManager.CurrentSeason;

        int yearOffset =
            (gameTimeManager.CurrentYear - 1) *
            daysPerSeason *
            seasonsPerYear;

        int seasonOffset =
            seasonIndex *
            daysPerSeason;

        int dayOffset =
            gameTimeManager.CurrentDay - 1;

        return
            yearOffset +
            seasonOffset +
            dayOffset;
    }

    private void RefreshUI()
    {
        if (currentBill == null)
        {
            if (billNameText != null)
            {
                billNameText.text =
                    "No Active Bill";
            }

            if (requestText != null)
            {
                requestText.text =
                    "No delivery request.";
            }

            if (rewardText != null)
            {
                rewardText.text =
                    "Reward: -";
            }

            if (deadlineText != null)
            {
                deadlineText.text =
                    "Deadline: -";
            }

            return;
        }

        if (billNameText != null)
        {
            billNameText.text =
                currentBill.billName;
        }

        if (requestText != null)
        {
            int warehouseAmount = 0;

            if (warehouse != null)
            {
                warehouseAmount =
                    warehouse.GetHoneyAmount(
                        currentBill.honeyType
                    );
            }

            requestText.text =
                $"{currentBill.honeyType}\n" +
                $"Required: {currentBill.requiredAmount} H\n" +
                $"Warehouse: {warehouseAmount} H";
        }

        if (rewardText != null)
        {
            rewardText.text =
                $"Reward: {currentBill.rewardGold} G";
        }

        if (deadlineText != null)
        {
            if (billExpired)
            {
                deadlineText.text =
                    "Deadline: EXPIRED";
            }
            else
            {
                int daysLeft =
                    GetDaysLeft();

                deadlineText.text =
                    $"Time Left: {daysLeft} Day" +
                    (daysLeft == 1 ? "" : "s");
            }
        }
    }

    private void SetStatus(
        string message)
    {
        if (statusText != null)
        {
            statusText.text =
                message;
        }

        Debug.Log(message);
    }
}