using UnityEngine;
using Mapbox.Directions;
using System.Collections.Generic;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;
using System.Collections;
using Mapbox.Unity;
using TMPro;

public class DirectionsFactoryWithBoxes : MonoBehaviour
{
    [SerializeField]
    AbstractMap _map;

    [SerializeField]
    Transform[] _waypoints;
    private List<Vector3> _cachedWaypoints;

    [SerializeField]
    [Range(1, 10)]
    private float UpdateFrequency = 2;

    [SerializeField]
    GameObject boxPrefab; // �ڽ� �������� ��ġ�ϱ� ���� ����

    [SerializeField]
    Transform player; // �÷��̾� ��ġ
    [SerializeField]
    Transform arrow; // ȭ��ǥ ������Ʈ (�÷��̾� �� �Ʒ��� ��ġ)

    private Directions _directions;
    private GameObject _directionsGO;
    private bool _recalculateNext;
    private List<GameObject> boxes = new List<GameObject>(); // ������ �ڽ����� ����Ʈ

    private float debugLogTimer = 0f; // ����� Ÿ�̸�
    private float debugLogInterval = 0.5f; // 0.5�ʸ��� �α� ���
    public TextMeshProUGUI log5;

    protected virtual void Awake()
    {
        if (_map == null)
        {
            _map = FindObjectOfType<AbstractMap>();
        }
        _directions = MapboxAccess.Instance.Directions;
        _map.OnInitialized += Query;
        _map.OnUpdated += Query;
    }

    public void Start()
    {
        _cachedWaypoints = new List<Vector3>(_waypoints.Length);
        foreach (var item in _waypoints)
        {
            _cachedWaypoints.Add(item.position);
        }
        _recalculateNext = false;

        StartCoroutine(QueryTimer());
    }

    protected virtual void OnDestroy()
    {
        _map.OnInitialized -= Query;
        _map.OnUpdated -= Query;
    }

    void Query()
    {
        var count = _waypoints.Length;
        var wp = new Vector2d[count];
        for (int i = 0; i < count; i++)
        {
            wp[i] = _waypoints[i].GetGeoPosition(_map.CenterMercator, _map.WorldRelativeScale);
        }
        var _directionResource = new DirectionResource(wp, RoutingProfile.Driving);
        _directionResource.Steps = true;
        _directions.Query(_directionResource, HandleDirectionsResponse);
    }

    public IEnumerator QueryTimer()
    {
        while (true)
        {
            yield return new WaitForSeconds(UpdateFrequency);
            for (int i = 0; i < _waypoints.Length; i++)
            {
                if (_waypoints[i].position != _cachedWaypoints[i])
                {
                    _recalculateNext = true;
                    _cachedWaypoints[i] = _waypoints[i].position;
                }
            }

            if (_recalculateNext)
            {
                Query();
                _recalculateNext = false;
            }
        }
    }

    void HandleDirectionsResponse(DirectionsResponse response)
    {
        if (response == null || response.Routes == null || response.Routes.Count < 1)
        {
            return;
        }

        // �ڽ� ���� ���� ���� �ڽ� ����
        foreach (var box in boxes)
        {
            Destroy(box);
        }
        boxes.Clear();

        // ��θ� ���� �ڽ��� ��ġ (ù ��°�� ������ ���� ����)
        var routePoints = response.Routes[0].Geometry;
        for (int i = 1; i < routePoints.Count - 1; i++)  // ù ��°(i=0)�� ������(i=routePoints.Count-1) ����
        {
            Vector3 worldPos = Conversions.GeoToWorldPosition(routePoints[i].x, routePoints[i].y, _map.CenterMercator, _map.WorldRelativeScale).ToVector3xz();
            worldPos.y += 1f; // Y���� 1��ŭ �ø�

            // �ڽ� ����
            GameObject box = Instantiate(boxPrefab, worldPos, Quaternion.identity);
            boxes.Add(box);
        }
    }

    void Update()
    {
        GameObject closestBox = FindClosestBox();

        if (closestBox != null)
        {
            // ȭ��ǥ�� ���� ����� �ڽ� �������� ȸ����Ŵ (Y�� �������θ�)
            Vector3 direction = closestBox.transform.position - player.position;
            direction.y = 0; // Y�� ȸ���� ���� ���� ���⸸ ���

            if (direction.sqrMagnitude > 0.1f) // �ʹ� ª�� �Ÿ��� ȸ������ ����
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                arrow.rotation = Quaternion.Slerp(arrow.rotation, lookRotation, Time.deltaTime * 5f); // �ε巴�� ȸ��
            }

            // �÷��̾ �ڽ��� ������ ������ �ڽ��� ����
            if (Vector3.Distance(player.position, closestBox.transform.position) < 8f)
            {
                Destroy(closestBox);
                boxes.Remove(closestBox);
            }
        }
        else
        {
            // �ڽ��� ���� �� waypoint[1]�� ȸ��
            Vector3 directionToWaypoint = _waypoints[1].position - player.position;
            directionToWaypoint.y = 0; // Y�� ȸ���� ���� ���� ���⸸ ���

            if (directionToWaypoint.sqrMagnitude > 0.1f) // �ʹ� ª�� �Ÿ��� ȸ������ ����
            {
                Quaternion lookRotation = Quaternion.LookRotation(directionToWaypoint);
                arrow.rotation = Quaternion.Slerp(arrow.rotation, lookRotation, Time.deltaTime * 5f); // �ε巴�� ȸ��
            }
        }

        // ����� Ÿ�̸Ӹ� �̿��Ͽ� 0.5�ʸ��� ȭ��ǥ�� rotation.y ���� ���
        debugLogTimer += Time.deltaTime;
        if (debugLogTimer >= debugLogInterval)
        {
            log5.text = $"RED Rotation.Y: {arrow.rotation.eulerAngles.y}";
            debugLogTimer = 0f; // Ÿ�̸� ����
        }
    }

    // ���� ����� �ڽ��� ã�� �Լ�
    GameObject FindClosestBox()
    {
        GameObject closest = null;
        float minDistance = Mathf.Infinity;

        foreach (var box in boxes)
        {
            float distance = (box.transform.position - player.position).sqrMagnitude;
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = box;
            }
        }

        return closest;
    }
}
