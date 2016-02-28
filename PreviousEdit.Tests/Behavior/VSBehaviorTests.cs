using Microsoft.VisualStudio.TestTools.UnitTesting;
using PreviousEdit.Behavior;

namespace PreviousEdit.Tests.Behavior
{
    [TestClass]
    public class VSBehaviorTests
    {
        [TestMethod]
        public void Add()
        {
            var behavior = new VSBehavior();
            behavior.Add("filename", 100, 1);
            Assert.IsTrue(behavior.CurrentItem.Equals("filename", 100, 1));
            behavior.Add("filename", 100, 1);
            Assert.IsTrue(behavior.CurrentItem.Equals("filename", 100, 1));
            behavior.Add("filename", 110, 1);
            Assert.IsTrue(behavior.CurrentItem.Equals("filename", 110, 1));
        }
        
        [TestMethod]
        public void Clear()
        {
            var behavior = new VSBehavior();
            behavior.Add("filename", 0, 1);
            behavior.Add("filename", 10, 1);
            behavior.Clear();
            Assert.IsTrue(behavior.CurrentItem.IsEmpty);
            Assert.IsFalse(behavior.CanBackward);
            Assert.IsFalse(behavior.CanForward);
        }

        [TestMethod]
        public void Backward()
        {
            var behavior = new VSBehavior();
            behavior.Add("filename0", 0, 1);
            behavior.Add("filename1", 0, 1);
            behavior.Add("filename2", 0, 1);
            behavior.Add("filename3", 0, 1);
            behavior.Add("filename4", 0, 1);
            behavior.Backward();
            Assert.IsTrue(behavior.CurrentItem.Equals("filename3", 0, 1));
            behavior.Backward();
            Assert.IsTrue(behavior.CurrentItem.Equals("filename2", 0, 1));
            behavior.Backward();
            Assert.IsTrue(behavior.CurrentItem.Equals("filename1", 0, 1));
            behavior.Backward();
            Assert.IsTrue(behavior.CurrentItem.Equals("filename0", 0, 1));
            behavior.Backward();
            behavior.Backward();
            behavior.Backward();
            Assert.IsTrue(behavior.CurrentItem.Equals("filename0", 0, 1));
        }

        [TestMethod]
        public void Forward_0()
        {
            var behavior = new VSBehavior();
            behavior.Add("filename0", 0, 1);
            behavior.Add("filename1", 0, 1);
            behavior.Add("filename2", 0, 1);
            behavior.Add("filename3", 0, 1);
            behavior.Add("filename4", 0, 1);
            behavior.Backward();
            behavior.Backward();
            behavior.Backward();
            behavior.Backward();
            behavior.Backward();
            Assert.IsTrue(behavior.CurrentItem.Equals("filename0", 0, 1));
            behavior.Forward();
            Assert.IsTrue(behavior.CurrentItem.Equals("filename1", 0, 1));
            behavior.Forward();
            Assert.IsTrue(behavior.CurrentItem.Equals("filename2", 0, 1));
            behavior.Forward();
            Assert.IsTrue(behavior.CurrentItem.Equals("filename3", 0, 1));
            behavior.Forward();
            Assert.IsTrue(behavior.CurrentItem.Equals("filename4", 0, 1));
        }

        [TestMethod]
        public void Forward_1()
        {
            var behavior = new VSBehavior();
            behavior.Add("filename0", 0, 1);
            behavior.Add("filename1", 0, 1);
            behavior.Add("filename2", 0, 1);
            behavior.Add("filename3", 0, 1);
            behavior.Add("filename4", 0, 1);
            behavior.Backward();
            behavior.Backward();
            Assert.IsTrue(behavior.CurrentItem.Equals("filename2", 0, 1));
            behavior.Forward();
            Assert.IsTrue(behavior.CurrentItem.Equals("filename3", 0, 1));
            behavior.Backward();
            behavior.Backward();
            Assert.IsTrue(behavior.CurrentItem.Equals("filename1", 0, 1));
            behavior.Forward();
            behavior.Forward();
            Assert.IsTrue(behavior.CurrentItem.Equals("filename3", 0, 1));
            behavior.Add("filename5", 0, 1);
            Assert.IsTrue(behavior.CurrentItem.Equals("filename5", 0, 1));
            behavior.Forward();
            behavior.Add("filename5", 0, 1);
        }

        [TestMethod]
        public void CanBackward()
        {
            var behavior = new VSBehavior();
            Assert.IsFalse(behavior.CanBackward);
            behavior.Add("filename0", 0, 1);
            behavior.Add("filename1", 0, 1);
            Assert.IsTrue(behavior.CanBackward);
            behavior.Backward();
            Assert.IsFalse(behavior.CanBackward);
        }

        [TestMethod]
        public void CanForward()
        {
            var behavior = new VSBehavior();
            Assert.IsFalse(behavior.CanForward);
            behavior.Add("filename0", 0, 1);
            behavior.Add("filename1", 0, 1);
            Assert.IsFalse(behavior.CanForward);
            behavior.Backward();
            Assert.IsTrue(behavior.CanForward);
            behavior.Forward();
            Assert.IsFalse(behavior.CanForward);
            behavior.Backward();
            behavior.Add("filename3", 0, 1);
            Assert.IsFalse(behavior.CanForward);
        }

        [TestMethod]
        public void Change()
        {
            var behavior = new VSBehavior();
            behavior.Add("filename0", 0, 1);
            behavior.Add("filename1", 1, 1);
            behavior.Add("filename0", 100, 2);
            behavior.Add("filename3", 1, 1);
            behavior.Add("filename0", 1000, 3);
            behavior.Update("filename0", 150, 1, 1);
            Assert.IsTrue(behavior.CurrentItem.Equals("filename0", 1001, 4));
            behavior.Update("filename0", 0, 10, 1);
            Assert.IsTrue(behavior.CurrentItem.Equals("filename0", 1011, 5));
            behavior.Backward();
            behavior.Backward();
            Assert.IsTrue(behavior.CurrentItem.Equals("filename0", 110, 3));
            behavior.Backward();
            behavior.Backward();
            Assert.IsTrue(behavior.CurrentItem.Equals("filename0", 10, 2));
        }

        [TestMethod]
        public void getBackward()
        {
            var behavior = new VSBehavior();
            Assert.IsNotNull(behavior.GetBackward());
            const string fileName = "fileName";
            behavior.Add(fileName, position: 0, line: 0);
            behavior.Add(fileName, position: 1, line: 1);
            var backward = behavior.GetBackward();
            Assert.AreEqual(1, backward.Count);
            Assert.IsTrue(new QueueItem { FileName = fileName, Position = 0, Line = 0 }.Equals(backward[0]));
        }

        [TestMethod]
        public void getForward()
        {
            var behavior = new VSBehavior();
            Assert.IsNotNull(behavior.GetForward());
            const string fileName = "fileName";
            behavior.Add(fileName, position: 0, line: 0);
            behavior.Add(fileName, position: 1, line: 1);
            behavior.Backward();
            var forward = behavior.GetForward();
            Assert.AreEqual(1, forward.Count);
            Assert.IsTrue(new QueueItem { FileName = fileName, Position = 1, Line = 1 }.Equals(forward[0]));
        }
    }
}