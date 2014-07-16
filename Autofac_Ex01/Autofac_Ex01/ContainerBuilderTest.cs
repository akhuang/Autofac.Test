using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac;
using Autofac.Core;
using Autofac.TestAssembly;
using Autofac.Core.Registration;

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

        public class A1 : IA { }
        public class A2 : IA { }

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
            Assert.IsTrue(container.IsRegistered<IA>());
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
        public void WithFactoryContextSingleton()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<AB>().As<IA>().SingleInstance();

            var container = cb.Build();
            var ctx = container.BeginLifetimeScope();
            var a1 = ctx.Resolve<IA>();
            var a2 = container.Resolve<IA>();
            ctx.Dispose();

            Assert.IsNotNull(a1);
            Assert.AreSame(a1, a2);
            Assert.IsFalse(((AB)a1).IsDisposed);
            Assert.IsFalse(((AB)a2).IsDisposed);
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

        [Test]
        public void RegisterAssemblyModules()
        {
            var assembly = typeof(AComponent).Assembly;
            var container = new ContainerBuilder();
            container.RegisterAssemblyModules(assembly);

            var builder = container.Build();

            Assert.IsTrue(builder.IsRegistered<AComponent>());
            Assert.That(builder.IsRegistered<AComponent>(), Is.True);
            Assert.That(builder.IsRegistered<BComponent>(), Is.True);
        }

        [Test]
        public void RegisterAssemblyModulesOfGenericType()
        {
            var assembly = typeof(AComponent).Assembly;
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules<AModule>(assembly);
            var container = builder.Build();

            Assert.That(container.IsRegistered<AComponent>(), Is.True);
            Assert.That(container.IsRegistered<BComponent>(), Is.False);
        }

        [Test]
        public void RegisterAssemblyModulesOfBaseGenericType()
        {
            var assembly = typeof(AComponent).Assembly;
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules<ModuleBase>(assembly);

            var container = builder.Build();

            Assert.That(container.IsRegistered<AComponent>(), Is.True);
            Assert.That(container.IsRegistered<BComponent>(), Is.True);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void OnlyAllowBuildOnce()
        {
            var target = new ContainerBuilder();
            target.Build();
            target.Build();
        }

        [Test]
        public void RegisterWithName()
        {
            var name = "object.registration";
            var builder = new ContainerBuilder();
            builder.RegisterType<object>().Named<object>(name);

            var container = builder.Build();

            object obj1;
            Assert.IsTrue(container.TryResolveNamed(name, typeof(object), out obj1));

            object obj2;
            Assert.IsFalse(container.TryResolve(typeof(object), out obj2));
        }

        [Test]
        public void RegisterWithKey()
        {
            var key = new object();
            var builder = new ContainerBuilder();
            builder.RegisterType<object>().Keyed<object>(key);

            var container = builder.Build();

            object obj1;
            Assert.IsTrue(container.TryResolveKeyed(key, typeof(object), out obj1));
            object obj2;
            Assert.IsFalse(container.TryResolve(typeof(object), out obj1));
        }

        [Test]
        public void RegisterWithMetadata()
        {
            var p1 = new KeyValuePair<string, object>("p1", "p1value");
            var p2 = new KeyValuePair<string, object>("p2", "p2value");

            var builder = new ContainerBuilder();
            builder.RegisterType<object>()
                .WithMetadata(p1.Key, p1.Value)
                .WithMetadata(p2.Key, p2.Value);

            var container = builder.Build();

            IComponentRegistration registration;
            Assert.IsTrue(container.ComponentRegistry.TryGetRegistration(new TypedService(typeof(object)), out registration));

            Assert.AreEqual(2, registration.Metadata.Count);
            Assert.IsTrue(registration.Metadata.Contains(p1));
            Assert.IsTrue(registration.Metadata.Contains(p2));
        }

        [Test]
        public void FiresPreparing()
        {
            //在builder之后preparingFired不会加1
            //必须Resolve后才会改变值

            int preparingFired = 0;
            var builder = new ContainerBuilder();
            builder.RegisterType<object>().OnPreparing(c => ++preparingFired);

            var container = builder.Build();

            container.Resolve<object>();
            Assert.AreEqual(1, preparingFired);
        }

        [Test]
        public void WhenPreparingHandlerProvidesParameters_ParamsProvidedToActivator()
        {
            IEnumerable<Parameter> parameters = new Parameter[] { new NamedParameter("n", 1) };
            IEnumerable<Parameter> actual = null;
            var cb = new ContainerBuilder();
            cb.RegisterType<object>()
                .OnPreparing(e => e.Parameters = parameters)
                .OnActivating(e => actual = e.Parameters);
            var container = cb.Build();
            container.Resolve<object>();
            Assert.False(parameters.Except(actual).Any());
        }

        [Test]
        public void RegisterIEnumberabe()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<A1>().As<IA>();
            builder.RegisterType<A2>().As<IA>();
            var container = builder.Build();

            var list = container.Resolve<IEnumerable<IA>>();

            Assert.IsNotNull(list);
            Assert.True(list.Count() == 2);
        }

        [Test]
        [ExpectedException(typeof(ComponentNotRegisteredException))]
        public void RegisterWithLifetimeScope()
        {
            var builder = new ContainerBuilder();

            var container = builder.Build();

            var lifttimeScope = container.Resolve<ILifetimeScope>();

            Assert.IsNotNull(lifttimeScope);

            lifttimeScope.BeginLifetimeScope(b =>
            {
                b.RegisterType<A1>().As<IA>();
            });

            //cannot resolve IA use container
            var a1 = container.Resolve<IA>();
        }

        [Test]
        public void RegisterGlobalObject()
        {
            var builder = new ContainerBuilder();
            builder.Register(x => new A1()).SingleInstance();
            var container = builder.Build();

            var objA = container.Resolve<A1>();
            var objB = container.Resolve<A1>();
            Assert.IsNotNull(objA);
            Assert.AreSame(objA, objB);


        }
    }
}
