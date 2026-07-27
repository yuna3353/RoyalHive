using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BuildZone : MonoBehaviour
{
    public enum ZoneType
    {
        Hive,
        Flower,
        Tree
    }

    [Header("구역 종류")]
    [SerializeField] private ZoneType zoneType;

    private Collider2D zoneCollider;

    public ZoneType Type => zoneType;

    private void Awake()
    {
        zoneCollider = GetComponent<Collider2D>();
    }

    public bool Contains(Vector2 worldPosition)
    {
        return zoneCollider != null &&
               zoneCollider.OverlapPoint(worldPosition);
    }
}