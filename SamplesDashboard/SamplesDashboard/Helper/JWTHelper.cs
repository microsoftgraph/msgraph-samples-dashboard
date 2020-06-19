// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Jose;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace SamplesDashboard.Helper
{
    public class JWTHelper  {

        /// <summary>
        /// Gets private key and uses it to decode the jwt token.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="payload"></param>
        /// <returns>jwt token</returns>
        public static string CreateEncodedJwtToken(string key, Dictionary<string, object> payload)
        {
            // Generate JWT
            using (var rsa = new RSACryptoServiceProvider())
            {
                var rsaParams = ToRSAParameters(GetPrivateKey(key));
                rsa.ImportParameters(rsaParams);
                return JWT.Encode(payload, rsa, JwsAlgorithm.RS256);
            }
        }
        private static RsaPrivateCrtKeyParameters GetPrivateKey(string privateKey)
        {
            using (var privateKeyReader = new StringReader(privateKey))
            {
                var pemReader = new PemReader(privateKeyReader);
                var keyPair = (AsymmetricCipherKeyPair)pemReader.ReadObject();
                return (RsaPrivateCrtKeyParameters)keyPair.Private;
            }
        }

        private static RSAParameters ToRSAParameters(RsaPrivateCrtKeyParameters privKey)
        {
            var rp = new RSAParameters
            {
                Modulus = privKey.Modulus.ToByteArrayUnsigned(),
                Exponent = privKey.PublicExponent.ToByteArrayUnsigned(),
                P = privKey.P.ToByteArrayUnsigned(),
                Q = privKey.Q.ToByteArrayUnsigned()
            };
            rp.D = ConvertRSAParametersField(privKey.Exponent, rp.Modulus.Length);
            rp.DP = ConvertRSAParametersField(privKey.DP, rp.P.Length);
            rp.DQ = ConvertRSAParametersField(privKey.DQ, rp.Q.Length);
            rp.InverseQ = ConvertRSAParametersField(privKey.QInv, rp.Q.Length);
            return rp;
        }

        private static byte[] ConvertRSAParametersField(BigInteger n, int size)
        {
            byte[] bs = n.ToByteArrayUnsigned();
            if (bs.Length == size)
                return bs;
            if (bs.Length > size)
                throw new ArgumentException("Specified size too small", nameof(size));

            byte[] padded = new byte[size];
            Array.Copy(bs, 0, padded, size - bs.Length, bs.Length);
            return padded;
        }        
    }
}
