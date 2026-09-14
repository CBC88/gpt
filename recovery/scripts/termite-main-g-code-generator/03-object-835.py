"""Provides a scripting component.
    Inputs:
        x: The x script variable
        y: The y script variable
    Output:
        a: The a output variable"""

__author__ = "Julian Jauk"
__version__ = "2023.10.19"

import Rhino
import scriptcontext as sc
import rhinoscriptsyntax as rs

print sc.doc.ModelUnitSystem