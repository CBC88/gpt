// Falls keine Daten da sind, abbrechen
    if (P == null || P.Count == 0) return;

    // Ein einziges Sammel-Mesh erstellen
    Mesh finalMesh = new Mesh();

    foreach (Polyline pl in P)
    {
        if (pl == null || pl.Count < 2) continue;

        // Erzeuge das Teil-Mesh für diesen Pfad
        Mesh pathMesh = CreateExtrudedPath(pl, W, H);

        // Füge es dem Haupt-Mesh hinzu
        if (pathMesh != null)
        {
            finalMesh.Append(pathMesh);
        }
    }

    // Alles auf einmal ausgeben
    A = finalMesh;