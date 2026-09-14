import re
import Grasshopper.Kernel.Types as gkt

if Texts:
    Digits = []
    for t in Texts:
        dl = re.findall(r"[-\d.]+",t)
        d = float(dl[0])
        Digits.append(gkt.GH_Number(d))
else:
    Digits = []
