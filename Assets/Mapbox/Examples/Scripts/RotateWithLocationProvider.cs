namespace Mapbox.Examples
{
    using Mapbox.Unity.Location;
    using Mapbox.Unity.Map;
    using Mapbox.Utils;
    using TMPro;
    using UnityEngine;

    public class RotateWithLocationProvider : MonoBehaviour
    {
        public TextMeshProUGUI log1;
        public TextMeshProUGUI log2;
        public TextMeshProUGUI log3;

        [SerializeField]
        AbstractMap _map;  // ���� (����)

        [SerializeField]
        Transform player;  // �÷��̾� ĳ���� (���� ���� ĳ����)

        [SerializeField]
        bool _useDeviceOrientation;  // ��� ������ ������� ����

        [SerializeField]
        float magneticDeclination = 0f;  // �ںϰ� ���� ���� (�ʿ� �� ����)

        [SerializeField]
        float _rotationFollowFactor = 5f;  // ȸ�� �ӵ� ���� (�������� ���� ��)

        ILocationProvider _locationProvider;
        Quaternion _targetRotation;  // ��ǥ ȸ�� �� ����

        public ILocationProvider LocationProvider
        {
            private get
            {
                if (_locationProvider == null)
                {
                    _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
                }

                return _locationProvider;
            }
            set
            {
                if (_locationProvider != null)
                {
                    _locationProvider.OnLocationUpdated -= LocationProvider_OnLocationUpdated;
                }
                _locationProvider = value;
                _locationProvider.OnLocationUpdated += LocationProvider_OnLocationUpdated;
            }
        }

        void Start()
        {
            LocationProvider.OnLocationUpdated += LocationProvider_OnLocationUpdated;
        }

        void OnDestroy()
        {
            if (LocationProvider != null)
            {
                LocationProvider.OnLocationUpdated -= LocationProvider_OnLocationUpdated;
            }
        }

        void LocationProvider_OnLocationUpdated(Location location)
        {
            // GPS ��ġ�� ��ȿ���� Ȯ��
            if (location.LatitudeLongitude != Vector2d.zero)
            {
                // ���� ��ħ�� ������ �޾ƿ� (�ں� ����)
                float rotationAngle = _useDeviceOrientation ? location.DeviceOrientation : location.UserHeading;

                // �ںϰ� ���� ���� ���� (�ںϰ� ������ ���̸� ����)
                rotationAngle -= magneticDeclination;

                // ��ǥ ȸ������ ���� (Y�ุ ȸ��)
                _targetRotation = Quaternion.Euler(0, rotationAngle, 0);

                // �α� ��� (���� ����)
                Debug.Log($"Player aligned to Heading: {rotationAngle}");
                log1.text = $"Device Orientation (True North): {location.DeviceOrientation}";
                log2.text = $"User Heading (GPS Movement Direction): {location.UserHeading}, Adjusted Heading: {rotationAngle}";
                log3.text = $"Calculated Rotation: {rotationAngle}";
            }
        }

        void Update()
        {
            // �� �����Ӹ��� ��ǥ ȸ�������� �ε巴�� ȸ��
            player.rotation = Quaternion.Lerp(player.rotation, _targetRotation, Time.deltaTime * _rotationFollowFactor);
        }
    }
}
