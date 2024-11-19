# dotnet_send_wake_on_lan_magicpacket

## 概要
* 指定の MAC アドレスに Wake On Lan のマジックパケットを送信するだけのプログラム

## ヘルプ

Description:  
　指定の MAC アドレスに Wake On Lan のマジックパケットを送信する。  

使用方法:  
　send_wake_on_lan_magicpacket <MACアドレス>  

引数:  
　<MACアドレス> ""XX-XX-XX-XX-XX-XX"" 形式で MAC アドレスを指定  

オプション:  
　--version      バージョンを表示します。  
　-?, -h, --help ヘルプを表示します。  

## ビルド

### Windows x64
```
dotnet publish -r win-x64
```
※要 Desktop development with C++ workload