# EDRPrison
EDRPrison leverages a legitimate WFP callout driver to silence EDR. Inspired by [Shutter](https://github.com/dsnezhkov/shutter), [FireBlock](https://www.mdsec.co.uk/2023/09/nighthawk-0-2-6-three-wise-monkeys/), and [EDRSilencer](https://github.com/netero1010/EDRSilencer), I have researched evasion based on network intervention. Different from them, EDRPrison installs and loads an external legitimate WFP callout driver rather than relying on the built-in WFP. Additionally, EDRPrison blocks EDR processes' outbound traffic by dynamically adding run-time filters without directly interacting with them and their executables.

In summary, EDRPrison has the following features and capabilities
- Utilize a legitimate WFP callout driver to extend capabilities while being benign
- Search for running EDR processes based on defined process names
- Identify packets that originated from EDR processes
- Dynamically add WFP filters based on packets' source process
- Avoid interaction with EDR processes and EDR executables


Please refer to the article for more technical details: 

### Components
To successfully run EDRPrison, elevated privilege is required. EDRPrison includes the following 3 components:

**EDRPrison.exe**: The main executable program. Can be executed in memory. The first execution installs WinDivert driver.

**WinDivert64.sys**: The signed WFP callout driver, should be on disk.

**WinDivert.dll**: A component of WinDivert project, should be on disk.


# Benefits And Improvements


# Test Example




# Detections and Mitigations

### Driver Load Event
- If the WinDivert driver is not installed on the system, EDRPrison will install the callout driver upon first execution. Both the OS and the telemetry will log this event.

### Existence of WinDivert Files
- EDRPrison and other WinDivert-dependent programs require WinDivert64.sys and WinDivert.dll to be on the disk.

### WinDivert Usage Detection Tools
- Some tools, such as WinDivertTool, can detect processes currently using WFP.

### Packet Drop/Block Actions Against EDR Processes
- Elastic has a detection rule that can be used to detect packet drop or block actions against security software processes.

### Review Registered WFP Providers, Filters, and Callouts
- Tool WFPExplorer helps administrators review active WFP sessions, registered callouts, and filters.

### Adminless
- A future feature that could add additional protections for driver installment.

# Further Evasion
From a red teamer's perspective, we can subvert some of the above detections depending on the environment's security configurations.

### Seek An Alternative To WinDivert
- If WinDivert is considered malicious in the environment, we can seek signed, open-source alternatives, have fewer records for malicious purposes, and allow packet interception, reinjection, and other manipulation.

### Reuse An Installed Or Built-in WFP Callout Driver
- If all external drivers are considered unauthorized unless approved, it is challenging but possible to reverse engineer an installed or built-in WFP callout driver and reuse its callout functions. Many security software solutions have their own WFP callout drivers.

### Change Action To Intercepted Packets
- Redirect or proxy the packets instead of blocking or dropping them.

 


# Credit

The following resources inspired and helped me a lot during my research. I extend my thanks to all the authors:

https://write-verbose.com/2022/05/31/EDRBypass/ 

https://medium.com/csis-techblog/silencing-microsoft-defender-for-endpoint-using-firewall-rules-3839a8bf8d18 

https://github.com/netero1010/EDRSilencer 

https://github.com/dsnezhkov/shutter 

https://www.mdsec.co.uk/2023/09/nighthawk-0-2-6-three-wise-monkeys/ 

https://github.com/amjcyber/EDRNoiseMaker 

https://www.securityartwork.es/2024/06/17/edr-silencer-2/ 

https://windowsir.blogspot.com/2024/01/edrsilencer.html 

https://github.com/TechnikEmpire/HttpFilteringEngine 

https://reqrypt.org/windivert.html 

https://learn.microsoft.com/en-us/defender-cloud-apps/network-requirements

https://learn.microsoft.com/en-us/defender-endpoint/configure-network-connections-microsoft-defender-antivirus 

https://blog.p1k4chu.com/security-research/adversarial-tradecraft-research-and-detection/edr-silencer-embracing-the-silence 

https://learn.microsoft.com/en-us/windows/win32/fwp/windows-filtering-platform-start-page 

https://blog.quarkslab.com/guided-tour-inside-windefenders-network-inspection-driver.html

https://adguard.com/kb/adguard-for-windows/solving-problems/wfp-driver/ 

https://www.elastic.co/guide/en/security/current/potential-evasion-via-windows-filtering-platform.html 

https://learn.microsoft.com/en-us/windows/win32/fwp/built-in-callout-identifiers 

https://learn.microsoft.com/en-us/samples/microsoft/windows-driver-samples/windows-filtering-platform-sample/ 

https://github.com/microsoft/windows-driver-samples/tree/main/network/trans/WFPSampler 

https://github.com/TechnikEmpire/CitadelCore 

https://github.com/TechnikEmpire/HttpFilteringEngine 

https://www.tripwire.com/state-of-security/divergent-malware-using-nodejs-windivert-in-fileless-attacks 
