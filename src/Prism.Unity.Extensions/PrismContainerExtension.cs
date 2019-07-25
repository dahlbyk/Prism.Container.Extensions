﻿using Prism.Ioc;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity;
using Unity.Injection;
using Unity.Lifetime;
using Unity.Resolution;

[assembly: InternalsVisibleTo("Prism.Unity.Extensions.Tests")]
[assembly: InternalsVisibleTo("Prism.Unity.Forms.Extended.Tests")]
namespace Prism.Unity.Extensions
{
    public partial class PrismContainerExtension : IContainerExtension<IUnityContainer>, IExtendedContainerRegistry
    {
        private static IContainerExtension<IUnityContainer> _current;
        public static IContainerExtension<IUnityContainer> Current
        {
            get
            {
                if (_current is null)
                {
                    Create();
                }

                return _current;
            }
        }

        internal static void Reset()
        {
            _current = null;
            GC.Collect();
        }

        public static IContainerExtension Create() =>
            Create(new UnityContainer());

        public static IContainerExtension Create(IUnityContainer container)
        {
            if (_current != null)
            {
                throw new NotSupportedException($"An instance of {nameof(PrismContainerExtension)} has already been created.");
            }

            return new PrismContainerExtension(container);
        }
        private PrismContainerExtension()
           : this(new UnityContainer())
        {
        }

        private PrismContainerExtension(IUnityContainer container)
        {
            _current = this;
            Instance = container;
            Instance.RegisterInstance<IContainerProvider>(this);
            Instance.RegisterInstance<IContainerExtension>(this);
            Instance.RegisterInstance<IContainerRegistry>(this);
            Instance.RegisterInstance<IServiceProvider>(this);
            Splat.Locator.SetLocator(this);
        }

        public IUnityContainer Instance { get; private set; }

        public void FinalizeExtension() { }

        public IContainerRegistry RegisterInstance(Type type, object instance)
        {
            Instance.RegisterInstance(type, instance);
            return this;
        }

        public IContainerRegistry RegisterInstance(Type type, object instance, string name)
        {
            Instance.RegisterInstance(type, name, instance);
            return this;
        }

        public IContainerRegistry RegisterSingleton(Type from, Type to)
        {
            Instance.RegisterSingleton(from, to);
            return this;
        }

        public IContainerRegistry RegisterSingleton(Type from, Type to, string name)
        {
            Instance.RegisterSingleton(from, to, name);
            return this;
        }

        public IContainerRegistry Register(Type from, Type to)
        {
            Instance.RegisterType(from, to);
            return this;
        }

        public IContainerRegistry Register(Type from, Type to, string name)
        {
            Instance.RegisterType(from, to, name);
            return this;
        }

        public object Resolve(Type type)
        {
            return Instance.Resolve(type);
        }

        public object Resolve(Type type, string name)
        {
            return Instance.Resolve(type, name);
        }

        public object Resolve(Type type, params (Type Type, object Instance)[] parameters)
        {
            var overrides = parameters.Select(p => new DependencyOverride(p.Type, p.Instance)).ToArray();
            return Instance.Resolve(type, overrides);
        }

        public object Resolve(Type type, string name, params (Type Type, object Instance)[] parameters)
        {
            var overrides = parameters.Select(p => new DependencyOverride(p.Type, p.Instance)).ToArray();
            return Instance.Resolve(type, name, overrides);
        }

        public bool IsRegistered(Type type)
        {
            return Instance.IsRegistered(type);
        }

        public bool IsRegistered(Type type, string name)
        {
            return Instance.IsRegistered(type, name);
        }

        public IContainerRegistry RegisterMany(Type implementingType, params Type[] serviceTypes)
        {
            Instance.RegisterType(implementingType);
            return RegisterManyInternal(implementingType, serviceTypes);
        }

        public IContainerRegistry RegisterManySingleton(Type implementingType, params Type[] serviceTypes)
        {
            Instance.RegisterSingleton(implementingType);
            return RegisterManyInternal(implementingType, serviceTypes);
        }

        private IContainerRegistry RegisterManyInternal(Type implementingType, Type[] serviceTypes)
        {
            if(serviceTypes is null || serviceTypes.Length == 0)
            {
                serviceTypes = implementingType.GetInterfaces().Where(x => x != typeof(IDisposable)).ToArray();
            }

            foreach(var service in serviceTypes)
            {
                Instance.RegisterFactory(service, c => c.Resolve(implementingType));
            }

            return this;
        }

        public IContainerRegistry RegisterDelegate(Type serviceType, Func<object> factoryMethod)
        {
            Instance.RegisterFactory(serviceType, _ => factoryMethod());
            return this;
        }

        public IContainerRegistry RegisterDelegate(Type serviceType, Func<IContainerProvider, object> factoryMethod)
        {
            Instance.RegisterFactory(serviceType, c => factoryMethod(c.Resolve<IContainerProvider>()));
            return this;
        }

        public IContainerRegistry RegisterDelegate(Type serviceType, Func<IServiceProvider, object> factoryMethod)
        {
            Instance.RegisterFactory(serviceType, c => factoryMethod(c.Resolve<IServiceProvider>()));
            return this;
        }

        public IContainerRegistry RegisterSingletonFromDelegate(Type serviceType, Func<object> factoryMethod)
        {
            Instance.RegisterFactory(serviceType, _ => factoryMethod(), new ContainerControlledLifetimeManager());
            return this;
        }

        public IContainerRegistry RegisterSingletonFromDelegate(Type serviceType, Func<IContainerProvider, object> factoryMethod)
        {
            Instance.RegisterFactory(serviceType, c => factoryMethod(c.Resolve<IContainerProvider>()), new ContainerControlledLifetimeManager());
            return this;
        }

        public IContainerRegistry RegisterSingletonFromDelegate(Type serviceType, Func<IServiceProvider, object> factoryMethod)
        {
            Instance.RegisterFactory(serviceType, c => factoryMethod(c.Resolve<IServiceProvider>()), new ContainerControlledLifetimeManager());
            return this;
        }

        public IContainerRegistry RegisterScoped(Type serviceType)
        {
            Instance.RegisterType(serviceType, new ExternallyControlledLifetimeManager());
            return this;
        }

        public IContainerRegistry RegisterScoped(Type serviceType, Type implementationType)
        {
            Instance.RegisterType(serviceType, implementationType, new ExternallyControlledLifetimeManager());
            return this;
        }

        public object GetService(Type serviceType)
        {
            return Instance.Resolve(serviceType);
        }
    }
}