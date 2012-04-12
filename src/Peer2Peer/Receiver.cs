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
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;

namespace Peer2Peer
{

	public struct MessageReceivedEventArgs
	{
		public Message MessageReceived;
	}

	public delegate void MessageReceivedEventHandler(MessageReceivedEventArgs eventArgs);

	public class Receiver : IDisposable
	{
		HashSet<long> macs;
		IEnumerable<IPAddress> localIfaces;
		public IEnumerable<IPEndPoint> EndPoints { get; private set; }
		private readonly bool loopbacksAllowed;
		private readonly PeerIdentifier peerId;
		public RsaKeyStore RsaKeyStore { get; set; }

		private UdpClient udp;
		public event MessageReceivedEventHandler MessageReceived;

		public Receiver(int port, PeerIdentifier peerId, bool loopbacksAllowed)
			: this(new IPEndPoint(0, port), peerId, loopbacksAllowed) { }

		public Receiver(IPEndPoint binding, PeerIdentifier peerId, bool loopbacksAllowed)
		{
			InitLocalIfaces();
			this.loopbacksAllowed = loopbacksAllowed;
			this.peerId = peerId;
			udp = BindClient(binding);
			EndPoints = binding.Address.ToInt32() == 0
								 ? (IEnumerable<IPEndPoint>)localIfaces.Select(i => new IPEndPoint(i, LocalPort)).ToList()
								 : new[] { udp.Client.LocalEndPoint as IPEndPoint };
			udp.BeginReceive(RequestCallback, null);
		}

		public int LocalPort { get { return ((IPEndPoint)udp.Client.LocalEndPoint).Port; } }

		private UdpClient BindClient(IPEndPoint binding)
		{
			try
			{
				udp = new UdpClient(binding);
			}
			catch (SocketException)
			{
				udp = new UdpClient(new IPEndPoint(binding.Address, 0));
			}
			udp.EnableBroadcast = true;
			return udp;

		}

		private void InitLocalIfaces()
		{
			var liFaces = new List<IPAddress>();
			macs = new HashSet<long>();
			foreach (var iface in NetworkInterface.GetAllNetworkInterfaces())
				foreach (var unicast in
					 iface.GetIPProperties().UnicastAddresses.Where(u => u.Address.AddressFamily == AddressFamily.InterNetwork))
				{
					liFaces.Add(unicast.Address);
					long mac = Mac.ConvertToMac(iface.GetPhysicalAddress().GetAddressBytes());
					if (!macs.Contains(mac)) macs.Add(mac);
				}
			localIfaces = liFaces;
		}

		private void RequestCallback(IAsyncResult ar)
		{
			try
			{
				if (udp == null) return;
				udp.BeginReceive(RequestCallback, null);
				var endPoint = new IPEndPoint(0, 0);
				var buffer = udp.EndReceive(ar, ref endPoint);
				var msg = new MessageFactory().GetMessage(buffer);
				if (!(msg is MessageVersion1))
					throw new NotSupportedException("Message version is not supported");
				var msgv1 = msg as MessageVersion1;
				if ((loopbacksAllowed || !macs.Contains(msgv1.Mac)) && (CheckPeer(msgv1.PeerId) && VerifySignature(msgv1)))
					OnMessageReceived(msg);
			}
			catch (ObjectDisposedException) { }
		}

		private bool VerifySignature(MessageVersion1 msgv1)
		{
			if (RsaKeyStore == null) return true;
			var rsa = new RSACryptoServiceProvider
				(RsaKeyStore.CspParameters ?? new CspParameters { Flags = CspProviderFlags.NoFlags });
			if (RsaKeyStore.CspParameters == null)
				rsa.ImportParameters(RsaKeyStore.RsaParameters);
			var signature = new byte[128];
			msgv1.Signature.CopyTo(signature, 0);
			Array.Clear(msgv1.Signature, 0, 128);
			var buffer = msgv1.Serialize();
			return rsa.VerifyData(buffer, new SHA1CryptoServiceProvider(), signature);
		}

		private bool CheckPeer(PeerIdentifier otherPeerId)
		{
			return !peerId.Equals(otherPeerId);
		}

		protected void OnMessageReceived(Message messageReceived)
		{
			var eventHandler = MessageReceived;
			if (eventHandler != null)
				eventHandler(new MessageReceivedEventArgs { MessageReceived = messageReceived });
		}

		public void Dispose()
		{
			if (udp == null) return;
			udp.Close();
			udp = null;
		}
	}
}

