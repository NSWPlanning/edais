:: "\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\SvcUtil.exe" /n:"*, eDAIS.CreateApplication.Initiator" CreateApplication_Initiator.2.3.0r2.wsdl ..\biv\*.xsd /mc /serializer:DataContractSerializer /importXmlTypes /tcv:Version35 /s

:: "\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\SvcUtil.exe" /n:"*, eDAIS.CreateApplication.Responder" CreateApplication_Responder.2.3.0r2.wsdl ..\biv\*.xsd /mc /serializer:DataContractSerializer /importXmlTypes /tcv:Version35 /s

:: "\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\SvcUtil.exe" /n:"*, eDAIS.DeclareDetermination.Responder" DeclareDetermination_Responder.2.3.0r2.wsdl ..\biv\*.xsd /mc /serializer:DataContractSerializer /importXmlTypes /tcv:Version35 /s

"\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\SvcUtil.exe" /n:"*, eDAIS" CreateApplication_Initiator.2.3.0r2.wsdl CreateApplication_Responder.2.3.0r2.wsdl DeclareDetermination_Responder.2.3.0r2.wsdl ..\biv\*.xsd /mc /serializer:Auto /importXmlTypes /tcv:Version35 /s /out:eDAIS.cs /synconly

