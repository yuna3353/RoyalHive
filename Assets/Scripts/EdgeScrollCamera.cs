using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class EdgeScrollCamera : MonoBehaviour
{
    [Header("카메라 이동")]
    [SerializeField] private float moveSpeed = 12f;

    [Tooltip("마우스가 화면 가장자리 몇 픽셀 안에 들어오면 이동할지 설정합니다.")]
    [SerializeField] private float edgeThickness = 25f;

    [Header("스페이스바 복귀")]
    [Tooltip("스페이스바를 누르면 카메라가 시작 위치로 돌아가는 속도입니다.")]
    [SerializeField] private float returnSpeed = 15f;

    [Tooltip("체크하면 스페이스바를 누르는 동안 부드럽게 시작 위치로 돌아갑니다.")]
    [SerializeField] private bool smoothReturn = true;

    [Header("카메라 이동 제한")]
    [Tooltip("카메라가 Ground 바깥으로 나가지 않도록 제한할 Ground의 Sprite Renderer입니다.")]
    [SerializeField] private SpriteRenderer groundRenderer;

    [Tooltip("Ground 경계에서 카메라가 조금 안쪽에 멈추도록 하는 여백입니다.")]
    [SerializeField] private Vector2 boundaryPadding = new Vector2(0.5f, 0.5f);

    private Camera cam;
    private Vector3 startPosition;
    private bool isReturning;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        startPosition = transform.position;
    }

    private void Update()
    {
        if (Keyboard.current == null || Mouse.current == null)
        {
            return;
        }

        HandleReturnInput();

        if (isReturning)
        {
            ReturnToStartPosition();
            return;
        }

        HandleEdgeMovement();
    }

    private void HandleReturnInput()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (smoothReturn)
            {
                isReturning = true;
            }
            else
            {
                transform.position = startPosition;
                ClampCameraPosition();
            }
        }
    }

    private void ReturnToStartPosition()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            startPosition,
            returnSpeed * Time.deltaTime
        );

        ClampCameraPosition();

        if (Vector3.Distance(transform.position, startPosition) <= 0.01f)
        {
            transform.position = startPosition;
            isReturning = false;
        }

        // 복귀 중 마우스 가장자리 이동 입력이 들어오면 복귀를 취소합니다.
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        bool mouseAtEdge =
            mousePosition.x <= edgeThickness ||
            mousePosition.x >= Screen.width - edgeThickness ||
            mousePosition.y <= edgeThickness ||
            mousePosition.y >= Screen.height - edgeThickness;

        if (mouseAtEdge)
        {
            isReturning = false;
        }
    }

    private void HandleEdgeMovement()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 moveDirection = Vector3.zero;

        // 왼쪽
        if (mousePosition.x <= edgeThickness)
        {
            moveDirection.x = -1f;
        }
        // 오른쪽
        else if (mousePosition.x >= Screen.width - edgeThickness)
        {
            moveDirection.x = 1f;
        }

        // 아래
        if (mousePosition.y <= edgeThickness)
        {
            moveDirection.y = -1f;
        }
        // 위
        else if (mousePosition.y >= Screen.height - edgeThickness)
        {
            moveDirection.y = 1f;
        }

        if (moveDirection.sqrMagnitude > 0f)
        {
            moveDirection.Normalize();

            transform.position +=
                moveDirection * moveSpeed * Time.deltaTime;

            ClampCameraPosition();
        }
    }

    private void ClampCameraPosition()
    {
        if (groundRenderer == null)
        {
            return;
        }

        Bounds groundBounds = groundRenderer.bounds;

        float cameraHalfHeight = cam.orthographicSize;
        float cameraHalfWidth = cameraHalfHeight * cam.aspect;

        float minX =
            groundBounds.min.x +
            cameraHalfWidth +
            boundaryPadding.x;

        float maxX =
            groundBounds.max.x -
            cameraHalfWidth -
            boundaryPadding.x;

        float minY =
            groundBounds.min.y +
            cameraHalfHeight +
            boundaryPadding.y;

        float maxY =
            groundBounds.max.y -
            cameraHalfHeight -
            boundaryPadding.y;

        Vector3 position = transform.position;

        // Ground가 카메라보다 작은 경우 Clamp의 최솟값과 최댓값이 뒤집히는 문제 방지
        if (minX <= maxX)
        {
            position.x = Mathf.Clamp(position.x, minX, maxX);
        }

        if (minY <= maxY)
        {
            position.y = Mathf.Clamp(position.y, minY, maxY);
        }

        transform.position = position;
    }

    /// <summary>
    /// 다른 스크립트에서 카메라의 복귀 위치를 현재 위치로 다시 지정할 때 사용합니다.
    /// </summary>
    public void SetCurrentPositionAsStart()
    {
        startPosition = transform.position;
    }

    /// <summary>
    /// 다른 스크립트에서 즉시 시작 위치로 돌려보낼 때 사용합니다.
    /// </summary>
    public void ReturnImmediately()
    {
        transform.position = startPosition;
        isReturning = false;
        ClampCameraPosition();
    }
}