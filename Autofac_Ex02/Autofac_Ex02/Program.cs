using Autofac;
using Autofac.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Autofac_Ex02
{
    class Program
    {
        static void Main(string[] args)
        {
            IQueryable<Memo> memos = new List<Memo>()
            {
                new Memo{ Title="aaa",DueDt=DateTime.Now},
                new Memo{ Title="bbbb",DueDt=new DateTime(2007,2,2)}
            }.AsQueryable();

            var builder = new ContainerBuilder();
            builder.Register(c => new MemoChecker(c.Resolve<IQueryable<Memo>>(), c.Resolve<IMemoDueNotifier>()));
            builder.Register(c => new PrintingNotifier(c.Resolve<TextWriter>())).As<IMemoDueNotifier>();
            builder.RegisterInstance(memos);
            builder.RegisterInstance(Console.Out).As<TextWriter>().ExternallyOwned();
            //var container = builder.Build();
            //var memoChecker = new MemoChecker(memos, new PrintingNotifier(Console.Out));
            //memoChecker.CheckNow();

            using (var container = builder.Build())
            {
                container.Resolve<MemoChecker>().CheckNow();
            }
            Console.ReadKey();

        }
    }
}
