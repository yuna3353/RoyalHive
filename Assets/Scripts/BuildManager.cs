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
        [Header("표시 정보")]
        public string displayName;

        [Header("종류와 설치 구역")]
        public PlaceableObject.PlaceableType objectType;
        public BuildZone.ZoneType requiredZone;

        [Header("Prefab과 부모")]
        public GameObject prefab;
        public Transform parent;

        [Header("가격과 점유 크기")]
        [Min(0)]
        public int cost;

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

    [Header("설치 격자")]
    [Min(0.1f)]
    [SerializeField] private float gridSize = 1f;

    [Header("건설 가능한 목록")]
    [SerializeField] private BuildOption[] buildOptions;

    private int currentMoney;
    private BuildMode currentMode;
    private BuildOption selectedOption;

    public int CurrentMoney => currentMoney;

    private void Awake()
    {
        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        currentMoney = startingMoney;
        currentMode = BuildMode.None;

        UpdateMoneyUI();

        if (buildPanel != null)
        {
            buildPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (Mouse.current == null ||
            worldCamera == null)
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

        Vector2 screenPosition =
            Mouse.current.position.ReadValue();

        Vector2 worldPosition =
            worldCamera.ScreenToWorldPoint(screenPosition);

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
        CancelCurrentMode();

        if (buildPanel != null)
        {
            buildPanel.SetActive(true);
        }
    }

    public void CloseBuildPanel()
    {
        CancelCurrentMode();

        if (buildPanel != null)
        {
            buildPanel.SetActive(false);
        }
    }

    public void SelectBuildOptionByIndex(int index)
    {
        if (buildOptions == null ||
            index < 0 ||
            index >= buildOptions.Length)
        {
            Debug.LogWarning(
                $"잘못된 Build Option 인덱스: {index}"
            );

            return;
        }

        BuildOption option = buildOptions[index];

        if (option == null || option.prefab == null)
        {
            Debug.LogWarning(
                $"Build Option {index}에 Prefab이 없습니다."
            );

            return;
        }

        selectedOption = option;
        currentMode = BuildMode.Build;

        if (buildPanel != null)
        {
            buildPanel.SetActive(false);
        }

        Debug.Log(
            $"{selectedOption.displayName} 설치 모드"
        );
    }

    public void SelectRemoveMode()
    {
        selectedOption = null;
        currentMode = BuildMode.Remove;

        if (buildPanel != null)
        {
            buildPanel.SetActive(false);
        }
    }

    public void CancelCurrentMode()
    {
        selectedOption = null;
        currentMode = BuildMode.None;
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

        Vector2 snappedPosition =
            SnapToGrid(worldPosition);

        BuildZone zone = FindBuildZone(snappedPosition);

        if (zone == null)
        {
            Debug.Log("설치 가능한 구역이 아닙니다.");
            return;
        }

        if (zone.Type != selectedOption.requiredZone)
        {
            Debug.Log(
                $"{selectedOption.displayName}은 " +
                $"{selectedOption.requiredZone} 구역에만 설치 가능합니다."
            );

            return;
        }

        if (IsPlaceOccupied(
                snappedPosition,
                selectedOption.footprint))
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
    }

    private void TryRemoveObject(Vector2 worldPosition)
    {
        Collider2D[] hits =
            Physics2D.OverlapPointAll(worldPosition);

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

            Destroy(placeable.gameObject);
            return;
        }

        Debug.Log("제거할 오브젝트가 없습니다.");
    }

    private BuildZone FindBuildZone(Vector2 position)
    {
        Collider2D[] hits =
            Physics2D.OverlapPointAll(position);

        foreach (Collider2D hit in hits)
        {
            BuildZone zone =
                hit.GetComponent<BuildZone>();

            if (zone != null &&
                zone.Contains(position))
            {
                return zone;
            }
        }

        return null;
    }

    private bool IsPlaceOccupied(
        Vector2 position,
        Vector2 footprint)
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            position,
            footprint * 0.9f,
            0f
        );

        foreach (Collider2D hit in hits)
        {
            PlaceableObject placeable =
                hit.GetComponentInParent<PlaceableObject>();

            if (placeable != null)
            {
                return true;
            }
        }

        return false;
    }

    private Vector2 SnapToGrid(Vector2 position)
    {
        float x =
            Mathf.Round(position.x / gridSize) *
            gridSize;

        float y =
            Mathf.Round(position.y / gridSize) *
            gridSize;

        return new Vector2(x, y);
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = $"{currentMoney}G";
        }
    }
}