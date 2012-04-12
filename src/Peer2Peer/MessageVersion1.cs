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
using System.IO;
using System.Linq;

namespace Peer2Peer
{
	public abstract class MessageVersion1 : Message
	{

		public long Mac { get; set; }
		public PeerIdentifier PeerId { get; set; }
		public ulong PacketIndex;
		public byte[] Signature { get; set; }

		protected MessageVersion1()
		{
			Signature = new byte[128];
		}

		public static MessageVersion1 Create(Stream stream)
		{
			var buffer = new byte[8];
			stream.Read(buffer, 0, 8);
			long mac = BitConverter.ToInt64(buffer, 0);
			var peerId = new byte[16];
			stream.Read(peerId, 0, 16);
			stream.Read(buffer, 0, 8);
			var packetIndex = (ulong)BitConverter.ToInt64(buffer, 0);
			var sign = new byte[128];
			stream.Read(sign, 0, 128);
			int messageType = stream.ReadByte();
			MessageVersion1 msg;
			switch ((MessageType)messageType)
			{
				case MessageType.AnnounceRegistration:
					msg = AnnounceRegistrationMessage.Create(stream);
					break;
				case MessageType.AnnounceLeaving:
					msg = AnnounceLeavingMessage.Create(stream);
					break;
				case MessageType.ReplyRegistration:
					msg = ReplyRegistrationMessage.Create(stream);
					break;
				case MessageType.UserMessage:
					msg = UserMessage.Create(stream);
					break;
				default:
					throw new Exception("Unsupported message type");
			}
			msg.Mac = mac;
			msg.PeerId = new PeerIdentifier(peerId);
			msg.PacketIndex = packetIndex;
			msg.Signature = sign;
			return msg;
		}

		protected override void Serialize(Stream stream)
		{
			stream.WriteByte(0x1); //version 1
			stream.Write(BitConverter.GetBytes(Mac), 0, 8);
			stream.Write(PeerId.Identifier.ToArray(), 0, 16);
			stream.Write(BitConverter.GetBytes(PacketIndex), 0, 8);
			stream.Write(Signature, 0, 128);
		}
	}
}
