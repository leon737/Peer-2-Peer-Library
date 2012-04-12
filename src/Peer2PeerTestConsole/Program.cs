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
using System.Text;
using Peer2Peer;
using System.Net;
using System.Threading;
using System.Security.Cryptography;

namespace Peer2PeerTestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var cspParam = new CspParameters { Flags = CspProviderFlags.UseMachineKeyStore | CspProviderFlags.UseExistingKey,
            KeyContainerName = "FreeStreamingKeyStore"};
            var keyStore = new RsaKeyStore(cspParam);

				Peer peer1 = new Peer(38412, true, keyStore);
            peer1.ApplicationJoined += new CloudRegistrationEventHandler(OnApplicationJoined);
            peer1.ApplicationDetected += new CloudRegistrationEventHandler(OnApplicationDetected);
            peer1.ApplicationLeaved += new CloudRegistrationEventHandler(OnApplicationLeaved);
            peer1.UserMessageReceived += new UserMessageReceivedEventHandler(OnUserMessageReceived);
            peer1.RegisterInCloud();

				//Peer peer2 = new Peer(10001, true, keyStore);
				//peer2.ApplicationJoined += new CloudRegistrationEventHandler(OnApplication2Joined);
				//peer2.ApplicationDetected += new CloudRegistrationEventHandler(OnApplication2Detected);
				//peer2.ApplicationLeaved += new CloudRegistrationEventHandler(OnApplication2Leaved);
				//peer2.UserMessageReceived += new UserMessageReceivedEventHandler(OnUserMessage2Received);
				//peer2.RegisterInCloud();

            Thread.Sleep(1000);

            //peer1.SendData(new byte[100]);
            //peer1.SendData(new byte[200]);
            //peer1.SendData(new byte[300]);
            //peer1.SendData(new byte[100000]);

            Thread.Sleep(1000);
            peer1.Dispose();
            //peer2.Dispose();

            Console.WriteLine("..");
            Console.ReadLine();
        }

        static void OnUserMessageReceived(Peer sender, UserMessageEventArgs eventArgs)
        {
            Console.WriteLine("User message received from app2");
        }

        static void OnApplicationLeaved(Peer sender, CloudRegistrationEventArgs eventArgs)
        {
            Console.WriteLine("Application leaved [" + ShowEndPoints(eventArgs.IpEndPoints) + "]");
        }

        static void OnApplicationDetected(Peer sender, CloudRegistrationEventArgs eventArgs)
        {
            Console.WriteLine("Application detected [" + ShowEndPoints(eventArgs.IpEndPoints) + "]");
        }

        static void OnApplicationJoined(Peer sender, CloudRegistrationEventArgs eventArgs)
        {
            Console.WriteLine("Application joined [" + ShowEndPoints(eventArgs.IpEndPoints) + "]");
        }


        static void OnUserMessage2Received(Peer sender, UserMessageEventArgs eventArgs)
        {
            Console.WriteLine("User message received from app. " + eventArgs.UserData.Length + " bytes received.");
        }

        static void OnApplication2Leaved(Peer sender, CloudRegistrationEventArgs eventArgs)
        {
            Console.WriteLine("Application2 leaved [" + ShowEndPoints(eventArgs.IpEndPoints) + "]");
        }

        static void OnApplication2Detected(Peer sender, CloudRegistrationEventArgs eventArgs)
        {
            Console.WriteLine("Application2 detected [" + ShowEndPoints(eventArgs.IpEndPoints) + "]");
        }

        static void OnApplication2Joined(Peer sender, CloudRegistrationEventArgs eventArgs)
        {
            Console.WriteLine("Application2 joined [" + ShowEndPoints(eventArgs.IpEndPoints) + "]");
        }

        private static string ShowEndPoints(IEnumerable<IPEndPoint> endPoints)
        {
            return string.Join(",",
                endPoints.Select(e => string.Format("{{{0}:{1}}}", e.Address.ToString(), e.Port.ToString()))
                );
        }

    }
}
