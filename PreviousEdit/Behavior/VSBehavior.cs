using JetBrains.Annotations;

namespace PreviousEdit.Behavior
{
    public class VSBehavior
    {
        readonly Queue queue = new Queue();
        public bool CanBackward => queue.CanBackward;
        public bool CanForward => queue.CanForward;
        public QueueItem CurrentItem => queue.CurrentItem;

        public void Clear() => queue.Clear();

        public void Backward() => queue.Backward();

        public void Forward() => queue.Forward();

        public void Add(string fileName, int position, int line)
        {
            var backwardItem = queue.GetBackwardItem();
            if (backwardItem.FileName == fileName && backwardItem.Line == line) 
                CurrentItem.Position = position;
            else queue.Add(fileName, position, line);
        }

        [NotNull]
        public QueueItem GetBackwardItem() => queue.GetBackwardItem();
    }
}