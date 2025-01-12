using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Lip.Connection.Network;
using Lip.Connection.Network.Packets.ConnectionVerify;

namespace Lip.Connection;

public enum ConnectionMode { Server, Client }


/// <summary>
/// Represents a secure connection between two endpoints.
/// </summary>
public partial class Connection
{
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
        get => _encoding.GetString(_hashedPassword);
        private init => _hashedPassword = SHA256.HashData(_encoding.GetBytes(value));
    }

    /// <summary>
    /// Gets or sets the connection mode (server or client).
    /// </summary>
    public ConnectionMode Mode { get; init; }

    /// <summary>
    /// Gets or sets the packet handler for incoming packets.
    /// </summary>
    public IPacketHandler? PacketHandler { get; set; }

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
            _client = new TcpClient(new IPEndPoint(address, port));
    }

    /// <summary>
    /// Starts the listener asynchronously.
    /// </summary>
    /// <param name="token">The cancellation token for the operation.</param>
    /// <exception cref="InvalidOperationException">Thrown when the connection mode is not server.</exception>
    public void StartListener(CancellationToken token = default)
    {
        if (Mode is ConnectionMode.Client) throw new InvalidOperationException("Cannot start listener in client mode.");
        else
        {
            _listener!.Start();
            Task.Run(async () =>
            {
                while (true)
                {
                    if (token.IsCancellationRequested) break;

                    // Accept incoming connections
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    await VerifyClientAsync(client.GetStream(), token);
                    if (Verified is false) throw new Exception("Failed to verify connection.");

                    // Get the network stream for the client
                    NetworkStream stream = client.GetStream();
                    PacketHandler?.Start(stream: stream, token: token);
                }

                _listener.Stop();
            }, token);
        }
    }

    /// <summary>
    /// Connects to the remote endpoint asynchronously.
    /// </summary>
    /// <param name="remoteEP">The remote endpoint to connect to.</param>
    /// <param name="token">The cancellation token for the operation.</param>
    public async ValueTask ConnectToAsync(IPEndPoint remoteEP, CancellationToken token = default)
    {
        if (Mode is ConnectionMode.Server) throw new InvalidOperationException("Server mode is not supported.");
        else
        {
            try
            {
                _client!.Connect(remoteEP);
                await VerifyServerAsync(_client.GetStream(), token);
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
    private async ValueTask VerifyServerAsync(NetworkStream stream, CancellationToken token = default)
    {
        if (Mode is ConnectionMode.Server) throw new InvalidOperationException("Server mode is not supported.");

        // Recive server public key
        (ConnectionVerifyPackets type, RSAPublicKeyPacket rsaPublicKey) = await PacketReciver.RecivePacketAsync<ConnectionVerifyPackets, RSAPublicKeyPacket>(null, stream, token);
        if (type is not ConnectionVerifyPackets.RSAPublicKey) throw new InvalidOperationException("Failed to recive server public key.");

        // Send password
        using var rsa = new RSACryptoServiceProvider();
        rsa.FromXmlString(rsaPublicKey.Key);
        await PacketSender.SendPacketAsync(null, stream, ConnectionVerifyPackets.Password, new PasswordPacket { PasswordData = rsa.Encrypt(_hashedPassword, false) }, token);

        // Recive password verification
        (type, PasswordVerifiedPacket passwordVerified) = await PacketReciver.RecivePacketAsync<ConnectionVerifyPackets, PasswordVerifiedPacket>(null, stream, token);
        if (type is not ConnectionVerifyPackets.PasswordVerified) throw new InvalidOperationException("Failed to recive password verification.");
        if (passwordVerified.Value is false) throw new InvalidOperationException("Password verification failed.");

        // Send client public key
        await PacketSender.SendPacketAsync(null, stream, ConnectionVerifyPackets.RSAPublicKey, new RSAPublicKeyPacket { Key = _cryptoServiceProvider.ToXmlString(false) }, token);

        // Recive and decrypt aes key
        (type, AESKeyPacket aesKeyPacket) = await PacketReciver.RecivePacketAsync<ConnectionVerifyPackets, AESKeyPacket>(null, stream, token);
        if (type is not ConnectionVerifyPackets.AesKeyReceived) throw new InvalidOperationException("Failed to recive aes key.");
        byte[] key = _cryptoServiceProvider.Decrypt(aesKeyPacket.Key, false);

        // Send aes received packet
        await PacketSender.SendPacketAsync(null, stream, ConnectionVerifyPackets.AesKey, new AESKeyPacket { Key = key }, token);

        _aesKey = key;
    }

    /// <summary>
    /// Verifies the client connection asynchronously.
    /// </summary>
    /// <param name="token">The cancellation token for the operation.</param>
    public async ValueTask VerifyClientAsync(NetworkStream stream, CancellationToken token = default)
    {
        if (Mode is ConnectionMode.Client) throw new InvalidOperationException("Client mode is not supported.");

        // Send the public key
        await PacketSender.SendPacketAsync(null, stream, ConnectionVerifyPackets.RSAPublicKey, new RSAPublicKeyPacket { Key = _cryptoServiceProvider.ToXmlString(false) }, token);

        // Recive and verify the password
        (ConnectionVerifyPackets type, PasswordPacket password) = await PacketReciver.RecivePacketAsync<ConnectionVerifyPackets, PasswordPacket>(null, stream, token);
        if (type is not ConnectionVerifyPackets.Password) throw new InvalidOperationException("Failed to recive password.");
        bool passwordVerified = password.Verify(_hashedPassword, _cryptoServiceProvider) is false;

        // Send the password verification
        await PacketSender.SendPacketAsync(null, stream, ConnectionVerifyPackets.PasswordVerified, new PasswordVerifiedPacket { Value = passwordVerified }, token);
        if (passwordVerified is false) return;

        // Recive client public key
        (type, RSAPublicKeyPacket rsaPublicKey) = await PacketReciver.RecivePacketAsync<ConnectionVerifyPackets, RSAPublicKeyPacket>(null, stream, token);
        if (type is not ConnectionVerifyPackets.RSAPublicKey) throw new Exception("Failed to recive client public key.");

        // Generate AES key and send
        using var rsa = new RSACryptoServiceProvider();
        rsa.FromXmlString(rsaPublicKey.Key);
        byte[] key = Guid.NewGuid().ToByteArray();
        await PacketSender.SendPacketAsync(null, stream, ConnectionVerifyPackets.AesKey, new AESKeyPacket { Key = rsa.Encrypt(key, false) }, token);

        // Recive AesReceived packet
        (type, AESKeyReceivedPacket aesKeyReceived) = await PacketReciver.RecivePacketAsync<ConnectionVerifyPackets, AESKeyReceivedPacket>(null, stream, token);
        if (type is not ConnectionVerifyPackets.AesKeyReceived && aesKeyReceived.Value is false) throw new Exception("Failed to recive AES key.");

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
        return ms.ToArray();
    }

    public void Dispose()
    {
        _listener?.Dispose();
        _client?.Dispose();
        _cryptoServiceProvider.Dispose();
    }
}
