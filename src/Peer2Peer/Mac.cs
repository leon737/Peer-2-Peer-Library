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
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Peer2Peer
{
	class Mac : IEqualityComparer<IPAddress>
	{
		public IPAddress IpAddress { get; private set; }
		public long MacAddress { get; private set; }

		public Mac(IPAddress ipAddress)
		{
			IpAddress = ipAddress;
			MacAddress = GetLocalMac();
		}

		public bool AreIpsEqual (IPAddress otherIpAddress)
		{
			return AreIpsEqual(IpAddress, otherIpAddress);
		}

		public static bool AreIpsEqual(IPAddress firstIpAddress, IPAddress otherIpAddress)
		{
			var address1 = firstIpAddress.GetAddressBytes();
			var address2 = otherIpAddress.GetAddressBytes();
			return address1.SequenceEqual(address2);
		}

		long GetLocalMac()
		{
			return (from iface in NetworkInterface.GetAllNetworkInterfaces()
					  where iface.GetIPProperties().UnicastAddresses.Where(u => AreIpsEqual(u.Address)).Any()
					  select ConvertToMac(iface.GetPhysicalAddress().GetAddressBytes())).FirstOrDefault();
		}

		public static long ConvertToMac(byte[] addressBytes)
		{
			var buffer = new byte[8];
			addressBytes.CopyTo(buffer, 0);
			return BitConverter.ToInt64(buffer, 0);
		}

		public bool Equals(IPAddress x, IPAddress y)
		{
			return AreIpsEqual(x, y);
		}

		public int GetHashCode(IPAddress ipAddress)
		{
			return ipAddress.GetAddressBytes().Aggregate((a, b) => (byte)(a ^ b));
		}
	}

	static class IpAddressExtension
	{
		public static long ToInt32 (this IPAddress address)
		{
			if (address.AddressFamily != AddressFamily.InterNetwork)
				throw new ArgumentOutOfRangeException();
			var bytes = address.GetAddressBytes();
			return BitConverter.ToInt32(bytes, 0);
		}

	}
}
