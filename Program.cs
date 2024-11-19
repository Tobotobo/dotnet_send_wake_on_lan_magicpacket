using System.Reflection;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

if (args.Length == 1 && args[0] == "--version") 
    goto show_version;

if (args.Length == 1 && MacAddrRegex().IsMatch(args[0]))
    goto send_magic_packet;

goto show_help;

// -------------------------------------------------------
send_magic_packet:

// IP アドレスとサブネットマスクからブロードキャストアドレスを取得
static IPAddress getBroadcastAddress(IPAddress ipAddress, IPAddress subnetMask)
{
    var ipBytes = ipAddress.GetAddressBytes();
    var maskBytes = subnetMask.GetAddressBytes();
    var broadcastBytes = new byte[ipBytes.Length];
    for (int i = 0; i < ipBytes.Length; i++)
    {
        broadcastBytes[i] = (byte)(ipBytes[i] | ~maskBytes[i]);
    }
    return new IPAddress(broadcastBytes);
}

// 全 NIC のブロードキャストアドレスを取得
static IPAddress[] getBroadcastAddresses() {
    return NetworkInterface.GetAllNetworkInterfaces()
        .Where(ni => 
            ni.OperationalStatus == OperationalStatus.Up 
            && (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet
                 || ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
        .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
        .Where(ua => 
            ua.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork 
            && ua.IPv4Mask != null)
        .Select(ua => getBroadcastAddress(ua.Address, ua.IPv4Mask))
        .ToArray();
}

// 指定した MAC アドレスで Wake on Lan 用のマジックパケットを作成
static byte[] createMagicPacket(string macAddrStr) {
    // byte[]に変換
    byte[] macAddr = macAddrStr.Split('-').Select(x => Convert.ToByte(x, 16)).ToArray();
    // 0xFFを6回繰り返した配列
    byte[] prefix = Enumerable.Repeat((byte)0xFF, 6).ToArray();
    // マジックパケットの生成
    byte[] magicPacket = [.. prefix, .. Enumerable.Repeat(macAddr, 16).SelectMany(b => b)];
    return magicPacket;
}

// 指定した送信先にマジックパケットを送信
static void sendMagicPacket(IPAddress[] broadcastAddresses, byte[] magicPacket) {
    using (UdpClient udpClient = new())
    {
        udpClient.EnableBroadcast = true;
        foreach (var ba in broadcastAddresses)
        {
            udpClient.Send(magicPacket, magicPacket.Length, new IPEndPoint(ba, 9));
        }
    }
}

// MAC アドレス
string macAddr = args[0];

// 送信先となる全 NIC のブロードキャストアドレスを取得
IPAddress[] broadcastAddresses = getBroadcastAddresses();

// 指定した MAC アドレスでマジックパケットを作成
byte[] magicPacket = createMagicPacket(macAddr);

// 指定した送信先にマジックパケットを送信
sendMagicPacket(broadcastAddresses, magicPacket);

return;

// -------------------------------------------------------
show_version:
Assembly assembly = Assembly.GetExecutingAssembly();
Version version = assembly.GetName().Version!;
Console.WriteLine($"{version}");
return;

// -------------------------------------------------------
show_help:
Console.WriteLine(@"Description:
  指定の MAC アドレスに Wake On Lan のマジックパケットを送信する。

使用方法:
  send_wake_on_lan_magicpacket <MACアドレス>

引数:
  <MACアドレス> ""XX-XX-XX-XX-XX-XX"" 形式で MAC アドレスを指定

オプション:
  --version      バージョンを表示します。
  -?, -h, --help ヘルプを表示します。
");
return;

// -------------------------------------------------------
partial class Program
{
    [GeneratedRegex(@"^([0-9A-Fa-f]{2}-){5}[0-9A-Fa-f]{2}$")]
    private static partial Regex MacAddrRegex();
}
