using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using TSRawStreamMarker.TransportStream.Packets;
using System.IO;

namespace TSRawStreamMarker
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            //InitializeComponent();
            string fPath = "";
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Transport Stream File|*.ts";
            dialog.Title = "Open File";
            if (dialog.ShowDialog() ?? false)
            {
                fPath = dialog.FileName;
            }
            else return;
            //SplitFile(fPath);
            using (var st = new System.IO.FileStream(fPath, 
                System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
            {
                Dictionary<int, ctns> packetCounter = new Dictionary<int, ctns>();
                var totalPacketLen = st.Length / 188;
                long packetCount = 0;
                Console.WriteLine("Checking Error or Countinuity for file:");
                Console.WriteLine(fPath);
                Console.WriteLine("Current / Total - IsError");

                Thread workThread = new Thread(new ThreadStart(() => {
                while (true)
                {
                    if (st.Position >= st.Length) break;
                    if ((byte)st.ReadByte() == TransportStream.TSPacket.SYNC_BYTE)
                    {
                        st.Position -= 1;
                        byte[] buffer = new byte[188];
                        if (st.Read(buffer, 0, 188) < 188) break;
                        var stopwatch = new System.Diagnostics.Stopwatch();
                            stopwatch.Start();
                        var packet = new TransportStream.TSPacket(buffer);
                            stopwatch.Stop();
                            System.Diagnostics.Debug.WriteLine($"This init took {stopwatch.ElapsedTicks} ticks.");
                        if (packetCounter.Keys.Contains(packet.PID))
                        {
                            var val = packetCounter[packet.PID];
                            val.TotalCount += 1;
                            val.TotalCountinuity += packet.CountinuityCounter == 0 && val.LastCounter ==15 ? 0 :(packet.CountinuityCounter - 1) == val.LastCounter ? 0 : 1;
                            val.ErrorCount += packet.IsError ? 1 : 0;
                            val.LastCounter = packet.CountinuityCounter;
                            packetCounter[packet.PID] = val;
                        }
                        else
                        {
                            var val = new ctns();
                            val.TotalCount = 1;
                            val.LastCounter = packet.CountinuityCounter;
                            val.TotalCountinuity = 0;
                            val.ErrorCount = packet.IsError ? 1 : 0;
                            packetCounter.Add(packet.PID, val);
                        }
                        if (packet.AdaptationFieldControl == TransportStream.AdaptationField.AdaptationWithPayload ||
                            packet.AdaptationFieldControl == TransportStream.AdaptationField.None)
                        {
                            var pat = TransportStream.Packets.Helper.TryGetPSISection(packet);
                            Console.WriteLine($"{packetCounter[packet.PID].TotalCount}[{packet.PID}] : {packet.AdaptationFieldControl} {packet.AdaptionField?.FieldLength} + {pat.TableID}[{pat.SectionLength}]");
                        }
                        else
                            Console.WriteLine($"{packetCounter[packet.PID].TotalCount}[{packet.PID}] : {packet.AdaptationFieldControl} {packet.AdaptionField?.FieldLength}");
                        packetCount += 1;
                    Console.Write($"[{packetCount} / {totalPacketLen}] - {packet.IsError}");
                    Console.Write("\r");
                    }
                }

            }));
            workThread.IsBackground = true;
            workThread.Start();
            workThread.Join(Timeout.Infinite);
            Console.WriteLine("Done!!");
            foreach(var i in packetCounter)
            {
                Console.WriteLine($"PID[{i.Key}]:");
                Console.WriteLine($"   Total Packet Count : {i.Value.TotalCount}");
                Console.WriteLine($"    Packet Lose Count : {i.Value.TotalCountinuity}");
                Console.WriteLine($"   Packet Error Count : {i.Value.ErrorCount}");
                Console.WriteLine("-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-");
            }
            Console.ReadKey(true);
            Application.Current.Shutdown();
            }
        }

        public void SplitFile(string path)
        {
            using (var st = new FileStream(path,
                FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Thread workThread = new Thread(new ThreadStart(() =>
                {
                    string root = new FileInfo(path).DirectoryName;
                    root = Path.Combine(root, Path.GetFileNameWithoutExtension(path));
                    if (!Directory.Exists(root)) Directory.CreateDirectory(root);
                    Dictionary<int, FileStream> programs = new Dictionary<int, FileStream>();
                    Dictionary<int,int> tracks = new Dictionary<int,int>();
                    while (true)
                    {
                        if (st.Position >= st.Length) break;
                        if ((byte)st.ReadByte() == TransportStream.TSPacket.SYNC_BYTE)
                        {
                            st.Position -= 1;

                            byte[] buffer = new byte[188];
                            if (st.Read(buffer, 0, 188) < 188) break;
                            var packet = new TransportStream.TSPacket(buffer);

                            //Find PAT
                            if(packet.PID == 0)//P.A.T packet
                            {
                                var pat = new PATPacket(packet.Payload, packet.IsPayloadEntry);
                                if(programs.Keys.Count<=0)
                                    foreach(var i in pat.Programs)
                                    {
                                        if (!programs.Keys.Contains(i.PID))
                                        {
                                            var fPath = Path.Combine(root, i.PID.ToString() + ".m2ts");
                                            var writestream = new FileStream(fPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                                            //Open a file stream for each map on pat.
                                            programs.Add(i.PID, writestream);
                                        }
                                    }
                                foreach(var i in programs.Values)
                                {
                                    i.Write(buffer, 0, buffer.Length);
                                }
                            }
                            else if(programs.Keys.Contains(packet.PID))
                            {
                                var pmt = new PMTPacket(packet.Payload, packet.IsPayloadEntry);
                                try
                                {
                                    foreach (var i in pmt.TrackList)
                                    {
                                        if (!tracks.Keys.Contains(i.ElementaryPID))
                                            tracks.Add(i.ElementaryPID, packet.PID);
                                    }
                                }
                                catch { }
                                programs[packet.PID].Write(buffer, 0, buffer.Length);
                            }
                            else
                            {
                                if (tracks.Keys.Contains(packet.PID))
                                {
                                    var tck = tracks[packet.PID];
                                    programs[tck].Write(buffer, 0, buffer.Length);
                                }
                            }

                        }
                    }
                    foreach(var i in programs.Values)
                    {
                        i.Close();
                        i.Dispose();
                    }
                }));
                workThread.IsBackground = true;
                workThread.Start();
                workThread.Join(Timeout.Infinite);
            }
        }

        struct ctns
        {
            public int LastCounter { get; set; }
            public long TotalCount { get; set; }
            public long TotalCountinuity { get; set; }
            public long ErrorCount { get; set; }
        } 
    }
}
