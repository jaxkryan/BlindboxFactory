using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public static class NTPClient
{
    public static DateTime GetNetworkTime()
    {
        try
        {
            const string ntpServer = "pool.ntp.org";
            var ntpData = new byte[48];
            // Set protocol version and mode (see RFC 2030)
            ntpData[0] = 0x1B;

            IPAddress[] addresses = Dns.GetHostEntry(ntpServer).AddressList;
            IPEndPoint ipEndPoint = new IPEndPoint(addresses[0], 123);

            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Connect(ipEndPoint);
                socket.ReceiveTimeout = 3000;
                socket.Send(ntpData);
                socket.Receive(ntpData);
            }

            // Offset for Transmit Timestamp (bytes 40 to 47)
            const byte serverReplyTime = 40;
            ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);
            ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

            intPart = SwapEndianness(intPart);
            fractPart = SwapEndianness(fractPart);

            ulong milliseconds = intPart * 1000 + (fractPart * 1000) / 0x100000000UL;
            // NTP time starts on Jan 1, 1900
            DateTime networkDateTime = new DateTime(1900, 1, 1).AddMilliseconds((long)milliseconds);

            return networkDateTime;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("NTP Error: " + ex.Message);
            // Fallback to device time (not secure if offline)
            return DateTime.UtcNow;
        }
    }

    static uint SwapEndianness(ulong x)
    {
        return (uint)(((x & 0x000000ff) << 24) +
                      ((x & 0x0000ff00) << 8) +
                      ((x & 0x00ff0000) >> 8) +
                      ((x & 0xff000000) >> 24));
    }
}
