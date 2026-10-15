using System.Net;
using System.Net.Sockets;
using System.Text;

namespace KST.POT.AsyncExamples.Common;

public static class Extensions
{
    public static string AsString(this in Span<byte> buffer)
    {
        var sb = new StringBuilder();
        var first = true;
        foreach (var item in buffer)
        {
            if (!first)
                sb.Append(',');
            sb.Append(item);
            first = false;
        }
        return sb.ToString();
    }

    public static string GetIPAddress(this Socket socket)
        => $"{(socket.RemoteEndPoint as IPEndPoint)?.Address}:{(socket.RemoteEndPoint as IPEndPoint)?.Port}";
}
