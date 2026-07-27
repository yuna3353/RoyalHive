using System;
using System.Collections.Generic;
using UnityEngine;

public class ForageSource : MonoBehaviour
{
    public enum SourceCategory
    {
        Tree,
        Flower
    }

    [Serializable]
    public class BloomPeriod
    {
        [Tooltip("개화하는 계절")]
        public GameTimeManager.Season season;

        [Tooltip("해당 계절의 개화 시작 날짜")]
        [Min(1)]
        public int startDay = 1;

        [Tooltip("해당 계절의 개화 종료 날짜")]
        [Min(1)]
        public int endDay = 10;
    }

    public static readonly List<ForageSource>
        ActiveSources = new();

    [Header("식물 기본 정보")]
    [SerializeField]
    private string displayName =
        "New Forage Source";

    [SerializeField] private SourceCategory category;

    [SerializeField]
    private string honeyName =
        "Honey";

    [TextArea]
    [SerializeField] private string description;

    [Header("개화 설정")]
    [SerializeField] private BloomPeriod[] bloomPeriods;

    [Tooltip("체크 해제 시 벌이 방문할 수 없습니다.")]
    [SerializeField] private bool canBeVisited = true;

    [Header("채집 설정")]
    [Tooltip("벌이 한 번 방문할 때 가져가는 꿀의 양")]
    [Min(0)]
    [SerializeField] private int honeyPerVisit = 1;

    [Tooltip("벌이 밀원에서 채집하는 기본 시간")]
    [Min(0f)]
    [SerializeField] private float collectDuration = 5f;

    [Tooltip("동시에 이 밀원을 예약할 수 있는 벌 수")]
    [Min(1)]
    [SerializeField] private int maxSimultaneousBees = 1;

    [Header("밀원 회복 설정")]
    [Tooltip("채집 후 다시 사용할 수 있을 때까지의 시간")]
    [Min(0f)]
    [SerializeField] private float nectarCooldown = 3f;

    [Tooltip("벌들이 겹치지 않도록 도착 위치를 분산하는 범위")]
    [Min(0f)]
    [SerializeField] private float visitRadius = 0.35f;

    private int reservedBeeCount;
    private float nextAvailableTime;

    public string DisplayName => displayName;
    public string HoneyName => honeyName;
    public SourceCategory Category => category;
    public int HoneyPerVisit => honeyPerVisit;
    public float CollectDuration => collectDuration;

    public bool IsAvailable =>
        CheckBloomAvailability() &&
        Time.time >= nextAvailableTime &&
        reservedBeeCount < maxSimultaneousBees;

    private void OnEnable()
    {
        if (!ActiveSources.Contains(this))
        {
            ActiveSources.Add(this);
        }
    }

    private void OnDisable()
    {
        ActiveSources.Remove(this);
    }

    public bool TryReserve()
    {
        if (!IsAvailable)
        {
            return false;
        }

        reservedBeeCount++;
        return true;
    }

    public int CompleteHarvest()
    {
        reservedBeeCount =
            Mathf.Max(0, reservedBeeCount - 1);

        nextAvailableTime =
            Time.time + nectarCooldown;

        return honeyPerVisit;
    }

    public void CancelReservation()
    {
        reservedBeeCount =
            Mathf.Max(0, reservedBeeCount - 1);
    }

    public Vector3 GetRandomVisitPosition()
    {
        Vector2 offset =
            UnityEngine.Random.insideUnitCircle *
            visitRadius;

        return transform.position +
               new Vector3(
                   offset.x,
                   offset.y,
                   0f
               );
    }

    private bool CheckBloomAvailability()
    {
        if (!canBeVisited || !isActiveAndEnabled)
        {
            return false;
        }

        GameTimeManager timeManager =
            GameTimeManager.Instance;

        // 시간 관리자가 아직 없으면 테스트를 위해 사용 가능 처리
        if (timeManager == null)
        {
            return true;
        }

        if (bloomPeriods == null ||
            bloomPeriods.Length == 0)
        {
            return false;
        }

        foreach (BloomPeriod period in bloomPeriods)
        {
            if (period == null)
            {
                continue;
            }

            bool correctSeason =
                period.season ==
                timeManager.CurrentSeason;

            bool correctDay =
                timeManager.CurrentDay >=
                period.startDay &&
                timeManager.CurrentDay <=
                period.endDay;

            if (correctSeason && correctDay)
            {
                return true;
            }
        }

        return false;
    }
}