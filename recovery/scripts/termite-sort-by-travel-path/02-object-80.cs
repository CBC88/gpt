if (P == null || P.Count == 0)
{
  A = null;
  T = null;
  L = 0;
  O = null;
  return;
}

// ------------------------------------------------------------
// SAFE DISTANCE
// ------------------------------------------------------------
double Dist(Point3d a, Point3d b)
{
  return a.DistanceTo(b);
}

S = Math.Max(2, S);
I = Math.Max(1, I);

// ------------------------------------------------------------
// PREP
// ------------------------------------------------------------
List<Curve> curves = new List<Curve>();
List<Point3d> pts = new List<Point3d>();

foreach (Curve c in P)
{
  if (c == null) continue;

  curves.Add(c.DuplicateCurve());
  pts.Add(c.PointAtNormalizedLength(0.5));
}

int n = curves.Count;

if (n == 0)
{
  A = null;
  T = null;
  L = 0;
  O = null;
  return;
}

// ------------------------------------------------------------
// BOUNDING BOX
// ------------------------------------------------------------
BoundingBox bb = BoundingBox.Empty;

for (int i = 0; i < pts.Count; i++)
  bb.Union(pts[i]);

double dx = Math.Max(1e-6, (bb.Max.X - bb.Min.X) / S);
double dy = Math.Max(1e-6, (bb.Max.Y - bb.Min.Y) / S);

// ------------------------------------------------------------
// CLUSTERING
// ------------------------------------------------------------
Dictionary<int, List<int>> clusters = new Dictionary<int, List<int>>();

for (int i = 0; i < n; i++)
{
  int gx = (int)((pts[i].X - bb.Min.X) / dx);
  int gy = (int)((pts[i].Y - bb.Min.Y) / dy);

  int key = gx * 73856093 ^ gy * 19349663;

  if (!clusters.ContainsKey(key))
    clusters[key] = new List<int>();

  clusters[key].Add(i);
}

Random rnd = new Random(1);

// ------------------------------------------------------------
// BEST RESULT
// ------------------------------------------------------------
List<Curve> bestOrder = null;
List<Line> bestTravels = null;
List<int> bestIndexOrder = null;
double bestCost = double.MaxValue;

List<int> clusterKeysBase = new List<int>(clusters.Keys);

// ------------------------------------------------------------
// MAIN LOOP
// ------------------------------------------------------------
for (int it = 0; it < I; it++)
{
  List<int> clusterKeys = new List<int>(clusterKeysBase);

  for (int i = 0; i < clusterKeys.Count; i++)
  {
    int j = rnd.Next(i, clusterKeys.Count);
    int tmp = clusterKeys[i];
    clusterKeys[i] = clusterKeys[j];
    clusterKeys[j] = tmp;
  }

  List<Curve> order = new List<Curve>();
  List<int> orderIdx = new List<int>();

  Point3d? currentEnd = null;

  // --------------------------------------------------------
  // BUILD
  // --------------------------------------------------------
  foreach (int key in clusterKeys)
  {
    List<int> cluster = new List<int>(clusters[key]);

    while (cluster.Count > 0)
    {
      double best = double.MaxValue;
      int bestIdx = 0;
      bool flip = false;

      for (int i = 0; i < cluster.Count; i++)
      {
        int id = cluster[i];
        Curve c = curves[id];

        double d1 = currentEnd == null ? 0 : Dist(currentEnd.Value, c.PointAtStart);
        double d2 = currentEnd == null ? 0 : Dist(currentEnd.Value, c.PointAtEnd);

        if (currentEnd == null)
        {
          bestIdx = i;
          flip = false;
          break;
        }

        if (d1 < best)
        {
          best = d1;
          bestIdx = i;
          flip = false;
        }

        if (d2 < best)
        {
          best = d2;
          bestIdx = i;
          flip = true;
        }
      }

      int chosen = cluster[bestIdx];
      Curve nc = curves[chosen].DuplicateCurve();

      // ----------------------------------------------------
      // FLIP CONTROL
      // ----------------------------------------------------
      bool doFlip = F && flip;

      if (doFlip)
        nc.Reverse();

      order.Add(nc);
      orderIdx.Add(chosen);

      currentEnd = nc.PointAtEnd;

      cluster.RemoveAt(bestIdx);
    }
  }

  // --------------------------------------------------------
  // 2-OPT
  // --------------------------------------------------------
  bool improved = true;
  int guard = 0;

  while (improved && guard < 20)
  {
    improved = false;
    guard++;

    for (int i = 0; i < order.Count - 1; i++)
    {
      for (int j = i + 1; j < order.Count; j++)
      {
        Curve aPrev = (i > 0) ? order[i - 1] : null;
        Curve a = order[i];
        Curve b = order[j];
        Curve bNext = (j < order.Count - 1) ? order[j + 1] : null;

        double before =
          (aPrev != null ? Dist(aPrev.PointAtEnd, a.PointAtStart) : 0) +
          Dist(a.PointAtEnd, b.PointAtStart) +
          (bNext != null ? Dist(b.PointAtEnd, bNext.PointAtStart) : 0);

        double after =
          (aPrev != null ? Dist(aPrev.PointAtEnd, b.PointAtEnd) : 0) +
          Dist(b.PointAtStart, a.PointAtEnd) +
          (bNext != null ? Dist(a.PointAtStart, bNext.PointAtStart) : 0);

        if (after < before)
        {
          order.Reverse(i, j - i + 1);

          // keep index sync
          orderIdx.Reverse(i, j - i + 1);

          improved = true;
        }
      }
    }
  }

  // --------------------------------------------------------
  // COST
  // --------------------------------------------------------
  double cost = 0;
  List<Line> travels = new List<Line>();

  for (int i = 0; i < order.Count - 1; i++)
  {
    Line l = new Line(order[i].PointAtEnd, order[i + 1].PointAtStart);
    travels.Add(l);
    cost += l.Length;
  }

  // --------------------------------------------------------
  // STORE BEST
  // --------------------------------------------------------
  if (cost < bestCost)
  {
    bestCost = cost;
    bestOrder = order;
    bestTravels = travels;
    bestIndexOrder = new List<int>(orderIdx);
  }
}

// ------------------------------------------------------------
// OUTPUT
// ------------------------------------------------------------
A = bestOrder;
T = bestTravels;
L = bestCost;
O = bestIndexOrder;