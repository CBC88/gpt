import Rhino
import rhinoscriptsyntax as rs
import scriptcontext as sc
from System.Windows.Forms import MessageBox, MessageBoxButtons, DialogResult

# -------------------------------------------------------------------
# INITIALIZE STICKY STORAGE
# -------------------------------------------------------------------
if "SLING_SURFACES" not in sc.sticky:
    sc.sticky["SLING_SURFACES"] = []

# stored signature of last input geometry (to detect changes)
if "SLING_GEOM_SIGNATURE" not in sc.sticky:
    sc.sticky["SLING_GEOM_SIGNATURE"] = None


# -------------------------------------------------------------------
# HELPERS
# -------------------------------------------------------------------
def geometry_signature(geom):
    """Create a simple signature that changes when geometry changes."""
    if geom is None:
        return None

    if hasattr(geom, "GeometryId"):  # Rhino 7/8 Breps have this
        try:
            return str(geom.GeometryId)
        except:
            pass

    # Fallback: bounding box + type + face count
    try:
        bbox = geom.GetBoundingBox(True)
        return (
            str(type(geom)),
            str(bbox.Min),
            str(bbox.Max),
            getattr(geom, "Faces", []).Count if hasattr(geom, "Faces") else None
        )
    except:
        return str(type(geom))


def pick_surfaces_from_view():
    """Pick sub-surfaces interactively in Rhino."""
    ghdoc = sc.doc
    sc.doc = Rhino.RhinoDoc.ActiveDoc

    go = Rhino.Input.Custom.GetObject()
    go.SetCommandPrompt("Select surface(s) from polysurface. Press Enter when done.")
    go.SubObjectSelect = True
    go.GeometryFilter = Rhino.DocObjects.ObjectType.Surface
    go.EnablePreSelect(False, True)

    rc = go.GetMultiple(1, 0)
    selected = []

    if rc == Rhino.Input.GetResult.Object:
        for i in range(go.ObjectCount):
            objref = go.Object(i)
            face = objref.Face()
            if face:
                brep = face.DuplicateFace(True)
                selected.append(brep)

    sc.doc = ghdoc
    return selected


# -------------------------------------------------------------------
# NEW HELPER → GET REMAINING SURFACES
# -------------------------------------------------------------------
def get_unselected_surfaces(geometry, selected_surfaces):
    """Return all faces of geometry except the ones in selected_surfaces."""
    if geometry is None or not isinstance(geometry, Rhino.Geometry.Brep):
        return []

    # Create list of all surfaces
    all_faces = [f.DuplicateFace(True) for f in geometry.Faces]

    # Compare geometries via bounding boxes (simple and robust)
    def bbox_sig(brep):
        bb = brep.GetBoundingBox(True)
        return (bb.Min, bb.Max)

    selected_sigs = {bbox_sig(s) for s in selected_surfaces}

    remaining = [f for f in all_faces if bbox_sig(f) not in selected_sigs]
    return remaining


# -------------------------------------------------------------------
# MAIN LOGIC
# -------------------------------------------------------------------
def main(trigger, geometry):

    # Compute geometry signature
    new_sig = geometry_signature(geometry)
    old_sig = sc.sticky["SLING_GEOM_SIGNATURE"]

    # ---------------------------------------------------------------
    # IF GEOMETRY HAS CHANGED → clear stored surfaces
    # ---------------------------------------------------------------
    if new_sig != old_sig:
        sc.sticky["SLING_SURFACES"] = []
        sc.sticky["SLING_GEOM_SIGNATURE"] = new_sig

    # No trigger → just output stored surfaces (and remaining)
    if not trigger:
        selected = sc.sticky["SLING_SURFACES"]
        remaining = get_unselected_surfaces(geometry, selected)
        return selected, remaining

    # ---------------------------------------------------------------
    # CASE 1 — no geometry
    # ---------------------------------------------------------------
    if geometry is None:
        selected = sc.sticky["SLING_SURFACES"]
        remaining = []
        return selected, remaining

    # ---------------------------------------------------------------
    # CASE 2 — true single surface
    # ---------------------------------------------------------------
    if isinstance(geometry, Rhino.Geometry.Surface):
        MessageBox.Show(
            "Termite detected only one surface. Divide your surface into smaller parts, "
            "join them, re-reference the polysurface, and try again.",
            "Termite Create Sling Contours No Polysurface Detected",
            MessageBoxButtons.OK
        )
        selected = sc.sticky["SLING_SURFACES"]
        remaining = []
        return selected, remaining

    # ---------------------------------------------------------------
    # CASE 3 — polysurface
    # ---------------------------------------------------------------
    if isinstance(geometry, Rhino.Geometry.Brep):

        # Polysurface but only 1 face → still a single surface
        if geometry.Faces.Count == 1:
            MessageBox.Show(
                "Termite detected only one surface. Divide your surface into smaller parts, "
                "join them, re-reference the polysurface, and try again.",
                "Termite Create Sling Contours No Polysurface Detected",
                MessageBoxButtons.OK
            )
            selected = sc.sticky["SLING_SURFACES"]
            remaining = []
            return selected, remaining

        # Prompt user
        result = MessageBox.Show(
            "Manually select sub-surface(s) in Rhino viewport for creating slings.",
            "Termite Create Sling Contours Select Sub-surface(s)",
            MessageBoxButtons.OKCancel
        )

        if result == DialogResult.Cancel:
            selected = sc.sticky["SLING_SURFACES"]
            remaining = get_unselected_surfaces(geometry, selected)
            return selected, remaining

        picked = pick_surfaces_from_view()
        sc.sticky["SLING_SURFACES"] = picked
        remaining = get_unselected_surfaces(geometry, picked)
        return picked, remaining

    # default fallback
    selected = sc.sticky["SLING_SURFACES"]
    remaining = get_unselected_surfaces(geometry, selected)
    return selected, remaining


# -------------------------------------------------------------------
# RUN
# -------------------------------------------------------------------
selected, remaining = main(trigger, geometry)
