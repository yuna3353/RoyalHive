using System.Collections.Generic;
using UnityEngine;

public class BeeAI : MonoBehaviour
{
    private enum BeeState
    {
        StartingDelay,
        Searching,
        MovingToSource,
        Collecting,
        ReturningToHive,
        Unloading,
        Resting
    }

    [Header("개체별 이동속도")]
    [Tooltip("벌 개체가 가질 수 있는 최소 이동속도")]
    [Min(0.1f)]
    [SerializeField] private float minMoveSpeed = 4f;

    [Tooltip("벌 개체가 가질 수 있는 최대 이동속도")]
    [Min(0.1f)]
    [SerializeField] private float maxMoveSpeed = 6f;

    [Tooltip("목표와 이 거리 이하로 가까워지면 도착으로 판정")]
    [Min(0.01f)]
    [SerializeField]
    private float arrivalDistance =
        0.1f;

    [Header("개체별 시간 차이")]
    [Tooltip("생성 후 첫 출발까지 기다리는 시간 범위")]
    [SerializeField]
    private Vector2 startDelayRange =
        new Vector2(0f, 1.5f);

    [Tooltip("식물의 기본 채집시간에 곱할 랜덤 배율")]
    [SerializeField]
    private Vector2 collectTimeMultiplierRange =
        new Vector2(0.8f, 1.2f);

    [Tooltip("벌통에 꿀을 저장하는 시간 범위")]
    [SerializeField]
    private Vector2 unloadDurationRange =
        new Vector2(1.5f, 2.5f);

    [Tooltip("꿀 저장 후 다음 출발 전 쉬는 시간 범위")]
    [SerializeField]
    private Vector2 restDurationRange =
        new Vector2(0.2f, 1f);

    [Header("비행 움직임")]
    [Tooltip("직선 경로에서 옆으로 흔들리는 폭")]
    [Min(0f)]
    [SerializeField]
    private float flightWaveAmount =
        0.05f;

    [Tooltip("벌마다 적용할 흔들림 속도 범위")]
    [SerializeField]
    private Vector2 flightWaveSpeedRange =
        new Vector2(4f, 8f);

    private HiveController homeHive;
    private ForageSource targetSource;

    private BeeState currentState =
        BeeState.StartingDelay;

    private float individualMoveSpeed;
    private float flightWaveSpeed;
    private float flightWaveOffset;
    private float stateTimer;

    private int carriedHoney;
    private Vector3 targetVisitPosition;
    private bool hasReservation;

    public void Initialize(HiveController hive)
    {
        homeHive = hive;

        individualMoveSpeed = Random.Range(
            Mathf.Min(minMoveSpeed, maxMoveSpeed),
            Mathf.Max(minMoveSpeed, maxMoveSpeed)
        );

        flightWaveSpeed = Random.Range(
            Mathf.Min(
                flightWaveSpeedRange.x,
                flightWaveSpeedRange.y
            ),
            Mathf.Max(
                flightWaveSpeedRange.x,
                flightWaveSpeedRange.y
            )
        );

        flightWaveOffset =
            Random.Range(0f, Mathf.PI * 2f);

        stateTimer = Random.Range(
            Mathf.Min(
                startDelayRange.x,
                startDelayRange.y
            ),
            Mathf.Max(
                startDelayRange.x,
                startDelayRange.y
            )
        );

        currentState = BeeState.StartingDelay;
    }

    private void Update()
    {
        if (homeHive == null)
        {
            return;
        }

        switch (currentState)
        {
            case BeeState.StartingDelay:
                UpdateStartingDelay();
                break;

            case BeeState.Searching:
                SearchForSource();
                break;

            case BeeState.MovingToSource:
                MoveToSource();
                break;

            case BeeState.Collecting:
                CollectHoney();
                break;

            case BeeState.ReturningToHive:
                ReturnToHive();
                break;

            case BeeState.Unloading:
                UnloadHoney();
                break;

            case BeeState.Resting:
                RestAtHive();
                break;
        }
    }

