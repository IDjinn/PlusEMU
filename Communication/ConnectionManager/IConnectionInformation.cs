using System;

namespace Plus.Communication.ConnectionManager;

public interface IConnectionInformation : IDisposable
{
    /// <summary>
    ///     This item contains the data parser for the connection
    /// </summary>
    IDataParser Parser { get; set; }

    /// <summary>
    ///     Disposes the current item
    /// </summary>
    void Dispose();

    /// <summary>
    ///     Is triggered when the user connects/disconnects
    /// </summary>
    event ConnectionInformation.ConnectionChange ConnectionChanged;

    /// <summary>
    ///     Starts this item packet processor
    ///     MUST be called before sending data
    /// </summary>
    void StartPacketProcessing();

    /// <summary>
    ///     Returns the ip of the current connection
    /// </summary>
    /// <returns>The ip of this connection</returns>
    string GetIp();

    /// <summary>
    ///     Returns the connection id
    /// </summary>
    /// <returns>The id of the connection</returns>
    int GetConnectionId();

    /// <summary>
    ///     Disconnects the current connection
    /// </summary>
    void Disconnect();

    void SendData(byte[] packet);
}