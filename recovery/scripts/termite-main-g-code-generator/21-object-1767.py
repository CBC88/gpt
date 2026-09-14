import Rhino.Geometry as rg
import Rhino
import ghpythonlib.parallel as parallel

# ---------------------------------------------------------
# Inputs from Grasshopper:
# curve    : List Access (Curve)
# radius   : float
# sides    : int
# scale    : float (Z-axis compression factor)
# accuracy : float
# ---------------------------------------------------------

s_val = sides if 'sides' in globals() else 8
sc_val = scale if 'scale' in globals() else 0.5
r_val = radius if 'radius' in globals() else 1.0
acc_val = accuracy if 'accuracy' in globals() else 0.1

def process_curve_optimized(crv):
    if not crv or not crv.IsValid: 
        return None
    
    # 1. Create the mesh pipe
    mesh = rg.Mesh.CreateFromCurvePipe(
        crv, r_val, s_val, acc_val, 
        rg.MeshPipeCapStyle.Dome, False
    )
    
    if not mesh: return None

    # 2. Extract Curve Parameters
    # Instead of ClosestPoint (Search), we use DivideByCount 
    # This matches the way MeshPipe distributes its segments.
    # We need to know how many 'rings' the mesh has.
    # MeshPipe creates (sides) vertices per division.
    
    verts = mesh.Vertices
    vert_count = verts.Count
    
    # We divide the curve by the number of segments used in the mesh
    # MeshPipe logic: segments = (vert_count / sides)
    num_segments = int(vert_count / s_val)
    
    # Get parameters along the curve once
    params = crv.DivideByCount(num_segments - 1, True)
    if not params: return mesh
    
    # 3. Apply Local Z-Scaling
    # We iterate through the vertices. Since they are ordered by ring,
    # we only call PointAt once per ring.
    for i in range(vert_count):
        # Determine which "ring" this vertex belongs to
        ring_index = i // s_val 
        if ring_index >= len(params): ring_index = len(params) - 1
        
        # Get the center point of this specific segment of the curve
        center_pt = crv.PointAt(params[ring_index])
        v = verts[i]
        
        # Scale the Z relative ONLY to the local center point
        # This keeps the bead "stuck" to the path regardless of height
        new_z = center_pt.Z + (v.Z - center_pt.Z) * sc_val
        mesh.Vertices.SetVertex(i, v.X, v.Y, new_z)

    mesh.Compact()
    return mesh

if __name__ == "__main__":
    input_curves = curve if isinstance(curve, list) else [curve]
    results = parallel.run(process_curve_optimized, input_curves, True)
    a = [m for m in results if m is not None]