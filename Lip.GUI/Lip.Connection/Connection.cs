using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Lip.Connection.Network;
using Lip.Connection.Network.Packets;
using Lip.Connection.Network.Packets.ConnectionVerify;

namespace Lip.Connection;

public enum ConnectionMode { Server, Client }


/// <summary>
/// Represents a secure connection between two endpoints.
/// </summary>
public partial class Connection
{
    private PacketSender? _sender;

    private PacketReceiver? _reciver;

    // Encoding used for converting strings to bytes
    private readonly UnicodeEncoding _encoding = new();

    // Hashed password for authentication
    private byte[] _hashedPassword = [];

    // TCP listener for server mode
    private readonly TcpListener? _listener;

    // TCP client for client mode
    private readonly TcpClient? _client;

    // Crypto service provider for encryption and decryption
    private readonly RSACryptoServiceProvider _cryptoServiceProvider;

    // AES key for encryption and decryption
    private byte[]? _aesKey;

    /// <summary>
    /// Gets or sets the hashed password for authentication.
    /// </summary>
    public string HashedPassword
    {
        get
        {
            var stringBuilder = new StringBuilder(_hashedPassword.Length * 3);

            foreach (byte b in _hashedPassword)
                stringBuilder.Append(b.ToString("X2")).Append(' ');

            return stringBuilder.ToString();
        }
        private init => _hashedPassword = SHA256.HashData(_encoding.GetBytes(value));
    }

    private Lock NetworkStreamLock { get; } = new();

    /// <summary>
    /// Gets or sets the connection mode (server or client).
    /// </summary>
    public ConnectionMode Mode { get; init; }

    /// <summary>
    /// Gets a value indicating whether the connection is verified.
    /// </summary>
    [MemberNotNullWhen(true, nameof(_aesKey))]
    public bool Verified => _aesKey is not null;

    /// <summary>
    /// Initializes a new instance of the <see cref="Connection"/> class.
    /// </summary>
    /// <param name="mode">The connection mode (server or client).</param>
    /// <param name="password">The password for authentication.</param>
    /// <param name="address">The IP address of the remote endpoint.</param>
    /// <param name="port">The port number of the remote endpoint.</param>
    public Connection(ConnectionMode mode, string password, IPAddress address, int port)
    {
        HashedPassword = password;
        Mode = mode;
        _cryptoServiceProvider = new RSACryptoServiceProvider();

        if (mode is ConnectionMode.Server)
            _listener = TcpListener.Create(port);
        else
        {
            _client = new TcpClient(new IPEndPoint(address, port))
#if DEBUG
            {
                ReceiveTimeout = 5000,
                SendTimeout = 5000
            }
#endif
            ;

        }
    }

    /// <summary>
    /// Starts the listener asynchronously.
    /// </summary>
    /// <param name="token">The cancellation token for the operation.</param>
    /// <exception cref="InvalidOperationException">Thrown when the connection mode is not server.</exception>
    public async ValueTask StartListener(CancellationToken token = default)
    {
        if (Mode is ConnectionMode.Client) throw new InvalidOperationException("Cannot start listener in client mode.");
        else
        {
            _listener!.Start();
            while (true)
            {
                if (token.IsCancellationRequested) break;

                // Accept incoming connections
                TcpClient client = await _listener.AcceptTcpClientAsync(token);
#if DEBUG
                client.ReceiveTimeout = int.MaxValue;
                client.SendTimeout = int.MaxValue;
#endif

                _sender = new PacketSender(this);
                _reciver = new PacketReceiver(this);
                await VerifyClientAsync(token);
                if (Verified is false) throw new Exception("Failed to verify connection.");

                // Get the network stream for the client
                NetworkStream stream = client.GetStream();
                StartPacketHandler(stream, token);
            }

            _listener.Stop();
        }
    }

    /// <summary>
    /// Connects to the remote endpoint asynchronously.
    /// </summary>
    /// <param name="remoteEP">The remote endpoint to connect to.</param>
    /// <param name="token">The cancellation token for the operation.</param>
    [MemberNotNull(nameof(_sender), nameof(_reciver))]
    public async ValueTask ConnectToAsync(IPEndPoint remoteEP, CancellationToken token = default)
    {
        if (Mode is ConnectionMode.Server) throw new InvalidOperationException("Server mode is not supported.");
        else
        {
            try
            {
                _client!.Connect(remoteEP);
                _sender = new PacketSender(this);
                _reciver = new PacketReceiver(this);
                await VerifyServerAsync(token);
                if (Verified is false) throw new Exception("Failed to verify connection.");
            }
            catch (Exception)
            {
                _client?.Close();
                throw;
            }
        }
    }

