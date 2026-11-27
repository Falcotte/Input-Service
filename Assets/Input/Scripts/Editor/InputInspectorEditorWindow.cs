using System.Collections.Generic;
using AngryKoala.Services;
using UnityEditor;
using UnityEngine;

namespace AngryKoala.Input
{
    public sealed class InputInspectorEditorWindow : EditorWindow
    {
        private IInputService _service;
        private InputService _inputService;

        private bool _autoRefreshEnabled;
        private double _nextAutoRefreshTime;

        private Vector2 _scroll;

        private readonly List<InputSurfaceData> _surfaces = new();
        private readonly List<InputFingerData> _fingers = new();
        private readonly List<object> _blockers = new();
        private readonly List<IPointerFilter> _pointerFilters = new();

        private readonly List<string> _blockerLines = new();
        private readonly List<string> _filterLines = new();
        private string _blockedText = string.Empty;
        private string _filtersText = string.Empty;

        private int _reservedStatusLines = _minimumStatusLines;
        private int _reservedFingerRows = _minimumFingerRows;

        private static GUIStyle _headerStyle;
        private static GUIStyle _cellStyle;
        private static GUIStyle _detailStyle;
        private static GUIStyle _mapLabelStyle;

        private const string _autoRefreshPref = "InputInspector.AutoRefresh";

        private const double _autoRefreshInterval = 0.05d;

        private const float _windowPadding = 8f;
        private const float _boxPadding = 8f;
        private const float _sectionSpacing = 8f;
        private const float _titleHeight = 20f;
        private const float _sectionChromeHeight = _boxPadding * 2f + _titleHeight;
        private const float _lineHeight = 18f;
        private const float _rowHeight = 20f;
        private const float _cellPaddingX = 6f;
        private const float _fitHeaderWeight = 0f;
        private const float _detailIndent = 16f;
        private const float _defaultToolbarHeight = 18f;
        private const float _refreshButtonWidth = 70f;

        private const int _minimumStatusLines = 2;
        private const int _minimumFingerRows = 1;

        private const float _wideMinimumWidth = 760f;
        private const float _wideAspectRatio = 1.6f;
        private const float _wideMapMaximumWidthRatio = 0.5f;

        private const float _mapMinimumHeight = 120f;
        private const float _mapBoxMinimumWidth = 120f;
        private const float _mapBorderThickness = 1f;
        private const float _mapLineThickness = 2f;
        private const float _touchDotRadius = 6f;
        private const float _mapLabelHeight = 14f;
        private const float _mapLabelInset = 2f;

        private const string _noneLabel = "-";
        private const string _ignoredLabel = "ignored";
        private const string _mouseLabel = "Mouse";

        private static readonly string[] _surfaceHeaders =
            { "Surface", "Pixel Rect", "Touches", "Primary Touch", "Joystick", "Last Gesture" };

        private static readonly float[] _surfaceWeights = { 2f, 2.5f, _fitHeaderWeight, 2.5f, 2f, 2f };
        private static readonly float[] _surfaceWidths = new float[6];

        private static readonly string[] _fingerHeaders = { "Finger", "Owner" };
        private static readonly float[] _fingerWeights = { 1f, 3f };
        private static readonly float[] _fingerWidths = new float[2];

        private static readonly GUIContent _measuredContent = new();

        private static readonly Color _headerColor = new(0.5f, 0.5f, 0.5f, 0.25f);

        private static readonly Color _darkColumnDarkSkinColor = new(0.18f, 0.18f, 0.18f, 1f);
        private static readonly Color _lightColumnDarkSkinColor = new(0.26f, 0.26f, 0.26f, 1f);
        private static readonly Color _darkColumnLightSkinColor = new(0.76f, 0.76f, 0.76f, 1f);
        private static readonly Color _lightColumnLightSkinColor = new(0.82f, 0.82f, 0.82f, 1f);

