using AngryKoala.Services;
using UnityEngine;

namespace AngryKoala.Input.Demo
{
    public sealed class DemoGestures : MonoBehaviour
    {
        [SerializeField]
        private Transform _player;

        private IInputService _inputService;
        private IInputSurface _screen;
        private Renderer _playerRenderer;
        private Material _playerMaterial;
        private Color _startColor;
        private Vector3 _startScale;

        private const float _minimumScale = 0.5f;
        private const float _maximumScale = 3f;

        private void Start()
        {
            _inputService = ServiceLocator.Get<IInputService>();

            _playerRenderer = _player.GetComponent<Renderer>();
            _playerMaterial = _playerRenderer.material;
            _startColor = _playerMaterial.color;
            _startScale = _player.localScale;

            _screen = _inputService.Screen;
            _screen.OnSwipe += HandleSwipe;
            _screen.OnPinch += HandlePinch;
            _inputService.OnBackPressed += HandleBackPressed;
        }

        private void HandleSwipe(SwipeData swipe)
        {
            _playerMaterial.color = swipe.Direction switch
            {
                SwipeDirection.Up => Color.yellow,
                SwipeDirection.Down => Color.cyan,
                SwipeDirection.Left => Color.magenta,
                _ => Color.green
            };
        }

        private void HandlePinch(PinchData pinch)
        {
            var scale = Mathf.Clamp(_player.localScale.x * pinch.ScaleDelta, _minimumScale, _maximumScale);

            _player.localScale = Vector3.one * scale;
        }

        private void HandleBackPressed()
        {
            _playerMaterial.color = _startColor;
            _player.localScale = _startScale;
        }

        private void OnDestroy()
        {
            if (_playerMaterial != null)
            {
                Destroy(_playerMaterial);
            }

            if (_screen == null)
            {
                return;
            }

            _screen.OnSwipe -= HandleSwipe;
            _screen.OnPinch -= HandlePinch;
            _inputService.OnBackPressed -= HandleBackPressed;
        }
    }
}
