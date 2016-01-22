using JetBrains.Annotations;

namespace PreviousEdit.Behavior
{
    public class VSBehavior
    {
        readonly Queue queue = new Queue();
        public bool CanBackward => queue.CanBackward;
        public bool CanForward => queue.CanForward;

        [NotNull]
        public QueueItem CurrentItem => queue.CurrentItem;

        public void Clear() => queue.Clear();

        public void Backward() => queue.Backward();

        public void Forward() => queue.Forward();

        public void Add([NotNull] string fileName, int position, int line)
        {
            var backwardItem = queue.GetBackwardItem();
            if (backwardItem.FileName == fileName && backwardItem.Line == line)
            {
                CurrentItem.FileName = fileName;
                CurrentItem.Position = position;
                CurrentItem.Line = line;
            }   
            else queue.Add(fileName, position, line);
        }

        [NotNull]
        public QueueItem GetBackwardItem() => queue.GetBackwardItem();
    }
}