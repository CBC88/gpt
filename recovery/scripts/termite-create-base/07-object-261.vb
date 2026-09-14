' Inputs:
'   A: Curve (InputCurve)
'   B: Integer (IterationCount)
'   C: Double (Distance)
'   D: Double (Tolerance)
' Outputs:
'   E: List of Curve (OffsetsList)
'   F: List of String (DebugLog)

Dim InputCurve As Curve = A
Dim IterationCount As Integer = B
Dim Distance As Double = C
Dim Tolerance As Double = D

Dim DebugLog As New System.Collections.Generic.List(Of String)()
Dim OffsetsList As New System.Collections.Generic.List(Of Curve)()
Dim CurrentCurves As New System.Collections.Generic.List(Of Curve)()
CurrentCurves.Add(InputCurve)

' --- Global sanity thresholds (R6 safety) ---
Dim bboxExplosionRatio As Double = 50.0   ' discard offsets whose bbox grows > 50x compared to parent
Dim minLengthFactor As Double = 0.25      ' keep curves >= 25% of median length
Dim cornerStyle As CurveOffsetCornerStyle = CurveOffsetCornerStyle.Sharp

' --- Inspect input curve ---
Dim inIsValid As Boolean = InputCurve.IsValid
Dim inIsClosed As Boolean = InputCurve.IsClosed
Dim inIsPlanar As Boolean = InputCurve.IsPlanar(Tolerance)
Dim inLen As Double = 0.0
Try : inLen = InputCurve.GetLength() : Catch : End Try
Dim inBbox As Rhino.Geometry.BoundingBox = InputCurve.GetBoundingBox(True)
Dim inDiag As Double = inBbox.Diagonal.Length

DebugLog.Add(String.Format("Input: Valid={0}, Closed={1}, Planar={2}, Len={3}, BBoxDiag={4}", inIsValid, inIsClosed, inIsPlanar, inLen, inDiag))
DebugLog.Add(String.Format("Params: Iterations={0}, Distance={1}, Tolerance={2}, CornerStyle={3}", IterationCount, Distance, Tolerance, cornerStyle))

' --- Determine a safe working plane (R6: stricter) ---
Dim crvPlane As Plane = Plane.WorldXY
If inIsPlanar Then
  Dim tmp As Plane = Plane.Unset
  If InputCurve.TryGetPlane(tmp, Tolerance) Then
    crvPlane = tmp
    DebugLog.Add(String.Format("Plane: From curve (Origin={0},{1},{2}  Normal={3},{4},{5})", crvPlane.OriginX, crvPlane.OriginY, crvPlane.OriginZ, crvPlane.Normal.X, crvPlane.Normal.Y, crvPlane.Normal.Z))
  Else
    DebugLog.Add("Plane: TryGetPlane failed → using WorldXY")
  End If
Else
  DebugLog.Add("Warning: Input not planar within tolerance → using WorldXY")
End If

' --- Determine offset sign based on area (guarded for R6) ---
Dim ActualDistance As Double = Distance
Try
  Dim testOffset As Curve() = InputCurve.Offset(crvPlane, Distance, Tolerance, cornerStyle)
  If testOffset Is Nothing OrElse testOffset.Length = 0 Then
    DebugLog.Add("SignCheck: testOffset returned nothing → keeping Distance sign as given")
  Else
    Dim ampOriginal = Rhino.Geometry.AreaMassProperties.Compute(InputCurve)
    Dim ampOffset = Rhino.Geometry.AreaMassProperties.Compute(testOffset(0))
    If ampOriginal Is Nothing OrElse ampOffset Is Nothing Then
      DebugLog.Add("SignCheck: AreaMassProperties = Nothing (R6 quirk) → keeping Distance sign as given")
    Else
      If ampOffset.Area > ampOriginal.Area Then
        ActualDistance = -Distance
        DebugLog.Add(String.Format("SignCheck: Offset area larger than original → using ActualDistance={0}", ActualDistance))
      Else
        DebugLog.Add(String.Format("SignCheck: Offset area smaller → using ActualDistance={0}", ActualDistance))
      End If
    End If
  End If
Catch ex As Exception
  DebugLog.Add("SignCheck: Exception during test offset → " & ex.Message & " → keeping Distance sign as given")
End Try

