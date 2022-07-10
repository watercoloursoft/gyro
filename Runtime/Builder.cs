using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gyro.Runtime
{
    public abstract class Provider : MonoBehaviour
    {
        public abstract void Prepare();
        public abstract void Begin();
    }

    public interface IExtension
    {
        public static void OnPrepare(object instance, Type provider)
        {
        }

        public static void OnBegin(object instance, Type provider)
        {
        }
    }

    public class UseExtension : Attribute
    {
        public readonly Type ExtensionClass;

        public UseExtension(Type e)
        {
            ExtensionClass = e;
        }
    }
    
    public static class Builder
    {
        private static readonly Dictionary<Type, object> Providers = new();
        private static readonly List<Type> Extensions = new();
        private static bool _starting;
        private static bool _started;

        private static void RunFor(Type extension, string methodName, Type provider, object providerInstance)
        {
            var fn = extension.GetMethod(methodName);
            if (fn != null)
            {
                fn.Invoke(providerInstance, new[] {providerInstance, provider});
            }
        }
        
        private static void RunExtensions(string methodName, Type provider, object providerInstance)
        {
            foreach (var extension in Extensions)
            {
                RunFor(extension, methodName, provider, providerInstance);
            }

            if (provider.GetCustomAttributes(typeof(UseExtension), false) is not UseExtension[] perProviderExtensions) return;
            foreach (var usage in perProviderExtensions)
            {
                RunFor(usage.ExtensionClass, methodName, provider, providerInstance);
            }
        }
        
        public static void AddExtension<T>()
        {
            var extensionType = typeof(T);
            if (_started || _starting)
            {
                Debug.LogError("Cannot add extensions after Gyro has started.");
                return;
            }

            var extensionInterface = extensionType.GetInterface(nameof(IExtension));
            if (extensionInterface != null)
            {
                Extensions.Add(extensionType);
            }
        }
        
        public static void AddProvider<T>(T provider)
        {
            var providerType = typeof(T);
            if (_started || _starting)
            {
                Debug.LogError("Cannot add providers after Gyro has started.");
                return;
            }

            if (Providers.TryGetValue(providerType, out _))
            {
                Debug.LogError("Provider already exists: " + providerType.Name);
                return;
            }

            var providerInterface = providerType.BaseType;
            if (providerInterface == typeof(Provider))
            {
                Providers.Add(providerType, provider);
            }
        }
        
        public static void Start()
        {
            if (_started || _starting)
            {
                Debug.LogError("Gyro has already started.");
            }

            _starting = true;

            foreach (var (providerType, providerObject) in Providers)
            {
                var prepareMethod = providerType.GetMethod("Prepare");
                RunExtensions("OnPrepare", providerType, providerObject);
                if (prepareMethod == null)
                    continue;
                prepareMethod.Invoke(providerObject, null);
            }
            
            foreach (var (providerType, providerObject) in Providers)
            {
                var startMethod = providerType.GetMethod("Begin");
                RunExtensions("OnBegin", providerType, providerObject);
                if (startMethod == null)
                    continue;
                startMethod.Invoke(providerObject, null);
            }

            _starting = false;
            _started = true;
        }
    }
}