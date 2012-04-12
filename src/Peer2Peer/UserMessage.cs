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

namespace Peer2Peer
{
    public class UserMessage : MessageVersion1
    {
        public byte[] UserData { get; set; }
        public ushort UserPacketsCount { get; set; }
        public ushort UserPacketIndex { get; set; }

        public new static UserMessage Create(Stream stream)
        {
            var buffer = new byte[4];
            stream.Read(buffer, 0, 2);
            var userPacketsCount = (ushort)BitConverter.ToInt16(buffer, 0);
            ushort userPacketsIndex = 1;
            if (userPacketsCount > 1)
            {
                stream.Read(buffer, 0, 2);
                userPacketsIndex = (ushort)BitConverter.ToInt16(buffer, 0);
            }
            stream.Read(buffer, 0, 4);
            var dataBuffer = new byte[BitConverter.ToInt32(buffer, 0)];            
            stream.Read(dataBuffer, 0, dataBuffer.Length);
            return new UserMessage { UserData = dataBuffer, UserPacketsCount = userPacketsCount, UserPacketIndex = userPacketsIndex };
        }

        protected override void Serialize(Stream stream)
        {
            base.Serialize(stream);
            stream.WriteByte((byte)MessageType.UserMessage);
            stream.Write(BitConverter.GetBytes(UserPacketsCount), 0, 2);
            if (UserPacketsCount > 1)
                stream.Write(BitConverter.GetBytes(UserPacketIndex), 0, 2);
            stream.Write(BitConverter.GetBytes(UserData.Length), 0, 4);
            stream.Write(UserData, 0, UserData.Length);
        }
    }
}
