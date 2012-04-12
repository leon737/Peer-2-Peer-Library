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

using System.Net.Sockets;
using System.Net;
using System;
using System.Security.Cryptography;

namespace Peer2Peer
{
	public struct MessageSentEventArgs
	{
		public int BytesSent;
	}

	public delegate void MessageSentEventHandler(MessageSentEventArgs eventArgs);


	class Sender
	{
		private readonly UdpClient udp;
		private readonly IPEndPoint ipEndPoint;
		public event MessageSentEventHandler MessageSent;
		private readonly PeerIdentifier peerId;
		public RsaKeyStore RsaKeyStore { get; set; }

		public Sender(int receiverPort, PeerIdentifier peerId)
			: this(new IPEndPoint(IPAddress.Broadcast, receiverPort), peerId) { }

		public Sender(IPEndPoint ipEndPoint, PeerIdentifier peerId)
		{
			udp = new UdpClient(0) { EnableBroadcast = true };
			this.ipEndPoint = ipEndPoint;
			this.peerId = peerId;
		}

		public void SendMessage(Message message, ulong packetIndex)
		{
			udp.Connect(ipEndPoint);
			var msg = (MessageVersion1)message;
			msg.Mac = new Mac(((IPEndPoint)(udp.Client.LocalEndPoint)).Address).MacAddress;
			msg.PeerId = peerId;
			msg.PacketIndex = packetIndex;
			var messageData = message.Serialize();
			if (RsaKeyStore != null)
			{
				msg.Signature = SignPackage(messageData);
				messageData = message.Serialize();
			}
			udp.BeginSend(messageData, messageData.Length, RequestCallback, null);
		}

		private byte[] SignPackage(byte[] messageData)
		{
			var rsa = new RSACryptoServiceProvider
				(RsaKeyStore.CspParameters ?? new CspParameters { Flags = CspProviderFlags.NoFlags });
			if (RsaKeyStore.CspParameters == null)
				rsa.ImportParameters(RsaKeyStore.RsaParameters);
			var signature = rsa.SignData(messageData, new SHA1CryptoServiceProvider());
			return signature;
		}

		private void RequestCallback(IAsyncResult ar)
		{
			var bytesSent = udp.EndSend(ar);
			OnMessageSent(bytesSent);
			udp.Close();
		}

		protected void OnMessageSent(int bytesSent)
		{
			var eventHandler = MessageSent;
			if (eventHandler != null)
				eventHandler(new MessageSentEventArgs { BytesSent = bytesSent });
		}
	}
}
