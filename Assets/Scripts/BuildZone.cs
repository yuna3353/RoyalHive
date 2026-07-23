using UnityEngine;

public class BuildZone : MonoBehaviour
{
    public enum ZoneType
    {
        Hive,
        Tree,
        Flower
    }

    [Header("설치 구역 종류")]
    [SerializeField] private ZoneType zoneType;

    private Collider2D zoneCollider;

    public ZoneType Type => zoneType;

    private void Awake()
    {
        zoneCollider = GetComponent<Collider2D>();

        if (zoneCollider == null)
        {
            Debug.LogError(
                $"{gameObject.name}에 Collider2D가 없습니다.",
                gameObject
            );
        }
    }

    public bool Contains(Vector2 worldPosition)
    {
        if (zoneCollider == null)
        {
            return false;
        }

        return zoneCollider.OverlapPoint(worldPosition);
    }
}