using System.Reflection;
using NUnit.Framework;
using Object = UnityEngine.Object;

namespace AngryKoala.Input.Tests
{
    internal static class ValidationTestUtility
    {
        /// <summary>
        /// The Editor calls OnValidate when a value is changed in the Inspector. JsonUtility does not, so the tests call it.
        /// </summary>
        public static void Validate(Object target)
        {
            var method = target.GetType().GetMethod("OnValidate",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null, $"{target.GetType().Name} has no OnValidate.");

            method.Invoke(target, null);
        }
    }
}