    /// <summary>
    /// Verifies the server connection asynchronously.
    /// </summary>
    /// <param name="token">The cancellation token for the operation.</param>
    public async ValueTask VerifyServerAsync(CancellationToken token = default)
    {
        if (Mode is ConnectionMode.Server) throw new InvalidOperationException("Server mode is not supported.");

        // Recive server public key
        RSAPublicKeyPacket rsaPublicKey = await _reciver!.RecivePacketAsync<ConnectionVerifyPackets, RSAPublicKeyPacket>(
            ConnectionVerifyPackets.RSAPublicKey,
            token);
        //if (type is not ConnectionVerifyPackets.RSAPublicKey) throw new InvalidOperationException("Failed to recive server public key.");

        // Send password
        using var rsa = new RSACryptoServiceProvider();
        rsa.FromXmlString(rsaPublicKey.Key);
        await _sender!.SendPacketAsync(ConnectionVerifyPackets.Password, new PasswordPacket { PasswordData = rsa.Encrypt(_hashedPassword, false) }, false, token);

        // Recive password verification
        PasswordVerifiedPacket passwordVerified = await _reciver!.RecivePacketAsync<ConnectionVerifyPackets, PasswordVerifiedPacket>(
            ConnectionVerifyPackets.PasswordVerified,
            token);
        //if (type is not ConnectionVerifyPackets.PasswordVerified) throw new InvalidOperationException("Failed to recive password verification.");
        if (passwordVerified.Value is false) throw new InvalidOperationException("Password verification failed.");

        // Send client public key
        await _sender.SendPacketAsync(ConnectionVerifyPackets.RSAPublicKey, new RSAPublicKeyPacket { Key = _cryptoServiceProvider.ToXmlString(false) }, false, token);

        // Recive and decrypt aes key
        AESKeyPacket aesKeyPacket = await _reciver!.RecivePacketAsync<ConnectionVerifyPackets, AESKeyPacket>(
            ConnectionVerifyPackets.AesKey,
            token);
        //if (type is not ConnectionVerifyPackets.AesKey) throw new InvalidOperationException("Failed to recive aes key.");
        byte[] key = _cryptoServiceProvider.Decrypt(aesKeyPacket.Key, false);

        // Send aes received packet
        await _sender.SendPacketAsync(ConnectionVerifyPackets.AesKey, new AESKeyReceivedPacket() { Value = true }, false, token);

        _aesKey = key;
    }

    /// <summary>
    /// Verifies the client connection asynchronously.
    /// </summary>
    /// <param name="token">The cancellation token for the operation.</param>
    public async ValueTask VerifyClientAsync(CancellationToken token = default)
    {
        if (Mode is ConnectionMode.Client) throw new InvalidOperationException("Client mode is not supported.");

        // Send the public key
        await _sender!.SendPacketAsync(ConnectionVerifyPackets.RSAPublicKey, new RSAPublicKeyPacket { Key = _cryptoServiceProvider.ToXmlString(false) }, false, token);

        // Recive and verify the password
        PasswordPacket password = await _reciver!.RecivePacketAsync<ConnectionVerifyPackets, PasswordPacket>(
            ConnectionVerifyPackets.Password,
            token);
        //if (type is not ConnectionVerifyPackets.Password) throw new InvalidOperationException("Failed to recive password.");
        bool passwordVerified = password.Verify(_hashedPassword, _cryptoServiceProvider);

        // Send the password verification
        await _sender.SendPacketAsync(ConnectionVerifyPackets.PasswordVerified, new PasswordVerifiedPacket { Value = passwordVerified }, false, token);
        if (passwordVerified is false) return;

        // Recive client public key
        RSAPublicKeyPacket rsaPublicKey = await _reciver!.RecivePacketAsync<ConnectionVerifyPackets, RSAPublicKeyPacket>(
            ConnectionVerifyPackets.RSAPublicKey,
            token);
        //if (type is not ConnectionVerifyPackets.RSAPublicKey) throw new Exception("Failed to recive client public key.");

        // Generate AES key and send
        using var rsa = new RSACryptoServiceProvider();
        rsa.FromXmlString(rsaPublicKey.Key);
        byte[] key = Guid.NewGuid().ToByteArray();
        await _sender.SendPacketAsync(ConnectionVerifyPackets.AesKey, new AESKeyPacket { Key = rsa.Encrypt(key, false) }, false, token);

        // Recive AesReceived packet
        AESKeyReceivedPacket aesKeyReceived = await _reciver!.RecivePacketAsync<ConnectionVerifyPackets, AESKeyReceivedPacket>(
            ConnectionVerifyPackets.AesKeyReceived,
            token);
        if (/*type is not ConnectionVerifyPackets.AesKeyReceived && */aesKeyReceived.Value is false) throw new Exception("Failed to recive AES key.");

        _aesKey = key;
    }

