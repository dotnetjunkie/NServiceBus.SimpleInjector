﻿namespace ObjectBuilder.SimpleInjector.AcceptanceTests
{
    using System;
    using NServiceBus;
    using NServiceBus.ObjectBuilder.Common;
    using NUnit.Framework;

    [TestFixture]
    public class When_building_components : BuilderFixture
    {

        [Test]
        public void Singleton_components_should_yield_the_same_instance()
        {
            Assert.AreEqual(builder.Build(typeof(SingletonComponent)), builder.Build(typeof(SingletonComponent)));
        }

        [Test]
        public void Singlecall_components_should_yield_unique_instances()
        {
            Assert.AreNotEqual(builder.Build(typeof(SinglecallComponent)), builder.Build(typeof(SinglecallComponent)));
        }

        [Test]
        public void UoW_components_should_resolve_from_main_container()
        {
            //Assert.NotNull(builder.Build(typeof(InstancePerUoWComponent)));
        }

        [Test]
        public void Lambda_uow_components_should_resolve_from_main_container()
        {
            Assert.NotNull(builder.Build(typeof(LambdaComponentUoW)));
        }

        [Test]
        public void Lambda_singlecall_components_should_yield_unique_instances()
        {
            Assert.AreNotEqual(builder.Build(typeof(SingleCallLambdaComponent)),
                builder.Build(typeof(SingleCallLambdaComponent)));
        }

        [Test]
        public void Lambda_singleton_components_should_yield_the_same_instance()
        {
            Assert.AreEqual(builder.Build(typeof(SingletonLambdaComponent)),
                builder.Build(typeof(SingletonLambdaComponent)));
        }

        [Test]
        public void Requesting_an_unregistered_component_should_throw()
        {

            Assert.That(() => builder.Build(typeof(UnregisteredComponent)),
                Throws.Exception);
        }

        [Test]
        public void Should_be_able_to_build_components_registered_after_first_build()
        {
            builder.Build(typeof(SingletonComponent));
            builder.Configure(typeof(UnregisteredComponent), DependencyLifecycle.SingleInstance);

            var unregisteredComponent = builder.Build(typeof(UnregisteredComponent)) as UnregisteredComponent;
            Assert.NotNull(unregisteredComponent);
            Assert.NotNull(unregisteredComponent.SingletonComponent);
        }

        [Test]
        public void Should_support_mixed_dependency_styles()
        {
            builder.Configure(typeof(ComponentWithBothConstructorAndSetterInjection), DependencyLifecycle.InstancePerCall);
            builder.Configure(typeof(ConstructorDependency), DependencyLifecycle.InstancePerCall);
            builder.Configure(typeof(SetterDependency), DependencyLifecycle.InstancePerCall);

            var component = (ComponentWithBothConstructorAndSetterInjection)builder.Build(typeof(ComponentWithBothConstructorAndSetterInjection));

            Assert.NotNull(component.ConstructorDependency);
            Assert.NotNull(component.SetterDependency);
        }


        protected override Action<IContainer> InitializeBuilder()
        {
            return config =>
            {
                config.Configure(typeof(SingletonComponent), DependencyLifecycle.SingleInstance);
                config.Configure(typeof(SinglecallComponent), DependencyLifecycle.InstancePerCall);
                //       config.Configure(typeof(InstancePerUoWComponent), DependencyLifecycle.InstancePerUnitOfWork);
                config.Configure(() => new SingletonLambdaComponent(), DependencyLifecycle.SingleInstance);
                config.Configure(() => new SingleCallLambdaComponent(), DependencyLifecycle.InstancePerCall);
                config.Configure(() => new LambdaComponentUoW(), DependencyLifecycle.InstancePerUnitOfWork);
            };
        }

        public class SingletonComponent
        {
        }
        public class SinglecallComponent
        {
        }
        public class UnregisteredComponent
        {
            public SingletonComponent SingletonComponent { get; set; }
        }
        public class SingletonLambdaComponent { }
        public class LambdaComponentUoW { }
        public class SingleCallLambdaComponent { }
    }

    public class StaticFactory
    {
        public ComponentCreatedByFactory Create()
        {
            return new ComponentCreatedByFactory();
        }
    }

    public class ComponentCreatedByFactory
    {
    }

    public class ComponentWithBothConstructorAndSetterInjection
    {
        public ComponentWithBothConstructorAndSetterInjection(ConstructorDependency constructorDependency)
        {
            ConstructorDependency = constructorDependency;
        }

        public ConstructorDependency ConstructorDependency { get; private set; }

        public SetterDependency SetterDependency { get; set; }
    }

    public class ConstructorDependency
    {
    }

    public class SetterDependency
    {
    }
}
