namespace Mapbox.Examples
{
    using Mapbox.Unity.Location;
    using TMPro;
    using UnityEngine;

    public class RotateWithLocationProvider : MonoBehaviour
    {
        /// <summary>
        /// Location property used for rotation: false=Heading (default), true=Orientation  
        /// </summary>
        [SerializeField]
        [Tooltip("Per default 'UserHeading' (direction the device is moving) is used for rotation. Check to use 'DeviceOrientation' (where the device is facing)")]
        bool _useDeviceOrientation;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        [Tooltip("Only evaluated when 'Use Device Orientation' is checked. Subtracts UserHeading from DeviceOrientation. Useful if map is rotated by UserHeading and DeviceOrientation should be displayed relative to the heading.")]
        bool _subtractUserHeading;

        /// <summary>
        /// The rate at which the transform's rotation tries to catch up to the provided heading.  
        /// </summary>
        [SerializeField]
        [Tooltip("The rate at which the transform's rotation tries to catch up to the provided heading.")]
        float _rotationFollowFactor = 1;

        /// <summary>
        /// Set this to true if you'd like to adjust the rotation of a RectTransform (in a UI canvas) with the heading.
        /// </summary>
        [SerializeField]
        bool _rotateZ;

        /// <summary>
        /// <para>Set this to true if you'd like to adjust the sign of the rotation angle.</para>
        /// <para>eg angle passed in 63.5, angle that should be used for rotation: -63.5.</para>
        /// <para>This might be needed when rotating the map and not objects on the map.</para>
        /// </summary>
        [SerializeField]
        [Tooltip("Set this to true if you'd like to adjust the sign of the rotation angle. eg angle passed in 63.5, angle that should be used for rotation: -63.5.")]
        bool _useNegativeAngle;

        /// <summary>
        /// Use a mock <see cref="T:Mapbox.Unity.Location.TransformLocationProvider"/>,
        /// rather than a <see cref="T:Mapbox.Unity.Location.EditorLocationProvider"/>.
        /// </summary>
        [SerializeField]
        bool _useTransformLocationProvider;

        Quaternion _targetRotation;

        /// <summary>
        /// The location provider.
        /// This is public so you change which concrete <see cref="ILocationProvider"/> to use at runtime.  
        /// </summary>
        ILocationProvider _locationProvider;
        public ILocationProvider LocationProvider
        {
            private get
            {
                if (_locationProvider == null)
                {
                    _locationProvider = _useTransformLocationProvider ?
                        LocationProviderFactory.Instance.TransformLocationProvider : LocationProviderFactory.Instance.DefaultLocationProvider;
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

        Vector3 _targetPosition;

        public TextMeshProUGUI log1;
        public TextMeshProUGUI log2;
        public TextMeshProUGUI log3;

        // 자북과 진북 간의 차이를 보정하는 값 (위치에 따라 다르므로 사용자에 의해 설정되어야 합니다)
        float magneticDeclination = 10.0f; // 이 값을 실제 위치에 맞게 조정

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
            // GPS의 방향과 나침반 데이터를 로그로 출력하여 확인
            float rotationAngle = _useDeviceOrientation ? location.DeviceOrientation : location.UserHeading;

            // 자북과 진북 간 차이를 보정
            rotationAngle += magneticDeclination;

            if (_useNegativeAngle)
            {
                rotationAngle *= -1f;
            }

            // 최적화: rotationAngle 값을 360도 내로 보정
            rotationAngle = (rotationAngle + 360) % 360;

            // 수정된 부분: 복잡한 조건문을 단순화하여 연산 속도 개선
            if (_useDeviceOrientation)
            {
                if (_subtractUserHeading)
                {
                    rotationAngle = (location.UserHeading - rotationAngle + 360) % 360;  // 간단한 보정
                }
                _targetRotation = Quaternion.Euler(getNewEulerAngles(rotationAngle));
            }
            else
            {
                if (location.IsUserHeadingUpdated)
                {
                    _targetRotation = Quaternion.Euler(getNewEulerAngles(rotationAngle));
                }
            }

            // 로그로 위치 및 방향 정보 출력
            log1.text = $"Device Orientation (True North): {location.DeviceOrientation}";
            log2.text = $"User Heading (GPS Movement Direction): {location.UserHeading}, Adjusted Heading: {rotationAngle}";
            log3.text = $"Calculated Rotation: {rotationAngle}";
        }

        private Vector3 getNewEulerAngles(float newAngle)
        {
            var localRotation = transform.localRotation;
            var currentEuler = localRotation.eulerAngles;
            var euler = Mapbox.Unity.Constants.Math.Vector3Zero;

            // Z 축 회전 사용 여부에 따른 Euler 각 설정
            if (_rotateZ)
            {
                euler.z = -newAngle;
                euler.x = currentEuler.x;
                euler.y = currentEuler.y;
            }
            else
            {
                euler.y = -newAngle;
                euler.x = currentEuler.x;
                euler.z = currentEuler.z;
            }

            return euler;
        }

        void Update()
        {
            // 회전 적용 시 시간 차를 이용하여 부드럽게 보정 (Lerp 사용)
            transform.localRotation = Quaternion.Lerp(transform.localRotation, _targetRotation, Time.deltaTime * _rotationFollowFactor);
        }
    }
}
