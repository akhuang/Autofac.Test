using Autofac;
using Autofac.Core;
using System;
using System.Collections.Generic;
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
            builder.RegisterType<IMemoDueNotifier>().As<PrintingNotifier>();

            var memoChecker = new MemoChecker(memos, new PrintingNotifier(Console.Out));
            memoChecker.CheckNow();
            Console.ReadKey();

        }
    }
}
