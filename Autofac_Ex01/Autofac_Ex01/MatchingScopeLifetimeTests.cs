using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac;
using Autofac.Core;
using Autofac.TestAssembly;

namespace Autofac_Ex01
{

    [TestFixture]
    public class MatchingScopeLifetimeTests
    {
        interface IA { };

        interface IB { };

        public class AB : DisposeTracker, IA, IB
        {
            public void Do() { }
        }


        [Test]
        public void InstancePerMatchingLifetimeScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<AB>().As<IA>().InstancePerMatchingLifetimeScope("shell");
            var container = builder.Build();

            var ex = Assert.Throws<DependencyResolutionException>(() => container.Resolve<IA>());
            Assert.IsNotNull(ex);
            Assert.That(ex.Message.Contains("shell"));

            var scope = container.BeginLifetimeScope("shell");
            var scopeInstance = scope.Resolve<IA>();

            Assert.IsNotNull(scopeInstance);

            var lifetimeScope = container.Resolve<ILifetimeScope>();
            //lifetimeScope.
        }
    }
}
