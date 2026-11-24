using System;
using AngryKoala.Services;
using UnityEngine;
using UnityEngine.UIElements;

namespace AngryKoala.Input.Demo
{
    public sealed class DemoHud : MonoBehaviour
    {
        // Remembers the last gesture a surface reported and says when it changes.
        private sealed class GestureLog : IDisposable
        {
            private readonly IInputSurface _surface;
            private readonly Action _onChanged;

            public string Last { get; private set; } = "-";

            public GestureLog(IInputSurface surface, Action onChanged)
            {
                _surface = surface;
                _onChanged = onChanged;

                _surface.OnTap += HandleTap;
                _surface.OnHold += HandleHold;
                _surface.OnSwipe += HandleSwipe;
                _surface.OnPinch += HandlePinch;
            }

            public void Dispose()
            {
                _surface.OnTap -= HandleTap;
                _surface.OnHold -= HandleHold;
                _surface.OnSwipe -= HandleSwipe;
                _surface.OnPinch -= HandlePinch;
            }

            private void HandleTap(TouchData touch) => Set("Tap");

            private void HandleHold(TouchData touch) => Set("Hold");

            private void HandleSwipe(SwipeData swipe) => Set($"Swipe {swipe.Direction}");

            private void HandlePinch(PinchData pinch) => Set($"Pinch x{pinch.TotalScale:0.00}");

            private void Set(string gesture)
            {
                Last = gesture;
                _onChanged();
            }
        }

        [SerializeField]
        private UIDocument _document;

        [SerializeField]
        private InputArea _moveArea;

        [SerializeField]
        private DemoSpawner _spawner;

        private IInputService _inputService;
        private GestureLog _screenLog;
        private GestureLog _moveLog;
        private VisualElement _container;
        private Label _statusLabel;
        private Toggle _blockToggle;
        private bool _isBlocking;

        private const float _margin = 24f;
        private const float _fontSize = 36f;

        private void OnEnable()
        {
            TryBuildUi();
        }

        private void OnDisable()
        {
            if (_isBlocking)
            {
                _isBlocking = false;
                _inputService?.Unblock(this);
            }

            if (_container == null)
            {
                return;
            }

            _container.UnregisterCallback<GeometryChangedEvent>(HandleGeometryChanged);
            _container.RemoveFromHierarchy();
            _container = null;
            _statusLabel = null;
            _blockToggle = null;
        }

        private void Start()
        {
            _inputService = ServiceLocator.Get<IInputService>();

            TryBuildUi();

            _screenLog = new GestureLog(_inputService.Screen, Refresh);
            _moveLog = new GestureLog(_inputService.GetArea(_moveArea), Refresh);

            Refresh();
        }

        private void TryBuildUi()
        {
            if (_container != null || _document == null)
            {
                return;
            }

            var root = _document.rootVisualElement;

            if (root == null)
            {
                return;
            }

            BuildUi(root);
        }

        private void BuildUi(VisualElement root)
        {
            root.pickingMode = PickingMode.Ignore;

            // Every container that covers more than its controls ignores pointers, or it would swallow the
            // taps meant for the screen and the move area.
            _container = new VisualElement();
            _container.pickingMode = PickingMode.Ignore;
            _container.style.position = Position.Absolute;
            _container.style.left = 0f;
            _container.style.top = 0f;
            _container.style.right = 0f;
            _container.style.bottom = 0f;
            _container.RegisterCallback<GeometryChangedEvent>(HandleGeometryChanged);

            _statusLabel = new Label();
            _statusLabel.pickingMode = PickingMode.Ignore;
            _statusLabel.style.fontSize = _fontSize;
            _statusLabel.style.color = Color.white;
            _statusLabel.style.backgroundColor = new Color(0f, 0f, 0f, 0.5f);
            _statusLabel.style.paddingLeft = _margin;
            _statusLabel.style.paddingRight = _margin;
            _statusLabel.style.paddingTop = _margin * 0.5f;
            _statusLabel.style.paddingBottom = _margin * 0.5f;

            var row = new VisualElement();
            row.pickingMode = PickingMode.Ignore;
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginTop = _margin * 0.5f;

            var clearButton = new Button(HandleClearClicked) { text = "Clear cubes" };
            clearButton.style.fontSize = _fontSize;
            clearButton.style.marginRight = _margin;

            _blockToggle = new Toggle("Block input");
            _blockToggle.SetValueWithoutNotify(_isBlocking);
            _blockToggle.style.fontSize = _fontSize;
            _blockToggle.style.backgroundColor = new Color(0f, 0f, 0f, 0.5f);
            _blockToggle.RegisterValueChangedCallback(HandleBlockChanged);

            row.Add(clearButton);
            row.Add(_blockToggle);
            _container.Add(_statusLabel);
            _container.Add(row);
            root.Add(_container);

            Refresh();
        }

        private void HandleGeometryChanged(GeometryChangedEvent changedEvent)
        {
            var panel = _container.panel;

            if (panel == null)
            {
                return;
            }

            var safeArea = Screen.safeArea;
            var topLeft = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(safeArea.xMin, Screen.height - safeArea.yMax));
            var bottomRight = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(safeArea.xMax, Screen.height - safeArea.yMin));
            var panelSize = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(Screen.width, Screen.height));

            _container.style.paddingLeft = topLeft.x + _margin;
            _container.style.paddingTop = topLeft.y + _margin;
            _container.style.paddingRight = panelSize.x - bottomRight.x + _margin;
        }

        private void HandleClearClicked()
        {
            _spawner.Clear();
        }

        private void HandleBlockChanged(ChangeEvent<bool> changeEvent)
        {
            _isBlocking = changeEvent.newValue;

            if (_isBlocking)
            {
                _inputService.Block(this);
            }
            else
            {
                _inputService.Unblock(this);
            }

            Refresh();
        }

        private void Refresh()
        {
            if (_inputService == null || _statusLabel == null || _screenLog == null)
            {
                return;
            }

            var blocked = _inputService.IsBlocked ? "yes" : "no";

            _statusLabel.text = $"Screen: {_screenLog.Last}\nMove area: {_moveLog.Last}\nBlocked: {blocked}";
        }

        private void OnDestroy()
        {
            _screenLog?.Dispose();
            _moveLog?.Dispose();

            if (_inputService != null)
            {
                _inputService.Unblock(this);
            }
        }
    }
}
