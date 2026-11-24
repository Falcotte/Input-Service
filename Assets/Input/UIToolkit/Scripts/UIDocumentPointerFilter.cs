using System;
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
            ServiceLocator.OnServiceRegistered += HandleServiceRegistered;
            ServiceLocator.OnServiceDeregistered += HandleServiceDeregistered;

            if (ServiceLocator.TryGet<IInputService>(out var service))
            {
                Attach(service);
            }
        }

        private void OnDisable()
        {
            ServiceLocator.OnServiceRegistered -= HandleServiceRegistered;
            ServiceLocator.OnServiceDeregistered -= HandleServiceDeregistered;

            Detach();
        }

        private void Attach(IInputService service)
        {
            if (ReferenceEquals(_registeredService, service))
            {
                return;
            }

            Detach();

            _registeredService = service;
            service.AddPointerFilter(this);
        }

        private void Detach()
        {
            var service = _registeredService;
            _registeredService = null;

            if (service == null || IsDestroyed(service))
            {
                return;
            }

            service.RemovePointerFilter(this);
        }

        private void HandleServiceRegistered(Type type, IService service)
        {
            if (type != typeof(IInputService) || service is not IInputService inputService)
            {
                return;
            }

            Attach(inputService);
        }

        private void HandleServiceDeregistered(Type type, IService service)
        {
            if (type != typeof(IInputService) || !ReferenceEquals(_registeredService, service))
            {
                return;
            }

            Detach();
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