    /// <summary>
    /// Encrypts the specified data using the AES key.
    /// </summary>
    /// <param name="data">The data to encrypt.</param>
    /// <returns>The encrypted data.</returns>
    /// <exception cref="Exception">Thrown if the connection is not verified.</exception>
    public byte[] EncryptData(byte[] data)
    {
        if (Verified is false) throw new Exception("Connection is not verifiedection");

        using Aes aes = Aes.Create();
        aes.Key = _aesKey;
        aes.IV = new byte[16];
        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using MemoryStream msEncrypt = new();
        using CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write);
        csEncrypt.Write(data, 0, data.Length);
        return msEncrypt.ToArray();
    }

    /// <summary>
    /// Decrypts the specified data using the AES key.
    /// </summary>
    /// <param name="data">The data to decrypt.</param>
    /// <returns>The decrypted data.</returns>
    /// <exception cref="Exception">Thrown if the connection is not verified.</exception>
    public byte[] DecryptData(byte[] data)
    {
        if (Verified is false) throw new Exception("Connection is not verified.");
        using Aes aes = Aes.Create();
        aes.Key = _aesKey;
        aes.IV = new byte[16];
        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using MemoryStream msDecrypt = new(data);
        using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
        using MemoryStream ms = new();
        csDecrypt.CopyTo(ms);
        aes.Clear();
        return ms.ToArray();
    }

    /// <summary>
    /// Disposes the connection and releases all resources.
    /// </summary>
    public void Dispose()
    {
        _listener?.Dispose();
        _client?.Dispose();
        _cryptoServiceProvider.Dispose();

    }

    private readonly Dictionary<string, Type> _packetTypes = [];

    private readonly ConcurrentDictionary<Type, List<IPacketHandler>> _handlers = [];

    private readonly ConcurrentDictionary<Enum, List<Action<byte[]>>> _packetRequests = [];

    private readonly Queue<(Packet, Action)> _packetsToSend = [];

    /// <summary>
    /// Starts the packet handler to process incoming packets.
    /// </summary>
    /// <param name="stream">The network stream to read packets from.</param>
    /// <param name="token">The cancellation token for the operation.</param>
    private void StartPacketHandler(NetworkStream stream, CancellationToken token = default) => Task.Run(async () =>
    {
        Span<byte> lengthBuffer = stackalloc byte[4];
        Span<byte> isEncryptedBuffer = stackalloc byte[1];
        Span<byte> typeBuffer = stackalloc byte[4];

        while (true)
        {
            if (token.IsCancellationRequested) return;

            Lock.Scope scope = NetworkStreamLock.EnterScope();
            try
            {

                Packet packet = await Packet.ReadAsync(stream, this);
                if (_packetTypes.TryGetValue(packet.PacketIdTypeName, out Type? type))
                {

                    var enumVal = (Enum)Enum.ToObject(type, packet.PacketId);

                    if (_packetRequests.TryGetValue(enumVal, out List<Action<byte[]>>? handlers))
                    {
                        foreach (Action<byte[]> handler in handlers) handler(packet.Data);
                        handlers.Clear();
                    }


                    if (_handlers.TryGetValue(type, out List<IPacketHandler>? handlerList))
                    {
                        foreach (IPacketHandler handler in handlerList)
                            handler.OnPacketReceived(enumVal, packet.Data);
                    }
                }

                while (_packetsToSend.TryDequeue(out (Packet packet, Action action) p))
                {
                    await p.packet.WriteAsync(stream, this);
                    p.action();
                }
            }
            finally
            {
                scope.Dispose();
            }


        }
    }, token);

    /// <summary>
    /// Requests a packet asynchronously.
    /// </summary>
    /// <typeparam name="TPacket">The type of the packet.</typeparam>
    /// <typeparam name="TPacketType">The type of the packet type enum.</typeparam>
    /// <param name="type">The packet type to request.</param>
    /// <returns>The requested packet.</returns>
    public async ValueTask<TPacket> RequestPacketAsync<TPacketType, TPacket>(TPacketType type)
        where TPacketType : Enum
        where TPacket : class, IPacket<TPacket>
    {
        var tcs = new TaskCompletionSource<TPacket>();
        if (_packetRequests.TryGetValue(type, out List<Action<byte[]>>? handlers) is false)
            _packetRequests[type] = handlers = [];

        handlers.Add(bytes =>
        {
            var packet = TPacket.Deserialize(bytes);
            tcs.SetResult(packet);
        });

        return await tcs.Task;
    }

    /// <summary>
    /// Sends a packet asynchronously.
    /// </summary>
    /// <typeparam name="TPacket">The type of the packet.</typeparam>
    /// <typeparam name="TPacketType">The type of the packet type enum.</typeparam>
    /// <param name="packetType">The packet type to send.</param>
    /// <param name="packet">The packet to send.</param>
    /// <param name="encryptData">Whether to encrypt the data.</param>
    /// <param name="token">The cancellation token for the operation.</param>
    public async ValueTask SendPacketAsync<TPacketType, TPacket>(
        TPacketType packetType,
        TPacket packet,
        bool encryptData = true,
        CancellationToken token = default)
        where TPacketType : Enum
        where TPacket : class, IPacket<TPacket>
    {
        byte[] data = packet.Serialize();

        var p = new Packet()
        {
            PacketIdTypeName = $"{packetType.GetType().Assembly.FullName}:{packetType.GetType().FullName}",
            PacketId = Convert.ToInt64(packetType),
            Length = data.Length,
            IsEncrypted = encryptData,
            Data = data
        };

        var tcs = new TaskCompletionSource();
        _packetsToSend.Enqueue((p, tcs.SetResult));
        await tcs.Task;
    }
}