        private static readonly Color _mapDarkBackgroundColor = new(0.12f, 0.12f, 0.12f, 1f);
        private static readonly Color _mapLightBackgroundColor = new(0.82f, 0.82f, 0.82f, 1f);
        private static readonly Color _mapBorderColor = new(0.5f, 0.5f, 0.5f, 1f);
        private static readonly Color _areaColor = new(0.15f, 0.55f, 0.95f, 1f);
        private static readonly Color _touchColor = new(0.95f, 0.4f, 0.1f, 1f);
        private static readonly Color _joystickColor = new(0.15f, 0.7f, 0.25f, 1f);

        private void OnEnable()
        {
            GetService();

            _autoRefreshEnabled = EditorPrefs.GetBool(_autoRefreshPref, true);

            _nextAutoRefreshTime = EditorApplication.timeSinceStartup + _autoRefreshInterval;

            EditorApplication.update += OnEditorUpdate;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        [MenuItem("Angry Koala/Input/Input Inspector")]
        private static void OpenWindow()
        {
            InputInspectorEditorWindow window = GetWindow<InputInspectorEditorWindow>("Input Inspector");

            window.minSize = new Vector2(600f, 400f);
            window.Show();
        }

        private void GetService()
        {
            if (!EditorApplication.isPlaying)
            {
                _service = null;
                _inputService = null;
                return;
            }

            if (!HasLiveService())
            {
                ServiceLocator.TryGet(out _service);
                _inputService = _service as InputService;
            }
        }

        private void OnEditorUpdate()
        {
            if (!EditorApplication.isPlaying)
            {
                return;
            }

            if (!HasLiveService())
            {
                GetService();
            }

            if (!_autoRefreshEnabled)
            {
                return;
            }

            double now = EditorApplication.timeSinceStartup;
            if (now >= _nextAutoRefreshTime)
            {
                Repaint();
                _nextAutoRefreshTime = now + _autoRefreshInterval;
            }
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    GetService();
                    ResetReservedHeights();
                    Repaint();
                    break;

                case PlayModeStateChange.ExitingPlayMode:
                    _service = null;
                    _inputService = null;
                    Repaint();
                    break;
            }
        }

