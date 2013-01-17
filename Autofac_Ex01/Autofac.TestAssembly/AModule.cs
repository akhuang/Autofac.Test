using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.TestAssembly
{
    public class AModule : ModuleBase
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => new AComponent()).As<AComponent>();
        }
    }
}
