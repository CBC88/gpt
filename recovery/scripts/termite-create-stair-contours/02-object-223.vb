Dim ContoursList As List(Of Curve) = x
Dim StartPoint As Point3d = y
Dim CurveParameter As Double

Dim SeamPointsList As New List(Of Point3d)()
Dim NextPoint As Point3d = StartPoint

For Each Contour As Curve In ContoursList
    If Contour IsNot Nothing Then
        If Contour.ClosestPoint(NextPoint, CurveParameter) Then
            SeamPointsList.Add(NextPoint)
            NextPoint = Contour.PointAt(CurveParameter)
        End If
    End If
Next

A = SeamPointsList
