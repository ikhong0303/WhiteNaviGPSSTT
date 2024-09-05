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
        AbstractMap _map;  // 지도 (고정)

        [SerializeField]
        Transform player;  // 플레이어 캐릭터 (지도 위의 캐릭터)

        [SerializeField]
        bool _useDeviceOrientation;  // 기기 방향을 사용할지 여부

        [SerializeField]
        float magneticDeclination = 0f;  // 자북과 진북 차이 (필요 시 수정)

        [SerializeField]
        float _rotationFollowFactor = 5f;  // 회전 속도 보정 (기존보다 높은 값)

        ILocationProvider _locationProvider;
        Quaternion _targetRotation;  // 목표 회전 값 저장

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
            // GPS 위치가 유효한지 확인
            if (location.LatitudeLongitude != Vector2d.zero)
            {
                // 현재 나침반 방향을 받아옴 (자북 기준)
                float rotationAngle = _useDeviceOrientation ? location.DeviceOrientation : location.UserHeading;

                // 자북과 진북 차이 보정 (자북과 진북의 차이를 적용)
                rotationAngle -= magneticDeclination;

                // 목표 회전값을 설정 (Y축만 회전)
                _targetRotation = Quaternion.Euler(0, rotationAngle, 0);

                // 로그 출력 (선택 사항)
                Debug.Log($"Player aligned to Heading: {rotationAngle}");
                log1.text = $"Device Orientation (True North): {location.DeviceOrientation}";
                log2.text = $"User Heading (GPS Movement Direction): {location.UserHeading}, Adjusted Heading: {rotationAngle}";
                log3.text = $"Calculated Rotation: {rotationAngle}";
            }
        }

        void Update()
        {
            // 매 프레임마다 목표 회전값으로 부드럽게 회전
            player.rotation = Quaternion.Lerp(player.rotation, _targetRotation, Time.deltaTime * _rotationFollowFactor);
        }
    }
}
