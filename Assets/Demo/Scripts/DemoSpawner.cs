using System.Collections.Generic;
using AngryKoala.Services;
using UnityEngine;

namespace AngryKoala.Input.Demo
{
    public sealed class DemoSpawner : MonoBehaviour
    {
        [SerializeField]
        private float _cubeSize = 0.4f;

        private readonly List<GameObject> _cubes = new();

        private IInputSurface _screen;
        private Camera _camera;

        private static readonly Plane _ground = new(Vector3.up, Vector3.zero);

        private void Start()
        {
            var inputService = ServiceLocator.Get<IInputService>();

            _camera = Camera.main;

            _screen = inputService.Screen;
            _screen.OnTap += HandleTap;
            _screen.OnHold += HandleHold;
        }

        public void Clear()
        {
            for (var index = 0; index < _cubes.Count; index++)
            {
                Destroy(_cubes[index]);
            }

            _cubes.Clear();
        }

        private void HandleTap(TouchData touch)
        {
            var ray = _camera.ScreenPointToRay(touch.Position);

            if (!_ground.Raycast(ray, out var distance))
            {
                return;
            }

            var point = ray.GetPoint(distance);
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            // The primitive comes with a collider that nothing here needs.
            Destroy(cube.GetComponent<Collider>());

            cube.transform.position = point + Vector3.up * (_cubeSize * 0.5f);
            cube.transform.localScale = Vector3.one * _cubeSize;

            _cubes.Add(cube);
        }

        private void HandleHold(TouchData touch)
        {
            Clear();
        }

        private void OnDestroy()
        {
            if (_screen == null)
            {
                return;
            }

            _screen.OnTap -= HandleTap;
            _screen.OnHold -= HandleHold;
        }
    }
}
