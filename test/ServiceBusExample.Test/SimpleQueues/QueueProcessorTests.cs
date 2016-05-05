using ServiceBusExample.SimpleQueues;
using ServiceBusExample.SimpleQueues.Stubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServiceBusExample.SimpleQueues.Messages;

namespace ServiceBusExample.SimpleQueues
{
    //
    // WARNING: This Test integrates with the Azure ServiceBus whos endpoint is defined in the App.config file.
    //
    // NOTE: For some reason this solution doesn't like async tests.
    //
    [TestClass]
    public class QueueProcessorTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            // Assure the Queue is cleared before starting.
            using (QueueManager<TestQueueMessage> manager = new QueueManager<TestQueueMessage>())
            {
                manager.ClearQueue();
                manager.DeadLetters.ClearQueue();
            }
        }

        #region Helper Methods...

        private QueueMessageBase EnqueueMessage(TestQueueMessage message)
        {
            using (QueueManager<TestQueueMessage> manager = new QueueManager<TestQueueMessage>())
            {
                manager.Send(message);
                return message;
            }
        }

        #endregion




        // Add a widget to the Queue. Retrieve it, and flag it deleted.
        [TestMethod]
        public async Task ProcessMessageTest()
        {
            // Arrange
            QueueConsumerStub consumer = new QueueConsumerStub();
            QueueProcessor processor = new QueueProcessor(consumer);
            var sentMessage = EnqueueMessage(new TestQueueMessage());

            // Act
            await processor.HeartbeatAsync();

            // Assert
            Assert.AreEqual(1, consumer.ProcessedMessages.Count);
            TestQueueMessage consumedMessage = (TestQueueMessage)consumer.ProcessedMessages[0];
            Assert.AreEqual(sentMessage, consumedMessage);
        }


        // Processes two messages
        [TestMethod]
        public async Task ProcessTwoMessages()
        {
            // Arrange
            QueueConsumerStub consumer = new QueueConsumerStub();
            QueueProcessor processor = new QueueProcessor(consumer);
            TestQueueMessage sentMessage1 = (TestQueueMessage)EnqueueMessage(new TestQueueMessage());
            TestQueueMessage sentMessage2 = (TestQueueMessage)EnqueueMessage(new TestQueueMessage());

            // Act
            await processor.HeartbeatAsync();

            // Assert
            Assert.AreEqual(2, consumer.ProcessedMessages.Count);

            var processedMessage1 = consumer.ProcessedMessages.FirstOrDefault(x => ((TestQueueMessage)x).Name == sentMessage1.Name);
            var processedMessage2 = consumer.ProcessedMessages.FirstOrDefault(x => ((TestQueueMessage)x).Name == sentMessage2.Name);

            Assert.IsNotNull(processedMessage1);
            Assert.IsNotNull(processedMessage2);
        }


        // Heartbeat and expect the Consumer to throw an exception which is swallowed by the Controller.
        // The controller will then requeue the message.
        [TestMethod]
        public async Task AttemptToDeliverAndFail()
        {
            // Arrange
            QueueConsumerStub consumer = new QueueConsumerStub().ThrowsException();
            QueueProcessor processor = new QueueProcessor(consumer);
            var widget = EnqueueMessage(new TestQueueMessage());

            // Act
            await processor.HeartbeatAsync(); // Expecting this to fail...

            // Assert
            int attempts;
            using (QueueManager<TestQueueMessage> manager = new QueueManager<TestQueueMessage>())
            {
                TestQueueMessage message = manager.Receive();
                attempts = message.Attempts;
                manager.Complete(message);
            }
            Assert.AreEqual(1, attempts);
        }


        // Tries to send a message 3 times, it is then thrown on the dead letter pool.
        [TestMethod]
        public async Task MoveToDeadLetterQueue()
        {
            // Arrange
            QueueConsumerStub consumer = new QueueConsumerStub().ThrowsException();
            QueueProcessor processor = new QueueProcessor(consumer);
            var widget = EnqueueMessage(new TestQueueMessage());

            // Act
            await processor.HeartbeatAsync(); // Expecting this to fail...
            await processor.HeartbeatAsync(); // As Above...
            await processor.HeartbeatAsync(); // This is the 3rd attempt...

            // Assert
            long count;
            long deadLetterCount;

            using (QueueManager<TestQueueMessage> manager = new QueueManager<TestQueueMessage>())
            {
                count = manager.GetCount();
                deadLetterCount = manager.DeadLetters.GetCount();
            }

            Assert.AreEqual(0, consumer.ProcessedMessages.Count);
            Assert.AreEqual(0, count);
            Assert.AreEqual(1, deadLetterCount);

            // Clean up
            using (QueueManager<TestQueueMessage> manager = new QueueManager<TestQueueMessage>())
            {
                manager.DeadLetters.ClearQueue();
            }
        }
    }
}
