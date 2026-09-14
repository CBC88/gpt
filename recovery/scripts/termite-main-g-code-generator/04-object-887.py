import Rhino.Geometry as rg

def compute_normals(mesh):
    # Check if the input is a valid mesh
    if not mesh.IsValid:
        return None  # Return None if the mesh is invalid
    
    # Compute the vertex normals
    mesh.Normals.ComputeNormals()
    
    # Compute the face normals
    mesh.FaceNormals.ComputeFaceNormals()
    
    return mesh

# Compute the normals for the input mesh
computed_mesh = compute_normals(mesh)

# Output the result
a = computed_mesh