    private void UpdateStartingDelay()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            currentState = BeeState.Searching;
        }
    }

    private void SearchForSource()
    {
        CancelCurrentReservation();

        targetSource = SelectSource();

        if (targetSource == null)
        {
            stateTimer = Random.Range(0.2f, 0.6f);
            currentState = BeeState.StartingDelay;
            return;
        }

        if (!targetSource.TryReserve())
        {
            targetSource = null;

            stateTimer = Random.Range(0.1f, 0.4f);
            currentState = BeeState.StartingDelay;
            return;
        }

        hasReservation = true;

        targetVisitPosition =
            targetSource.GetRandomVisitPosition();

        currentState = BeeState.MovingToSource;
    }

    private ForageSource SelectSource()
    {
        List<ForageSource> availableSources = new();

        foreach (
            ForageSource source
            in ForageSource.ActiveSources)
        {
            if (source != null &&
                source.IsAvailable)
            {
                availableSources.Add(source);
            }
        }

        if (availableSources.Count == 0)
        {
            return null;
        }

        bool makeSmartChoice =
            Random.value <=
            homeHive.SmartChoiceChance;

        if (!makeSmartChoice)
        {
            // 판단에 실패한 경우 이용 가능한 밀원 중 무작위 선택
            int randomIndex = Random.Range(
                0,
                availableSources.Count
            );

            return availableSources[randomIndex];
        }

        ForageSource bestSource = null;
        float bestScore = float.MinValue;

        foreach (
            ForageSource source
            in availableSources)
        {
            float distance = Vector2.Distance(
                transform.position,
                source.transform.position
            );

            float honeyScore =
                source.HoneyPerVisit *
                homeHive.HoneyImportance;

            float distancePenalty =
                distance *
                homeHive.DistanceImportance;

            // 점수가 비슷할 때 벌마다 선택이 갈리도록 작은 랜덤값 추가
            float randomVariation =
                Random.Range(-0.25f, 0.25f);

            float finalScore =
                honeyScore -
                distancePenalty +
                randomVariation;

            if (finalScore > bestScore)
            {
                bestScore = finalScore;
                bestSource = source;
            }
        }

        return bestSource;
    }

    private void MoveToSource()
    {
        if (targetSource == null)
        {
            CancelCurrentReservation();
            currentState = BeeState.Searching;
            return;
        }

        MoveTowards(targetVisitPosition);

        if (HasArrived(targetVisitPosition))
        {
            float multiplier = Random.Range(
                Mathf.Min(
                    collectTimeMultiplierRange.x,
                    collectTimeMultiplierRange.y
                ),
                Mathf.Max(
                    collectTimeMultiplierRange.x,
                    collectTimeMultiplierRange.y
                )
            );

            stateTimer =
                targetSource.CollectDuration *
                multiplier;

            currentState = BeeState.Collecting;
        }
    }

    private void CollectHoney()
    {
        if (targetSource == null)
        {
            CancelCurrentReservation();
            currentState = BeeState.Searching;
            return;
        }

        stateTimer -= Time.deltaTime;

        if (stateTimer > 0f)
        {
            return;
        }

        carriedHoney =
            targetSource.CompleteHarvest();

        hasReservation = false;
        targetSource = null;

        currentState = BeeState.ReturningToHive;
    }

    private void ReturnToHive()
    {
        MoveTowards(homeHive.transform.position);

        if (HasArrived(homeHive.transform.position))
        {
            stateTimer = Random.Range(
                Mathf.Min(
                    unloadDurationRange.x,
                    unloadDurationRange.y
                ),
                Mathf.Max(
                    unloadDurationRange.x,
                    unloadDurationRange.y
                )
            );

            currentState = BeeState.Unloading;
        }
    }

    private void UnloadHoney()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer > 0f)
        {
            return;
        }

        homeHive.StoreHoney(carriedHoney);
        carriedHoney = 0;

        stateTimer = Random.Range(
            Mathf.Min(
                restDurationRange.x,
                restDurationRange.y
            ),
            Mathf.Max(
                restDurationRange.x,
                restDurationRange.y
            )
        );

        currentState = BeeState.Resting;
    }

    private void RestAtHive()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            currentState = BeeState.Searching;
        }
    }

    private void MoveTowards(Vector3 target)
    {
        Vector3 direction =
            target - transform.position;

        Vector3 perpendicular = new Vector3(
            -direction.y,
            direction.x,
            0f
        ).normalized;

        float wave = Mathf.Sin(
            Time.time * flightWaveSpeed +
            flightWaveOffset
        ) * flightWaveAmount;

        Vector3 animatedTarget =
            target + perpendicular * wave;

        transform.position = Vector3.MoveTowards(
            transform.position,
            animatedTarget,
            individualMoveSpeed * Time.deltaTime
        );
    }

    private bool HasArrived(Vector3 target)
    {
        return Vector2.Distance(
            transform.position,
            target
        ) <= arrivalDistance;
    }

    private void CancelCurrentReservation()
    {
        if (targetSource != null &&
            hasReservation)
        {
            targetSource.CancelReservation();
        }

        targetSource = null;
        hasReservation = false;
    }

    private void OnDisable()
    {
        CancelCurrentReservation();
    }

    private void OnDestroy()
    {
        CancelCurrentReservation();
    }
}