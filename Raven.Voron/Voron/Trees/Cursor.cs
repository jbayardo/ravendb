namespace Voron.Trees
{
    using Sparrow;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Voron.Util;

    public class Cursor : IDisposable
    {
        public LinkedList<Page> Pages = new LinkedList<Page>();

        private static readonly ObjectPool<Dictionary<long, Page>> _pagesByNumPool = new ObjectPool<Dictionary<long, Page>>(() => new Dictionary<long, Page>(NumericEqualityComparer.Instance), 50);

        private readonly Dictionary<long, Page> _pagesByNum;

        private bool _anyOverrides;

        public Cursor()
        {
            _pagesByNum = _pagesByNumPool.Allocate();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _pagesByNum.Clear();
                _pagesByNumPool.Free(_pagesByNum);
            }
        }

        public void Update(LinkedListNode<Page> node, Page newVal)
        {
            var oldPageNumber = node.Value.PageNumber;
            var newPageNumber = newVal.PageNumber;

            if (oldPageNumber == newPageNumber)
            {
                _pagesByNum[oldPageNumber] = newVal;
                node.Value = newVal;
                return;
            }

            _anyOverrides = true;
            _pagesByNum[oldPageNumber] = newVal;
            _pagesByNum.Add(newPageNumber, newVal);
            node.Value = newVal;
        }

        public Page ParentPage
        {
            get
            {
                LinkedListNode<Page> linkedListNode = Pages.First;
                if (linkedListNode == null)
                    throw new InvalidOperationException("No pages in cursor");
                linkedListNode = linkedListNode.Next;
                if (linkedListNode == null)
                    throw new InvalidOperationException("No parent page in cursor");
                return linkedListNode.Value;
            }
        }

        public Page CurrentPage
        {
            get
            {
                LinkedListNode<Page> linkedListNode = Pages.First;
                if (linkedListNode == null)
                    throw new InvalidOperationException("No pages in cursor");
                return linkedListNode.Value;
            }
        }

        public int PageCount
        {
            get { return Pages.Count; }
        }

        public void Push(Page p)
        {
            Pages.AddFirst(p);
            _pagesByNum.Add(p.PageNumber, p);
        }

        public Page Pop()
        {
            if (Pages.Count == 0)
                throw new InvalidOperationException("No page to pop");

            var p = Pages.First.Value;
            Pages.RemoveFirst();

            var removedPrimary = _pagesByNum.Remove(p.PageNumber);
            var removedSecondary = false;

            if (_anyOverrides)
            {
                var pagesNumbersToRemove = new HashSet<long>();

                foreach (var page in _pagesByNum.Where(page => page.Value.PageNumber == p.PageNumber))
                    pagesNumbersToRemove.Add(page.Key);

                foreach (var pageToRemove in pagesNumbersToRemove)
                    removedSecondary |= _pagesByNum.Remove(pageToRemove);
            }

            Debug.Assert(removedPrimary || removedSecondary);

            return p;
        }
    }
}
