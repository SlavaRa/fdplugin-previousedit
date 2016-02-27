using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace PreviousEdit
{
    public class Queue
    {
        [CanBeNull]
        public event EventHandler Change;

        readonly List<QueueItem> backward = new List<QueueItem>();
        readonly List<QueueItem> forward = new List<QueueItem>();
        public int Count => backward.Count;

        [NotNull]
        public QueueItem CurrentItem { get; } = new QueueItem();

        public bool CanBackward => backward.Count > 0;
        public bool CanForward => forward.Count > 0;

        public void Add([NotNull] string fileName, int position, int line)
        {
            if (CurrentItem.Equals(fileName, position, line)) return;
            forward.Clear();
            var count = backward.Count;
            if (count > 0 && backward.Last().Equals(CurrentItem)) backward.RemoveAt(count - 1);
            else
            {
                if (!CurrentItem.IsEmpty) backward.Add(CurrentItem.Clone());
                CurrentItem.FileName = fileName;
                CurrentItem.Position = position;
                CurrentItem.Line = line;
            }
        }

        public void Clear()
        {
            backward.Clear();
            forward.Clear();
            CurrentItem.Clear();
            Change?.Invoke(this, EventArgs.Empty);
        }

        public void Backward()
        {
            if (backward.Count == 0) return;
            var last = backward.Last();
            if (!CurrentItem.IsEmpty) forward.Add(CurrentItem.Clone());
            last.CopyTo(CurrentItem);
            backward.Remove(last);
            Change?.Invoke(this, EventArgs.Empty);
        }

        public void Forward()
        {
            if (forward.Count == 0) return;
            var last = forward.Last();
            if (!CurrentItem.IsEmpty) backward.Add(CurrentItem.Clone());
            last.CopyTo(CurrentItem);
            forward.Remove(last);
            Change?.Invoke(this, EventArgs.Empty);
        }

        [NotNull]
        public QueueItem GetBackwardItem() => CanBackward ? backward.Last() : new QueueItem();

        [NotNull]
        public ToolStripItem[] GetProvider()
        {
            var items = new List<QueueItem>(backward);
            items.Add(CurrentItem);
            var result = new List<ToolStripItem>();
            for (var i = items.Count - 1; i >= 0; i--)
            {
                var it = items[i];
                var text = $"{Path.GetFileName(it.FileName)}: Line:{it.Line} Position:{it.Position}";
                var button = new ToolStripMenuItem
                {
                    Text = text,
                    Width = 340,
                    Tag = it,
                };
                if (button.Tag != CurrentItem) button.Click += OnBackwardMenuClick;
                else button.ImageKey = "current";
                result.Add(button);
            }
            return result.ToArray();
        }

        public void Update([NotNull] string fileName, int startPosition, int charsAdded, int linesAdded)
        {
            forward.Clear();
            if (fileName == CurrentItem.FileName) backward.Add(CurrentItem);
            var endPosition = startPosition + Math.Abs(charsAdded);
            if (linesAdded < 0)
            {
                foreach (var it in backward.ToList())
                {
                    var itemPosition = it.Position;
                    if (it.FileName == fileName && itemPosition >= startPosition && itemPosition < endPosition)
                    {
                        it.Clear();
                        backward.Remove(it);
                    }
                }
            }
            foreach (var it in backward)
            {
                if (it.FileName == fileName && it.Position >= startPosition)
                {
                    it.Position += charsAdded;
                    it.Line += linesAdded;
                }
            }
            if (backward.Contains(CurrentItem)) backward.Remove(CurrentItem);
            else Backward();
            Change?.Invoke(this, EventArgs.Empty);
        }

        void OnBackwardMenuClick(object sender, EventArgs args)
        {
            var item = (QueueItem) ((ToolStripMenuItem) sender).Tag;
            var index = backward.IndexOf(item) + 1;
            var count = backward.Count - index;
            var items = backward.GetRange(index, count);
            backward.RemoveRange(index, count);
            forward.AddRange(items);
            CurrentItem.Clear();
            Backward();
        }
    }

    public class QueueItem
    {
        public string FileName { get; set; }

        int position;

        public int Position
        {
            get { return position; }
            set { position = Math.Max(0, value); }
        }

        int line;

        public int Line
        {
            get { return line; }
            set { line = Math.Max(0, value); }
        }

        public bool IsEmpty => string.IsNullOrEmpty(FileName) && Position == 0 && Line == 0;

        public bool Equals([NotNull] QueueItem to) => Equals(to.FileName, to.Position, to.Line);

        public bool Equals([NotNull] string fileName, int position, int line)
        {
            return fileName == FileName && position == Position && line == Line;
        }

        [NotNull]
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

        public override string ToString() => $"{nameof(FileName)}:{FileName}, {nameof(Line)}:{Line}, {nameof(Position)}:{Position}";
    }
}