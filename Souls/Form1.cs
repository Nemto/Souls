using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Souls
{
    public partial class Form1 : Form
    {

        #region "Imports"
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(int dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
        [In, Out] byte[] lpBuffer, UInt32 nSize, ref IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
        [In, Out] byte[] lpBuffer, UInt32 nSize, ref IntPtr lpNumberOfBytesRead);

        const int PROCESS_ALL_ACCESS = 0x001F0FFF;
        #endregion

        public Form1()
        {
            InitializeComponent();
            Update();
        }

        private IntPtr darksoulsHandle, baseAddress;
        private int pid;
        private Process darksoulsProcess;
        private byte[] currentSouls = new byte[4];

        [StructLayout(LayoutKind.Explicit)]
        public struct Offset
        {
            [FieldOffset(0xE8)]
            public int souls;
        }

        Offset offset = new Offset();

        private void Update()
        {
            darksoulsProcess = Process.GetProcessesByName("DarkSoulsII").FirstOrDefault();
            pid = darksoulsProcess.Id;
            darksoulsHandle = OpenProcess(PROCESS_ALL_ACCESS, false, pid);
            baseAddress = darksoulsProcess.MainModule.BaseAddress;

            ReadSouls(darksoulsHandle, playerStats() + 0xE8, currentSouls);

            textBox1.Text = BitConverter.ToInt32(currentSouls, 0).ToString();
        }

        private IntPtr playerStats()
        {
            int lvl1 = 0x01028F58;
            int lvl2 = 0x388;


            var _buffer = new byte[4];

            ReadSouls(darksoulsHandle, baseAddress + lvl1, _buffer);
            ReadSouls(darksoulsHandle, (IntPtr)BitConverter.ToInt32(_buffer, 0) + lvl2, _buffer);

            return (IntPtr)BitConverter.ToInt32(_buffer, 0);
        }

        private static bool WriteSouls(IntPtr pHandle, IntPtr lpAddress, int lpBuffer)
        {
            var write = BitConverter.GetBytes(lpBuffer);

            IntPtr _written = new IntPtr(0);
            return WriteProcessMemory(pHandle, lpAddress, write, (uint)write.Length, ref _written);
        }

        private static bool ReadSouls(IntPtr pHandle, IntPtr lpAddress, byte[] lpBuffer)
        {
            IntPtr _read = new IntPtr(0);
            return ReadProcessMemory(pHandle, lpAddress, lpBuffer, (uint)lpBuffer.Length, ref _read);
        }

        private void button_plus5k_Click(object sender, EventArgs e)
        {
            WriteSouls(darksoulsHandle, playerStats() + 0xE8, BitConverter.ToInt32(currentSouls, 0) + 5000);
            Update();
        }
    }
}
