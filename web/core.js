/* Termite Web Workbench — independent polyline implementation.
Original Termite: Julian Jauk. See recovery/README.md.
No claim of Rhino/Grasshopper numerical equivalence. */
const TermiteCore = (() => {
"use strict";
const EPS=1e-9;
const distance=(a,b)=>Math.hypot(a[0]-b[0],a[1]-b[1],a[2]-b[2]);
const clone=p=>({...p,points:p.points.map(q=>q.slice())});
const length=p=>p.points.slice(1).reduce((s,q,i)=>s+distance(p.points[i],q),0);
function validate(input){
 const raw=Array.isArray(input)?input:input.paths;
 if(!Array.isArray(raw)||!raw.length)throw Error("Provide a non-empty paths array.");
 if(raw.length>10000)throw Error("Maximum 10,000 paths.");
 let count=0;
 const paths=raw.map((p,i)=>{
  const pts=Array.isArray(p)?p:p.points;
  if(!Array.isArray(pts)||!pts.length)throw Error("Path "+(i+1)+" has no points.");
  count+=pts.length;if(count>200000)throw Error("Maximum 200,000 points.");
  const points=pts.map(q=>{
   if(!Array.isArray(q)||q.length<2||q.length>3||q.some(v=>typeof v!=="number"||!Number.isFinite(v)||Math.abs(v)>1e7))
    throw Error("Coordinates must be finite numeric [x,y] or [x,y,z] arrays.");
   return [q[0],q[1],q.length===3?q[2]:0];
  });
  return {id:String(p.id??i+1),layer:String(p.layer??"Default"),points};
 });
 return paths;
}
function pointAt(p,d){
 const pts=p.points,L=length(p);
 if(pts.length===1||L<EPS)return pts[0].slice();
 if(d<=0)return pts[0].slice();if(d>=L)return pts.at(-1).slice();
 let acc=0;
 for(let i=1;i<pts.length;i++){const seg=distance(pts[i-1],pts[i]);if(seg>EPS&&acc+seg>=d){
 const t=(d-acc)/seg;return pts[i].map((v,k)=>pts[i-1][k]+t*(v-pts[i-1][k]));}acc+=seg;}
 return pts.at(-1).slice();
}
function centroid(p){ // Vertex mean, explicitly used by the workbench axis/distance tools.
 const pts=p.points.length>1&&distance(p.points[0],p.points.at(-1))<EPS?p.points.slice(0,-1):p.points;
 return [0,1,2].map(k=>pts.reduce((s,q)=>s+q[k],0)/pts.length);
}
function bounds(paths){
 const min=[Infinity,Infinity,Infinity],max=[-Infinity,-Infinity,-Infinity];
 paths.forEach(p=>p.points.forEach(q=>q.forEach((v,k)=>{min[k]=Math.min(min[k],v);max[k]=Math.max(max[k],v)})));
 return {min,max};
}
function travelLength(paths){let n=0;for(let i=1;i<paths.length;i++)n+=distance(paths[i-1].points.at(-1),paths[i].points[0]);return n;}
function sortLength(paths,descending=false){
 const out=paths.map(clone).sort((a,b)=>length(a)-length(b));return descending?out.reverse():out;
}
function sortAxis(paths,axis=2,descending=false){
 const out=paths.map(clone).sort((a,b)=>centroid(a)[axis]-centroid(b)[axis]);return descending?out.reverse():out;
}
function sortDistance(paths,origin=[0,0,0],descending=false){
 const out=paths.map(clone).sort((a,b)=>distance(centroid(a),origin)-distance(centroid(b),origin));return descending?out.reverse():out;
}
function filterLength(paths,min){
 if(!Number.isFinite(min)||min<0)throw Error("Minimum length must be non-negative.");
 return paths.filter(p=>length(p)>min).map(clone);
}
function reversePaths(paths){return paths.map(p=>({...clone(p),points:p.points.slice().reverse().map(q=>q.slice())}));}
function alternate(paths,axis=2,resolution=.001){
 if(!(resolution>0))throw Error("Layer resolution must be positive.");
 const sorted=sortAxis(paths,axis);let prev=null,layer=-1;
 return sorted.map(p=>{const key=Math.round(centroid(p)[axis]/resolution);
 if(key!==prev){layer++;prev=key;}return layer%2?reversePaths([p])[0]:p;});
}
function shiftSeams(paths,fraction){
 if(!Number.isFinite(fraction))throw Error("Seam fraction must be finite.");
 return paths.map(p=>{
 const q=clone(p);if(q.points.length<3||distance(q.points[0],q.points.at(-1))>EPS)throw Error("Seam shifting requires closed polylines.");
 const L=length(q),d=((fraction%1)+1)%1*L;if(d<EPS||L<EPS)return q;
 let acc=0;
 for(let i=1;i<q.points.length;i++){const seg=distance(q.points[i-1],q.points[i]);if(acc+seg>=d&&seg>EPS){
 const start=pointAt(q,d);const middle=q.points.slice(i).concat(q.points.slice(1,i));
 return {...q,points:[start,...middle,start.slice()].filter((v,j,a)=>j===0||distance(v,a[j-1])>EPS)};
 }acc+=seg;}return q;
 });
}
function stack(paths,count,step){
 if(!Number.isInteger(count)||count<1||count>1000||!Number.isFinite(step)||step<=0)throw Error("Use 1–1000 layers and positive layer height.");
 if(paths.reduce((s,p)=>s+p.points.length,0)*count>200000)throw Error("Stack exceeds 200,000 points.");
 const out=[];for(let i=0;i<count;i++)for(const p of paths)out.push({...p,id:p.id+"-L"+i,points:p.points.map(q=>[q[0],q[1],q[2]+i*step])});return out;
}
function spiralStack(paths,samples=100){
 if(paths.length<2)throw Error("Select at least two stacked closed paths.");
 if(!Number.isInteger(samples)||samples<4||samples>2000)throw Error("Samples must be 4–2000.");
 if(paths.length*samples>200000)throw Error("Spiral exceeds 200,000 points.");
 const sorted=sortAxis(paths,2);
 if(sorted.some(p=>distance(p.points[0],p.points.at(-1))>EPS))throw Error("Spiral inputs must be closed.");
 const points=[];
 for(let i=0;i<sorted.length-1;i++){
 const a=sorted[i],b=sorted[i+1],la=length(a),lb=length(b);
 for(let j=0;j<samples;j++){const t=j/samples,pa=pointAt(a,t*la),pb=pointAt(b,t*lb);points.push(pa.map((v,k)=>v+(pb[k]-v)*t));}
 }points.push(sorted.at(-1).points[0].slice());
 return [{id:"spiral",layer:sorted[0].layer,points}];
}
// Legacy seeded System.Random sequence used by the recovered C# script.
function dotNetRandom(seed=1){
 const a=Array(56).fill(0),M=2147483647;let mj=161803398-Math.abs(seed),mk=1;a[55]=mj;
 for(let i=1;i<55;i++){let ii=21*i%55;a[ii]=mk;mk=mj-mk;if(mk<0)mk+=M;mj=a[ii];}
 for(let k=1;k<5;k++)for(let i=1;i<56;i++){a[i]-=a[1+(i+30)%55];if(a[i]<0)a[i]+=M;}
 let x=0,y=21;return ()=>{if(++x>=56)x=1;if(++y>=56)y=1;let r=a[x]-a[y];if(r===M)r--;if(r<0)r+=M;a[x]=r;return r/M;};
}
// Port of recovery/scripts/termite-sort-by-travel-path/02-object-80.cs.
// Retains original endpoint and 2-opt behaviour, including its limitations.
function sortTravel(paths,{subdivisions=8,iterations=10,flip=true}={}){
 if(!paths.length)return [];
 if(paths.length>250)throw Error("Travel optimization is limited to 250 paths in this version.");
 const S=Math.max(2,Math.trunc(subdivisions)),I=Math.max(1,Math.min(100,Math.trunc(iterations)));
 if(!Number.isFinite(S)||!Number.isFinite(I))throw Error("Invalid optimization settings.");
 const pts=paths.map(p=>pointAt(p,length(p)/2)),bb=bounds([{points:pts}]);
 const dx=Math.max(1e-6,(bb.max[0]-bb.min[0])/S),dy=Math.max(1e-6,(bb.max[1]-bb.min[1])/S);
 const clusters=new Map();
 pts.forEach((p,i)=>{const gx=Math.trunc((p[0]-bb.min[0])/dx),gy=Math.trunc((p[1]-bb.min[1])/dy),key=Math.imul(gx,73856093)^Math.imul(gy,19349663);
 if(!clusters.has(key))clusters.set(key,[]);clusters.get(key).push(i);});
 const random=dotNetRandom(1);let bestCost=Infinity,bestOrder=[];
 for(let it=0;it<I;it++){
 const keys=[...clusters.keys()];for(let i=0;i<keys.length;i++){const j=i+Math.floor(random()*(keys.length-i));[keys[i],keys[j]]=[keys[j],keys[i]];}
 const order=[];let end=null;
 for(const key of keys){const ids=clusters.get(key).slice();
 while(ids.length){let best=Infinity,idx=0,rev=false;
 for(let i=0;i<ids.length;i++){const c=paths[ids[i]];if(end===null){idx=i;rev=false;break;}
 const d1=distance(end,c.points[0]),d2=distance(end,c.points.at(-1));
 if(d1<best){best=d1;idx=i;rev=false;}if(d2<best){best=d2;idx=i;rev=true;}}
 const c=clone(paths[ids.splice(idx,1)[0]]);if(flip&&rev)c.points.reverse();order.push(c);end=c.points.at(-1);
 }}
 let improved=true,guard=0;
 while(improved&&guard<20){improved=false;guard++;
 for(let i=0;i<order.length-1;i++)for(let j=i+1;j<order.length;j++){
 const prev=i?order[i-1]:null,a=order[i],b=order[j],next=j<order.length-1?order[j+1]:null;
 const before=(prev?distance(prev.points.at(-1),a.points[0]):0)+distance(a.points.at(-1),b.points[0])+(next?distance(b.points.at(-1),next.points[0]):0);
 const after=(prev?distance(prev.points.at(-1),b.points.at(-1)):0)+distance(b.points[0],a.points.at(-1))+(next?distance(a.points[0],next.points[0]):0);
 if(after<before){order.splice(i,j-i+1,...order.slice(i,j+1).reverse());improved=true;}
 }}
 const cost=travelLength(order);if(cost<bestCost){bestCost=cost;bestOrder=order;}
 }return bestOrder;
}
function parseCSV(text){
 const lines=text.trim().split(/\r?\n/).filter(s=>s.trim());
 if(!lines.length)throw Error("CSV is empty.");
 const header=lines.shift().trim().toLowerCase().split(",").map(s=>s.trim());
 for(const name of ["path","x","y","z"])if(!header.includes(name))throw Error("CSV needs path,x,y,z headers; layer is optional.");
 const groups=new Map();
 for(let i=0;i<lines.length;i++){const fields=lines[i].split(",").map(s=>s.trim()),get=k=>fields[header.indexOf(k)];
 if(fields.length!==header.length)throw Error("CSV field count mismatch on row "+(i+2));
 if(!get("path")||["x","y","z"].some(k=>get(k)===""))throw Error("Missing CSV value on row "+(i+2));
 const id=get("path"),layer=header.includes("layer")?get("layer"):"Default";
 if(!groups.has(id))groups.set(id,{id,layer,points:[]});
 if(groups.get(id).layer!==layer)throw Error("Each path must have one layer.");
 groups.get(id).points.push(["x","y","z"].map(k=>Number(get(k))));
 }return validate([...groups.values()]);
}
function csv(paths){return "path,x,y,z,layer\n"+paths.flatMap((p,i)=>p.points.map(q=>[i,...q,p.layer.replace(/[,\r\n"]/g,"_")].join(","))).join("\n");}
function sample(){
 return Array.from({length:24},(_,k)=>({id:String(k+1),layer:"Default",points:Array.from({length:97},(_,j)=>{
 const t=j/96*Math.PI*2,r=32+5*Math.sin(k/23*Math.PI)+2*Math.sin(t*6+k*.13);
 return [r*Math.cos(t),r*Math.sin(t),k*1.5];})}));
}
return {distance,length,pointAt,centroid,bounds,travelLength,validate,sortLength,sortAxis,sortDistance,filterLength,reversePaths,alternate,shiftSeams,stack,spiralStack,sortTravel,dotNetRandom,parseCSV,csv,sample};
})();
if(typeof module!=="undefined")module.exports=TermiteCore;