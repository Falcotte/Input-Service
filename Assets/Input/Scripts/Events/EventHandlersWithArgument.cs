using System;
using System.Collections.Generic;
using UnityEngine;

namespace AngryKoala.Input
{
    /// <summary>
    /// The handlers of one event, raised by index so that a raise allocates nothing.
    /// </summary>
    internal sealed class EventHandlersWithArgument<TArgument>
    {
        private readonly List<Action<TArgument>> _handlers = new();
        private readonly string _eventName;
        private readonly string _ownerName;

        private int _raiseDepth;
        private bool _hasEmptySlots;

        private static readonly Predicate<Action<TArgument>> _isEmpty = IsEmpty;

        public EventHandlersWithArgument(string eventName, string ownerName)
        {
            _eventName = eventName;
            _ownerName = ownerName;
        }

        public void Add(Action<TArgument> handler)
        {
            if (handler == null)
            {
                return;
            }

            _handlers.Add(handler);
        }

        public void Remove(Action<TArgument> handler)
        {
            if (handler == null)
            {
                return;
            }

            var index = _handlers.LastIndexOf(handler);

            if (index < 0)
            {
                return;
            }

            if (_raiseDepth > 0)
            {
                _handlers[index] = null;
                _hasEmptySlots = true;
                return;
            }

            _handlers.RemoveAt(index);
        }

        public void Raise(TArgument argument)
        {
            var count = _handlers.Count;

            if (count == 0)
            {
                return;
            }

            _raiseDepth++;

            for (var index = 0; index < count; index++)
            {
                var handler = _handlers[index];

                if (handler == null)
                {
                    continue;
                }

                if (HandlerTarget.IsDestroyed(handler))
                {
                    _handlers[index] = null;
                    _hasEmptySlots = true;
                    continue;
                }

                try
                {
                    handler(argument);
                }
                catch (Exception exception)
                {
                    var message = $"A handler of {_eventName} on surface '{_ownerName}' threw.";
                    var context = HandlerTarget.GetContext(handler);

                    if (context != null)
                    {
                        Debug.LogError(message, context);
                        Debug.LogException(exception, context);
                    }
                    else
                    {
                        Debug.LogError(message);
                        Debug.LogException(exception);
                    }
                }
            }

            _raiseDepth--;

            if (_raiseDepth == 0 && _hasEmptySlots)
            {
                _handlers.RemoveAll(_isEmpty);
                _hasEmptySlots = false;
            }
        }

        private static bool IsEmpty(Action<TArgument> handler) => handler == null;
    }
}
