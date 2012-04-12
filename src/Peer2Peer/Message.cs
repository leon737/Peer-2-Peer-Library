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

namespace Peer2Peer
{
	abstract public class Message
	{
		public virtual byte[] Serialize()
		{
			using (var ms = new MemoryStream())
			{
				Serialize(ms);
				var buffer = new byte[ms.Length];
				ms.Position = 0;
				ms.Read(buffer, 0, buffer.Length);
				return buffer;
			}

		}

		protected abstract void Serialize(Stream stream);
	}

	public enum MessageType
	{
		AnnounceRegistration = 0x0,
		AnnounceLeaving = 0x1,
		ReplyRegistration = 0x2,
		UserMessage = 0x80
	}
}
