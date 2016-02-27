using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PreviousEdit.Tests
{
    [TestClass]
    public class QueueItemTests
    {
        [TestMethod]
        public void Equals()
        {
            var item = new QueueItem {FileName = "test", Position = 100, Line = 10};
            Assert.IsTrue(item.Equals("test", 100, 10));
            Assert.IsFalse(item.Equals(string.Empty, 100, 10));
        }

        [TestMethod]
        public void Clone()
        {
            var item = new QueueItem {FileName = "test", Position = 100, Line = 10};
            Assert.IsTrue(item.Equals(item.Clone()));
        }

        [TestMethod]
        public void Clear()
        {
            var item = new QueueItem {FileName = "test", Position = 100, Line = 10};
            item.Clear();
            Assert.IsTrue(item.Equals(string.Empty, 0, 0));
        }

        [TestMethod]
        public void IsEmpty()
        {
            Assert.IsTrue(new QueueItem().IsEmpty);
        }

        [TestMethod]
        public void CopyTo()
        {
            var it0 = new QueueItem {FileName = "filename", Position = 10, Line = 1};
            var it1 = new QueueItem();
            Assert.IsFalse(it0.Equals(it1));
            it0.CopyTo(it1);
            Assert.IsTrue(it0.Equals(it1));
        }
    }

    [TestClass]
    public class QueueTests
    {
        [TestMethod]
        public void Add()
        {
            var queue = new Queue();
            queue.Add("filename", 100, 1);
            Assert.AreEqual(0, queue.Count);
            Assert.IsTrue(queue.CurrentItem.Equals("filename", 100, 1));
            queue.Add("filename", 100, 1);
            Assert.AreEqual(0, queue.Count);
            Assert.IsTrue(queue.CurrentItem.Equals("filename", 100, 1));
            queue.Add("filename", 110, 1);
            Assert.AreEqual(1, queue.Count);
            Assert.IsTrue(queue.CurrentItem.Equals("filename", 110, 1));
        }

        [TestMethod]
        public void Clear()
        {
            var queue = new Queue();
            queue.Add("filename", 0, 1);
            queue.Add("filename", 10, 1);
            queue.Clear();
            Assert.AreEqual(0, queue.Count);
            Assert.IsTrue(queue.CurrentItem.IsEmpty);
        }

        [TestMethod]
        public void Backward()
        {
            var queue = new Queue();
            queue.Add("filename0", 0, 1);
            queue.Add("filename1", 0, 1);
            queue.Add("filename2", 0, 1);
            queue.Add("filename3", 0, 1);
            queue.Add("filename4", 0, 1);
            queue.Backward();
            Assert.IsTrue(queue.CurrentItem.Equals("filename3", 0, 1));
            queue.Backward();
            Assert.IsTrue(queue.CurrentItem.Equals("filename2", 0, 1));
            queue.Backward();
            Assert.IsTrue(queue.CurrentItem.Equals("filename1", 0, 1));
            queue.Backward();
            Assert.IsTrue(queue.CurrentItem.Equals("filename0", 0, 1));
            queue.Backward();
            queue.Backward();
            queue.Backward();
            Assert.IsTrue(queue.CurrentItem.Equals("filename0", 0, 1));
        }

        [TestMethod]
        public void Forward_0()
        {
            var queue = new Queue();
            queue.Add("filename0", 0, 1);
            queue.Add("filename1", 0, 1);
            queue.Add("filename2", 0, 1);
            queue.Add("filename3", 0, 1);
            queue.Add("filename4", 0, 1);
            queue.Backward();
            queue.Backward();
            queue.Backward();
            queue.Backward();
            queue.Backward();
            Assert.IsTrue(queue.CurrentItem.Equals("filename0", 0, 1));
            queue.Forward();
            Assert.IsTrue(queue.CurrentItem.Equals("filename1", 0, 1));
            queue.Forward();
            Assert.IsTrue(queue.CurrentItem.Equals("filename2", 0, 1));
            queue.Forward();
            Assert.IsTrue(queue.CurrentItem.Equals("filename3", 0, 1));
            queue.Forward();
            Assert.IsTrue(queue.CurrentItem.Equals("filename4", 0, 1));
        }

        [TestMethod]
        public void Forward_1()
        {
            var queue = new Queue();
            queue.Add("filename0", 0, 1);
            queue.Add("filename1", 0, 1);
            queue.Add("filename2", 0, 1);
            queue.Add("filename3", 0, 1);
            queue.Add("filename4", 0, 1);
            queue.Backward();
            queue.Backward();
            Assert.IsTrue(queue.CurrentItem.Equals("filename2", 0, 1));
            queue.Forward();
            Assert.IsTrue(queue.CurrentItem.Equals("filename3", 0, 1));
            queue.Backward();
            queue.Backward();
            Assert.IsTrue(queue.CurrentItem.Equals("filename1", 0, 1));
            queue.Forward();
            queue.Forward();
            Assert.IsTrue(queue.CurrentItem.Equals("filename3", 0, 1));
            queue.Add("filename5", 0, 1);
            Assert.IsTrue(queue.CurrentItem.Equals("filename5", 0, 1));
            queue.Forward();
            queue.Add("filename5", 0, 1);
        }

        [TestMethod]
        public void CanBackward()
        {
            var queue = new Queue();
            Assert.IsFalse(queue.CanBackward);
            queue.Add("filename0", 0, 1);
            queue.Add("filename1", 0, 1);
            Assert.IsTrue(queue.CanBackward);
            queue.Backward();
            Assert.IsFalse(queue.CanBackward);
        }

        [TestMethod]
        public void GetBackwardItem()
        {
            var queue = new Queue();
            Assert.IsTrue(queue.GetBackwardItem().IsEmpty);
            queue.Add("filename0", 0, 1);
            queue.Add("filename1", 0, 1);
            Assert.IsTrue(queue.GetBackwardItem().Equals("filename0", 0, 1));
        }
        
        [TestMethod]
        public void CanForward()
        {
            var queue = new Queue();
            Assert.IsFalse(queue.CanForward);
            queue.Add("filename0", 0, 1);
            queue.Add("filename1", 0, 1);
            Assert.IsFalse(queue.CanForward);
            queue.Backward();
            Assert.IsTrue(queue.CanForward);
            queue.Forward();
            Assert.IsFalse(queue.CanForward);
            queue.Backward();
            queue.Add("filename3", 0, 1);
            Assert.IsFalse(queue.CanForward);
        }

        [TestMethod]
        public void Change()
        {
            var queue = new Queue();
            queue.Add("filename0", 0, 1);
            queue.Add("filename1", 1, 1);
            queue.Add("filename0", 100, 2);
            queue.Add("filename3", 1, 1);
            queue.Add("filename0", 1000, 3);
            queue.Change("filename0", 150, 1, 1);
            Assert.IsTrue(queue.CurrentItem.Equals("filename0", 1001, 4));
            queue.Change("filename0", 0, 10, 1);
            Assert.IsTrue(queue.CurrentItem.Equals("filename0", 1011, 5));
            queue.Backward();
            queue.Backward();
            Assert.IsTrue(queue.CurrentItem.Equals("filename0", 110, 3));
            queue.Backward();
            queue.Backward();
            Assert.IsTrue(queue.CurrentItem.Equals("filename0", 10, 2));
        }

        [TestMethod]
        public void Change_0()
        {
            var queue = new Queue();
            queue.Add("filename0", 0, 1);
            queue.Add("filename0", 100, 2);
            queue.Add("filename0", 1000, 3);
            queue.Change("filename0", 0, -1, -1);
            Assert.IsTrue(queue.CurrentItem.Equals("filename0", 999, 2));
            queue.Backward();
            Assert.IsTrue(queue.CurrentItem.Equals("filename0", 99, 1));
            queue.Backward();
            Assert.IsTrue(queue.CurrentItem.Equals("filename0", 0, 0));
        }

        [TestMethod]
        public void RemoveLines()
        {
            var queue = new Queue();
            queue.Add("filename0", 0, 1);
            queue.Add("filename0", 10, 2);
            queue.Add("filename0", 20, 3);
            queue.Add("filename0", 30, 4);
            queue.Add("filename0", 40, 5);
            queue.RemoveLines("filename0", 20, 10, 1);
            Assert.AreEqual(4, queue.CurrentItem.Line);
            queue.RemoveLines("filename0", 0, 10, 1);
            queue.Backward();
            Assert.AreEqual(2, queue.CurrentItem.Line);
        }
    }
}