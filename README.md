# EDRPrison
EDRPrison leverages a legitimate WFP callout driver, [WinDivert](https://reqrypt.org/windivert.html), to effectively silence EDR systems. Drawing inspiration from tools like [Shutter](https://github.com/dsnezhkov/shutter), [FireBlock](https://www.mdsec.co.uk/2023/09/nighthawk-0-2-6-three-wise-monkeys/), and [EDRSilencer](https://github.com/netero1010/EDRSilencer), this project focuses on network-based evasion techniques. Unlike its predecessors, EDRPrison installs and loads an external legitimate WFP callout driver instead of relying solely on the built-in WFP. Additionally, it blocks outbound traffic from EDR processes by dynamically adding runtime filters without directly interacting with the EDR processes or their executables.


In summary, EDRPrison has the following key features and capabilities
- Legitimate WFP Callout Driver: Utilizes a legitimate WFP callout driver to enhance capabilities while maintaining a benign profile.
- EDR Process Detection: Searches for running EDR processes based on predefined process names.
- Packet Identification: Identifies packets originating from EDR processes.
- Dynamic Filter Addition: Dynamically adds WFP filters based on the source process of the packets.
- Non-Intrusive Approach: Avoids direct interaction with EDR processes and their executables, ensuring stealth and reducing the risk of detection.



Please refer to the article for more technical details: 

### Components
Elevated privileges are required to run EDRPrison successfully. EDRPrison comprises the following three components:

- EDRPrison: They are the main program and its dependencies. Its first execution installs the WinDivert driver.
- WinDivert64.sys: This is the signed WFP callout driver.
- WinDivert.dll: A component of the WinDivert project.


# Benefits And Improvements
EDRPrison offers several enhancements and improvements over its predecessors, making it a more robust and stealthy tool for network-based EDR evasion:

1. Instead of adding static WFP filters to EDR process executables, EDRPrison dynamically adds runtime WFP filters based on the packets' source process. 
2. Avoids obtaining a handle to EDR processes or EDR executables, reducing the risk of detection and interference with the EDR systems.
3. By loading a legitimate WFP callout driver, EDRPrison extends its capabilities while maintaining a benign profile. 

# Known Issues
1. Currently, EDRPrison is written in C#, requiring multiple files to be present on disk, which compromises stealth. I plan to reimplement it in C++ to allow the main program to be executed entirely in memory, enhancing its stealth capabilities.
2. There is a delay between the program's execution and the initial blocking of some EDR processes' network connections. This delay could permit telemetry to be sent to cloud servers within the first few seconds. I am working on resolving this issue to ensure immediate interception and blocking.


# Test Example
Due to the resources available to me, I have tested EDRPrison against Elastic Endpoint and Microsoft Defender for Endpoint (MDE) on my physical server so far.

Relevant processes for Elastic Endpoint and MDE are hardcoded in the source code. During the tests, neither the main program nor WinDivert was detected by the security systems.

I tested a few common malware samples, such as Mimikatz. These samples can still be detected because, even without internet connectivity, EDR systems retain basic detection capabilities such as hash-based signatures. After executing the malware, the number of packets increased, indicating that they contained alert data.

![image](/screenshot/edrprison.jpg)

While some detections occur locally, they do not appear on the EDR panel. Without internet connectivity, EDR systems are unable to leverage advanced capabilities like machine learning and cloud computing to prevent more sophisticated malware attacks.

![image](/screenshot/es.png)

![image](/screenshot/mde.png)

# Detections and Mitigations
The following approaches can be used to detect or mitigate the use of EDRPrison. However, depending on the environment, some of these detections could result in false positives (FP).

### Driver Load Event

If the WinDivert driver is not already installed on the system, EDRPrison will install the callout driver upon first execution. Both the OS and telemetry systems will log this event.

### Existence of WinDivert Files

EDRPrison and other programs dependent on WinDivert require the presence of WinDivert64.sys and WinDivert.dll on the disk. Monitoring for these files can help in detecting such programs.

### WinDivert Usage Detection Tools

Tools like [WinDivertTool](https://github.com/basil00/WinDivertTool) can detect processes that are currently utilizing the Windows Filtering Platform (WFP).

![image](/screenshot/windiverttool.jpg)

### Packet Drop/Block Actions Against EDR Processes

Elastic has a detection [rule](https://www.elastic.co/guide/en/security/current/potential-evasion-via-windows-filtering-platform.html) that can identify packet drop or block actions against security software processes, which can indicate the presence of EDRPrison.

### Review Registered WFP Providers, Filters, and Callouts

Tool [WFPExplorer](https://github.com/jdu2600/WFPExplorer) assists administrators in reviewing active WFP sessions, registered callouts, and filters. 

![image](/screenshot/session.png)

![image](/screenshot/callout.png)

![image](/screenshot/provider.png)

![image](/screenshot/filter.png)

### Adminless

A potential future feature could add additional protections for driver installation, further enhancing the security against unauthorized use of drivers like WinDivert.

# Red Team Strategies to Subvert Detections
From a red team perspective, several strategies can be employed to subvert the aforementioned detections, depending on the environment's security configurations.

### Seek An Alternative To WinDivert

If WinDivert is considered malicious in the environment, alternative signed, open-source drivers can be used. These alternatives should have fewer records of malicious use and still support packet interception, reinjection, and other manipulation techniques.

### Reuse An Installed Or Built-in WFP Callout Driver

In environments where external drivers are unauthorized unless approved, it is challenging but feasible to reverse-engineer an installed or built-in WFP callout driver. By reusing its callout functions, red teamers can leverage existing drivers. Many security software solutions include their own WFP callout drivers that can be repurposed.

### Change Action To Intercepted Packets

Instead of blocking or dropping intercepted packets, red teamers can redirect or proxy them. This method can avoid detection rules focused on packet drop or block actions, while still achieving the desired interference with EDR processes.

 


# Credit

The following resources inspired and helped me a lot during my research. I extend my thanks to all the authors:

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

https://github.com/microsoft/windows-driver-samples/tree/main/network/trans/WFPSampler 

https://github.com/TechnikEmpire/CitadelCore 

https://github.com/TechnikEmpire/HttpFilteringEngine 
