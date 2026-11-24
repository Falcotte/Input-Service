using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace AngryKoala.Input.Tests
{
    public abstract class InputServiceAreaTestBase : InputServiceTestBase
    {
        private readonly List<InputArea> _areas = new();

        protected static readonly Vector2 LeftHalfMinimum = Vector2.zero;
        protected static readonly Vector2 LeftHalfMaximum = new(0.5f, 1f);
        protected static readonly Vector2 RightHalfMinimum = new(0.5f, 0f);
        protected static readonly Vector2 RightHalfMaximum = Vector2.one;

        protected static Vector2 PointInLeftHalf => ScreenPoint(0.25f, 0.5f);

        protected static Vector2 PointInRightHalf => ScreenPoint(0.75f, 0.5f);

        [UnityTearDown]
        public IEnumerator DestroyAreas()
        {
            foreach (var area in _areas)
            {
                if (area != null)
                {
                    Object.Destroy(area);
                }
            }

            _areas.Clear();

            yield return null;
        }

        protected InputArea CreateArea(string areaName, Vector2 minimum, Vector2 maximum,
            JoystickMode joystickMode = JoystickMode.None, float joystickRadius = 1f, float joystickDeadZone = 0.1f,
            Vector2 fixedJoystickPosition = default)
        {
            var area = InputAreaTestUtility.CreateArea(areaName, InputAreaSpace.Screen, minimum, maximum, joystickMode,
                joystickRadius, joystickDeadZone, fixedJoystickPosition);

            _areas.Add(area);

            return area;
        }

        protected InputArea CreateLeftHalfArea(string areaName = "Left Half")
        {
            return CreateArea(areaName, LeftHalfMinimum, LeftHalfMaximum);
        }

        protected InputArea CreateRightHalfArea(string areaName = "Right Half")
        {
            return CreateArea(areaName, RightHalfMinimum, RightHalfMaximum);
        }
    }
}
