using AngryKoala.Services;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace AngryKoala.Input
{
    /// <summary>
    /// Makes the input service leave a finger to the UI Toolkit document when it lands on an element that can be picked.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class UIDocumentPointerFilter : MonoBehaviour, IPointerFilter
    {
        private UIDocument _document;
        private IInputService _registeredService;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            ServiceLocator.WhenRegistered<IInputService>(HandleServiceRegistered);
        }

        private void OnDisable()
        {
            var service = _registeredService;
            _registeredService = null;

            if (service == null || IsDestroyed(service))
            {
                return;
            }

            service.RemovePointerFilter(this);
        }

        private void HandleServiceRegistered(IInputService service)
        {
            if (!isActiveAndEnabled || ReferenceEquals(_registeredService, service))
            {
                return;
            }

            _registeredService = service;
            service.AddPointerFilter(this);
        }

        public bool TakesPointer(Vector2 screenPosition)
        {
            if (_document == null)
            {
                return false;
            }

            var root = _document.rootVisualElement;

            if (root == null || root.panel == null)
            {
                return false;
            }

            var panel = root.panel;
            var panelPosition = RuntimePanelUtils.ScreenToPanel(panel,
                new Vector2(screenPosition.x, Screen.height - screenPosition.y));

            return panel.Pick(panelPosition) != null;
        }

        #region Utility

        private static bool IsDestroyed(IInputService service)
        {
            return service is Object unityObject && unityObject == null;
        }

        #endregion
    }
}
