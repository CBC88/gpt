import Rhino
import scriptcontext as sc
import System

# ----------------------
# Settings
# ----------------------
PLANE_SIZE = 1.0
PLANE_X = 999999.0
PLANE_Y = 999999.0

# Sticky keys for persistent GUIDs
STICKY_UPPER = "TermiteUpperLimitGUID"
STICKY_LOWER = "TermiteLowerLimitGUID"

# Plane names
NAME_UPPER = "TermiteUpperLimit"
NAME_LOWER = "TermiteLowerLimit"


# ----------------------
# Helpers
# ----------------------
def get_existing_plane(sticky_key, name):
    """Return existing plane from sticky or fallback search by name"""
    doc = Rhino.RhinoDoc.ActiveDoc
    guid = sc.sticky.get(sticky_key, None)
    if guid:
        obj = doc.Objects.FindId(guid)
        if obj:
            return obj
    # fallback search by name
    for obj in doc.Objects:
        if isinstance(obj, Rhino.DocObjects.ClippingPlaneObject):
            if obj.Name == name:
                sc.sticky[sticky_key] = obj.Id
                return obj
    return None


def create_or_update_plane(z, sticky_key, name, flip=False):
    """Create or update a persistent clipping plane"""
    doc = Rhino.RhinoDoc.ActiveDoc
    existing = get_existing_plane(sticky_key, name)

    # define plane far away
    plane = Rhino.Geometry.Plane.WorldXY
    plane.OriginX = PLANE_X
    plane.OriginY = PLANE_Y
    plane.OriginZ = z
    if flip:
        plane.Flip()

    if existing:
        geo = existing.Geometry
        geo.Plane = plane
        existing.CommitChanges()
        cp_obj = existing
    else:
        view = doc.Views.ActiveView
        if not view:
            return
        obj_id = doc.Objects.AddClippingPlane(
            plane,
            PLANE_SIZE,
            PLANE_SIZE,
            view.ActiveViewportID
        )
        if obj_id == System.Guid.Empty:
            return
        cp_obj = doc.Objects.FindId(obj_id)
        attrs = cp_obj.Attributes
        attrs.Name = name
        cp_obj.Attributes = attrs
        cp_obj.CommitChanges()
        # enable clipping in all viewports
        for v in doc.Views:
            cp_obj.Geometry.AddClipViewportId(v.ActiveViewportID)
        cp_obj.CommitChanges()
        sc.sticky[sticky_key] = cp_obj.Id

    return cp_obj


def delete_plane(sticky_key):
    doc = Rhino.RhinoDoc.ActiveDoc
    existing = get_existing_plane(sticky_key, "")
    if existing:
        doc.Objects.Delete(existing, True)
        if sticky_key in sc.sticky:
            del sc.sticky[sticky_key]


# ----------------------
# MAIN
# ----------------------
old_doc = sc.doc
sc.doc = Rhino.RhinoDoc.ActiveDoc

try:
    if Enable:
        # Upper limit (flipped)
        create_or_update_plane(ZHeightUpperLimit, STICKY_UPPER, NAME_UPPER, flip=True)
        # Lower limit (normal)
        create_or_update_plane(ZHeightLowerLimit, STICKY_LOWER, NAME_LOWER, flip=False)
    else:
        delete_plane(STICKY_UPPER)
        delete_plane(STICKY_LOWER)
finally:
    sc.doc = old_doc

Rhino.RhinoDoc.ActiveDoc.Views.Redraw()
