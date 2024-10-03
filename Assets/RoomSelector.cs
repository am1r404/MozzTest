using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RoomSelector : MonoBehaviour
{
    [Header("Room Buttons")]
    public Button bedroom1Button;
    public Button bedroom2Button;
    public Button bedroom3Button;
    public Button livingRoomButton;
    public Button bathroomButton;
    public Button kitchenButton;
    public Button diningRoomButton;
    public Button terraceButton;

    [Header("Room Points")]
    public Transform bedroom1Point;
    public Transform bedroom2Point;
    public Transform bedroom3Point;
    public Transform livingRoomPoint;
    public Transform bathroomPoint;
    public Transform kitchenPoint;
    public Transform diningRoomPoint;
    public Transform terracePoint;

    [Header("Arrow Settings")]
    public GameObject arrow;
    public float moveSpeed = 5f;
    public LineRenderer lineRenderer;

    [Header("UI Elements")]
    public GameObject panel;

    private Vector3[] pathCorners;
    private int currentPathIndex = 0;

    private NavMeshPath path;
    private bool isPathValid = false;

    void Start()
    {
        bedroom1Button.onClick.AddListener(() => SelectRoom(bedroom1Point));
        bedroom2Button.onClick.AddListener(() => SelectRoom(bedroom2Point));
        bedroom3Button.onClick.AddListener(() => SelectRoom(bedroom3Point));
        livingRoomButton.onClick.AddListener(() => SelectRoom(livingRoomPoint));
        bathroomButton.onClick.AddListener(() => SelectRoom(bathroomPoint));
        kitchenButton.onClick.AddListener(() => SelectRoom(kitchenPoint));
        diningRoomButton.onClick.AddListener(() => SelectRoom(diningRoomPoint));
        terraceButton.onClick.AddListener(() => SelectRoom(terracePoint));

        if (lineRenderer == null && arrow != null)
        {
            lineRenderer = arrow.GetComponent<LineRenderer>();
        }

        path = new NavMeshPath();

        NavMeshHit hit;
        if (NavMesh.SamplePosition(arrow.transform.position, out hit, 1.0f, NavMesh.AllAreas))
        {
            arrow.transform.position = hit.position;
        }
        else
        {
            Debug.LogWarning("Arrow is not on the NavMesh at the start.");
        }
    }

    void SelectRoom(Transform destination)
    {
        NavMeshHit hit;
        Vector3 destinationPosition;

        if (NavMesh.SamplePosition(destination.position, out hit, 1.0f, NavMesh.AllAreas))
        {
            destinationPosition = hit.position;
            Debug.Log("Selected Room: " + destination.name);
            RecalculatePath(destinationPosition);

            if (isPathValid)
            {
                currentPathIndex = 0;
                Debug.Log("Path is valid, arrow is ready to move when mouse button is held down.");
            }
            else
            {
                Debug.LogWarning("Cannot move. Path is invalid.");
            }

            if (panel != null)
            {
                panel.SetActive(false);
            }
            else
            {
                Debug.LogWarning("Panel reference is missing.");
            }
        }
        else
        {
            Debug.LogWarning("Selected room is not on the NavMesh.");
            isPathValid = false;
        }
    }

    void RecalculatePath(Vector3 destinationPosition)
    {
        if (arrow == null)
            return;

        if (NavMesh.CalculatePath(arrow.transform.position, destinationPosition, NavMesh.AllAreas, path))
        {
            isPathValid = path.status == NavMeshPathStatus.PathComplete;

            if (isPathValid)
            {
                Debug.Log("Path successfully calculated.");
                UpdateLineRenderer();

                pathCorners = path.corners;
            }
            else
            {
                Debug.LogWarning("Path calculation failed: Path is incomplete.");
                isPathValid = false;
            }
        }
        else
        {
            isPathValid = false;
            Debug.LogWarning("Failed to calculate path.");
        }
    }

    void Update()
    {
        if (arrow != null)
        {
            if (Input.GetMouseButton(0) && isPathValid && currentPathIndex < pathCorners.Length)
            {
                MoveAlongPath();
            }
        }
        else
        {
            Debug.LogWarning("Arrow reference is missing.");
        }
    }

    void MoveAlongPath()
    {
        if (!isPathValid || currentPathIndex >= pathCorners.Length)
            return;

        Vector3 targetPosition = pathCorners[currentPathIndex];
        arrow.transform.position = Vector3.MoveTowards(arrow.transform.position, targetPosition, moveSpeed * Time.deltaTime);

        Vector3 direction = targetPosition - arrow.transform.position;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

            // Option 1: Using rotation offset
            Quaternion rotationOffset = Quaternion.Euler(0, 0, 0); // Adjust as needed
            arrow.transform.rotation = targetRotation * rotationOffset;

            // Option 2: Using Transform.Rotate
            // arrow.transform.rotation = targetRotation;
            // arrow.transform.Rotate(90, 0, 90, Space.Self); // Adjust as needed
        }

        if (Vector3.Distance(arrow.transform.position, targetPosition) < 0.1f)
        {
            currentPathIndex++;
            if (currentPathIndex >= pathCorners.Length)
            {
                isPathValid = false;
                Debug.Log("Arrow has reached the destination.");
            }
        }
    }


    void UpdateLineRenderer()
    {
        if (!isPathValid || lineRenderer == null)
        {
            if (lineRenderer != null)
            {
                lineRenderer.positionCount = 0;
            }
            return;
        }

        lineRenderer.positionCount = path.corners.Length;
        lineRenderer.SetPositions(path.corners);
    }
}
