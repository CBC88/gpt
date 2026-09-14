const C = require("./core.js");
function runCoreTests(C){
 const results=[];const test=(name,f)=>{try{f();results.push({name,passed:true});}catch(e){results.push({name,passed:false,error:e.message});}};
 const eq=(a,b)=>{if(JSON.stringify(a)!==JSON.stringify(b))throw Error(JSON.stringify(a)+" !== "+JSON.stringify(b));};
 const near=(a,b,e=1e-8)=>{if(Math.abs(a-b)>e)throw Error(a+" != "+b);};
 const throws=f=>{let yes=false;try{f();}catch(e){yes=true;}if(!yes)throw Error("Expected rejection");};
 const line=(id,a,b)=>({id,layer:"Default",points:[a,b]});
 test("3D length and arc-length interpolation",()=>{const p=line("a",[0,0,0],[3,4,12]);near(C.length(p),13);eq(C.pointAt(p,6.5),[1.5,2,6]);});
 test("Repeated vertices do not break interpolation",()=>{eq(C.pointAt({points:[[0,0,0],[0,0,0],[2,0,0]]},1),[1,0,0]);});
 test("Single-point path interpolation",()=>eq(C.pointAt({points:[[3,2,1]]},7),[3,2,1]));
 test("Reject invalid coordinate data",()=>{throws(()=>C.validate([{points:[[NaN,0,0]]}]));throws(()=>C.validate([{points:[["1",0,0]]}]));throws(()=>C.validate([]));});
 test("2D input receives zero Z",()=>eq(C.validate([[[1,2],[3,4]]])[0].points,[[1,2,0],[3,4,0]]));
 test("Length sort retains original paths and IDs",()=>{const p=[line("long",[0,0,0],[8,0,0]),line("short",[0,0,0],[2,0,0])],snapshot=JSON.stringify(p);eq(C.sortLength(p).map(p=>p.id),["short","long"]);eq(JSON.stringify(p),snapshot);eq(C.sortLength(p,true).map(p=>p.id),["long","short"]);});
 test("Minimum-length boundary is strict",()=>{const p=[line("equal",[0,0,0],[5,0,0]),line("long",[0,0,0],[6,0,0])];eq(C.filterLength(p,5).map(x=>x.id),["long"]);throws(()=>C.filterLength(p,-1));});
 test("Travel length connects ends to starts",()=>near(C.travelLength([line("a",[0,0,0],[2,0,0]),line("b",[5,4,0],[8,4,0])]),5));
 test("Closed seam shift preserves length and closure",()=>{const p={id:"square",points:[[0,0,0],[10,0,0],[10,10,0],[0,10,0],[0,0,0]]};for(const fraction of [.125,.25,.7,1]){const q=C.shiftSeams([p],fraction)[0];near(C.length(q),40);eq(q.points[0],q.points.at(-1));}eq(C.shiftSeams([p],.125)[0].points[0],[5,0,0]);throws(()=>C.shiftSeams([line("a",[0,0,0],[1,0,0])],.2));});
 test("Stack copies preserve geometry and increase Z",()=>{const p=line("a",[0,0,0],[2,0,0]),r=C.stack([p],3,1.5);eq(r.map(x=>x.points[0][2]),[0,1.5,3]);eq(p.points[0],[0,0,0]);throws(()=>C.stack([p],1.5,1));});
 test("Alternating layers reverse every second group",()=>{const p=[0,1,2].map(z=>line(String(z),[0,0,z],[2,0,z]));eq(C.alternate(p).map(x=>x.points[0][0]),[0,2,0]);});
 test("Spiral begins and ends at the expected layer seam",()=>{const p={id:"square",points:[[0,0,0],[1,0,0],[1,1,0],[0,1,0],[0,0,0]]};const r=C.spiralStack(C.stack([p],3,2),8)[0];eq(r.points[0],[0,0,0]);eq(r.points.at(-1),[0,0,4]);eq(r.points.length,17);for(let i=1;i<r.points.length;i++)if(r.points[i][2]<r.points[i-1][2])throw Error("Decreasing Z");});
 test("Legacy System.Random(1) known sequence",()=>{const r=C.dotNetRandom(1);eq([r(),r(),r()].map(x=>Math.round(x*2147483647)),[534011718,237820880,1002897798]);});
 test("Travel sorter is deterministic, preserves paths, and does not mutate input",()=>{const p=[line("a",[8,0,0],[9,0,0]),line("b",[1,0,0],[0,0,0]),line("c",[4,0,0],[3,0,0])],copy=JSON.stringify(p);const a=C.sortTravel(p,{iterations:3}),b=C.sortTravel(p,{iterations:3});eq(a,b);eq(a.map(x=>x.id).sort(),["a","b","c"]);eq(JSON.stringify(p),copy);near(a.reduce((s,p)=>s+C.length(p),0),3);});
 test("Travel sorter handles empty and singleton input",()=>{eq(C.sortTravel([]),[]);const p=line("a",[0,0,0],[1,0,0]);eq(C.sortTravel([p]),[p]);});
 test("CSV round-trip preserves coordinates and simple layers",()=>{const p=[line("0",[0,0,0],[3,4,0]),line("1",[1,2,3],[4,5,6])];eq(C.parseCSV(C.csv(p)),p);});
 test("Malformed CSV is rejected",()=>{throws(()=>C.parseCSV("x,y,z\n0,0,0"));throws(()=>C.parseCSV("path,x,y,z\n1,,0,0"));throws(()=>C.parseCSV("path,x,y,z\n1,Infinity,0,0"));});
 test("Sample geometry has finite dimensions and 24 paths",()=>{const p=C.validate(C.sample());eq(p.length,24);eq(C.bounds(p).max[2],34.5);});
 return results;
}
const results=runCoreTests(C);for(const r of results)console.log((r.passed?"PASS ":"FAIL ")+r.name+(r.error?" — "+r.error:""));if(results.some(r=>!r.passed))process.exitCode=1;
