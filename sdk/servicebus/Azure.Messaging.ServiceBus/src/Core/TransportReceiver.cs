﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Messaging.ServiceBus.Core
{
    /// <summary>
    /// Provides an abstraction for generalizing a message receiver so that a dedicated instance may provide operations
    /// for a specific transport, such as AMQP or JMS.  It is intended that the public <see cref="ServiceBusReceiver" /> and <see cref="ServiceBusProcessor" /> employ
    /// a transport receiver via containment and delegate operations to it rather than understanding protocol-specific details
    /// for different transports.
    /// </summary>
    internal abstract class TransportReceiver
    {
        /// <summary>
        /// Indicates whether or not this receiver has been closed.
        /// </summary>
        ///
        /// <value>
        /// <c>true</c> if the consumer is closed; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsClosed { get; }

        /// <summary>
        /// The name of the Service Bus entity that the receiver is connected to, specific to the
        /// Service Bus namespace that contains it.
        /// </summary>
        public virtual string EntityName { get; }

        /// <summary>
        /// The Session Id associated with the receiver.
        /// </summary>
        public virtual string SessionId { get; protected set; }

        /// <summary>
        /// Receives a batch of <see cref="ServiceBusReceivedMessage" /> from the entity using <see cref="ReceiveMode"/> mode.
        /// </summary>
        ///
        /// <param name="maximumMessageCount">The maximum number of messages that will be received.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <returns>List of messages received. Returns null if no message is found.</returns>
        public abstract Task<IList<ServiceBusReceivedMessage>> ReceiveBatchAsync(
            int maximumMessageCount,
            CancellationToken cancellationToken);

        /// <summary>
        /// Closes the connection to the transport consumer instance.
        /// </summary>
        ///
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        public abstract Task CloseAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Opens an AMQP link for use with receiver operations.
        /// </summary>
        ///
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <returns>A task to be resolved on when the operation has completed.</returns>
        public abstract Task OpenLinkAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Completes a <see cref="ServiceBusReceivedMessage"/>. This will delete the message from the service.
        /// </summary>
        ///
        /// <param name="receivedMessages">The <see cref="ServiceBusReceivedMessage"/> message to complete.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <remarks>
        /// This operation can only be performed on a message that was received by this receiver
        /// when <see cref="ReceiveMode"/> is set to <see cref="ReceiveMode.PeekLock"/>.
        /// </remarks>
        ///
        /// <returns>A task to be resolved on when the operation has completed.</returns>
        public abstract Task CompleteAsync(
            IEnumerable<ServiceBusReceivedMessage> receivedMessages,
            CancellationToken cancellationToken);

        /// <summary> Indicates that the receiver wants to defer the processing for the message.</summary>
        ///
        /// <param name="message">The <see cref="ServiceBusReceivedMessage"/> to defer.</param>
        /// <param name="propertiesToModify">The properties of the message to modify while deferring the message.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <remarks>
        /// A lock token can be found in <see cref="ServiceBusReceivedMessage.LockToken"/>,
        /// only when <see cref="ReceiveMode"/> is set to <see cref="ReceiveMode.PeekLock"/>.
        /// In order to receive this message again in the future, you will need to save the <see cref="ServiceBusReceivedMessage.SequenceNumber"/>
        /// and receive it using ReceiveDeferredMessageBatchAsync(IEnumerable, CancellationToken).
        /// Deferring messages does not impact message's expiration, meaning that deferred messages can still expire.
        /// This operation can only be performed on messages that were received by this receiver.
        /// </remarks>
        ///
        /// <returns>A task to be resolved on when the operation has completed.</returns>
        public abstract Task DeferAsync(
            ServiceBusReceivedMessage message,
            IDictionary<string, object> propertiesToModify = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Fetches the next batch of active messages without changing the state of the receiver or the message source.
        /// </summary>
        ///
        /// <param name="fromSequenceNumber">The sequence number from where to read the message.</param>
        /// <param name="messageCount">The maximum number of messages that will be fetched.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <remarks>
        /// The first call to PeekBatchBySequenceAsync(long, int, CancellationToken) fetches the first active message for this receiver. Each subsequent call
        /// fetches the subsequent message in the entity.
        /// Unlike a received message, peeked message will not have lock token associated with it, and hence it cannot be Completed/Abandoned/Deferred/Deadlettered/Renewed.
        /// Also, unlike <see cref="ReceiveBatchAsync(int, CancellationToken)"/>, this method will fetch even Deferred messages (but not Deadlettered message)
        /// </remarks>
        ///
        /// <returns>List of <see cref="ServiceBusReceivedMessage" /> that represents the next message to be read. Returns null when nothing to peek.</returns>
        public abstract Task<IList<ServiceBusReceivedMessage>> PeekBatchBySequenceAsync(
            long? fromSequenceNumber,
            int messageCount = 1,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Abandons a <see cref="ServiceBusReceivedMessage"/>. This will make the message available again for processing.
        /// </summary>
        ///
        /// <param name="message">The <see cref="ServiceBusReceivedMessage"/> to abandon.</param>
        /// <param name="propertiesToModify">The properties of the message to modify while abandoning the message.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <remarks>
        /// Abandoning a message will increase the delivery count on the message.
        /// This operation can only be performed on messages that were received by this receiver
        /// when <see cref="ReceiveMode"/> is set to <see cref="ReceiveMode.PeekLock"/>.
        /// </remarks>
        ///
        /// <returns>A task to be resolved on when the operation has completed.</returns>
        public abstract Task AbandonAsync(
            ServiceBusReceivedMessage message,
            IDictionary<string, object> propertiesToModify = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Moves a message to the deadletter sub-queue.
        /// </summary>
        ///
        /// <param name="message">The <see cref="ServiceBusReceivedMessage"/> to deadletter.</param>
        /// <param name="deadLetterReason">The reason for deadlettering the message.</param>
        /// <param name="deadLetterErrorDescription">The error description for deadlettering the message.</param>
        /// <param name="propertiesToModify">The properties of the message to modify while moving to sub-queue.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <remarks>
        /// In order to receive a message from the deadletter queue, you will need a new
        /// <see cref="ServiceBusReceiver"/> with the corresponding path.
        /// You can use EntityNameHelper.FormatDeadLetterPath(string)"/> to help with this.
        /// This operation can only be performed on messages that were received by this receiver
        /// when <see cref="ReceiveMode"/> is set to <see cref="ReceiveMode.PeekLock"/>.
        /// </remarks>
        ///
        /// <returns>A task to be resolved on when the operation has completed.</returns>
        public abstract Task DeadLetterAsync(
            ServiceBusReceivedMessage message,
            string deadLetterReason = default,
            string deadLetterErrorDescription = default,
            IDictionary<string, object> propertiesToModify = default,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Receives a <see cref="IList{Message}"/> of deferred messages identified by <paramref name="sequenceNumbers"/>.
        /// </summary>
        ///
        /// <param name="sequenceNumbers">An <see cref="IEnumerable{T}"/> containing the sequence numbers to receive.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <returns>Messages identified by sequence number are returned. Returns null if no messages are found.
        /// Throws if the messages have not been deferred.</returns>
        /// <seealso cref="DeferAsync"/>
        public abstract Task<IList<ServiceBusReceivedMessage>> ReceiveDeferredMessageBatchAsync(
            IEnumerable<long> sequenceNumbers,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Renews the lock on the message. The lock will be renewed based on the setting specified on the queue.
        /// </summary>
        /// <returns>New lock token expiry date and time in UTC format.</returns>
        ///
        /// <param name="lockToken">Lock token associated with the message.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        public abstract Task<DateTime> RenewMessageLockAsync(
            string lockToken,
            CancellationToken cancellationToken);

        /// <summary>
        /// Renews the lock on the session specified by the <see cref="SessionId"/>. The lock will be renewed based on the setting specified on the entity.
        /// </summary>
        ///
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        public abstract Task<DateTime> RenewSessionLockAsync(
            CancellationToken cancellationToken);
    }
}