        private void OnGUI()
        {
            SetGUIStyles();
            DrawToolbar();

            if (!Application.isPlaying)
            {
                DrawMessage("The Input Inspector shows data in Play Mode.", MessageType.Info);
                return;
            }

            if (!HasLiveService())
            {
                DrawMessage("No IInputService is registered.", MessageType.Warning);
                return;
            }

            if (Event.current.type == EventType.Layout)
            {
                RefreshData();
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll, false, false, GUILayout.ExpandWidth(true));

            DrawContent();

            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying || !HasLiveService());

                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(_refreshButtonWidth)))
                {
                    Repaint();
                }

                EditorGUI.EndDisabledGroup();

                GUILayout.FlexibleSpace();

                bool autoRefreshEnabled =
                    GUILayout.Toggle(_autoRefreshEnabled, "Auto-refresh", EditorStyles.toolbarButton);
                if (autoRefreshEnabled != _autoRefreshEnabled)
                {
                    _autoRefreshEnabled = autoRefreshEnabled;
                    EditorPrefs.SetBool(_autoRefreshPref, _autoRefreshEnabled);
                    _nextAutoRefreshTime = EditorApplication.timeSinceStartup + _autoRefreshInterval;
                }
            }
        }

        private void DrawMessage(string text, MessageType type)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(_windowPadding);

                using (new EditorGUILayout.VerticalScope())
                {
                    GUILayout.Space(_windowPadding);
                    EditorGUILayout.HelpBox(text, type);
                }

                GUILayout.Space(_windowPadding);
            }
        }

        private void RefreshData()
        {
            _service.GetData(_surfaces);

            if (_inputService != null)
            {
                _inputService.GetTrackedFingers(_fingers);
                _inputService.GetBlockers(_blockers);
                _inputService.GetPointerFilters(_pointerFilters);
            }
            else
            {
                _fingers.Clear();
                _blockers.Clear();
                _pointerFilters.Clear();
            }

            UpdateStatusText();
        }

        private void UpdateStatusText()
        {
            _blockerLines.Clear();
            foreach (object blocker in _blockers)
            {
                _blockerLines.Add(Describe(blocker));
            }

            _filterLines.Clear();
            foreach (IPointerFilter pointerFilter in _pointerFilters)
            {
                _filterLines.Add(Describe(pointerFilter));
            }

            if (_inputService == null)
            {
                _blockedText = _service.IsBlocked ? "Blocked" : "Not blocked";
            }
            else
            {
                _blockedText = _blockers.Count > 0 ? $"Blocked by {_blockers.Count}" : "Not blocked";
            }

            _filtersText = $"Pointer filters: {_pointerFilters.Count}";

            _reservedStatusLines = Mathf.Max(_reservedStatusLines,
                _minimumStatusLines + _blockerLines.Count + _filterLines.Count);
            _reservedFingerRows = Mathf.Max(_reservedFingerRows, _fingers.Count);
        }

        private void ResetReservedHeights()
        {
            _reservedStatusLines = _minimumStatusLines;
            _reservedFingerRows = _minimumFingerRows;
        }

        private void DrawContent()
        {
            bool isWide = IsWideLayout();

            float columnHeight = GetStatusHeight() + GetSurfacesHeight() + GetFingersHeight() + _sectionSpacing * 2f;
            float viewportHeight = position.height - GetToolbarHeight() - _windowPadding * 2f;
            float mapMinimumBoxHeight = _sectionChromeHeight + _mapMinimumHeight;

            float mapBoxHeight = isWide
                ? Mathf.Max(mapMinimumBoxHeight, viewportHeight)
                : Mathf.Max(mapMinimumBoxHeight, viewportHeight - columnHeight - _sectionSpacing);

            float contentHeight = isWide
                ? Mathf.Max(columnHeight, mapBoxHeight)
                : columnHeight + _sectionSpacing + mapBoxHeight;

            Rect content = GUILayoutUtility.GetRect(0f, contentHeight + _windowPadding * 2f,
                GUILayout.ExpandWidth(true));

            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            var area = new Rect(content.x + _windowPadding, content.y + _windowPadding,
                content.width - _windowPadding * 2f, contentHeight);

            if (isWide)
            {
                float mapInnerHeight = mapBoxHeight - _sectionChromeHeight;
                float naturalWidth = mapInnerHeight * GetScreenAspect() + _boxPadding * 2f;
                float mapBoxWidth = Mathf.Max(_mapBoxMinimumWidth,
                    Mathf.Min(naturalWidth, area.width * _wideMapMaximumWidthRatio));

                var column = new Rect(area.x, area.y, area.width - mapBoxWidth - _sectionSpacing, columnHeight);
                var mapBox = new Rect(area.xMax - mapBoxWidth, area.y, mapBoxWidth, mapBoxHeight);

                DrawColumn(column);
                DrawScreenMapSection(mapBox);
            }
            else
            {
                var column = new Rect(area.x, area.y, area.width, columnHeight);
                var mapBox = new Rect(area.x, area.y + columnHeight + _sectionSpacing, area.width, mapBoxHeight);

                DrawColumn(column);
                DrawScreenMapSection(mapBox);
            }
        }

        private void DrawColumn(Rect column)
        {
            float y = column.y;

            float statusHeight = GetStatusHeight();
            DrawStatusSection(new Rect(column.x, y, column.width, statusHeight));
            y += statusHeight + _sectionSpacing;

            float surfacesHeight = GetSurfacesHeight();
            DrawSurfacesSection(new Rect(column.x, y, column.width, surfacesHeight));
            y += surfacesHeight + _sectionSpacing;

            DrawFingersSection(new Rect(column.x, y, column.width, GetFingersHeight()));
        }

        private void DrawStatusSection(Rect box)
        {
            Rect inner = DrawSectionFrame(box, "Status");

            int line = 0;

            GUI.Label(GetLineRect(inner, line++), _blockedText, EditorStyles.boldLabel);

            foreach (string blockerLine in _blockerLines)
            {
                GUI.Label(GetLineRect(inner, line++), blockerLine, _detailStyle);
            }

            GUI.Label(GetLineRect(inner, line++), _filtersText, EditorStyles.boldLabel);

            foreach (string filterLine in _filterLines)
            {
                GUI.Label(GetLineRect(inner, line++), filterLine, _detailStyle);
            }
        }

        private void DrawSurfacesSection(Rect box)
        {
            Rect inner = DrawSectionFrame(box, "Surfaces");

            var table = new Rect(inner.x, inner.y, inner.width, (_surfaces.Count + 1) * _rowHeight);
            DrawTable(table, _surfaceHeaders, _surfaceWeights, _surfaceWidths);

            for (int index = 0; index < _surfaces.Count; index++)
            {
                InputSurfaceData data = _surfaces[index];
                TouchData touch = data.PrimaryTouch;
                JoystickData joystick = data.Joystick;
                int row = index + 1;

                DrawCell(table, _surfaceWidths, row, 0, data.Name);

                Rect rect = data.PixelRect;
                DrawCell(table, _surfaceWidths, row, 1,
                    $"{rect.x:0}, {rect.y:0}, {rect.width:0} x {rect.height:0}");

                DrawCell(table, _surfaceWidths, row, 2, data.TouchCount.ToString());

                DrawCell(table, _surfaceWidths, row, 3,
                    data.TouchCount > 0
                        ? $"({touch.Position.x:0}, {touch.Position.y:0})  {touch.Duration:0.00}s"
                        : _noneLabel);

                DrawCell(table, _surfaceWidths, row, 4,
                    joystick.IsActive ? $"({joystick.Direction.x:0.00}, {joystick.Direction.y:0.00})" : _noneLabel);

                DrawCell(table, _surfaceWidths, row, 5,
                    string.IsNullOrEmpty(data.LastGesture) ? _noneLabel : data.LastGesture);
            }
        }

        private void DrawFingersSection(Rect box)
        {
            Rect inner = DrawSectionFrame(box, "Fingers");

            var table = new Rect(inner.x, inner.y, inner.width, (_reservedFingerRows + 1) * _rowHeight);
            DrawTable(table, _fingerHeaders, _fingerWeights, _fingerWidths);

            for (int index = 0; index < _reservedFingerRows; index++)
            {
                int row = index + 1;

                if (index >= _fingers.Count)
                {
                    DrawCell(table, _fingerWidths, row, 0, _noneLabel);
                    DrawCell(table, _fingerWidths, row, 1, _noneLabel);
                    continue;
                }

                InputFingerData finger = _fingers[index];

                DrawCell(table, _fingerWidths, row, 0, finger.IsMouse ? _mouseLabel : finger.Id.ToString());
                DrawCell(table, _fingerWidths, row, 1, finger.OwnerName ?? _ignoredLabel);
            }
        }

        private void DrawScreenMapSection(Rect box)
        {
            Rect inner = DrawSectionFrame(box, "Screen Map");

            if (!TryGetScreen(out Rect screen))
            {
                return;
            }

            float scale = Mathf.Min(inner.width / screen.width, inner.height / screen.height);
            if (scale <= 0f)
            {
                return;
            }

            float mapWidth = screen.width * scale;
            float mapHeight = screen.height * scale;

            var map = new Rect(inner.x + (inner.width - mapWidth) * 0.5f, inner.y + (inner.height - mapHeight) * 0.5f,
                mapWidth, mapHeight);

            GUI.BeginClip(map);
            DrawMapContent(screen, scale, map.height);
            GUI.EndClip();
        }

        private void DrawMapContent(Rect screen, float scale, float mapHeight)
        {
            var local = new Rect(0f, 0f, screen.width * scale, mapHeight);

            EditorGUI.DrawRect(local, EditorGUIUtility.isProSkin ? _mapDarkBackgroundColor : _mapLightBackgroundColor);
            DrawOutline(local, _mapBorderColor);

            for (int index = 0; index < _surfaces.Count; index++)
            {
                InputSurfaceData data = _surfaces[index];

                if (data.Area == null)
                {
                    continue;
                }

                Rect rect = ToMapRect(data.PixelRect, screen, scale, mapHeight);
                DrawOutline(rect, _areaColor);

                _mapLabelStyle.normal.textColor = _areaColor;
                GUI.Label(new Rect(rect.x + _mapLabelInset, rect.y, Mathf.Max(0f, rect.width - _mapLabelInset),
                    _mapLabelHeight), data.Name, _mapLabelStyle);
            }

            Color handlesColor = Handles.color;

            for (int index = 0; index < _surfaces.Count; index++)
            {
                InputSurfaceData data = _surfaces[index];

                if (data.TouchCount > 0)
                {
                    Handles.color = _touchColor;
                    Handles.DrawSolidDisc(ToMapPoint(data.PrimaryTouch.Position, screen, scale, mapHeight),
                        Vector3.forward, _touchDotRadius);
                }

                if (data.Joystick.IsActive)
                {
                    Vector2 center = ToMapPoint(data.Joystick.Center, screen, scale, mapHeight);
                    Vector2 handle = ToMapPoint(data.Joystick.HandlePosition, screen, scale, mapHeight);

                    Handles.color = _joystickColor;
                    Handles.DrawWireDisc(center, Vector3.forward, Mathf.Max(1f, data.Joystick.Radius * scale));
                    Handles.DrawLine(center, handle, _mapLineThickness);
                }
            }

            Handles.color = handlesColor;
        }

        #region Utility

        private void SetGUIStyles()
        {
            if (_headerStyle != null)
            {
                return;
            }

            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip
            };

            _cellStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip
            };

            _detailStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip,
                padding = new RectOffset((int)_detailIndent, 0, 0, 0)
            };

            _mapLabelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.UpperLeft,
                clipping = TextClipping.Clip
            };
        }

        private bool HasLiveService()
        {
            return _service is UnityEngine.Object serviceObject ? serviceObject != null : _service != null;
        }

        private bool IsWideLayout()
        {
            return position.width >= _wideMinimumWidth && position.width >= position.height * _wideAspectRatio;
        }

        private bool TryGetScreen(out Rect screen)
        {
            screen = default;

            if (_surfaces.Count == 0)
            {
                return false;
            }

            screen = _surfaces[0].PixelRect;
            return screen.width > 0f && screen.height > 0f;
        }

        private float GetScreenAspect()
        {
            return TryGetScreen(out Rect screen) ? screen.width / screen.height : 1f;
        }

        private float GetStatusHeight()
        {
            return _sectionChromeHeight + _reservedStatusLines * _lineHeight;
        }

        private float GetSurfacesHeight()
        {
            return _sectionChromeHeight + (_surfaces.Count + 1) * _rowHeight;
        }

        private float GetFingersHeight()
        {
            return _sectionChromeHeight + (_reservedFingerRows + 1) * _rowHeight;
        }

        private static float GetToolbarHeight()
        {
            return EditorStyles.toolbar.fixedHeight > 0f ? EditorStyles.toolbar.fixedHeight : _defaultToolbarHeight;
        }

        private static string Describe(object source)
        {
            if (source is UnityEngine.Object unityObject)
            {
                return unityObject != null ? $"{unityObject.name} ({unityObject.GetType().Name})" : "(destroyed)";
            }

            return source != null ? source.ToString() : "(null)";
        }

        private static Rect DrawSectionFrame(Rect box, string title)
        {
            GUI.Box(box, GUIContent.none, EditorStyles.helpBox);

            GUI.Label(new Rect(box.x + _boxPadding, box.y + _boxPadding, box.width - _boxPadding * 2f, _titleHeight),
                title, EditorStyles.boldLabel);

            float top = _boxPadding + _titleHeight;
            return new Rect(box.x + _boxPadding, box.y + top, box.width - _boxPadding * 2f,
                box.height - top - _boxPadding);
        }

        private static Rect GetLineRect(Rect inner, int line)
        {
            return new Rect(inner.x, inner.y + line * _lineHeight, inner.width, _lineHeight);
        }

        private static Color GetColumnColor(int column)
        {
            bool isDarkColumn = column % 2 == 0;

            if (EditorGUIUtility.isProSkin)
            {
                return isDarkColumn ? _darkColumnDarkSkinColor : _lightColumnDarkSkinColor;
            }

            return isDarkColumn ? _darkColumnLightSkinColor : _lightColumnLightSkinColor;
        }

        private static void DrawTable(Rect table, string[] headers, float[] weights, float[] widths)
        {
            SetColumnWidths(table.width, headers, weights, widths);

            float x = table.x;

            for (int index = 0; index < widths.Length; index++)
            {
                EditorGUI.DrawRect(new Rect(x, table.y, widths[index], table.height), GetColumnColor(index));
                x += widths[index];
            }

            EditorGUI.DrawRect(new Rect(table.x, table.y, table.width, _rowHeight), _headerColor);
            EditorGUI.DrawRect(
                new Rect(table.x, table.y + _rowHeight - _mapBorderThickness, table.width, _mapBorderThickness),
                _mapBorderColor);
            DrawOutline(table, _mapBorderColor);

            for (int index = 0; index < headers.Length; index++)
            {
                DrawCell(table, widths, 0, index, headers[index]);
            }
        }

        private static void SetColumnWidths(float tableWidth, string[] headers, float[] weights, float[] widths)
        {
            float totalWeight = 0f;
            float fittedWidth = 0f;

            for (int index = 0; index < weights.Length; index++)
            {
                if (weights[index] > _fitHeaderWeight)
                {
                    totalWeight += weights[index];
                    continue;
                }

                _measuredContent.text = headers[index];
                widths[index] = Mathf.Min(_headerStyle.CalcSize(_measuredContent).x + _cellPaddingX * 2f,
                    tableWidth - fittedWidth);
                fittedWidth += widths[index];
            }

            float unit = totalWeight > 0f ? (tableWidth - fittedWidth) / totalWeight : 0f;

            for (int index = 0; index < weights.Length; index++)
            {
                if (weights[index] > _fitHeaderWeight)
                {
                    widths[index] = weights[index] * unit;
                }
            }
        }

        private static void DrawCell(Rect table, float[] widths, int row, int column, string text)
        {
            float offset = 0f;

            for (int index = 0; index < column; index++)
            {
                offset += widths[index];
            }

            var cell = new Rect(table.x + offset + _cellPaddingX, table.y + row * _rowHeight,
                Mathf.Max(0f, widths[column] - _cellPaddingX * 2f), _rowHeight);

            GUI.Label(cell, text, row == 0 ? _headerStyle : _cellStyle);
        }

        private static Rect ToMapRect(Rect pixelRect, Rect screen, float scale, float mapHeight)
        {
            return new Rect((pixelRect.x - screen.x) * scale, mapHeight - (pixelRect.yMax - screen.y) * scale,
                pixelRect.width * scale, pixelRect.height * scale);
        }

        private static Vector2 ToMapPoint(Vector2 pixelPoint, Rect screen, float scale, float mapHeight)
        {
            return new Vector2((pixelPoint.x - screen.x) * scale, mapHeight - (pixelPoint.y - screen.y) * scale);
        }

        private static void DrawOutline(Rect rect, Color color)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, _mapBorderThickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - _mapBorderThickness, rect.width, _mapBorderThickness),
                color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, _mapBorderThickness, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - _mapBorderThickness, rect.y, _mapBorderThickness, rect.height),
                color);
        }

        #endregion
    }
}
