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

        public class AB : IA, IB
        {
        }

        [Test]
        public void SimpleReg()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<AB>();

            var container = builder.Build();
            var a = container.Resolve<AB>();
            Assert.IsNotNull(a);
        }

    }
}
