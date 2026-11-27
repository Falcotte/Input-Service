using System;
using Object = UnityEngine.Object;

namespace AngryKoala.Input
{
    internal static class HandlerTarget
    {
        public static bool IsDestroyed(Delegate handler)
        {
            return handler.Target is Object unityObject && unityObject == null;
        }

        public static Object GetContext(Delegate handler)
        {
            return handler.Target as Object;
        }
    }
}