' --- Iterative offsets ---
For i As Integer = 0 To System.Math.Max(0, IterationCount - 1)
  DebugLog.Add(String.Format("--- Iteration {0} ---", i + 1))

  Dim NextIterationCurves As New System.Collections.Generic.List(Of Curve)()
  Dim Lengths As New System.Collections.Generic.List(Of Double)()

  Dim idx As Integer = 0
  For Each crv As Curve In CurrentCurves
    idx += 1

    ' Determine a plane per curve (safer for derived offsets)
    Dim thisPlane As Plane = crvPlane
    If crv.IsPlanar(Tolerance) Then
      Dim tmp As Plane = Plane.Unset
      If crv.TryGetPlane(tmp, Tolerance) Then thisPlane = tmp
    End If

    Dim parentBBox = crv.GetBoundingBox(True)
    Dim parentDiag As Double = parentBBox.Diagonal.Length
    Dim parentLen As Double = 0.0
    Try : parentLen = crv.GetLength() : Catch : End Try

    DebugLog.Add(String.Format("  [Curv {0}] Len={1}, BBoxDiag={2}, Planar={3}", idx, parentLen, parentDiag, crv.IsPlanar(Tolerance)))

    Dim OffsetCurve() As Curve = Nothing
    Try
      OffsetCurve = crv.Offset(thisPlane, ActualDistance, Tolerance, cornerStyle)
    Catch ex As Exception
      DebugLog.Add(String.Format("    Offset EXCEPTION: {0}", ex.Message))
      Continue For
    End Try

    If OffsetCurve Is Nothing OrElse OffsetCurve.Length = 0 Then
      DebugLog.Add("    Offset: returned Nothing or empty")
      Continue For
    End If

    DebugLog.Add(String.Format("    Offset: returned {0} fragment(s)", OffsetCurve.Length))

    ' Try to join fragments
    Dim joined As Curve() = Nothing
    Try
      joined = Rhino.Geometry.Curve.JoinCurves(OffsetCurve, Tolerance)
    Catch ex As Exception
      DebugLog.Add("    JoinCurves EXCEPTION: " & ex.Message)
    End Try
    If joined Is Nothing OrElse joined.Length = 0 Then
      joined = OffsetCurve
      DebugLog.Add("    JoinCurves: no joins → using raw fragments")
    Else
      DebugLog.Add(String.Format("    JoinCurves: joined to {0} curve(s)", joined.Length))
    End If

    Dim kept As Integer = 0
    For Each oc As Curve In joined
      Dim reasons As New System.Text.StringBuilder()

      If oc Is Nothing Then
        DebugLog.Add("      Discard: Null curve")
        Continue For
      End If

      If Not oc.IsValid Then reasons.Append("Invalid; ")
      If Not oc.IsClosed Then reasons.Append("Open; ")

      ' bbox explosion check (R6 runaway)
      Dim bb As Rhino.Geometry.BoundingBox = oc.GetBoundingBox(True)
      Dim diag As Double = bb.Diagonal.Length
      If parentDiag > Rhino.RhinoMath.ZeroTolerance Then
        If diag > parentDiag * bboxExplosionRatio Then reasons.Append("BBoxExplosion; ")
      End If

      Dim olen As Double = 0.0
      Try : olen = oc.GetLength() : Catch : End Try
      If olen < System.Math.Abs(Distance) Then reasons.Append("TooShort; ")

      If reasons.Length > 0 Then
        DebugLog.Add(String.Format("      Discard: {0} | Len={1}, BBoxDiag={2}", reasons.ToString(), olen, diag))
        Continue For
      End If

      NextIterationCurves.Add(oc)
      Lengths.Add(olen)
      kept += 1
    Next

    DebugLog.Add(String.Format("    Kept {0} curve(s) from this parent", kept))
  Next

  ' Length-based filtering (median)
  If NextIterationCurves.Count > 1 Then
    Lengths.Sort()
    Dim medianLength As Double = Lengths(Lengths.Count \ 2)
    Dim threshold As Double = medianLength * minLengthFactor
    Dim FilteredCurves As New System.Collections.Generic.List(Of Curve)()
    For Each crv As Curve In NextIterationCurves
      Dim L As Double = 0.0
      Try : L = crv.GetLength() : Catch : End Try
      If L >= threshold Then
        FilteredCurves.Add(crv)
      Else
        DebugLog.Add(String.Format("  Filter: removed small fragment (Len={0} < {1})", L, threshold))
      End If
    Next
    NextIterationCurves = FilteredCurves
  End If

  If NextIterationCurves.Count = 0 Then
    DebugLog.Add("No valid curves after filtering → stopping iterations")
    Exit For
  End If

  OffsetsList.AddRange(NextIterationCurves)
  DebugLog.Add(String.Format("Iteration {0}: produced {1} curve(s); cumulative={2}", i + 1, NextIterationCurves.Count, OffsetsList.Count))

  CurrentCurves = NextIterationCurves
Next

E = OffsetsList
F = DebugLog

