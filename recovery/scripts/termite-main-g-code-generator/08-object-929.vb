    ' Declare output variable E
    Dim rhinoVersion As String

    ' Get the current Rhino version using the Version property
    rhinoVersion = Rhino.RhinoApp.Version.ToString()

    ' Output the version string
    A = rhinoVersion

