using Assets.World;
using UnityEngine;

namespace Assets.Scripts
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] float _stickMinZoom, _stickMaxZoom;
        [SerializeField] float _swivelMinZoom, _swivelMaxZoom;
        [SerializeField] float _moveSpeedMinZoom, _moveSpeedMaxZoom;
        [SerializeField] float _rotationSpeed;
        [SerializeField] GameMap _gameMap;

        static CameraController _instance;

        Transform _swivel, _stick;
        float _zoom = 1f;
        float _rotationAngle;

        #region Unity life-cycle methods
        void Awake()
        {
            _instance = this;
            _swivel = transform.GetChild(0);
            _stick = _swivel.GetChild(0);
        }

        void Start()
        {
            // initialization
            _instance.AdjustZoom(-0.4f); // start position is zero and we want 60% zoom so we subtract 0.4f
            _instance.AdjustRotation(0f);

            Vector3 mapPos = _gameMap.transform.position;
            Vector3 camPos = _instance.gameObject.transform.localPosition;
            _instance.gameObject.transform.localPosition = new Vector3(mapPos.x, camPos.y, camPos.z);
        }

        void Update()
        {
            float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
            if (zoomDelta != 0f)
                AdjustZoom(zoomDelta);

            float rotationDelta = Input.GetAxis("Rotation");
            if (rotationDelta != 0f)
                AdjustRotation(rotationDelta);

            float xDelta = Input.GetAxis("Horizontal");
            float zDelta = Input.GetAxis("Vertical");
            if (xDelta != 0f || zDelta != 0f)
                AdjustPosition(xDelta, zDelta);
        }
        #endregion

        void AdjustZoom(float delta)
        {
            _zoom = Mathf.Clamp01(_zoom + delta);

            float distance = Mathf.Lerp(_stickMinZoom, _stickMaxZoom, _zoom);
            _stick.localPosition = new Vector3(0f, 0f, distance);

            float angle = Mathf.Lerp(_swivelMinZoom, _swivelMaxZoom, _zoom);
            _swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
        }

        void AdjustRotation(float delta)
        {
            _rotationAngle += delta * _rotationSpeed * Time.deltaTime;

            if (_rotationAngle < 0f)
                _rotationAngle += 360f;
            else if (_rotationAngle >= 360f)
                _rotationAngle -= 360f;

            transform.localRotation = Quaternion.Euler(0f, _rotationAngle, 0f);
        }

        void AdjustPosition(float xDelta, float zDelta)
        {
            Vector3 direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
            float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
            float distance = Mathf.Lerp(_moveSpeedMinZoom, _moveSpeedMaxZoom, _zoom) * damping * Time.deltaTime;

            Vector3 position = transform.localPosition;
            position += direction * distance;
            transform.localPosition = ClampPosition(position);
        }

        Vector3 ClampPosition(Vector3 position)
        {
            float xMax = (GameMap.GridSizeX - 0.5f) * GameMap.CELL_SIZE;
            position.x = Mathf.Clamp(position.x, 0f, xMax);

            float zMax = (GameMap.GridSizeY - 1) * (1.5f * GameMap.CELL_SIZE);
            position.z = Mathf.Clamp(position.z, 0f, zMax);

            return position;
        }

        Vector3 WrapPosition(Vector3 position)
        {
            float width = GameMap.GridSizeX * GameMap.CELL_SIZE;
            while (position.x < 0f)
                position.x += width;

            while (position.x > width)
                position.x -= width;

            float zMax = (GameMap.GridSizeY - 1) * (1.5f * GameMap.CELL_SIZE);
            position.z = Mathf.Clamp(position.z, 0f, zMax);

            return position;
        }
    }
}
