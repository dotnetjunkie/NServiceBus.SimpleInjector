using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus.ObjectBuilder.Common;

namespace NServiceBus.ObjectBuilder.SimpleInjector
{
    public class SimpleInjectorObjectBuilder : IContainer
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public object Build(Type typeToBuild)
        {
            throw new NotImplementedException();
        }

        public IContainer BuildChildContainer()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> BuildAll(Type typeToBuild)
        {
            throw new NotImplementedException();
        }

        public void Configure(Type component, DependencyLifecycle dependencyLifecycle)
        {
            throw new NotImplementedException();
        }

        public void Configure<T>(Func<T> component, DependencyLifecycle dependencyLifecycle)
        {
            throw new NotImplementedException();
        }

        public void ConfigureProperty(Type component, string property, object value)
        {
            throw new NotImplementedException();
        }

        public void RegisterSingleton(Type lookupType, object instance)
        {
            throw new NotImplementedException();
        }

        public bool HasComponent(Type componentType)
        {
            throw new NotImplementedException();
        }

        public void Release(object instance)
        {
            throw new NotImplementedException();
        }
    }
}
