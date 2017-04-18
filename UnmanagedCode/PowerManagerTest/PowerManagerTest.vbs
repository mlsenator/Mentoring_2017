Dim powerManager
Set powerManager = CreateObject("PowerManagerCOM.PowerManagerCom")
Dim result 
result = ""
result = result & "Last Sleep Time: " & powerManager.GetLastSleepTime() & vbCrLf
result = result & "Last Wake Time: " & powerManager.GetLastWakeTime() & vbCrLf
result = result & "Battery Present: " & powerManager.GetSystemBatteryState() & vbCrLf
result = result & "Current system cooling mode: " & powerManager.GetSystemCoolingMode() & vbCrLf & "  **(0 - active, 1 - passive, 2- invalid)"
MsgBox result