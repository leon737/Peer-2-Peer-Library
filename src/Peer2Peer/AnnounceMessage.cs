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
using System.Net;
using System.Linq;
using System.Collections.Generic;

namespace Peer2Peer
{
	public abstract class AnnounceMessage : MessageVersion1
	{
		public IEnumerable<IPEndPoint> EndPoints { get; set; }

		protected abstract MessageType GetMessageType { get; }

		protected virtual AnnounceMessage Deserialize(Stream stream)
		{			
			EndPoints = ReadEndPoints(stream);
			return this;
		}

		protected IEnumerable<IPEndPoint> ReadEndPoints(Stream stream)
		{
			var portBuffer = new byte[2];
			stream.Read(portBuffer, 0, 2);
			int endPointsCount = BitConverter.ToInt16(portBuffer, 0);
			var endPoints = new List<IPEndPoint>(endPointsCount);
			for (int i = 0; i < endPointsCount; i++)
			{
				var addressBuffer = new byte[4];
				stream.Read(addressBuffer, 0, 4);
				var address = new IPAddress(addressBuffer);
				stream.Read(portBuffer, 0, 2);
				int port = (ushort)BitConverter.ToInt16(portBuffer, 0);
				endPoints.Add(new IPEndPoint(address, port));
			}
			return endPoints;
		}

		protected override void Serialize(Stream stream)
		{
			base.Serialize(stream);
			stream.WriteByte((byte)GetMessageType);			
				WriteEndPoints(stream, EndPoints);
		}

		  protected void WriteEndPoints(Stream stream, IEnumerable<IPEndPoint> endPoints)
		  {
				stream.Write(BitConverter.GetBytes((short)endPoints.Count()), 0, 2);
				foreach (var endPoint in endPoints)
				{
					 stream.Write(endPoint.Address.GetAddressBytes(), 0, 4);
					 stream.Write(BitConverter.GetBytes(endPoint.Port), 0, 2);
				}
		  }
	}
}
