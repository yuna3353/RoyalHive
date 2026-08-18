using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HiveController))]
public class BeeSpawner : MonoBehaviour
{
    [Header("Bee Prefab")]
    [SerializeField]
    private BeeAI beePrefab;

    [Header("Bee Parent")]
    [SerializeField]
    private string beeParentObjectName = "Bees";

    [SerializeField]
    private bool createParentIfMissing = true;

    [Header("Bee Count")]
    [Min(0)]
    [SerializeField]
    private int initialBeeCount = 3;

    [Min(1)]
    [SerializeField]
    private int maxBeeCount = 10;

    [Min(0f)]
    [SerializeField]
    private float spawnRadius = 0.3f;

    private HiveController hive;
    private Transform beeParent;

    private readonly List<BeeAI> spawnedBees =
        new();

    public int CurrentBeeCount
    {
        get
        {
            CleanupDestroyedBees();
            return spawnedBees.Count;
        }
    }

    public int MaxBeeCount =>
        maxBeeCount;

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
            beeParent =
                parentObject.transform;

            return;
        }

        if (!createParentIfMissing)
        {
            Debug.LogWarning(
                $"{name}: Could not find {beeParentObjectName}.",
                gameObject
            );

            return;
        }

        GameObject newParent =
            new GameObject(
                beeParentObjectName
            );

        beeParent =
            newParent.transform;
    }

    public void SpawnBee()
    {
        CleanupDestroyedBees();

        if (beePrefab == null)
        {
            Debug.LogWarning(
                $"{name}: Bee Prefab is not assigned.",
                gameObject
            );

            return;
        }

        if (spawnedBees.Count >= maxBeeCount)
        {
            Debug.Log(
                $"{name}: Maximum bee count reached.",
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

        BeeAI bee =
            Instantiate(
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