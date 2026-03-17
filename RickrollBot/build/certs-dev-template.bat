@echo off
setlocal

net session >nul 2>&1
if errorlevel 1 (
    echo This script must be run as Administrator.
    exit /b 1
)

set "CallSignalingPort=<CALL_SIGNALING_PORT>"
set "InstanceInternalPort=<INSTANCE_INTERNAL_PORT>"
set /A CallSignalingPort2=CallSignalingPort+1
set "CertHash=<CERTIFICATE_THUMBPRINT>"
set "AppId={aeeb866d-e17b-406f-9385-32273d2f8691}"

REM Deleting bindings
netsh http delete sslcert ipport=0.0.0.0:%CallSignalingPort% >nul 2>&1
netsh http delete sslcert ipport=0.0.0.0:%InstanceInternalPort% >nul 2>&1
netsh http delete urlacl url=https://+:%CallSignalingPort%/ >nul 2>&1
netsh http delete urlacl url=https://+:%InstanceInternalPort%/ >nul 2>&1
netsh http delete urlacl url=http://+:%CallSignalingPort2%/ >nul 2>&1

REM Add URLACL bindings
netsh http add urlacl url=https://+:%CallSignalingPort%/ sddl=D:(A;;GX;;;S-1-1-0)
netsh http add urlacl url=https://+:%InstanceInternalPort%/ sddl=D:(A;;GX;;;S-1-1-0)
netsh http add urlacl url=http://+:%CallSignalingPort2%/ sddl=D:(A;;GX;;;S-1-1-0)

REM ensure the app id matches the GUID in AssemblyInfo.cs
REM Ensure the certhash matches the certificate

netsh http add sslcert ipport=0.0.0.0:%CallSignalingPort% certhash=%CertHash% appid=%AppId%
netsh http add sslcert ipport=0.0.0.0:%InstanceInternalPort% certhash=%CertHash% appid=%AppId%
