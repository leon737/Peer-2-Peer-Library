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
    public class AnnounceLeavingMessage : AnnounceMessage
    {
        public new static AnnounceLeavingMessage Create(Stream stream)
        {
            return new AnnounceLeavingMessage().Deserialize(stream) as AnnounceLeavingMessage;
        }

        protected override MessageType GetMessageType
        {
            get { return MessageType.AnnounceLeaving; }
        }
    }
}
