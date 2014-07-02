using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac_Ex02
{
    public class MemoChecker
    {
        IQueryable<Memo> _memos;
        IMemoDueNotifier _notifier;

        public MemoChecker(IQueryable<Memo> memos, IMemoDueNotifier notifier)
        {
            _memos = memos;
            _notifier = notifier;
        }

        public void CheckNow()
        {
            var overDueMemos = _memos.Where(m => m.DueDt < DateTime.Now);

            foreach (var item in overDueMemos)
            {
                _notifier.MemoIsDue(item);
            }
        }
    }
}
