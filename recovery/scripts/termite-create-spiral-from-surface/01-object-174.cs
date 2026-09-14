
    List<Curve> ContoursList = A; // DECLARE INPUT
    Point3d StartPoint = B; // DECLARE INPUT
    double CurveParameter; // DECLARE VARIABLE

    List<Point3d> SeamPointsList = new List<Point3d>(); // CREATE EMPTY LIST
    int ContoursCount = 0 + (ContoursList.Count); // SET ITERATION ACCORDING TO INPUT

    ContoursCount = (ContoursList.Count);

    Point3d NextPoint = StartPoint;

    foreach (Curve Contour in ContoursList){
      if (Contour.ClosestPoint(NextPoint, out CurveParameter)){
        SeamPointsList.Add(NextPoint);
        NextPoint = Contour.PointAt(CurveParameter);
      }
    }
    SeamPoints = SeamPointsList; // DECLARE OUTPUT
