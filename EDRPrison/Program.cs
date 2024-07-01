using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using WindivertDotnet;

namespace App
{
    internal class Program
    {
        static ConcurrentDictionary<string, string> concurrentDictionary = new ConcurrentDictionary<string, string>();
        static ConcurrentDictionary<string, int> processDictionary = new ConcurrentDictionary<string, int>();

        static void initData()
        {
            processDictionary.TryAdd("MsMpEng.exe", 1);
            processDictionary.TryAdd("MsSense.exe", 1);
            processDictionary.TryAdd("SenseIR.exe", 1);
            processDictionary.TryAdd("SenseNdr.exe", 1);
            processDictionary.TryAdd("SenseCncProxy.exe", 1);
            processDictionary.TryAdd("SenseSampleUploader.exe", 1);
            processDictionary.TryAdd("elastic-endpoint.exe", 1);
            processDictionary.TryAdd("elastic-agent.exe", 1);
        }
        static string GetProcessNameByPID(int pid)
        {
            string processName = string.Empty;
            try
            {
                Process process = Process.GetProcessById(pid);
                processName = process.ProcessName;
            }
            catch (Exception ex)
            {
            }
            return processName + ".exe";
        }
        public unsafe static string GetRmAddrPortFlow(WinDivertAddress addr)
        {
            string strAddr = addr.Flow->RemoteAddr.ToString();
            string strPort = addr.Flow->RemotePort.ToString();
            if (strAddr.StartsWith("::ffff:"))
            {
                strAddr = strAddr.Substring("::ffff:".Length) + ":" + strPort;
            }
            else
            {
                strAddr = strAddr + ":" + strPort;
            }
            return strAddr;
        }

        public unsafe static string GetRmAddrPortNetwork(WinDivertParseResult result)
        {
            string strAddr = string.Empty;
            string strPort = string.Empty;
            if (result.IPV4Header != null)
            {
                strAddr = result.IPV4Header->DstAddr.ToString();
            }
            if (result.IPV6Header != null)
            {
                strAddr = result.IPV6Header->DstAddr.ToString();
            }
            strPort = result.TcpHeader->DstPort.ToString();
            strAddr = strAddr + ":" + strPort;
            return strAddr;
        }

        public unsafe static string GetProcessID(WinDivertAddress addr)
        {
            int procId = addr.Flow->ProcessId;
            return GetProcessNameByPID(procId);
        }
        public static unsafe void InsertDictionary(WinDivertAddress addr)
        {
            try
            {
                string procName = GetProcessNameByPID(addr.Flow->ProcessId);
                string rmAddrPort = GetRmAddrPortFlow(addr);
                if (processDictionary.ContainsKey(procName))
                {
                    concurrentDictionary.TryAdd(rmAddrPort, "[+] Block:" + procName + " Remote:" + rmAddrPort);
                }
            }
            catch (Exception ex)
            {
            }
        }
        public static unsafe bool CheckInDictionary(WinDivertParseResult result, out string DictValue, out string processName)
        {
            bool reflag = false;
            DictValue = string.Empty;
            processName = string.Empty;
            string rmAddrPort = GetRmAddrPortNetwork(result);
            if (concurrentDictionary.ContainsKey(rmAddrPort))
            {
                concurrentDictionary.TryGetValue(rmAddrPort, out DictValue);
                string[] parts = DictValue.Split(new string[] { "Block:", " Remote" }, StringSplitOptions.None);
                if (parts.Length > 1)
                {
                    processName = parts[1].Trim();
                }
                reflag = true;
            }
            return reflag;
        }
        public static unsafe bool DoWorkPacket_Step1(WinDivertParseResult result)
        {
            bool reflag = false;
            if (result.IPV4Header == null && result.IPV6Header == null)
            {
                reflag = true;
            }
            return reflag;
        }

        static async Task Main(string[] args)
        {
            initData();

            Task task1 = DoWorkAsyncFlow();
            Task task2 = DoWorkAsyncNETWORK();

            
            await Task.WhenAll(task1, task2);
            Console.WriteLine("Both Tasks Finished");
        }

        static async Task DoWorkAsyncFlow()
        {
            var filter = Filter.True.And(f => f.Tcp.DstPort!=3389); //For remote testing purpose
            using var divert = new WinDivert(filter, WinDivertLayer.Flow);
            using var packet = new WinDivertPacket();
            using var addr = new WinDivertAddress();

            while (true)
            {
                var recvLength = await divert.RecvAsync(packet, addr);
                InsertDictionary(addr);
            }
        }

        static async Task DoWorkAsyncNETWORK()
        {
            var filter = Filter.True.And(f => f.Network.Outbound);
            using var divert = new WinDivert(filter, WinDivertLayer.Network);
            using var packet = new WinDivertPacket();
            using var addr = new WinDivertAddress();

            while (true)
            {
                var recvLength = await divert.RecvAsync(packet, addr);
                var result = packet.GetParseResult();

                if (DoWorkPacket_Step1(result))
                {
                    continue;
                }

                string BlockMessage = string.Empty;
                string ProcessName = string.Empty;
                if (!CheckInDictionary(result, out BlockMessage, out ProcessName))
                {
                    var checkState = packet.CalcChecksums(addr);
                    var sendLength = await divert.SendAsync(packet, addr);
                }
                else
                {
                    processDictionary.TryGetValue(ProcessName, out var limitLen);
                    if (recvLength < limitLen)
                    {
                        var checkState = packet.CalcChecksums(addr);
                        var sendLength = await divert.SendAsync(packet, addr);
                    }
                    else
                    {
                        if (!BlockMessage.Equals(string.Empty))
                        {
                            Console.WriteLine($"{BlockMessage} PacketLen:{recvLength}");
                        }
                    }
                }
            }
        }
    }
}