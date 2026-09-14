    ' Inputs:
    '   (none)
    ' Output:
    '   A (Integer)

    If System.Environment.OSVersion.Platform = PlatformID.MacOSX Or _
      System.Environment.OSVersion.Platform = PlatformID.Unix Then
      ' Likely macOS (Unix-based)
      A = 0
    Else
      ' Default to Windows
      A = 1
    End If
