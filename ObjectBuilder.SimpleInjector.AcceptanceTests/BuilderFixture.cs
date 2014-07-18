using System;
using NServiceBus.ObjectBuilder.Common;
using NServiceBus.ObjectBuilder.SimpleInjector;
using NUnit.Framework;

namespace ObjectBuilder.SimpleInjector.AcceptanceTests
{
    public class BuilderFixture
    {
        protected IContainer builder;

        protected virtual Action<IContainer> InitializeBuilder()
        {
            //no-op
            return c => { };
        }

        [SetUp]
        public void SetUp()
        {
            builder = new SimpleInjectorObjectBuilder();

            this.InitializeBuilder()(builder);
        }

        [TearDown]
        public void DisposeContainers()
        {
            if (builder != null)
                builder.Dispose();
        }

    }
}