using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Autofac_Ex02
{
    public interface IMemoDueNotifier
    {
        void MemoIsDue(Memo memo);
    }

    public class PrintingNotifier : IMemoDueNotifier
    {
        TextWriter _writer;
        public PrintingNotifier(TextWriter writer)
        {
            _writer = writer;
        }


        public void MemoIsDue(Memo memo)
        {
            _writer.WriteLine(memo.Title);
        }
    }
}
