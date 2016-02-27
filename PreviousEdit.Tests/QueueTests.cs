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
        public void Add_1()
        {
            const string fileName = "filename";
            var queue = new Queue();
            queue.Add(fileName, position: 100, line: 1);
            Assert.AreEqual(0, queue.Count);
            Assert.IsTrue(queue.CurrentItem.Equals(fileName, position: 100, line: 1));
            queue.Add(fileName, position: 100, line: 2);
            Assert.AreEqual(1, queue.Count);
            Assert.IsTrue(queue.CurrentItem.Equals(fileName, position: 100, line: 2));
            Assert.IsTrue(queue.GetBackwardItem().Equals(fileName, position: 100, line: 1));
            queue.Add(fileName, position: 100, line: 1);
            Assert.AreEqual(2, queue.Count);
            Assert.IsTrue(queue.CurrentItem.Equals(fileName, position: 100, line: 1));
            Assert.IsTrue(queue.GetBackwardItem().Equals(fileName, position: 100, line: 2));
            queue.Add(fileName, position: 100, line: 2);
            Assert.AreEqual(3, queue.Count);
            Assert.IsTrue(queue.CurrentItem.Equals(fileName, position: 100, line: 2));
            Assert.IsTrue(queue.GetBackwardItem().Equals(fileName, position: 100, line: 1));
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
            const string filename0 = "filename0";
            queue.Add(filename0, position: 0, line: 1);
            queue.Add("filename1", 1, 1);
            queue.Add(filename0, position: 100, line: 2);
            queue.Add("filename3", 1, 1);
            queue.Add(filename0, position: 1000, line: 3);
            queue.Update(filename0, startPosition: 150, charsAdded: 1, linesAdded: 1);
            Assert.IsTrue(queue.CurrentItem.Equals(filename0, position: 1001, line: 4));
            queue.Update(filename0, 0, 10, 1);
            Assert.IsTrue(queue.CurrentItem.Equals(filename0, position: 1011, line: 5));
            queue.Backward();
            queue.Backward();//skip filename3
            Assert.IsTrue(queue.CurrentItem.Equals(filename0, position: 110, line: 3));
            queue.Backward();
            queue.Backward();//skip filename1
            Assert.IsTrue(queue.CurrentItem.Equals(filename0, position: 10, line: 2));
        }

        [TestMethod]
        public void Change_0()
        {
            const string fileName = "filename0";
            var queue = new Queue();
            queue.Add(fileName, position: 0, line: 1);
            queue.Add(fileName, position: 100, line: 2);
            queue.Add(fileName, position: 1000, line: 3);
            queue.Update(fileName, startPosition: 101, charsAdded: -999, linesAdded: -1);
            Assert.IsTrue(queue.CurrentItem.Equals(fileName, position: 100, line: 2));
            queue.Backward();
            Assert.IsTrue(queue.CurrentItem.Equals(fileName, position: 0, line: 1));
        }

        [TestMethod]
        public void Change_1()
        {
            const string filename = "filename";
            var queue = new Queue();
            queue.Add(filename, position: 0, line: 1);
            queue.Add(filename, position: 4, line: 2);
            queue.Add(filename, position: 9, line: 3);
            Assert.IsTrue(queue.CurrentItem.Equals(filename, position: 9, line: 3));
            queue.Update(filename, startPosition: 3, charsAdded: 100, linesAdded: 0);
            Assert.IsTrue(queue.CurrentItem.Equals(filename, position: 109, line: 3));
            queue.Backward();
            Assert.IsTrue(queue.CurrentItem.Equals(filename, position: 104, line: 2));
        }

        [TestMethod]
        public void Change_2()
        {
            const string filename = "filename";
            var queue = new Queue();
            queue.Add(filename, position: 0, line: 1);
            queue.Add(filename, position: 6, line: 2);
            queue.Add(filename, position: 8, line: 3);
            Assert.IsTrue(queue.CurrentItem.Equals(filename, position: 8, line: 3));
            queue.Update(filename, startPosition: 7, charsAdded: -1, linesAdded: 0);
            Assert.IsTrue(queue.CurrentItem.Equals(filename, position: 7, line: 3));
            queue.Backward();
            Assert.IsTrue(queue.CurrentItem.Equals(filename, position: 6, line: 2));
        }

        [TestMethod]
        public void Change_3()
        {
            const string filename = "filename";
            var queue = new Queue();
            queue.Add(filename, position: 0, line: 1);
            queue.Add(filename, position: 6, line: 2);
            queue.Add(filename, position: 8, line: 3);
            Assert.IsTrue(queue.CurrentItem.Equals(filename, position: 8, line: 3));
            queue.Update(filename, startPosition: 4, charsAdded: -4, linesAdded: -1);
            Assert.IsTrue(queue.CurrentItem.Equals(filename, position: 4, line: 2));
            queue.Backward();
            Assert.IsTrue(queue.CurrentItem.Equals(filename, position: 0, line: 1));
        }
    }
}