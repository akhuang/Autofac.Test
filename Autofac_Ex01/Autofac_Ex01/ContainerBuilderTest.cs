using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac;

namespace Autofac_Ex01
{
    [TestFixture]
    public class ContainerBuilderTest
    {
        interface IA { };

        interface IB { };

        public class AB : DisposeTracker, IA, IB
        {
            public void Do() { }
        }

        [Test]
        public void SimpleReg()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<AB>();
            var container = builder.Build();
            var a = container.Resolve<AB>();
            Assert.IsNotNull(a);
            Assert.IsInstanceOf<AB>(a);
        }

        [Test]
        public void SimpleRegInterface()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<AB>().As<IA>();
            var container = builder.Build();
            var a = container.Resolve<IA>();

            Assert.IsNotNull(a);
            Assert.IsInstanceOf<AB>(a);
            Assert.IsFalse(container.IsRegistered<AB>());
        }

        [Test]
        public void WithExternalFactory()
        {
            //在container回收后，a1,a2内存不会被回收
            //用a1,a2是两个不同的实例

            var builder = new ContainerBuilder();
            builder.RegisterType<AB>().As<IA>().ExternallyOwned();
            var container = builder.Build();
            var a1 = container.Resolve<IA>();
            var a2 = container.Resolve<IA>();

            container.Dispose();

            Assert.IsNotNull(a1);
            Assert.AreNotEqual(a1, a2);
            Assert.IsFalse(((AB)a1).IsDisposed);
        }

        [Test]
        public void WithInternalSingleton()
        {
            //在container回收后，a1,a2内存也被回收
            //用a1,a2是两个相同的实例

            var builder = new ContainerBuilder();
            builder.RegisterType<AB>()
                .As<IA>()
                .OwnedByLifetimeScope() //实例跟随container一起dispose;默认
                .SingleInstance();

            var container = builder.Build();
            var a1 = container.Resolve<IA>();
            var a2 = container.Resolve<IA>();

            container.Dispose();

            Assert.IsNotNull(a1);
            Assert.AreSame(a1, a2);
            Assert.IsTrue(((AB)a1).IsDisposed);
        }

        [Test]
        public void WithFactoryContext()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<AB>().As<IA>();

            var container = cb.Build();
            var ctx = container.BeginLifetimeScope();
            var a1 = ctx.Resolve<IA>();
            var a2 = ctx.Resolve<IA>();
            ctx.Dispose();

            Assert.IsNotNull(a1);
            Assert.AreNotSame(a1, a2);
            Assert.IsTrue(((AB)a1).IsDisposed);

        }

        [Test]
        public void RegistrationOrderingPreserved()
        {
            //先进后出
            var target = new ContainerBuilder();
            var inst1 = new object();
            var inst2 = new object();

            target.RegisterInstance(inst1);
            target.RegisterInstance(inst2);

            Assert.AreSame(inst2, target.Build().Resolve<object>());
        }

        class ObjectModule : Module
        {
            public bool ConfigureCalled { get; private set; }

            protected override void Load(ContainerBuilder builder)
            {
                if (builder == null)
                    throw new ArgumentNullException("builder");

                ConfigureCalled = true;
                builder.RegisterType<object>().SingleInstance();
            }
        }

        [Test]
        public void RegisterModule()
        {
            //注册模块
            //builder之前注册是没有生效的

            var mod = new ObjectModule();
            var target = new ContainerBuilder();
            target.RegisterModule(mod);

            Assert.IsFalse(mod.ConfigureCalled);

            var container = target.Build();
            Assert.IsTrue(mod.ConfigureCalled);
            Assert.IsTrue(container.IsRegistered<object>());
        }
    }
}
