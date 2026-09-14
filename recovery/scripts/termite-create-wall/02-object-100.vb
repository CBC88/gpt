
' Inputs:
'   A: Curve (InputCurve)
'   B: Integer (IterationCount)
'   C: Double (Distance)
'   D: Double (Tolerance)
' Output:
'   Offsets: List of Curve

Dim InputCurve As Curve = A
Dim IterationCount As Integer = B
Dim Distance As Double = C
Dim Tolerance As Double = D

' Handle zero offset case: output input directly
If Distance = 0 Then
  Offsets = New List(Of Curve) From {InputCurve}
  Return
End If

Dim origin As New Point3d(0, 0, 0)
Dim vectxy As New Vector3d(0, 0, 1)
Dim xy As New Plane(origin, vectxy)

Dim BooleanSwitch As Boolean = True
Dim PreviousLength As Double = 999999
Dim ActualDistance As Double = Distance
Dim PreviousCurve As Curve = InputCurve

Dim OffsetsList As New List(Of Curve)()
Dim NextCurvesList As New List(Of Curve) From {InputCurve}
Dim NextTestCurvesList As New List(Of Curve) From {InputCurve}

' --- Offset direction based on area comparison ---
Dim OffsetTestCurve() As Curve = NextTestCurvesList(0).Offset(xy, Distance, Tolerance, CurveOffsetCornerStyle.Sharp)
If OffsetTestCurve IsNot Nothing AndAlso OffsetTestCurve.Length > 0 Then
  Dim areaOriginal As Double = 0.0
  Dim areaOffset As Double = 0.0

  Dim ampOriginal = Rhino.Geometry.AreaMassProperties.Compute(InputCurve)
  Dim ampOffset = Rhino.Geometry.AreaMassProperties.Compute(OffsetTestCurve(0))

  If ampOriginal IsNot Nothing Then areaOriginal = ampOriginal.Area
  If ampOffset IsNot Nothing Then areaOffset = ampOffset.Area

  ' Compare areas to detect direction
  Dim wentOutward As Boolean = (areaOffset > areaOriginal)

  ' --- New logic ---
  ' If user gave negative distance → we want INSIDE (smaller area)
  ' If user gave positive distance → we want OUTSIDE (larger area)
  ' If current offset does not match that intention, reverse it.
  If (Distance < 0 AndAlso wentOutward) Or (Distance > 0 AndAlso Not wentOutward) Then
    ActualDistance = Distance * -1
  End If
End If
' --------------------------------------------------

For i As Integer = 0 To IterationCount - 1
  If BooleanSwitch Then
    Dim OffsetCurve() As Curve = NextCurvesList(i).Offset(xy, ActualDistance, Tolerance, CurveOffsetCornerStyle.Sharp)

    If OffsetCurve IsNot Nothing AndAlso OffsetCurve.Length = 1 Then
      Dim candidate As Curve = OffsetCurve(0)

      If InputCurve.IsCircle(0.01) Then
        If candidate.GetLength() < PreviousLength Then
          PreviousLength = candidate.GetLength()
          PreviousCurve = candidate
          NextCurvesList.Add(candidate)
          OffsetsList.Add(candidate)
        Else
          BooleanSwitch = False
        End If
      Else
        Dim EventList = Rhino.Geometry.Intersect.Intersection.CurveCurve(candidate, PreviousCurve, 0.1, 0.1)
        If candidate.GetLength() < PreviousLength AndAlso EventList.Count = 0 Then
          PreviousLength = candidate.GetLength()
          PreviousCurve = candidate
          NextCurvesList.Add(candidate)
          OffsetsList.Add(candidate)
        Else
          BooleanSwitch = False
        End If
      End If
    Else
      BooleanSwitch = False
    End If
  End If
Next

Offsets = OffsetsList ' Output result


