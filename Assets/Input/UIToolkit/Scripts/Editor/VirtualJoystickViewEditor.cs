using UnityEditor;
using UnityEngine;

namespace AngryKoala.Input
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(VirtualJoystickView))]
    public sealed class VirtualJoystickViewEditor : Editor
    {
        private SerializedProperty _areaProperty;

        private SerializedObject _areaObject;
        private Object _shownArea;

        private static GUIContent _titleContent;

        private const string _areaPropertyName = "_area";
        private const string _modePropertyName = "_joystickMode";
        private const string _radiusPropertyName = "_joystickRadius";
        private const string _deadZonePropertyName = "_joystickDeadZone";
        private const string _fixedPositionPropertyName = "_fixedJoystickPosition";

        private const string _noAreaMessage = "Assign an area to choose the joystick's mode.";
        private const string _differentAreasMessage = "The selected views use different areas.";
        private const string _assetMessage = "These settings are stored on the area asset.";
        private const string _unreadableAreaMessage = "The area's joystick settings could not be read.";

        private void OnEnable()
        {
            _areaProperty = serializedObject.FindProperty(_areaPropertyName);
        }

        private void OnDisable()
        {
            DisposeAreaObject();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            DrawAreaJoystick();
        }

        private void DrawAreaJoystick()
        {
            _titleContent ??= new GUIContent("Area Joystick");

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(_titleContent, EditorStyles.boldLabel);

                if (_areaProperty.hasMultipleDifferentValues)
                {
                    EditorGUILayout.LabelField(_differentAreasMessage, EditorStyles.wordWrappedMiniLabel);
                    return;
                }

                Object area = _areaProperty.objectReferenceValue;

                if (area == null)
                {
                    DisposeAreaObject();
                    EditorGUILayout.LabelField(_noAreaMessage, EditorStyles.wordWrappedMiniLabel);
                    return;
                }

                DrawAreaProperties(area);
            }
        }

        private void DrawAreaProperties(Object area)
        {
            if (_areaObject == null || _shownArea != area)
            {
                DisposeAreaObject();

                _areaObject = new SerializedObject(area);
                _shownArea = area;
            }

            _areaObject.Update();

            SerializedProperty mode = _areaObject.FindProperty(_modePropertyName);
            SerializedProperty radius = _areaObject.FindProperty(_radiusPropertyName);
            SerializedProperty deadZone = _areaObject.FindProperty(_deadZonePropertyName);
            SerializedProperty fixedPosition = _areaObject.FindProperty(_fixedPositionPropertyName);

            if (mode == null || radius == null || deadZone == null || fixedPosition == null)
            {
                EditorGUILayout.HelpBox(_unreadableAreaMessage, MessageType.Warning);
                return;
            }

            var joystickMode = (JoystickMode)mode.intValue;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(mode);

            EditorGUI.BeginDisabledGroup(joystickMode == JoystickMode.None);
            EditorGUILayout.PropertyField(radius);
            EditorGUILayout.PropertyField(deadZone);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(joystickMode != JoystickMode.Fixed);
            EditorGUILayout.PropertyField(fixedPosition);
            EditorGUI.EndDisabledGroup();

            if (EditorGUI.EndChangeCheck())
            {
                _areaObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(area);
            }

            EditorGUILayout.LabelField(_assetMessage, EditorStyles.wordWrappedMiniLabel);
        }

        #region Utility

        private void DisposeAreaObject()
        {
            _areaObject?.Dispose();
            _areaObject = null;
            _shownArea = null;
        }

        #endregion
    }
}
