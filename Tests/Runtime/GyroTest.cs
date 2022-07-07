using System;
using System.Reflection;
using Gyro.Runtime;
using UnityEngine;

namespace Gyro.Tests.Runtime
{
    public class LegumeStartExtension : IExtension {
        public static void OnPrepare(object instance, Type provider)
        {
            var legumeStartField = provider.GetField("_legumeStartAmount", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (legumeStartField == null) return;
            legumeStartField.SetValue(instance, 10);
        }
    }

    [UseExtension(typeof(LegumeStartExtension))]
    public class BeanProvider : IProvider {
        private int _legumeStartAmount;

        public void Prepare() { }

        public void Start() {
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