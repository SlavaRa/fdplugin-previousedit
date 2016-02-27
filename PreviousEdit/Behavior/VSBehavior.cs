using System;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace PreviousEdit.Behavior
{
    public class VSBehavior
    {
        public event EventHandler Change
        {
            add { queue.Change += value; }
            remove { queue.Change -= value; }
        }

        readonly Queue queue = new Queue();
        public bool CanBackward => queue.CanBackward;
        public bool CanForward => queue.CanForward;
        
        [NotNull]
        public QueueItem CurrentItem => queue.CurrentItem;

        public void Clear() => queue.Clear();

        public void Backward() => queue.Backward();

        public void Forward() => queue.Forward();

        public void Add([NotNull] string fileName, int position, int line) => queue.Add(fileName, position, line);

        [NotNull]
        public QueueItem GetBackwardItem() => queue.GetBackwardItem();

        public void Update([NotNull] string fileName, int startPosition, int charsAdded, int linesAdded)
        {
            queue.Update(fileName, startPosition, charsAdded, linesAdded);
        }

        public ToolStripItem[] GetProvider() => queue.GetProvider();
    }
}