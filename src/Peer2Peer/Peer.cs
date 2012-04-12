/* 
    Peer 2 Peer Library
    Copyright (C) 2011 - 2012 Leonid Gordo

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace Peer2Peer
{

    public struct CloudRegistrationEventArgs
    {
        public IEnumerable<IPEndPoint> IpEndPoints { get; set; }
    }

    public struct UserMessageEventArgs
    {
        public byte[] UserData { get; set; }
    }

    public delegate void CloudRegistrationEventHandler(Peer sender, CloudRegistrationEventArgs eventArgs);
    public delegate void UserMessageReceivedEventHandler(Peer sender, UserMessageEventArgs eventArgs);

    public class Peer : IDisposable
    {
        Receiver receiver;
        private readonly List<Neighbor> neighbors;
        private readonly PeerIdentifier peerId;

        public event CloudRegistrationEventHandler ApplicationJoined;
        public event CloudRegistrationEventHandler ApplicationLeaved;
        public event CloudRegistrationEventHandler ApplicationDetected;
        public event UserMessageReceivedEventHandler UserMessageReceived;
        public RsaKeyStore RsaKeyStore { get; private set;}


    	readonly int broadcastPort;
        bool registeredInCloud;
        long outgoingPacketIndex;
        const int MaxPacketSize = 65300;


        public Peer(int port, bool loopbacksAllowed, RsaKeyStore rsaKeyStore)
        {
            neighbors = new List<Neighbor>();
            broadcastPort = port;
            peerId = PeerIdentifier.Create();
            RsaKeyStore = rsaKeyStore;
            receiver = new Receiver(port, peerId, loopbacksAllowed) { RsaKeyStore = rsaKeyStore};
            receiver.MessageReceived += OnMessageReceived;
        }

        public void Dispose()
        {
            if (receiver != null)
            {
                if (registeredInCloud)
                    UnregisterFromCloud();
                receiver.Dispose();
                receiver = null;
            }
        }

      
        private void OnMessageReceived(MessageReceivedEventArgs eventArgs)
        {
            var message = eventArgs.MessageReceived;
            TypeSwitch.Do(message,
               TypeSwitch.Case<AnnounceRegistrationMessage>(
               () => ProcessMessage((AnnounceRegistrationMessage)message)),
               TypeSwitch.Case<AnnounceLeavingMessage>(() => ProcessMessage((AnnounceLeavingMessage)message)),
               TypeSwitch.Case<ReplyRegistrationMessage>(() => ProcessMessage((ReplyRegistrationMessage)message)),
               TypeSwitch.Case<UserMessage>(() => ProcessMessage((UserMessage)message)),
               TypeSwitch.Default(() => { throw new NotSupportedException("Message type not supported"); }));

        }

        private IEnumerable<MessageVersion1> ApplyMessageBuffer(MessageVersion1 message, out Neighbor neighbor)
        {

            neighbor = null;
            if (!CheckNeighbor(message)) return null;
            neighbor = neighbors.Single(n => n.Identifier.Equals(message.PeerId));
            if (message.PacketIndex <= neighbor.IncomingPacketIndex) return null;
            if (message.PacketIndex > neighbor.IncomingPacketIndex + 1)
            {
                lock (neighbor)
                    neighbor.AddMessageToBuffer(message);
                return null;
            }
            lock (neighbor)
                if (neighbor.HasBufferedMessages)
                {
                    neighbor.AddMessageToBuffer(message);
                    neighbor.IncomingPacketIndex += (ulong)neighbor.BufferedMessages.Count;
                    return neighbor.RemoveMessagesFromBuffer();
                }
                else
                {
                    neighbor.IncomingPacketIndex++;
                    return new[] { message };
                }
        }

        private void ProcessMessage(AnnounceRegistrationMessage message)
        {
            if (CheckNeighbor(message)) return;
            foreach (var endPoint in message.EndPoints)
            {
                SendMessage(endPoint, new ReplyRegistrationMessage { EndPoints = receiver.EndPoints, Neighbors = neighbors });
                AddNeighbor(message.PeerId, endPoint);
            }
            OnApplicationJoined(message.EndPoints);
        }

        private bool CheckNeighbor(MessageVersion1 message)
        {
            return neighbors.Contains(new Neighbor { Identifier = message.PeerId });
        }

        protected void OnApplicationJoined(IEnumerable<IPEndPoint> endPoints)
        {
            var eventHandler = ApplicationJoined;
            if (eventHandler != null)
                eventHandler(this, new CloudRegistrationEventArgs { IpEndPoints = endPoints });
        }

        private void ProcessMessage(AnnounceLeavingMessage message)
        {
            if (!CheckNeighbor(message)) return;
            RemoveNeighbor(message.PeerId);
            OnApplicationLeaved(message.EndPoints);
        }

        protected void OnApplicationLeaved(IEnumerable<IPEndPoint> endPoints)
        {
            var eventHandler = ApplicationLeaved;
            if (eventHandler != null)
                eventHandler(this, new CloudRegistrationEventArgs { IpEndPoints = endPoints });
        }

        private void ProcessMessage(ReplyRegistrationMessage message)
        {
            if (CheckNeighbor(message)) return;
            foreach (var endPoint in message.EndPoints)
                AddNeighbor(message.PeerId, endPoint);
            OnApplicationDetected(message.EndPoints);
        }

        protected void OnApplicationDetected(IEnumerable<IPEndPoint> endPoints)
        {
            var eventHandler = ApplicationDetected;
            if (eventHandler != null)
                eventHandler(this, new CloudRegistrationEventArgs { IpEndPoints = endPoints });
        }

        private void ProcessMessage(UserMessage message)
        {
            Neighbor neighbor;
            var messages = ApplyMessageBuffer(message, out neighbor);
            if (messages != null)
                foreach (var msg in messages)
                {
                    var userMessage = (UserMessage)msg;
                    if (userMessage.UserPacketsCount > 1)
                    {
                        var queue = neighbor.AddMessageToQueue(message);
                        if (queue.QueueReady)
                        {
                            var queueMessages = neighbor.ExtractMessagesFromQueue(queue);
                            var buffer = new byte[queueMessages.Sum(u => u.UserData.Length)];
                            int offset = 0;
                            foreach (var queueMessage in queueMessages) 
                            {
                                queueMessage.UserData.CopyTo(buffer, offset);
                                offset += queueMessage.UserData.Length;
                            }
                            OnUserMessageReceived(buffer);
                        }
                    }
                    else
                        OnUserMessageReceived(((UserMessage)msg).UserData);
                }
        }

        protected void OnUserMessageReceived(byte[] binaryData)
        {
            var eventHandler = UserMessageReceived;
            if (eventHandler != null)
                eventHandler(this, new UserMessageEventArgs { UserData = binaryData });
        }

        public void SendBroadcastMessage(Message message)
        {
            var sender = new Sender(broadcastPort, peerId) {  RsaKeyStore = RsaKeyStore};
            sender.SendMessage(message, 0);
        }

        public void SendData(byte[] data)
        {
            if (data.Length <= MaxPacketSize)
                SendMessage(new UserMessage { UserData = data });
            else
            {
                int offset = 0;
                var packetsCount = (int)Math.Ceiling(data.Length / (double)MaxPacketSize);
                var packetIndex = 0;
                while (offset < data.Length)
                {
                    SendMessage(new UserMessage
                    {
                        UserData = data.Skip(offset).Take(MaxPacketSize).ToArray(),
                        UserPacketsCount = (ushort)packetsCount,
                        UserPacketIndex = (ushort)packetIndex++
                    });
                    offset += MaxPacketSize;
                }
            }
        }

        public void SendMessage(Message message)
        {
            lock (neighbors)
                foreach (var neighbor in neighbors)
                {
                    if (message is UserMessage)
                        Interlocked.Increment(ref outgoingPacketIndex);
                    foreach (var endPoint in neighbor.IpEndPoints)
                    {
                        var sender = new Sender(endPoint, peerId) { RsaKeyStore = RsaKeyStore };
                        sender.SendMessage(message, message is UserMessage ? (ulong)outgoingPacketIndex : 0);
                    }
                }
        }

        public void SendMessage(IPEndPoint endPoint, Message message)
        {
            var sender = new Sender(endPoint, peerId) { RsaKeyStore = RsaKeyStore };
            sender.SendMessage(message,
                message is UserMessage ?
                (ulong)Interlocked.Increment(ref outgoingPacketIndex) : 0);
        }

        public void RegisterInCloud()
        {
            var msg = new AnnounceRegistrationMessage { EndPoints = receiver.EndPoints };
            SendBroadcastMessage(msg);
            registeredInCloud = true;
        }

        public void UnregisterFromCloud()
        {
            var msg = new AnnounceLeavingMessage { EndPoints = receiver.EndPoints };
            SendMessage(msg);
            registeredInCloud = false;
        }

        void AddNeighbor(PeerIdentifier peerIdentifier, IPEndPoint endPoint)
        {
            lock (neighbors)
            {
                var neighbor = new Neighbor { Identifier = peerIdentifier };
                if (!neighbors.Contains(neighbor))
                {
                    neighbor.IpEndPoints = new[] { endPoint };
                    neighbors.Add(neighbor);
                }
                else
                {
                    var ne = neighbors.Find(n => n.Equals(neighbor));
                    ne.IpEndPoints = ne.IpEndPoints.Concat(new[] { endPoint });
                }
            }
        }

        void RemoveNeighbor(PeerIdentifier peerIdentifier)
        {
            lock (neighbors)
            {
                var neighbor = new Neighbor { Identifier = peerIdentifier };
                if (neighbors.Contains(neighbor))
                    neighbors.Remove(neighbor);
            }
        }


    }
}
