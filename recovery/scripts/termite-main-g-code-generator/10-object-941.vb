

' Inputs:
'   trigger (Boolean)
'   defaultName (String)
'   fileContent (List of String)
' Output:
'   A (Boolean)

Static wasTriggered As Boolean = False
Static result As Boolean = False
Static resultReported As Boolean = False

A = False

If trigger And Not wasTriggered Then
  wasTriggered = True
  result = False
  resultReported = False

  Dim t As New System.Threading.Thread(Sub()
    Try
      Dim sfd As New System.Windows.Forms.SaveFileDialog()
      sfd.Filter = "G-Code files (*.gcode)|*.gcode"
      sfd.FileName = defaultName & ".gcode"
      sfd.Title = "Save G-Code File As"

      If sfd.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
        System.IO.File.WriteAllLines(sfd.FileName, fileContent.ToArray())
        result = True
      End If
    Catch ex As Exception
      result = False
    End Try
  End Sub)

  t.SetApartmentState(System.Threading.ApartmentState.STA)
  t.Start()
  t.Join()
End If

' Reset trigger state when input is released
If Not trigger Then
  wasTriggered = False
End If

' Output True only once
If result And Not resultReported Then
  A = True
  resultReported = True
Else
  A = False
End If


