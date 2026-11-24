using AngryKoala.Services;
using UnityEngine;

namespace AngryKoala.Input.Demo
{
    public sealed class DemoPlayer : MonoBehaviour
    {
        [SerializeField]
        private InputArea _area;

        [SerializeField]
        private float _speed = 4f;

        [SerializeField]
        private float _hopHeight = 1f;

        [SerializeField]
        private float _hopDuration = 0.4f;

        private IInputSurface _surface;
        private float _groundHeight;
        private float _hopTimer;
        private bool _isHopping;

        private void Start()
        {
            var inputService = ServiceLocator.Get<IInputService>();

            _surface = inputService.GetArea(_area);
            _surface.OnTap += HandleTap;

            _groundHeight = transform.position.y;
        }

        private void HandleTap(TouchData touch)
        {
            _hopTimer = 0f;
            _isHopping = true;
        }

        private void Update()
        {
            var direction = _surface.Joystick.Direction;
            var position = transform.position;

            position += new Vector3(direction.x, 0f, direction.y) * (_speed * Time.deltaTime);
            position.y = _groundHeight + GetHopOffset();

            transform.position = position;
        }

        private float GetHopOffset()
        {
            if (!_isHopping)
            {
                return 0f;
            }

            _hopTimer += Time.deltaTime;

            if (_hopTimer >= _hopDuration)
            {
                _isHopping = false;
                return 0f;
            }

            return Mathf.Sin(_hopTimer / _hopDuration * Mathf.PI) * _hopHeight;
        }

        private void OnDestroy()
        {
            if (_surface != null)
            {
                _surface.OnTap -= HandleTap;
            }
        }
    }
}
