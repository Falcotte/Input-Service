using System;
using UnityEngine;

namespace AngryKoala.Input.Tests
{
    internal static class JoystickAreaFactory
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

            public InputAreaFields(JoystickMode joystickMode, Vector2 fixedJoystickPosition, float joystickRadius)
            {
                _space = (int)InputAreaSpace.Screen;
                _minimum = Vector2.zero;
                _maximum = new Vector2(0.5f, 1f);
                _joystickMode = (int)joystickMode;
                _joystickRadius = joystickRadius;
                _joystickDeadZone = 0.1f;
                _fixedJoystickPosition = fixedJoystickPosition;
            }
        }

        /// <summary>
        /// An area covering the left half of the screen, with a joystick radius of 1 cm unless given.
        /// </summary>
        public static InputArea CreateLeftHalfArea(JoystickMode joystickMode, Vector2 fixedJoystickPosition = default,
            float joystickRadius = 1f)
        {
            var area = ScriptableObject.CreateInstance<InputArea>();
            area.name = "Left Half " + joystickMode;

            var fields = new InputAreaFields(joystickMode, fixedJoystickPosition, joystickRadius);

            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(fields), area);

            return area;
        }
    }
}
