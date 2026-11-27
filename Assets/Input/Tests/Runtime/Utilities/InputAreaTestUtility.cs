using System;
using UnityEngine;

namespace AngryKoala.Input.Tests
{
    internal static class InputAreaTestUtility
    {
        [Serializable]
        private sealed class InputAreaFields
        {
            [SerializeField]
            private int _space;

            [SerializeField]
            private Vector2 _minimum;

            [SerializeField]
            private Vector2 _maximum;

            [SerializeField]
            private int _joystickMode;

            [SerializeField]
            private float _joystickRadius;

            [SerializeField]
            private float _joystickDeadZone;

            [SerializeField]
            private Vector2 _fixedJoystickPosition;

            public InputAreaFields(InputAreaSpace space, Vector2 minimum, Vector2 maximum, JoystickMode joystickMode,
                float joystickRadius, float joystickDeadZone, Vector2 fixedJoystickPosition)
            {
                _space = (int)space;
                _minimum = minimum;
                _maximum = maximum;
                _joystickMode = (int)joystickMode;
                _joystickRadius = joystickRadius;
                _joystickDeadZone = joystickDeadZone;
                _fixedJoystickPosition = fixedJoystickPosition;
            }
        }

        [Serializable]
        private sealed class InputAreaJoystickModeField
        {
            [SerializeField]
            private int _joystickMode;

            public InputAreaJoystickModeField(JoystickMode joystickMode)
            {
                _joystickMode = (int)joystickMode;
            }
        }

        public static InputArea CreateArea(string areaName, InputAreaSpace space, Vector2 minimum, Vector2 maximum,
            JoystickMode joystickMode = JoystickMode.None, float joystickRadius = 1f, float joystickDeadZone = 0.1f,
            Vector2 fixedJoystickPosition = default)
        {
            var area = ScriptableObject.CreateInstance<InputArea>();
            area.name = areaName;

            var fields = new InputAreaFields(space, minimum, maximum, joystickMode, joystickRadius, joystickDeadZone,
                fixedJoystickPosition);

            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(fields), area);

            return area;
        }

        public static void SetJoystickMode(InputArea area, JoystickMode joystickMode)
        {
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(new InputAreaJoystickModeField(joystickMode)), area);
        }
    }
}
