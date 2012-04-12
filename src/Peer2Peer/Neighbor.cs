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

using System.Linq;
using System.Net;
using System.Collections.Generic;

namespace Peer2Peer
{
    public class Neighbor
    {
        public PeerIdentifier Identifier { get; set; }
        public IEnumerable<IPEndPoint> IpEndPoints { get; set; }
        public ulong IncomingPacketIndex { get; set; }
        public List<MessageVersion1> BufferedMessages { get; set; }
        public UserMessagesQueuesSet Queues { get; set; }


        public override bool Equals(object obj)
        {
            var neighbor = obj as Neighbor;
            if (neighbor == null) return false;
            return Identifier.Equals(neighbor.Identifier);
        }

        public override int GetHashCode()
        {
            return Identifier.Identifier.GetHashCode();
        }

        public void AddMessageToBuffer(MessageVersion1 message)
        {
            if (BufferedMessages == null)
                BufferedMessages = new List<MessageVersion1>();
            BufferedMessages.Add(message);
        }

        public IEnumerable<MessageVersion1> RemoveMessagesFromBuffer()
        {
            var messages = BufferedMessages.OrderBy(m => m.PacketIndex);
            BufferedMessages.Clear();
            return messages;
        }

        public bool HasBufferedMessages
        {
            get { return BufferedMessages != null && BufferedMessages.Count > 0; }
        }

        public UserMessagesQueue AddMessageToQueue(UserMessage message)
        {            
            ulong desiredStartPacketIndex = message.PacketIndex - message.UserPacketIndex;
            UserMessagesQueue queue;
            lock (this)
            {
                if (Queues == null)
                    Queues = new UserMessagesQueuesSet();
                queue = Queues.GetQueue(desiredStartPacketIndex, message.UserPacketsCount);
                queue.Messages[message.UserPacketIndex] = message;
            }
            return queue;
        }

        public IEnumerable<UserMessage> ExtractMessagesFromQueue(UserMessagesQueue queue)
        {
            IEnumerable<UserMessage> messages;
            lock (this)
            {
                messages = queue.Messages.ToList();
                Queues.Queues.Remove(queue);
                queue.Messages = null;
            }
            return messages;
        }




    }
}
