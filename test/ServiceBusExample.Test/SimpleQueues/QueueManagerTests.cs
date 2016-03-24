using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceBusExample.SimpleQueues.Messages;
using ServiceBusExample.SimpleQueues.Stubs;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using System.IO.Compression;

namespace ServiceBusExample.SimpleQueues.Tests
{
    [TestClass]
    public class QueueManagerTests
    {

        [TestInitialize]
        public void TestInitialize()
        {
            // Assure the Queue is cleared before starting.
            using (QueueManager<RedWidget> manager = new QueueManager<RedWidget>())
            {
                manager.ClearQueue();
                manager.DeadLetters.ClearQueue();
            }

            // Assure the Queue is cleared before starting.
            using (QueueManager<BlueWidget> manager = new QueueManager<BlueWidget>())
            {
                manager.ClearQueue();
            }
        }

        // Add a widget to the Queue. Retrieve it, and flag it deleted.
        [TestMethod]
        public void SendMessageTest()
        {
            using (QueueManager<RedWidget> manager = new QueueManager<RedWidget>())
            {
                // Arrange
                var sentWidget = new RedWidget();

                // Act
                manager.Send(sentWidget);

                // Assert
                RedWidget receivedWidget = manager.Receive();
                Assert.AreEqual(sentWidget, receivedWidget);

                // Cleanup
                manager.Complete(receivedWidget);
            }
        }

        // Add an item to the Queue and assure the count is 1 or more.
        [TestMethod]
        public void QueueCountTest()
        {
            using (QueueManager<RedWidget> manager = new QueueManager<RedWidget>())
            {
                // Arrange
                var widget = new RedWidget();
                manager.Send(widget);

                // Act
                long count = manager.GetCount();

                // Assert
                Assert.IsTrue(count >= 1);

                // Cleanup
                widget = manager.Receive();
                manager.Complete(widget);
            }
        }

        /// <summary>
        /// Get 2 messages, send the 1st one to the back of the queue, check the order is correct.
        /// </summary>
        [TestMethod]
        public void MoveMessageToBackOfQueueTest()
        {
            using (QueueManager<RedWidget> manager = new QueueManager<RedWidget>())
            {
                // Arrange (Add the 1st and 2nd widgets to the queue)
                var sentWidget1 = new RedWidget();
                var sentWidget2 = new RedWidget();
                manager.Send(sentWidget1);
                manager.Send(sentWidget2);
                var moveWidget1 = manager.Receive();

                // Act (Receive and move the 1st widget to the back of the queue)
                manager.MoveToEnd(moveWidget1);

                // Assert (Consume the 2nd Widget & check the order is correct)
                RedWidget receivedWidget1 = manager.Receive();
                manager.Complete(receivedWidget1);
                RedWidget receivedWidget2 = manager.Receive();
                manager.Complete(receivedWidget2);

                Assert.AreEqual(sentWidget1, receivedWidget2);
                // Cleanup
            }
        }


        // Add an item to the Queue and assure the count is 1 or more.
        [TestMethod]
        public void MoveToDeadLetterQueue()
        {
            using (QueueManager<RedWidget> manager = new QueueManager<RedWidget>())
            {
                // Arrange
                var widget = new RedWidget();
                manager.Send(widget);
                var receivedWidget = manager.Receive();

                // Act
                manager.MoveToDeadLetter(receivedWidget);

                // Assert
                long count = manager.GetCount();
                long deadLetterCount = manager.DeadLetters.GetCount();
                Assert.AreEqual(0, count);
                Assert.AreEqual(1, deadLetterCount);

                // Cleanup
                var recievedWidget2 = manager.DeadLetters.Receive();
                manager.DeadLetters.Complete(recievedWidget2);
            }
        }


        // Add an item to the Queue and assure the count is 1 or more.
        [TestMethod]
        public void ClearDeadLetterQueue()
        {
            using (QueueManager<RedWidget> manager = new QueueManager<RedWidget>())
            {
                // Arrange
                var widget = new RedWidget();
                manager.Send(widget);
                var receivedWidget = manager.Receive();
                manager.MoveToDeadLetter(receivedWidget);

                // Act
                manager.DeadLetters.ClearQueue();

                // Assert
                long deadLetterCount = manager.DeadLetters.GetCount();
                Assert.AreEqual(0, deadLetterCount);
            }
        }
    }
}
