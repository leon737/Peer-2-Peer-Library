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

using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Peer2Peer
{
	public class ReplyRegistrationMessage : AnnounceMessage
	{
		public IEnumerable<Neighbor> Neighbors { get; set; }
		
		public new static ReplyRegistrationMessage Create(Stream stream)
		{
			return new ReplyRegistrationMessage().Deserialize(stream) as ReplyRegistrationMessage;
		}

		protected override AnnounceMessage Deserialize(Stream stream)
		{
			base.Deserialize(stream);
			var buffer = new byte[2];
			stream.Read(buffer, 0, 2);
			int neighborsCount = BitConverter.ToInt16(buffer, 0);
			var neighbors = new List<Neighbor>(neighborsCount);
				for (int i = 0; i < neighborsCount; i++)
					 neighbors.Add(ReadNeighbor(stream));
				Neighbors = neighbors;
				return this;
		}

		  protected override void Serialize(Stream stream)
		  {
				base.Serialize(stream);
				stream.Write(BitConverter.GetBytes((short)Neighbors.Count()), 0, 2);
				foreach (var neighbor in Neighbors)
				{
					 stream.Write(neighbor.Identifier.Identifier.ToArray(), 0, 16);
					 WriteEndPoints(stream, neighbor.IpEndPoints);
				}
		  }

		  private Neighbor ReadNeighbor(Stream stream)
		  {
				var buffer = new byte[16];
				stream.Read(buffer, 0, 16);
				return new Neighbor
				{
					 Identifier = new PeerIdentifier(buffer),
					 IpEndPoints = ReadEndPoints(stream)
				};
		  }

		protected override MessageType GetMessageType
		{
			get { return MessageType.ReplyRegistration; }
		}
	}
}
