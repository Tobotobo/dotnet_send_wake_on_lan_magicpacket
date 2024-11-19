using System.Reflection;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;

if (args.Length == 1 && args[0] == "--version") 
    goto show_version;

if (args.Length == 1 && MacAddrRegex().IsMatch(args[0]))
    goto send_magic_packet;

goto show_help;

// -------------------------------------------------------
send_magic_packet:

// AA-BB-CC-DD-EE-FF
string macAddrStr = args[0]; 

// byte[]に変換
byte[] macAddr = macAddrStr.Split('-').Select(x => Convert.ToByte(x, 16)).ToArray();

// 0xFFを6回繰り返した配列
byte[] prefix = Enumerable.Repeat((byte)0xFF, 6).ToArray();

// マジックパケットの生成
byte[] magicPacket = [.. prefix, .. Enumerable.Repeat(macAddr, 16).SelectMany(b => b)];

// 出力 (例: デバッグ用に16進数文字列で表示)
Console.WriteLine(string.Join(" ", magicPacket.Select(b => b.ToString("X2"))));

// UDPクライアントを作成
using (UdpClient udpClient = new())
{
    // ブロードキャストアドレスを設定
    IPAddress broadcastAddress = IPAddress.Broadcast;

    // ポート9に接続
    udpClient.Connect(broadcastAddress, 9);

    // マジックパケットを送信
    udpClient.Send(magicPacket, magicPacket.Length);
}
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
