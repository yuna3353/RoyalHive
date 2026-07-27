using UnityEngine;

public class PlaceableObject : MonoBehaviour
{
    public enum PlaceableType
    {
        Hive,
        Flower,
        Tree
    }

    [Header("설치 오브젝트 정보")]
    [SerializeField] private PlaceableType objectType;

    [Tooltip("제거할 때 돌려받는 금액")]
    [Min(0)]
    [SerializeField] private int refundAmount;

    public PlaceableType ObjectType => objectType;
    public int RefundAmount => refundAmount;
}