
__author__ = "Julian Jauk"
__version__ = "2023.01.27"

import rhinoscriptsyntax as rs
import Rhino as Rhino

if rs.IsCurve (x):
    print "Curve"
    a = "True"
    
else:
    print "Not a Curve"
    a = "False"
    pass
    
