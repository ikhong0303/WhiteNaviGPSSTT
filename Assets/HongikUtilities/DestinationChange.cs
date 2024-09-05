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
    private AbstractMap _map;  // ���� ���� ����

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
        // Geocoder �ʱ�ȭ
        _geocoder = MapboxAccess.Instance.Geocoder;
    }

    // �ּҸ� ������ �浵�� ��ȯ�ϰ� ���� ������Ʈ�ϴ� �޼���
    public void UpdateMapWithAddress(string address)
    {
        // �ּҸ� ������� Geocoding ��û ����
        _geocodeResource = new ForwardGeocodeResource(address);
        _geocodeResource.Autocomplete = false;

        // Geocoding API ��û ������
        _geocoder.Geocode(_geocodeResource, HandleGeocodeResponse);
    }
    public TextMeshProUGUI log4;
    // Geocoding ������ ó���ϴ� �޼���
    private void HandleGeocodeResponse(ForwardGeocodeResponse response)
    {
        if (response == null || response.Features == null || response.Features.Count == 0)
        {
            log4.text = "Geocode ��û ����: �ּҸ� ã�� �� �����ϴ�.";
            return;
        }

        // ù ��° ����� ����Ͽ� ������ �浵�� ����
        var feature = response.Features[0];
        Vector2d coordinates = feature.Center;

        // ���� ������Ʈ
        log4.text = "��θ��ȳ��մϴ�";
        StartSFX.Play();
        _map.UpdateMap(coordinates, _map.Zoom);  // ���� �� ������ ����Ͽ� �� �߽��� ������Ʈ
        Destination.transform.position = Vector3.zero;
        
    }
}
