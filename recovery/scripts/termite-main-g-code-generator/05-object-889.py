import Rhino.Geometry as rg

def create_mesh_pipe(curve, radius):
    # Ensure the curve is valid
    if curve is None or not curve.IsValid:
        return None  # Return None if the curve is invalid
    
    # Create a pipe mesh around the curve using CreateFromCurvePipe method
    # The radius is the pipe thickness
    # We don't need to specify any cap or join behavior, as it's handled by the method
    
    pipe_mesh = rg.Mesh.CreateFromCurvePipe(curve, radius, segments, accuracy, rg.MeshPipeCapStyle.Dome, False)  # 12 segments for smoothness
    
    # Return the generated pipe mesh
    return pipe_mesh

# Ensure inputs are provided and valid
if curve is None or radius is None:
    a = None  # If inputs are missing, return None
else:
    # Compute the piped mesh
    piped_mesh = create_mesh_pipe(curve, radius)
    # Output the resulting piped mesh
    a = piped_mesh
