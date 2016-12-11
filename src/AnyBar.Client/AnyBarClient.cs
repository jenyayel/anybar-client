using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AnyBar
{
    /// <summary>
    /// Client for changing icon type of AnyBar
    /// </summary>
    public class AnyBarClient : IDisposable
    {
        public const int DEFAULT_PORT = 1738;

        private Socket _socket;
        private EndPoint _endPoint;

        /// <summary>
        /// Initialize a new instance of AnyBar client
        /// </summary>
        /// <param name="host">DNS name of the host where AnyBar is installed</param>
        /// <param name="port">The port number on which AnyBar listens</param>
        public AnyBarClient(string host = "localhost", int port = DEFAULT_PORT)
        {
            if (String.IsNullOrEmpty(host)) throw new ArgumentNullException(nameof(host));
            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort) throw new ArgumentOutOfRangeException(nameof(port));

            if(host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                _endPoint = new IPEndPoint(IPAddress.Loopback, port);
            else
            {
#if NET452
                var ips = System.Net.Dns.GetHostAddresses(host);
                if (ips.Length == 0)
                    throw new ArgumentException("Unable to retrieve address from specified host name.", "hostName");
                _endPoint = new IPEndPoint(ips[0], port);
#else
                throw new NotSupportedException();
#endif
            }
            _socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
        }

        /// <summary>
        /// Initialize a new instance of AnyBar client
        /// </summary>
        /// <param name="ipAddress">IP address of the host where AnyBar is installed</param>
        /// <param name="port">The port number on which AnyBar listens</param>
        public AnyBarClient(IPAddress ipAddress, int port = DEFAULT_PORT)
        {
            if (ipAddress == null) throw new ArgumentNullException(nameof(ipAddress));
            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort) throw new ArgumentOutOfRangeException(nameof(port));

            _endPoint = new IPEndPoint(ipAddress, port);
            _socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
        }
#if NET452
        /// <summary>
        /// Asynchronously changes the icon of AnyBar
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public Task ChangeAsync(AnyBarImage image)
        {

            var tcs = new TaskCompletionSource<int>(_socket);
            var buffer = image.ToByteArray();

            _socket.BeginSendTo(buffer, 0, buffer.Length, SocketFlags.Broadcast, _endPoint, iar =>
            {
                var innerTcs = (TaskCompletionSource<int>)iar.AsyncState;
                try { innerTcs.TrySetResult(((Socket)innerTcs.Task.AsyncState).EndSendTo(iar)); }
                catch (Exception e) { innerTcs.TrySetException(e); }
            }, tcs);
            return tcs.Task;
        }
#else
        /// <summary>
        /// Asynchronously changes the icon of AnyBar
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public async Task ChangeAsync(AnyBarImage image)
        {
            await _socket.SendToAsync(
                new ArraySegment<byte>(image.ToByteArray()),
                SocketFlags.None,
                _endPoint).ConfigureAwait(false);
        }
#endif

        /// <summary>
        /// Synchronously changes the icon of  AnyBar
        /// </summary>
        /// <param name="image"></param>
        public void Change(AnyBarImage image)
        {
            _socket.SendTo(
                image.ToByteArray(), 
                SocketFlags.None, 
                _endPoint);
        }

        /// <summary>
        /// Release resources allocated by <see cref="Socket"/>
        /// </summary>
        public void Dispose()
        {
            _socket.Dispose();
        }
    }
}
