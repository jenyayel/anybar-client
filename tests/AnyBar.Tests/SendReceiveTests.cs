using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AnyBar.Tests
{
    public class SendReceiveTests
    {
        [Theory]
        [InlineData(AnyBarImage.Red)]
        public void SendReceiveTest(AnyBarImage color)
        {
            const int ackTimeout = 1000;
            const int testTimeout = 30000;
            var datagram = Encoding.UTF8.GetBytes(color.ToString().ToLowerInvariant());
            var datagramSize = datagram.Length;
            var address = IPAddress.Loopback;

            var server = new Socket(address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            var port = server.BindToAnonymousPort(address);
            var client = new AnyBarClient(address, port);

            var receiverAck = new ManualResetEventSlim();
            var senderAck = new ManualResetEventSlim();
            
            var serverThread = new Thread(() =>
            {
                using (server)
                {
                    var endpoint = (IPEndPoint)server.LocalEndPoint;
                    var remote = endpoint.Create(endpoint.Serialize());
                    var recvBuffer = new byte[datagramSize];

                    int received = server.ReceiveFrom(recvBuffer, SocketFlags.None, ref remote);
                    Assert.Equal(datagramSize, received);

                    var receivedChecksums = Fletcher32.Checksum(recvBuffer, 0, received);

                    receiverAck.Set();
                    Assert.True(senderAck.Wait(ackTimeout));
                    senderAck.Reset();
                }
            });

            serverThread.Start();
            
            using (client)
            {
                client.Change(color);

                Assert.True(receiverAck.Wait(ackTimeout));
                receiverAck.Reset();
                senderAck.Set();
            }

            Assert.True(serverThread.Join(testTimeout));
        }
    }
}
