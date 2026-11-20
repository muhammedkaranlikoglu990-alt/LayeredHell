import sys, os
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', 'Blender'))
from ll_blender.geo import Geo
def vol(g):
    v=0
    for f in g.faces:
        p=[g.verts[i] for i in f]
        for k in range(1,len(p)-1):
            a,b,c=p[0],p[k],p[k+1]
            v+=(a[0]*(b[1]*c[2]-b[2]*c[1])-a[1]*(b[0]*c[2]-b[2]*c[0])+a[2]*(b[0]*c[1]-b[1]*c[0]))/6
    return v
mk={'box':lambda g:g.box((0,0,.5),(1,1,1),'wood_dark'),
 'cylY':lambda g:g.cylinder((0,0,0),.3,1,'steel_brushed',axis='y'),
 'cylX':lambda g:g.cylinder((0,0,0),.3,1,'steel_brushed',axis='x'),
 'cylZ':lambda g:g.cylinder((0,0,0),.3,1,'steel_brushed'),
 'sphere':lambda g:g.sphere((0,0,0),.5,'rubber'),
 'wedge':lambda g:g.wedge((0,0,0),(1,1,1),'rubber'),
 'hbar':lambda g:g.hbar((0,0,0),(1,1,1),.1,'rubber'),
 'taperbox':lambda g:g.box((0,0,0),(1,1,1),'rubber',rot_z=.7,taper=.5)}
bad=0
for n,f in mk.items():
    g=Geo('ENV_MAT_2K'); f(g); g.validate(); v=vol(g); print(n, round(v,4)); bad+= v<=0
print('OK' if not bad else 'WINDING PROBLEM')
sys.exit(bad)
