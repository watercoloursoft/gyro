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

    public class GyroTest : MonoBehaviour
    {
        private T GetOrCreateComponent<T>() where T : MonoBehaviour
        {
            var comp = GetComponent<T>();
            if (comp != null)
                return comp;
            return gameObject.AddComponent(typeof(T)) as T;
        }
        
        private void Start()
        {
            Builder.AddProvider<BeanProvider>(GetOrCreateComponent<BeanProvider>());
            Builder.Start();
        }
    }
}