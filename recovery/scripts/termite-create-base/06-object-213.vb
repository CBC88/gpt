' Inputs:
'   C (Curve)
' Outputs:
'   A (Boolean)
'   B (String)

A = False
B = ""

If C Is Nothing Then
    B = "No curve provided."
    Return
End If

If Not C.IsClosed Then
    B = "Curve is not closed."
    Return
End If

' Sample points along curve
Dim pts() As Rhino.Geometry.Point3d = Nothing
C.DivideByCount(10, True, pts)

If pts Is Nothing OrElse pts.Length < 3 Then
    B = "Could not sample points from curve."
    Return
End If

' Check if all Z values are approximately equal
Dim firstZ As Double = pts(0).Z
Dim maxDeviation As Double = 0.0

For Each pt As Rhino.Geometry.Point3d In pts
    Dim deviation As Double = Math.Abs(pt.Z - firstZ)
    If deviation > maxDeviation Then maxDeviation = deviation
Next

Dim tolerance As Double = 0.01 ' Acceptable Z variation
If maxDeviation <= tolerance Then
    A = True
    B = "Curve is closed, planar (Z deviation: " & maxDeviation.ToString("0.000") & ")."
Else
    B = "Curve is not planar in Z (max deviation: " & maxDeviation.ToString("0.000") & ")."
End If


