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
using System.Security.Cryptography;

namespace Peer2Peer
{
    public class RsaKeyStore
    {
        const int RsaKeyStoreRawLength = 579;
        public RSAParameters RsaParameters { get; private set; }
        public byte[] RawData { get; private set; }
        public CspParameters CspParameters { get; private set; }

        public RsaKeyStore(byte[] rawData)
        {
            if (rawData == null || rawData.Length != RsaKeyStoreRawLength)
                throw new ArgumentException();
            RawData = rawData;
            int index = 0;
            RsaParameters = new RSAParameters();
            Array.Copy(RawData, index, RsaParameters.D, 0, 128); index += 128;
            Array.Copy(RawData, index, RsaParameters.DP, 0, 64); index += 64;
            Array.Copy(RawData, index, RsaParameters.DQ, 0, 64); index += 64;
            Array.Copy(RawData, index, RsaParameters.Exponent, 0, 3); index += 3;
            Array.Copy(RawData, index, RsaParameters.InverseQ, 0, 64); index += 64;
            Array.Copy(RawData, index, RsaParameters.Modulus, 0, 128); index += 128;
            Array.Copy(RawData, index, RsaParameters.P, 0, 64); index += 64;
            Array.Copy(RawData, index, RsaParameters.Q, 0, 64);
        }

        public RsaKeyStore(RSAParameters rsaParameters)
        {
            RsaParameters = rsaParameters;
            RawData = new byte[RsaKeyStoreRawLength];
            int index = 0;
            Array.Copy(RsaParameters.D, 0, RawData, index, 128); index += 128;
            Array.Copy(RsaParameters.DP, 0, RawData, index, 64); index += 64;
            Array.Copy(RsaParameters.DQ, 0, RawData, index, 64); index += 64;
            Array.Copy(RsaParameters.Exponent, 0, RawData, index, 3); index += 3;
            Array.Copy(RsaParameters.InverseQ, 0, RawData, index, 64); index += 64;
            Array.Copy(RsaParameters.Modulus, 0, RawData, index, 128); index += 128;
            Array.Copy(RsaParameters.P, 0, RawData, index, 64); index += 64;
            Array.Copy(RsaParameters.Q, 0, RawData, index, 64); 
        }

        public RsaKeyStore(CspParameters cspParameters)
        {
            CspParameters = cspParameters;
        }
       
    }
}
