using System;
using System.Reflection;
using Gyro.Runtime;
using UnityEngine;

namespace Gyro.Tests.Runtime
{
    public class LegumeStartExtension : IExtension {
        public static void OnPrepare(Type provider)
        {
            var legumeStartField = provider.GetField("_legumeStartAmount", BindingFlags.NonPublic | BindingFlags.Static);
            if (legumeStartField == null) return;
            // Passing null because it is static
            legumeStartField.SetValue(null, 10);
        }
    }

    [UseExtension(typeof(LegumeStartExtension))]
    public class BeanProvider : IProvider {
        private static int _legumeStartAmount;
        public static void Start() {
            Debug.Log("Starting with " + _legumeStartAmount + " beans.");
        }
    }
    
    public class GyroTest : MonoBehaviour
    {
        private void Start()
        {
            Builder.AddProvider(typeof(BeanProvider));
            // Builder.AddExtension(typeof(LegumeStartExtension));
            Builder.Start();
        }
    }
}