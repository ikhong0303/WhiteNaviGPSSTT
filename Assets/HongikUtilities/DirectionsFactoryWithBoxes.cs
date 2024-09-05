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
    GameObject boxPrefab; // 박스 프리팹을 배치하기 위한 변수

    [SerializeField]
    Transform player; // 플레이어 위치
    [SerializeField]
    Transform arrow; // 화살표 오브젝트 (플레이어 발 아래에 위치)

    private Directions _directions;
    private GameObject _directionsGO;
    private bool _recalculateNext;
    private List<GameObject> boxes = new List<GameObject>(); // 생성된 박스들의 리스트

    private float debugLogTimer = 0f; // 디버그 타이머
    private float debugLogInterval = 0.5f; // 0.5초마다 로그 출력
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

        // 박스 생성 전에 기존 박스 제거
        foreach (var box in boxes)
        {
            Destroy(box);
        }
        boxes.Clear();

        // 경로를 따라 박스를 배치 (첫 번째와 마지막 지점 제외)
        var routePoints = response.Routes[0].Geometry;
        for (int i = 1; i < routePoints.Count - 1; i++)  // 첫 번째(i=0)와 마지막(i=routePoints.Count-1) 제외
        {
            Vector3 worldPos = Conversions.GeoToWorldPosition(routePoints[i].x, routePoints[i].y, _map.CenterMercator, _map.WorldRelativeScale).ToVector3xz();
            worldPos.y += 1f; // Y값을 1만큼 올림

            // 박스 생성
            GameObject box = Instantiate(boxPrefab, worldPos, Quaternion.identity);
            boxes.Add(box);
        }
    }

    void Update()
    {
        GameObject closestBox = FindClosestBox();

        if (closestBox != null)
        {
            // 화살표를 가장 가까운 박스 방향으로 회전시킴 (Y축 기준으로만)
            Vector3 direction = closestBox.transform.position - player.position;
            direction.y = 0; // Y축 회전을 위해 수평 방향만 고려

            if (direction.sqrMagnitude > 0.1f) // 너무 짧은 거리는 회전하지 않음
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                arrow.rotation = Quaternion.Slerp(arrow.rotation, lookRotation, Time.deltaTime * 5f); // 부드럽게 회전
            }

            // 플레이어가 박스에 가까이 있으면 박스를 제거
            if (Vector3.Distance(player.position, closestBox.transform.position) < 8f)
            {
                Destroy(closestBox);
                boxes.Remove(closestBox);
            }
        }
        else
        {
            // 박스가 없을 때 waypoint[1]로 회전
            Vector3 directionToWaypoint = _waypoints[1].position - player.position;
            directionToWaypoint.y = 0; // Y축 회전을 위해 수평 방향만 고려

            if (directionToWaypoint.sqrMagnitude > 0.1f) // 너무 짧은 거리는 회전하지 않음
            {
                Quaternion lookRotation = Quaternion.LookRotation(directionToWaypoint);
                arrow.rotation = Quaternion.Slerp(arrow.rotation, lookRotation, Time.deltaTime * 5f); // 부드럽게 회전
            }
        }

        // 디버그 타이머를 이용하여 0.5초마다 화살표의 rotation.y 값을 출력
        debugLogTimer += Time.deltaTime;
        if (debugLogTimer >= debugLogInterval)
        {
            log5.text = $"RED Rotation.Y: {arrow.rotation.eulerAngles.y}";
            debugLogTimer = 0f; // 타이머 리셋
        }
    }

    // 가장 가까운 박스를 찾는 함수
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
