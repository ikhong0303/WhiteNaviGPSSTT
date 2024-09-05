using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Geocoding;
using Mapbox.Utils;
using Mapbox.Unity.Map;
using Mapbox.Unity;
using UnityEngine.UI;
using TMPro;
using Mapbox.Directions;
using Samples.Whisper;

public class DestinationChange : MonoBehaviour
{
    public AudioSource StartSFX;

    [SerializeField]
    private AbstractMap _map;  // 기존 맵을 참조

    private ForwardGeocodeResource _geocodeResource;
    private Geocoder _geocoder;
    public TMP_InputField addressInputField;

    public GameObject Destination;
    public void OnSearchButtonClicked()
    {
        string address = addressInputField.text;
        UpdateMapWithAddress(address);
    }

    void Start()
    {
        // Geocoder 초기화
        _geocoder = MapboxAccess.Instance.Geocoder;
    }

    // 주소를 위도와 경도로 변환하고 맵을 업데이트하는 메서드
    public void UpdateMapWithAddress(string address)
    {
        // 주소를 기반으로 Geocoding 요청 생성
        _geocodeResource = new ForwardGeocodeResource(address);
        _geocodeResource.Autocomplete = false;

        // Geocoding API 요청 보내기
        _geocoder.Geocode(_geocodeResource, HandleGeocodeResponse);
    }
    public TextMeshProUGUI log4;
    // Geocoding 응답을 처리하는 메서드
    private void HandleGeocodeResponse(ForwardGeocodeResponse response)
    {
        if (response == null || response.Features == null || response.Features.Count == 0)
        {
            log4.text = "Geocode 요청 실패: 주소를 찾을 수 없습니다.";
            return;
        }

        // 첫 번째 결과를 사용하여 위도와 경도를 얻음
        var feature = response.Features[0];
        Vector2d coordinates = feature.Center;

        // 맵을 업데이트
        log4.text = "경로를안내합니다";
        StartSFX.Play();
        _map.UpdateMap(coordinates, _map.Zoom);  // 기존 줌 레벨을 사용하여 맵 중심을 업데이트
        Destination.transform.position = Vector3.zero;
        
    }
}
