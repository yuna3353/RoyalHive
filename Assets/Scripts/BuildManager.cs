using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BuildManager : MonoBehaviour
{
    private enum BuildMode
    {
        None,
        Build,
        Remove
    }

    [Serializable]
    public class BuildOption
    {
        public string displayName;

        public PlaceableObject.PlaceableType objectType;

        public BuildZone.ZoneType requiredZone;

        public GameObject prefab;

        public Transform parent;

        [Min(0)]
        public int cost;

        [Tooltip("겹침 판정에 사용할 오브젝트 크기입니다.")]
        public Vector2 footprint = Vector2.one;
    }

    [Header("카메라")]
    [SerializeField] private Camera worldCamera;

    [Header("UI")]
    [SerializeField] private GameObject buildPanel;
    [SerializeField] private TMP_Text moneyText;

    [Header("경제")]
    [Min(0)]
    [SerializeField] private int startingMoney = 1000;

    [Header("설치 설정")]
    [SerializeField] private float gridSize = 1f;

    [SerializeField] private BuildOption hiveOption;
    [SerializeField] private BuildOption cherryTreeOption;

    private int currentMoney;
    private BuildMode currentMode = BuildMode.None;
    private BuildOption selectedOption;

    public int CurrentMoney => currentMoney;

    private void Awake()
    {
        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        currentMoney = startingMoney;

        UpdateMoneyUI();

        if (buildPanel != null)
        {
            buildPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (Mouse.current == null || worldCamera == null)
        {
            return;
        }

        if (!Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Vector2 mouseScreenPosition =
            Mouse.current.position.ReadValue();

        Vector2 worldPosition =
            worldCamera.ScreenToWorldPoint(mouseScreenPosition);

        if (currentMode == BuildMode.Build)
        {
            TryPlaceObject(worldPosition);
        }
        else if (currentMode == BuildMode.Remove)
        {
            TryRemoveObject(worldPosition);
        }
    }

    public void OpenBuildPanel()
    {
        if (buildPanel != null)
        {
            buildPanel.SetActive(true);
        }

        CancelCurrentMode();
    }

    public void CloseBuildPanel()
    {
        if (buildPanel != null)
        {
            buildPanel.SetActive(false);
        }

        CancelCurrentMode();
    }

    public void SelectHive()
    {
        SelectBuildOption(hiveOption);
    }

    public void SelectCherryTree()
    {
        SelectBuildOption(cherryTreeOption);
    }

    public void SelectRemoveMode()
    {
        selectedOption = null;
        currentMode = BuildMode.Remove;

        if (buildPanel != null)
        {
            buildPanel.SetActive(false);
        }

        Debug.Log("제거 모드가 활성화되었습니다.");
    }

    public void CancelCurrentMode()
    {
        selectedOption = null;
        currentMode = BuildMode.None;
    }

    private void SelectBuildOption(BuildOption option)
    {
        if (option == null || option.prefab == null)
        {
            Debug.LogWarning("설치 옵션 또는 Prefab 연결이 없습니다.");
            return;
        }

        selectedOption = option;
        currentMode = BuildMode.Build;

        if (buildPanel != null)
        {
            buildPanel.SetActive(false);
        }

        Debug.Log($"{option.displayName} 설치 모드가 활성화되었습니다.");
    }

    private void TryPlaceObject(Vector2 worldPosition)
    {
        if (selectedOption == null)
        {
            return;
        }

        if (currentMoney < selectedOption.cost)
        {
            Debug.Log("돈이 부족합니다.");
            return;
        }

        Vector2 snappedPosition = SnapToGrid(worldPosition);

        BuildZone zone = FindBuildZone(snappedPosition);

        if (zone == null)
        {
            Debug.Log("설치 가능한 구역이 아닙니다.");
            return;
        }

        if (zone.Type != selectedOption.requiredZone)
        {
            Debug.Log(
                $"{selectedOption.displayName}은 이 구역에 설치할 수 없습니다."
            );
            return;
        }

        if (IsPlaceOccupied(snappedPosition, selectedOption.footprint))
        {
            Debug.Log("다른 오브젝트와 겹칩니다.");
            return;
        }

        Instantiate(
            selectedOption.prefab,
            snappedPosition,
            Quaternion.identity,
            selectedOption.parent
        );

        currentMoney -= selectedOption.cost;
        UpdateMoneyUI();

        Debug.Log(
            $"{selectedOption.displayName} 설치 완료. 남은 돈: {currentMoney}G"
        );
    }

    private void TryRemoveObject(Vector2 worldPosition)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPosition);

        foreach (Collider2D hit in hits)
        {
            PlaceableObject placeable =
                hit.GetComponentInParent<PlaceableObject>();

            if (placeable == null)
            {
                continue;
            }

            currentMoney += placeable.RefundAmount;
            UpdateMoneyUI();

            Debug.Log(
                $"{placeable.name} 제거 완료. " +
                $"{placeable.RefundAmount}G 환불"
            );

            Destroy(placeable.gameObject);
            return;
        }

        Debug.Log("제거할 오브젝트가 없습니다.");
    }

    private BuildZone FindBuildZone(Vector2 position)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(position);

        foreach (Collider2D hit in hits)
        {
            BuildZone zone = hit.GetComponent<BuildZone>();

            if (zone != null && zone.Contains(position))
            {
                return zone;
            }
        }

        return null;
    }

    private bool IsPlaceOccupied(
        Vector2 position,
        Vector2 footprint
    )
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            position,
            footprint * 0.9f,
            0f
        );

        foreach (Collider2D hit in hits)
        {
            if (hit.GetComponentInParent<PlaceableObject>() != null)
            {
                return true;
            }
        }

        return false;
    }

    private Vector2 SnapToGrid(Vector2 position)
    {
        float snappedX =
            Mathf.Round(position.x / gridSize) * gridSize;

        float snappedY =
            Mathf.Round(position.y / gridSize) * gridSize;

        return new Vector2(snappedX, snappedY);
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = $"{currentMoney}G";
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (selectedOption == null)
        {
            return;
        }

        Gizmos.DrawWireCube(
            transform.position,
            selectedOption.footprint
        );
    }
}