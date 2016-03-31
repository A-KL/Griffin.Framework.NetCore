using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Griffin.Net.Buffers;
using Griffin.Net.Channels;
using Griffin.Net.Protocols;
//using Griffin.Net.Protocols.MicroMsg;
using Griffin.Net.Protocols.Serializers;

namespace Griffin.Net
{
    public class ErrorEventArgs : EventArgs
    {
        private Exception exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.IO.ErrorEventArgs"/> class.
        /// </summary>
        /// <param name="exception">An <see cref="T:System.Exception"/> that represents the error that occurred. </param>
        public ErrorEventArgs(Exception exception)
        {
            this.exception = exception;
        }

        /// <summary>
        /// Gets the <see cref="T:System.Exception"/> that represents the error that occurred.
        /// </summary>
        /// 
        /// <returns>
        /// An <see cref="T:System.Exception"/> that represents the error that occurred.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public virtual Exception GetException()
        {
            return this.exception;
        }
    }


    /// <summary>
    ///     Listens on one of the specified protocols
    /// </summary>
    public class ChannelTcpListener : IMessagingListener
    {
        private readonly ConcurrentStack<ITcpChannel> _channels = new ConcurrentStack<ITcpChannel>();
        private IBufferSlicePool _bufferPool;
        private ITcpChannelFactory _channelFactory;
        private ChannelTcpListenerConfiguration _configuration;
        private Socket listener;
        private SocketAsyncEventArgs listenerArgs;
        private MessageHandler _messageReceived;
        private MessageHandler _messageSent;
        private volatile bool _shuttingDown;

        /// <summary>
        /// </summary>
        /// <param name="configuration"></param>
        public ChannelTcpListener(ChannelTcpListenerConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            this.Configure(configuration);
            this.ChannelFactory = new TcpChannelFactory();


            this.listenerArgs = new SocketAsyncEventArgs();
            this.listenerArgs.Completed += this.OnAcceptSocket;
        }

        /// <summary>
        /// </summary>
        public ChannelTcpListener()
        {
            //this.Configure(
            //    new ChannelTcpListenerConfiguration(
            //        () => new MicroMessageDecoder(new DataContractMessageSerializer()),
            //        () => new MicroMessageEncoder(new DataContractMessageSerializer()))
            //    );

            this.ChannelFactory = new TcpChannelFactory();


            this.listenerArgs = new SocketAsyncEventArgs();
            this.listenerArgs.Completed += this.OnAcceptSocket;
        }

        /// <summary>
        ///     Port that the server is listening on.
        /// </summary>
        /// <remarks>
        ///     You can use port <c>0</c> in <see cref="Start" /> to let the OS assign a port. This method will then give you the
        ///     assigned port.
        /// </remarks>
        public int LocalPort
        {
            get
            {
                if (this.listener == null)
                    return -1;

                return ((IPEndPoint) this.listener.LocalEndPoint).Port;
            }
        }

        /// <summary>
        ///     Used to create channels. Default is <see cref="TcpChannelFactory" />.
        /// </summary>
        public ITcpChannelFactory ChannelFactory
        {
            get { return this._channelFactory; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                this._channelFactory = value;
            }
        }

        /// <summary>
        ///     Delegate invoked when a new message is received
        /// </summary>
        public MessageHandler MessageReceived
        {
            get { return this._messageReceived; }
            set { this._messageReceived = value ?? delegate { }; }
        }

        /// <summary>
        ///     Delegate invoked when a message has been sent to the remote end point
        /// </summary>
        public MessageHandler MessageSent
        {
            get { return this._messageSent; }
            set { this._messageSent = value ?? delegate { }; }
        }

        /// <summary>
        ///     A client has connected (nothing has been sent or received yet)
        /// </summary>
        public event EventHandler<ClientConnectedEventArgs> ClientConnected = delegate { };

        /// <summary>
        ///     A client has disconnected
        /// </summary>
        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected = delegate { };

        /// <summary>
        ///     Start this listener.
        /// </summary>
        /// <remarks>
        /// This also pre-configures 20 channels that can be used and reused during the lifetime of 
        /// this listener.
        /// </remarks>
        /// <param name="address">Address to accept connections on</param>
        /// <param name="port">Port to use. Set to <c>0</c> to let the OS decide which port to use. </param>
        /// <seealso cref="LocalPort" />
        public virtual void Start(IPAddress address, int port)
        {
            if (port < 0)
                throw new ArgumentOutOfRangeException("port", port, "Port must be 0 or more.");
            if (this.listener != null)
                throw new InvalidOperationException("Already listening.");

            this._shuttingDown = false;
            this.listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.listener.Bind(new IPEndPoint(address, port));


            for (int i = 0; i < 20; i++)
            {
                var decoder = this._configuration.DecoderFactory();
                var encoder = this._configuration.EncoderFactory();
                var channel = this._channelFactory.Create(this._bufferPool.Pop(), encoder, decoder);
                this._channels.Push(channel);
            }

            this.listener.Listen(100);

            this.StartAccept();
        }

