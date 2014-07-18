using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NServiceBus.ObjectBuilder.Common;
using SimpleInjector;
using SimpleInjector.Advanced;
using SimpleInjector.Extensions.LifetimeScoping;

namespace NServiceBus.ObjectBuilder.SimpleInjector
{
    public class SimpleInjectorObjectBuilder : IContainer
    {
        // Should we use LifetimeScope or ExecutionContextScope here?
        // LifetimeScope is thread-specific, while ExecutionContextScope flows with async methods.
        private static readonly ScopedLifestyle UnitOfWorkLifestyle = new LifetimeScopeLifestyle();
        private static readonly Func<Container, Scope> BeginScope = c => c.BeginLifetimeScope();

        private readonly List<IDisposable> disposableSingletons = new List<IDisposable>();

        private readonly Scope scope;

        ///<summary>
        ///Instantiates the class with an empty SimpleInjector container.
        ///</summary>
        public SimpleInjectorObjectBuilder()
            : this(new Container())
        {
        }

        ///<summary>
        ///Instantiates the class utilizing the given LifetimeScope.
        ///</summary>
        ///<param name="scope"></param>
        public SimpleInjectorObjectBuilder(Container container)
        {
            if (container == null) throw new ArgumentNullException("container");

            this.container = container;
            this.scope = new Scope();
        }

        public Container container { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.scope.Dispose();
            }
        }

        public object Build(Type typeToBuild)
        {
            return this.container.GetInstance(typeToBuild);
        }

        public IEnumerable<object> BuildAll(Type typeToBuild)
        {
            return this.container.GetAllInstances(typeToBuild);
        }

        public virtual IContainer BuildChildContainer()
        {
            return new SimpleInjectorChildContainer(this.container, BeginScope(this.container));
        }
        
        public void Configure(Type component, DependencyLifecycle dependencyLifecycle)
        {
            Lifestyle lifestyle = ToLifestyle(dependencyLifecycle);

            Registration registration = lifestyle.CreateRegistration(component, component, this.container);

            this.Register(component, registration);
        }

        public void Configure<T>(Func<T> component, DependencyLifecycle dependencyLifecycle)
        {
            Lifestyle lifestyle = ToLifestyle(dependencyLifecycle);

            Func<object> instanceCreator = () => component.Invoke();

            Registration registration = lifestyle.CreateRegistration(typeof(T), instanceCreator, this.container);

            this.Register(typeof(T), registration);
        }

        public void ConfigureProperty(Type component, string property, object value)
        {
            if (value == null) throw new ArgumentNullException("value");

            PropertyInfo prop = component.GetProperty(property);

            if (prop == null)
            {
                throw new ArgumentException("property '" + property + "' not found on type " + component.FullName);
            }

            if (!prop.PropertyType.IsInstanceOfType(value))
            {
                throw new ArgumentException("value type is not an instance of " + prop.PropertyType.FullName, "value");
            }

            var actionType = typeof(Action<>).MakeGenericType(component);

            var parameter = Expression.Parameter(component);

            Delegate action = Expression.Lambda(actionType,
                Expression.Assign(
                    Expression.Property(parameter, property),
                    Expression.Constant(value)),
                parameter)
                .Compile();

            ((dynamic)this.container).RegisterInitializer((dynamic)action);
        }

        public void RegisterSingleton(Type lookupType, object instance)
        {
            Lifestyle lifestyle = ToLifestyle(DependencyLifecycle.SingleInstance);

            Registration registration = lifestyle.CreateRegistration(lookupType, () => instance, this.container);

            this.Register(lookupType, registration);
        }

        public bool HasComponent(Type componentType)
        {
            return container.GetRegistration(componentType) != null;
        }

        public void Release(object instance)
        {
            // No-op in Simple Injector. See: https://bit.ly/1jWJnT6 
        }

        private void Register(Type component, Registration registration)
        {
            this.container.AddRegistration(component, registration);

            var interfaceTypes =
                from interfaceType in component.GetInterfaces()
                where interfaceType.Namespace.StartsWith("NServiceBus")
                select interfaceType;

            foreach (var interfaceType in interfaceTypes)
            {
                this.container.AppendToCollection(interfaceType, registration);
            }
        }

        private Lifestyle ToLifestyle(DependencyLifecycle dependencyLifecycle)
        {
            switch (dependencyLifecycle)
            {
                case DependencyLifecycle.InstancePerCall:
                    return Lifestyle.Transient;

                // TODO: Make sure that singletons are registered with RegisterForDisposal in the global Scope
                // when created.
                case DependencyLifecycle.SingleInstance:
                    return Lifestyle.Singleton;

                case DependencyLifecycle.InstancePerUnitOfWork:
                    return UnitOfWorkLifestyle;

                default:
                    throw new System.ComponentModel.InvalidEnumArgumentException(
                        "dependencyLifecycle", 
                        (int)dependencyLifecycle, 
                        typeof(DependencyLifecycle));
            }
        }

        private sealed class SimpleInjectorChildContainer : IContainer
        {
            private readonly Container container;
            private readonly Scope scope;

            public SimpleInjectorChildContainer(Container container, Scope scope)
            {
                this.container = container;
                this.scope = scope;
            }
            
            public object Build(Type typeToBuild)
            {
                return this.container.GetInstance(typeToBuild);
            }

            public IEnumerable<object> BuildAll(Type typeToBuild)
            {
                return this.container.GetAllInstances(typeToBuild);
            }

            public IContainer BuildChildContainer()
            {
                throw new NotSupportedException();
            }

            public void Configure(Type component, DependencyLifecycle dependencyLifecycle)
            {
                throw new NotSupportedException();
            }

            public void Configure<T>(Func<T> component, DependencyLifecycle dependencyLifecycle)
            {
                throw new NotSupportedException();
            }

            public void ConfigureProperty(Type component, string property, object value)
            {
                throw new NotSupportedException();
            }

            public void RegisterSingleton(Type lookupType, object instance)
            {
                throw new NotSupportedException();
            }

            public bool HasComponent(Type componentType)
            {
                throw new NotSupportedException();
            }
            
            public void Release(object instance)
            {
                // No-op
            }

            public void Dispose()
            {
                this.scope.Dispose();
            }
        }
    }
}