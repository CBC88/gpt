import rhinoscriptsyntax
import Rhino
firstPt = allPts[startIndex]
del allPts[startIndex]

visitedPts = [firstPt]
def TravelingSalesMan(fromPt,i):
    nextIndex = Rhino.Collections.Point3dList.ClosestIndexInList(allPts,fromPt)
    nextPt = allPts[nextIndex]
    visitedPts.append(nextPt)
    del allPts[nextIndex]
    if(i>0):
        TravelingSalesMan(nextPt,i-1)
TravelingSalesMan(firstPt,iterations)
a = visitedPts
