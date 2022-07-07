using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Gyro.Runtime
{
    public interface IProvider
    {
        public void Prepare();
        public void Start();
    }

    public interface IExtension
    {
        public static void OnPrepare(object instance, Type provider) => throw new NotImplementedException();
        public static void OnStart(object instance, Type provider) => throw new NotImplementedException();
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
        private static readonly List<Type> Providers = new();
        private static readonly List<Type> Extensions = new();
        private static bool _starting;
        private static bool _started;

        private static void RunFor(Type extension, string methodName, Type provider, object providerInstance)
        {
            var fn = extension.GetMethod(methodName);
            if (fn != null)
            {
                fn.Invoke(providerInstance, new object[] {providerInstance, provider});
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
        
        public static void AddExtension(Type extension)
        {
            if (_started || _starting)
            {
                Debug.LogError("Cannot add extensions after Gyro has started.");
                return;
            }

            var extensionInterface = extension.GetInterface(nameof(IExtension));
            if (extensionInterface != null)
            {
                Extensions.Add(extension);
            }
        }
        
        public static void AddProvider(Type provider)
        {
            if (_started || _starting)
            {
                Debug.LogError("Cannot add providers after Gyro has started.");
                return;
            }

            if (Providers.Contains(provider))
            {
                Debug.LogError("Provider already exists: " + provider.Name);
                return;
            }

            var providerInterface = provider.GetInterface(nameof(IProvider));
            if (providerInterface != null)
            {
                Providers.Add(provider);
            }
        }
        
        public static void Start()
        {
            if (_started || _starting)
            {
                Debug.LogError("Gyro has already started.");
            }

            _starting = true;

            Dictionary<Type, object> instances = new();

            foreach (var provider in Providers)
            {
                var prepareMethod = provider.GetMethod("Prepare");
                var constructor = provider.GetConstructors().FirstOrDefault(c => c.GetParameters().Length == 0);
                if (constructor == null)
                {
                    Debug.LogError("No constructor found for type: " + provider.Name);
                    continue;
                }
                var instance = constructor.Invoke(new object[] {});
                instances.Add(provider, instance);
                RunExtensions("OnPrepare", provider, instance);
                if (prepareMethod == null)
                    continue;
                prepareMethod.Invoke(instance, null);
            }
            
            foreach (var provider in Providers)
            {
                var startMethod = provider.GetMethod("Start");
                if (startMethod == null)
                    continue;
                var success = instances.TryGetValue(provider, out var instance);
                if (!success)
                {
                    Debug.LogError("Failed to get instance: " + provider.Name);
                    return;
                }
                RunExtensions("OnStart", provider, instance);
                startMethod.Invoke(instance, null);
            }

            _starting = false;
            _started = true;
        }
    }
}