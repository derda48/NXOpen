imports Snap
imports Snap.Create
Imports NXOpen,NXOpen.UF
imports system
Imports System.IO
Imports System.Collections.Generic

Module BSurf
Sub Main()
    Dim theUFSession As UFSession = UFSession.GetUFSession()
    Dim theSession As NXOpen.Session = NXOpen.Session.GetSession()
    Dim workPart As NXOpen.Part = theSession.Parts.Work


    ' BSpline variables
    dim nu as Integer = 5
    dim nv as Integer = 4
    dim Gradu as Integer = 3
    dim Gradv as Integer = 3
    dim knoten_u as double() = {}'{0,0,0,0,0.5,1,1,1,1} 'Grad+1 wiederholungen ::::: Grad+1+N items
    dim knoten_v as double() = {}'{0,0,0,0,0.5,1,1,1,1} 'Grad+1 wiederholungen ::::: Grad+1+N items
   
    dim p1 as double() = {0,0,0,1,
    0,0,0,1,
    0,0,0,1,
    0,0,0,1,
    0,0,0,1,
    0,-1,1,1,
    1,0,1,1,
    0,1,1,1,
    -1,0,1,1,
    0,-1,1,1,
    0,-2,2,1,
    2,0,2,1,
    0,2,2,1,
    -2,0,2,1,
    0,-2,2,1,
    0,-1,3,1,
    1,0,3,1,
    0,1,3,1,
    -1,0,3,1,
    0,-1,3,1}

    knoten_u = bsurf_vorbereitung_knoten(Gradu,nu,knoten_u)
    knoten_v = bsurf_vorbereitung_knoten(Gradv,nv,knoten_v)

    ' create BSurface (created as Body)
    dim bsurftag as NXOpen.Tag
    theUFSession.Modl.CreateBsurf(nu,nv,Gradu+1,Gradv+1,knoten_u, knoten_v, p1,bsurftag,0,0)

    dim body1 = theSession.GetObjectManager.Get(bsurftag)
    body1.color = 181
    body1.redisplayobject

end Sub

function bsurf_vorbereitung_knoten(gradu, nu,knotenu)
        'dim knotenu as double() = {}'{0,0,0,0,0.5,1,1,1,1} 'Grad+1 repetitions ::::: Grad+1+N items
        for index_3 as Integer = 0 to (Gradu)
            Array.Resize(knotenu,knotenu.Length+1)
            knotenu(knotenu.Length-1) = 0
        next
        dim zwischenu as integer = (Gradu+1+nu)-2*(Gradu+1)
        for index_3 as Integer = 0 to (zwischenu-1)
            Array.Resize(knotenu,knotenu.Length+1)
            knotenu(knotenu.Length-1) = 1/(zwischenu+1)*(index_3+1)
        next
        for index_3 as Integer = 0 to (Gradu)
            Array.Resize(knotenu,knotenu.Length+1)
            knotenu(knotenu.Length-1) = 1
        next
        return knotenu
end function

end Module