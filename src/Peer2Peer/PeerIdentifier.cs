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

namespace Peer2Peer
{
    public class PeerIdentifier
    {
        public IEnumerable<byte> Identifier { get; private set; }

        public PeerIdentifier(IEnumerable<byte> identifier)
        {
            Identifier = identifier;
        }

        public static PeerIdentifier Create()
        {
            return new PeerIdentifier(Guid.NewGuid().ToByteArray());
        }

        public override bool Equals(object obj)
        {
            var other = obj as PeerIdentifier;
            if (other == null) return false;
            return Identifier.SequenceEqual(other.Identifier);
        }

        public override int GetHashCode()
        {
            return Identifier.Aggregate((a, b) => (byte)(a ^ b));
        }
    }
}
