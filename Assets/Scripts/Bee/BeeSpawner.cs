using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HiveController))]
public class BeeSpawner : MonoBehaviour
{
    [Header("벌 Prefab")]
    [SerializeField] private BeeAI beePrefab;

    [Header("벌 부모 자동 탐색")]
    [Tooltip("생성된 벌을 정리할 Scene 오브젝트 이름")]
    [SerializeField]
    private string beeParentObjectName =
        "Bees";

    [Tooltip("Bees 오브젝트가 없으면 자동 생성")]
    [SerializeField]
    private bool createParentIfMissing =
        true;

    [Header("벌 수 설정")]
    [Min(0)]
    [SerializeField] private int initialBeeCount = 3;

    [Min(1)]
    [SerializeField] private int maxBeeCount = 10;

    [Tooltip("벌통 주변의 생성 위치 랜덤 범위")]
    [Min(0f)]
    [SerializeField] private float spawnRadius = 0.3f;

    private HiveController hive;
    private Transform beeParent;

    private readonly List<BeeAI> spawnedBees =
        new();

    private void Awake()
    {
        hive = GetComponent<HiveController>();
        FindOrCreateBeeParent();
    }

    private void Start()
    {
        for (int i = 0;
             i < initialBeeCount;
             i++)
        {
            SpawnBee();
        }
    }

    private void FindOrCreateBeeParent()
    {
        GameObject parentObject =
            GameObject.Find(beeParentObjectName);

        if (parentObject != null)
        {
            beeParent = parentObject.transform;
            return;
        }

        if (!createParentIfMissing)
        {
            Debug.LogWarning(
                $"{name}: {beeParentObjectName}을 찾지 못했습니다.",
                gameObject
            );

            return;
        }

        GameObject newParent =
            new GameObject(beeParentObjectName);

        beeParent = newParent.transform;
    }

    public void SpawnBee()
    {
        CleanupDestroyedBees();

        if (beePrefab == null)
        {
            Debug.LogWarning(
                $"{name}: BeePrefab이 연결되지 않았습니다.",
                gameObject
            );

            return;
        }

        if (spawnedBees.Count >= maxBeeCount)
        {
            Debug.Log(
                $"{name}: 최대 벌 수에 도달했습니다.",
                gameObject
            );

            return;
        }

        Vector2 randomOffset =
            Random.insideUnitCircle *
            spawnRadius;

        Vector3 spawnPosition =
            transform.position +
            new Vector3(
                randomOffset.x,
                randomOffset.y,
                0f
            );

        BeeAI bee = Instantiate(
            beePrefab,
            spawnPosition,
            Quaternion.identity,
            beeParent
        );

        bee.Initialize(hive);
        spawnedBees.Add(bee);
    }

    private void CleanupDestroyedBees()
    {
        spawnedBees.RemoveAll(
            bee => bee == null
        );
    }
}