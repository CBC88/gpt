    ' --- INPUTS ---
    ' C : List of Curves (input circles)
    ' --- OUTPUT ---
    ' TrimmedCurves : List of non-overlapping closed curves

    If (C Is Nothing) Then Exit Sub
    If (C.Count = 0) Then Exit Sub

    Dim tol As Double = 0.001
    Dim changed As Boolean = True
    Dim maxPasses As Integer = 5

    ' --- Duplicate input curves ---
    Dim curves As New System.Collections.Generic.List(Of Rhino.Geometry.Curve)
    Dim i As Integer
    For i = 0 To C.Count - 1
      If (Not C(i) Is Nothing) Then
        curves.Add(C(i).DuplicateCurve())
      End If
    Next

    ' --- Iterative trimming loop ---
    Dim pass As Integer = 0
    While changed And pass < maxPasses
      changed = False
      pass = pass + 1

      Dim newCurves As New System.Collections.Generic.List(Of Rhino.Geometry.Curve)

      For i = 0 To curves.Count - 1
        Dim baseCrv As Rhino.Geometry.Curve = curves(i)
        If (baseCrv Is Nothing) Then Continue For

        Dim allTrimParams As New System.Collections.Generic.List(Of Double)
        Dim closureLines As New System.Collections.Generic.List(Of Rhino.Geometry.Curve)

        ' --- find intersections per other curve ---
        Dim j As Integer
        For j = 0 To curves.Count - 1
          If (i = j) Then Continue For
          Dim otherCrv As Rhino.Geometry.Curve = curves(j)
          If (otherCrv Is Nothing) Then Continue For

          Dim inters As Rhino.Geometry.Intersect.CurveIntersections = _
            Rhino.Geometry.Intersect.Intersection.CurveCurve(baseCrv, otherCrv, tol, tol)

          If (Not inters Is Nothing) AndAlso inters.Count > 0 Then
            Dim pts As New System.Collections.Generic.List(Of Rhino.Geometry.Point3d)
            Dim pars As New System.Collections.Generic.List(Of Double)

            Dim k As Integer
            For k = 0 To inters.Count - 1
              pars.Add(inters(k).ParameterA)
              pts.Add(inters(k).PointA)
            Next

            If (pars.Count = 2) Then
              ' Sort parameters and matching points
              If (pars(0) > pars(1)) Then
                Dim tmpPar As Double = pars(0)
                pars(0) = pars(1)
                pars(1) = tmpPar
                Dim tmpPt As Rhino.Geometry.Point3d = pts(0)
                pts(0) = pts(1)
                pts(1) = tmpPt
              End If
              allTrimParams.AddRange(pars)
              ' store closure line for THIS pair only
              closureLines.Add(New Rhino.Geometry.LineCurve(pts(0), pts(1)))
            End If
          End If
        Next

        ' --- no intersections ---
        If allTrimParams.Count = 0 Then
          newCurves.Add(baseCrv)
          Continue For
        End If

        allTrimParams.Sort()
        Dim segments() As Rhino.Geometry.Curve = baseCrv.Split(allTrimParams)
        If (segments Is Nothing) Then
          newCurves.Add(baseCrv)
          Continue For
        End If

        ' --- keep only segments outside of other curves ---
        Dim kept As New System.Collections.Generic.List(Of Rhino.Geometry.Curve)
        Dim s As Rhino.Geometry.Curve
        For Each s In segments
          If (s Is Nothing) Then Continue For
          Dim mp As Rhino.Geometry.Point3d = s.PointAtNormalizedLength(0.5)
          Dim insideAny As Boolean = False
          For j = 0 To curves.Count - 1
            If (i = j) Then Continue For
            Dim testCrv As Rhino.Geometry.Curve = curves(j)
            Dim contain As Rhino.Geometry.PointContainment = _
              testCrv.Contains(mp, Rhino.Geometry.Plane.WorldXY, tol)
            If (contain = Rhino.Geometry.PointContainment.Inside) Then
              insideAny = True
              Exit For
            End If
          Next
          If (Not insideAny) Then kept.Add(s)
        Next

        ' --- combine kept arcs + closure lines ---
        Dim allSegs As New System.Collections.Generic.List(Of Rhino.Geometry.Curve)
        Dim seg As Rhino.Geometry.Curve
        For Each seg In kept
          allSegs.Add(seg)
        Next

        Dim ln As Rhino.Geometry.Curve
        For Each ln In closureLines
          allSegs.Add(ln)
        Next

        ' --- join and close ---
        Dim joined() As Rhino.Geometry.Curve = Rhino.Geometry.Curve.JoinCurves(allSegs, tol)
        If (Not joined Is Nothing) AndAlso joined.Length > 0 Then
          Dim cr As Rhino.Geometry.Curve = joined(0)
          If (Not cr.IsClosed) Then cr.MakeClosed(tol)
          newCurves.Add(cr)
        Else
          newCurves.Add(baseCrv)
        End If
      Next

      ' --- detect changes ---
      If (newCurves.Count = curves.Count) Then
        Dim diffCount As Integer = 0
        For i = 0 To curves.Count - 1
          If (System.Math.Abs(curves(i).GetLength() - newCurves(i).GetLength()) > tol) Then
            diffCount = diffCount + 1
          End If
        Next
        changed = (diffCount > 0)
      Else
        changed = True
      End If

      curves = newCurves
    End While

    TrimmedCurves = curves
