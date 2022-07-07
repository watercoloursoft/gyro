using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gyro.Runtime
{
    public interface IProvider
    {
        public static void Prepare() => throw new NotImplementedException();
        public static void Start() => throw new NotImplementedException();
    }

    public interface IExtension
    {
        public static void OnPrepare(Type provider) => throw new NotImplementedException();
        public static void OnStart(Type provider) => throw new NotImplementedException();
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

        private static void RunFor(Type extension, string methodName, Type provider)
        {
            var fn = extension.GetMethod(methodName);
            if (fn != null)
            {
                fn.Invoke(null, new object[] {provider});
            }
        }
        
        private static void RunExtensions(string methodName, Type provider)
        {
            foreach (var extension in Extensions)
            {
                RunFor(extension, methodName, provider);
            }

            if (provider.GetCustomAttributes(typeof(UseExtension), false) is not UseExtension[] perProviderExtensions) return;
            foreach (var usage in perProviderExtensions)
            {
                RunFor(usage.ExtensionClass, methodName, provider);
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
                Debug.LogError("Cannot add extensions after Gyro has started.");
                return;
            }

            if (Providers.Contains(provider))
            {
                Debug.LogError("Provider already exists");
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

            foreach (var provider in Providers)
            {
                var prepareMethod = provider.GetMethod("Prepare");
                RunExtensions("OnPrepare", provider);
                if (prepareMethod == null)
                    continue;
                prepareMethod.Invoke(null, null);
            }
            
            foreach (var provider in Providers)
            {
                var startMethod = provider.GetMethod("Start");
                if (startMethod == null)
                    continue;
                RunExtensions("OnStart", provider);
                startMethod.Invoke(null, null);
            }

            _starting = false;
            _started = true;
        }
    }
}