        /// <summary>
        ///     Stop the listener.
        /// </summary>
        public virtual void Stop()
        {
            this._shuttingDown = true;
            this.listener.Dispose();
        }

        /// <summary>
        ///     An internal error occurred
        /// </summary>
        public event EventHandler<ErrorEventArgs> ListenerError = delegate { };

        /// <summary>
        ///     To allow the sub classes to configure this class in their constructors.
        /// </summary>
        /// <param name="configuration"></param>
        protected void Configure(ChannelTcpListenerConfiguration configuration)
        {
            this._bufferPool = configuration.BufferPool;
            this._configuration = configuration;
        }


        /// <summary>
        ///     A client has connected (nothing has been sent or received yet)
        /// </summary>
        /// <param name="channel">Channel which we created for the remote socket.</param>
        /// <returns></returns>
        protected virtual ClientConnectedEventArgs OnClientConnected(ITcpChannel channel)
        {
            var args = new ClientConnectedEventArgs(channel);
            this.ClientConnected(this, args);
            return args;
        }

        /// <summary>
        ///     A client has disconnected
        /// </summary>
        /// <param name="channel">Channel representing the client that disconnected</param>
        /// <param name="exception">
        ///     Exception which was used to detect disconnect (<c>SocketException</c> with status
        ///     <c>Success</c> is created for graceful disconnects)
        /// </param>
        protected virtual void OnClientDisconnected(ITcpChannel channel, Exception exception)
        {
            this.ClientDisconnected(this, new ClientDisconnectedEventArgs(channel, exception));
        }

        /// <summary>
        ///     Receive a new message from the specified client
        /// </summary>
        /// <param name="source">Channel for the client</param>
        /// <param name="msg">Message (as decoded by the specified <see cref="IMessageDecoder" />).</param>
        protected virtual void OnMessage(ITcpChannel source, object msg)
        {
            this._messageReceived(source, msg);
        }

        private void OnChannelDisconnect(ITcpChannel source, Exception exception)
        {
            this.OnClientDisconnected(source, exception);
            source.Cleanup();
            this._channels.Push(source);
        }

        private void StartAccept()
        {
           // this.maxNumberAcceptedClients.WaitOne();

            this.listenerArgs.AcceptSocket = null;
            var willRaiseEvent = this.listener.AcceptAsync(this.listenerArgs);
            if (!willRaiseEvent)
            {
                this.OnAcceptSocket(this, this.listenerArgs);
            }
        }

        //private void OnAccept(object sender, SocketAsyncEventArgs e)
        //{
        //    if (e.SocketError != SocketError.Success)
        //    {
        //        this.shutdown.Set();
        //        return;
        //    }
        //    Interlocked.Increment(ref this.numConnectedSockets);

        //    ServerClientContext context;
        //    if (!this.contexts.TryPop(out context))
        //        throw new InvalidOperationException("Failed to get a new client context, all is currently in use.");

        //    if (!this.ValidateClient(e.AcceptSocket))
        //    {
        //        try
        //        {
        //            e.AcceptSocket.Shutdown(SocketShutdown.Send);
        //        }
        //        catch
        //        {
        //        }
        //        e.AcceptSocket.Dispose();
        //        return;
        //    }

        //    var client = this.CreateClient(e.AcceptSocket.RemoteEndPoint);
        //    context.Assign(e.AcceptSocket, client);
        //    this.OnClientConnected(context);

        //    this.StartAccept();
        //}

        private void OnAcceptSocket(object sender, SocketAsyncEventArgs e)
        {
            if (_shuttingDown)
                return;

            try
            {
                // Required as _listener.Stop() disposes the underlying socket
                // thus causing an objectDisposedException here.
                var socket = e.AcceptSocket;
                
                ITcpChannel channel;
                if (!_channels.TryPop(out channel))
                {
                    var decoder = _configuration.DecoderFactory();
                    var encoder = _configuration.EncoderFactory();
                    channel = _channelFactory.Create(_bufferPool.Pop(), encoder, decoder);
                }

                channel.Disconnected = OnChannelDisconnect;
                channel.MessageReceived = OnMessage;
                channel.Assign(socket);

                var args = OnClientConnected(channel);
                if (!args.MayConnect)
                {
                    if (args.Response != null)
                        channel.Send(args.Response);
                    channel.Close();
                    return;
                }
            }
            catch (Exception exception)
            {
                ListenerError(this, new ErrorEventArgs(exception));
            }


            this.StartAccept();
        }
    }
}