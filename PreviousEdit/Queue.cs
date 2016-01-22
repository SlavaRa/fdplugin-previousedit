using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace PreviousEdit
{
    public class Queue
    {
        readonly List<QueueItem> backward = new List<QueueItem>();
        readonly List<QueueItem> forward = new List<QueueItem>();
        public int Count => backward.Count;

        [NotNull]
        public QueueItem CurrentItem { get; } = new QueueItem();

        public bool CanBackward => backward.Count > 0;
        public bool CanForward => forward.Count > 0;

        public void Add([NotNull] string filename, int position, int line)
        {
            if (CurrentItem.Equals(filename, position, line)) return;
            forward.Clear();
            if (!CurrentItem.IsEmpty) backward.Add(CurrentItem.Clone());
            CurrentItem.FileName = filename;
            CurrentItem.Position = position;
            CurrentItem.Line = line;
        }

        public void Clear()
        {
            backward.Clear();
            forward.Clear();
            CurrentItem.Clear();
        }

        public void Backward()
        {
            if (backward.Count == 0) return;
            var last = backward.Last();
            if (!CurrentItem.IsEmpty) forward.Add(CurrentItem.Clone());
            last.CopyTo(CurrentItem);
            backward.Remove(last);
        }

        public void Forward()
        {
            if (forward.Count == 0) return;
            var last = forward.Last();
            if (!CurrentItem.IsEmpty) backward.Add(CurrentItem.Clone());
            last.CopyTo(CurrentItem);
            forward.Remove(last);
        }

        [NotNull]
        public QueueItem GetBackwardItem() => CanBackward ? backward.Last() : new QueueItem();
    }

    public class QueueItem
    {
        public string FileName { get; set; }
        public int Position { get; set; }
        public int Line { get; set; }
        public bool IsEmpty => string.IsNullOrEmpty(FileName) && Position == 0 && Line == 0;

        public bool Equals([NotNull] QueueItem to) => Equals(to.FileName, to.Position, to.Line);

        public bool Equals(string fileName, int position, int line)
        {
            return fileName == FileName && position == Position && line == Line;
        }

        public QueueItem Clone() => new QueueItem {FileName = FileName, Position = Position, Line = Line};

        public void Clear()
        {
            FileName = string.Empty;
            Position = 0;
            Line = 0;
        }

        public void CopyTo([NotNull] QueueItem to)
        {
            to.FileName = FileName;
            to.Position = Position;
            to.Line = Line;
        }
    }
}