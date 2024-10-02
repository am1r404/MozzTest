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

    [Header("Go Button")]
    public Button goButton;

    private Transform pointB;
    private bool isGoButtonPressed = false;
    
    private NavMeshPath path;
    private bool isPathValid = false;
    private int currentPathIndex = 0;
    private bool isMoving = false;

    void Start()
    {
        // Add listeners to room buttons
        bedroom1Button.onClick.AddListener(() => SelectRoom(bedroom1Point));
        bedroom2Button.onClick.AddListener(() => SelectRoom(bedroom2Point));
        bedroom3Button.onClick.AddListener(() => SelectRoom(bedroom3Point));
        livingRoomButton.onClick.AddListener(() => SelectRoom(livingRoomPoint));
        bathroomButton.onClick.AddListener(() => SelectRoom(bathroomPoint));
        kitchenButton.onClick.AddListener(() => SelectRoom(kitchenPoint));
        diningRoomButton.onClick.AddListener(() => SelectRoom(diningRoomPoint));
        terraceButton.onClick.AddListener(() => SelectRoom(terracePoint));

        // Add listeners to Go button
        AddGoButtonListeners();

        // Initialize LineRenderer if not assigned
        if (lineRenderer == null && arrow != null)
        {
            lineRenderer = arrow.GetComponent<LineRenderer>();
        }
        
        path = new NavMeshPath();
    }

    void SelectRoom(Transform destination)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(destination.position, out hit, 1.0f, NavMesh.AllAreas))
        {
            pointB = destination;
            pointB.position = hit.position;
            Debug.Log("Selected Room: " + destination.name);
            RecalculatePath();
        }
        else
        {
            Debug.LogWarning("Selected room is not on the NavMesh.");
        }
    }
    
    void RecalculatePath()
    {
        if (arrow == null || pointB == null)
            return;

        if (NavMesh.CalculatePath(arrow.transform.position, pointB.position, NavMesh.AllAreas, path))
        {
            isPathValid = path.status == NavMeshPathStatus.PathComplete;
            UpdateLineRenderer();
        }
        else
        {
            isPathValid = false;
            Debug.LogWarning("Failed to calculate path.");
        }
    }

    void Update()
    {
        if (arrow != null && pointB != null)
        {
            if (isMoving)
            {
                MoveAlongPath();
            }
        }
    }
    
    void MoveAlongPath()
    {
        if (!isMoving || !isPathValid || currentPathIndex >= path.corners.Length)
            return;

        Vector3 targetPosition = path.corners[currentPathIndex];
        arrow.transform.position = Vector3.MoveTowards(arrow.transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Rotate the arrow towards the next waypoint
        Vector3 direction = targetPosition - arrow.transform.position;
        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrow.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }

        if (Vector3.Distance(arrow.transform.position, targetPosition) < 0.1f)
        {
            currentPathIndex++;
            if (currentPathIndex >= path.corners.Length)
            {
                isMoving = false;
                Debug.Log("Arrow has reached the destination: " + pointB.name);
            }
        }
    }
    
    void HandlePlayerMovement()
    {
        float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float moveY = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
        arrow.transform.position += new Vector3(moveX, moveY, 0);
    }

    void MoveArrowTowardsPointB()
    {
        // If you want the arrow to move towards Point B, uncomment the following line
        // arrow.transform.position = Vector3.MoveTowards(arrow.transform.position, pointB.position, moveSpeed * Time.deltaTime);

        // Optionally, check if arrow has reached the destination
        if (Vector3.Distance(arrow.transform.position, pointB.position) < 0.1f)
        {
            Debug.Log("Arrow has reached the destination: " + pointB.name);
            // Additional logic when destination is reached
        }
    }

    void RotateArrowTowardsPointB()
    {
        Vector3 direction = pointB.position - arrow.transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        arrow.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    void UpdateLineRenderer()
    {
        if (!isPathValid || lineRenderer == null)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        lineRenderer.positionCount = path.corners.Length;
        lineRenderer.SetPositions(path.corners);
    }

    void AddGoButtonListeners()
    {
        EventTrigger trigger = goButton.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = goButton.gameObject.AddComponent<EventTrigger>();
        }

        // Pointer Down event
        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
        pointerDownEntry.eventID = EventTriggerType.PointerDown;
        pointerDownEntry.callback.AddListener((data) => { OnGoButtonDown(); });
        trigger.triggers.Add(pointerDownEntry);

        // Pointer Up event
        EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry();
        pointerUpEntry.eventID = EventTriggerType.PointerUp;
        pointerUpEntry.callback.AddListener((data) => { OnGoButtonUp(); });
        trigger.triggers.Add(pointerUpEntry);
    }

    public void OnGoButtonDown()
    {
        if (isPathValid)
        {
            isMoving = true;
            currentPathIndex = 0;
            Debug.Log("Go button pressed");
        }
        else
        {
            Debug.LogWarning("Cannot move. Path is invalid.");
        }
    }

    public void OnGoButtonUp()
    {
        isMoving = false;
        Debug.Log("Go button released");
    }

    public void UpdatePlayerPosition(Vector3 gpsPosition)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(gpsPosition, out hit, 1.0f, NavMesh.AllAreas))
        {
            arrow.transform.position = hit.position;
            RecalculatePath();
        }
        else
        {
            Debug.LogWarning("GPS position is not on the NavMesh.");
        }
    }
}
