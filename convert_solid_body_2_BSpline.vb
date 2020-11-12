' use Snap to create 2 Points, used to create a line through a body
' input number(Integer) of planes and number of points on each plane  (automatically creates a BSurface through the points)
' at the end, the user can select 10 points (one after another) and get their index in the points_double list



Imports System
Imports NXOpen
imports snap,Snap.UI.Input,Snap.Create,snap.ui
Imports NXOpen.UF
 
Module Module1
    
    Dim nullNXOpen_Features_Feature As NXOpen.Features.Feature = Nothing
    Dim theSession As Session = Session.GetSession()
    Dim theUfSession As UFSession = UFSession.GetUFSession()
    Dim lw As ListingWindow = theSession.ListingWindow
    Dim workPart As Part = theSession.Parts.Work
 
Sub Main()
    '####   select the cutter
    dim sel_body as NXopen.body= func_SelectBody("Select a body", true)
    
    '####   create the initial line trough the body
    '####   variante 2: automatic line creation (every point of the cutter is accessible)
    dim bounding_box_double = func_surrounding_box(sel_body)
    
    dim p_min as new point3d(bounding_box_double(0),bounding_box_double(1),bounding_box_double(2))
    dim p_max as new point3d(bounding_box_double(3),bounding_box_double(4),bounding_box_double(5))
    dim line_0 as nxopen.line = workPart.Curves.CreateLine(p_min,p_max)
    
    Dim pos1_0 As Position = GetPosition("Create the starting point for the plane line on a face").Position
    Dim pos2_0 As Position = GetPosition("Create the ending point for the plane line on a face").Position
    dim p1_0 as New Point3d(pos1_0.X,pos1_0.Y,pos1_0.Z)
    dim p2_0 as New Point3d(pos2_0.X,pos2_0.Y,pos2_0.Z)
    dim line_1 as nxopen.line = workPart.Curves.CreateLine(p1_0,p2_0)
    dim dir = workpart.directions.CreateDirection(line_1,sense.forward,smartobject.updateoption.withinmodeling)

    '####   input "nu"(the final "nu" can differ) and "nv"
    dim number_planes_input As Integer = func_getting_a_number(true)
    dim nv As Integer = func_getting_a_number(false)
    dim nu as integer = 0               '####   final "nu"

    '####   create the pointset on the initial line to create the planes
    dim pointset_0 = func_CreatePointSet_line(line_1, number_planes_input)
    dim planes as datumplane() = {}
    dim instersectingcurves as NXOpen.Features.IntersectionCurve() = {}
    dim pointsets() = {}
    dim points_sortiert() as nxopen.point = {}
    dim points_sortiert_neu() as nxopen.point = {}
    dim points_double as double() = {}
    dim knoten_u as double() = {}       '####   {0,0,0,0,0.5,1,1,1,1} ## Grad+1 wiederholungen ## Grad+1+N items
    dim knoten_v as double() = {}       '####   {0,0,0,0,0.5,1,1,1,1} ## Grad+1 wiederholungen ## Grad+1+N items
    dim Gradu = 3
    dim Gradv = 3

    dim free_space = 0
    dim min_index as integer = 0
    dim zwischen_pointset = nothing
    dim zwischen_intersectingcurve = nothing

    '####   loop for every point on the initial line
    for index_nu as integer = 0 to pointset_0.GetEntities().length-1
        Array.Resize(planes,planes.Length+1)
        planes(planes.Length-1) = func_CreateDatumPlane(pointset_0.GetEntities(index_nu),dir)

    '####   test for intersection (in variante 2 maybe no intersection between plane and cutter)
    try
        zwischen_intersectingcurve = func_CreateIntersectionCurve(sel_body,planes(index_nu))
    catch ex1 as NXOpen.NXException
        continue for
    end try
        Array.Resize(instersectingcurves,instersectingcurves.Length+1)
        instersectingcurves(instersectingcurves.Length-1) = zwischen_intersectingcurve
    
    '####   test for intersection curves (maybe multiple intersection curves are created, so the creation of a pointset is not possible)
    try 
        zwischen_pointset = func_CreatePointSet_feature(instersectingcurves(instersectingcurves.Length-1),nv)
    catch ex2 as NXOpen.NXException
        continue for
    end try
        Array.Resize(pointsets,pointsets.Length+1)
        pointsets(pointsets.Length-1) = zwischen_pointset


        'pointsets(nu).GetEntities(1).Color = 181
        'pointsets(nu).GetEntities(1).RedisplayObject
        'pointsets(nu).GetEntities(2).Color = 106
        'pointsets(nu).GetEntities(2).RedisplayObject


    '####   create an array of sorted points (startingpoint of the new pointset is nearest point to the startingpoint of the last pointset)
        dim anticounter as integer = 1
        dim zwischen_point as nxopen.point
        min_index = func_get_min_index(nu,nv,min_index,pointsets)
        for counter as integer = 0 to nv-1
            if (min_index+counter) < (pointsets(nu).GetEntities().length) then
                Array.Resize(points_sortiert,points_sortiert.Length+1)
                points_sortiert(points_sortiert.Length-1) = pointsets(nu).GetEntities(min_index+counter)
            else 
                if counter <> (nv-1) then
                    Array.Resize(points_sortiert,points_sortiert.Length+1)
                    points_sortiert(points_sortiert.Length-1) = pointsets(nu).GetEntities(anticounter)
                else
                    Array.Resize(points_sortiert,points_sortiert.Length+1)
                    points_sortiert(points_sortiert.Length-1) = pointsets(nu).GetEntities(min_index)
                end if                
                anticounter = anticounter+1
            end if
        next

        nu = nu + 1
    next


    '####   convert the points_sortiert array to an double array
    for index_nu1 as integer = 0 to nu-1
        for index_nv as integer = 0 to nv-1-free_space
            Array.Resize(points_double,points_double.Length+1)  
            points_double(points_double.Length-1) = points_sortiert((index_nu1*nv)+index_nv).coordinates.X
            Array.Resize(points_double,points_double.Length+1)  
            points_double(points_double.Length-1) = points_sortiert((index_nu1*nv)+index_nv).coordinates.Y
            Array.Resize(points_double,points_double.Length+1)  
            points_double(points_double.Length-1) = points_sortiert((index_nu1*nv)+index_nv).coordinates.Z
            Array.Resize(points_double,points_double.Length+1)  
            points_double(points_double.Length-1) = 1
        next
    next


    knoten_u = func_bsurf_vorbereitung_knoten(Gradu,nu,knoten_u)
    knoten_v = func_bsurf_vorbereitung_knoten(Gradv,nv-free_space,knoten_v)
    dim bsurftag as NXOpen.Tag

    '####   create the B-Spline Surface
    theUFSession.Modl.CreateBsurf(nv,nu-free_space,Gradu+1,Gradv+1,knoten_v,knoten_u,points_double,bsurftag,0,0)

        
    'infowindow.writeline(nu)
    'infowindow.writeline(points_sortiert.length)
    'infowindow.writeline(points_double.length)
    func_index_ermittlung_1(points_sortiert)
End Sub


    function func_get_min_index(nu , nv , min_index, pointsets)
        dim dist_arr as double() = {}
        if nu > 0 then
            dim smallest as double = 10000
            for index_0 as integer = 0 to nv-1
                Array.Resize(dist_arr,dist_arr.Length+1)
                dist_arr(dist_arr.Length-1) = func_find_distance(pointsets(nu-1).GetEntities(min_index), pointsets(nu).GetEntities(index_0))
            next
            for each element as double in dist_arr
                smallest = system.Math.min(smallest,element)
            next

            'pointsets(nu-1).GetEntities(min_index).Color = 181
            'pointsets(nu-1).GetEntities(min_index).RedisplayObject

            min_index = array.indexof(of double)(dist_arr,smallest)
            
            'pointsets(nu).GetEntities(min_index).Color = 106'44
            'pointsets(nu).GetEntities(min_index).RedisplayObject
        end if
        return min_index
    end function


    function func_surrounding_box_v2(obj as nxopen.body)
        Dim csys As NXOpen.Tag = NXOpen.Tag.Null
        Dim min_corner(2) As Double
        Dim directions(2, 2) As Double
        Dim distances(2) As Double
        Dim edge_len(2) As String

        theUfSession.Modl.AskBoundingBoxExact(obj.tag, csys, min_corner, directions,distances)
        
        dim origin_new as new position(min_corner(0),min_corner(1),min_corner(2))
        edge_len(0) = Replace(distances(0).ToString(),",",".")
        edge_len(1) = Replace(distances(1).ToString(),",",".")
        edge_len(2) = Replace(distances(2).ToString(),",",".")
        Dim brick As NX.Block = Block(origin_new, edge_len(0), edge_len(1), edge_len(2))
    end function


    function func_surrounding_box(obj as nxopen.body)
        dim bounding_box_double(5) as double
        theUFSession.Modl.AskBoundingBox(obj.tag,bounding_box_double)
        return bounding_box_double
    end function


    function func_find_distance(p1 as nxopen.point,p2 as nxopen.point)
        Dim guess1() As Double = {p1.coordinates.X,p1.coordinates.Y,p1.coordinates.Z}
        Dim guess2() As Double = {p2.coordinates.X,p2.coordinates.Y,p2.coordinates.Z}
        Dim pt1(2) As Double 
        Dim pt2(2) As Double 
        Dim minDist As Double
        theUfSession.Modl.AskMinimumDist(nothing, nothing, 1, guess1, 1, guess2, minDist, pt1, pt2)
        return minDist
    end function


    function func_index_ermittlung(punkte_arr)
        for k as integer = 0 to 10

        dim selObj as nxobject
        dim prompt As String = "wähle punkt"
        Dim theUI As UI = UI.GetUI
        Dim title As String = "Selection"
        Dim includeFeatures As Boolean = False
        Dim keepHighlighted As Boolean = False
        Dim selAction As nxopen.Selection.SelectionAction = nxopen.Selection.SelectionAction.ClearAndEnableSpecific
        Dim cursor As Point3d
        Dim scope As nxopen.Selection.SelectionScope = nxopen.Selection.SelectionScope.WorkPart
        Dim selectionMask_array(1) As nxopen.Selection.MaskTriple
            With selectionMask_array(0)
            .Type = UFConstants.UF_point_type
                .Subtype = UFConstants.UF_point_subtype
                .SolidBodySubtype = 0
            End With

        Dim resp As nxopen.Selection.Response = theUI.SelectionManager.SelectObject( _
            prompt, title, scope, selAction, _
            includeFeatures, keepHighlighted, selectionMask_array, selobj, cursor)

            dim index_arr() as integer
            dim punkt_arr_double_1(2) as double
            dim punkt_arr_double_2(2) as double
            dim obj_1 as nxopen.point = directcast(selobj,nxopen.point)
            punkt_arr_double_1(0) = obj_1.coordinates.X
            punkt_arr_double_1(1) = obj_1.coordinates.Y
            punkt_arr_double_1(2) = obj_1.coordinates.Z

            for punkt_index as integer = 0 to punkte_arr.length-1
                punkt_arr_double_2(0) = punkte_arr(punkt_index).coordinates.X
                punkt_arr_double_2(1) = punkte_arr(punkt_index).coordinates.Y
                punkt_arr_double_2(2) = punkte_arr(punkt_index).coordinates.Z
                if punkt_arr_double_1(0) = punkt_arr_double_2(0) and _
                punkt_arr_double_1(1) = punkt_arr_double_2(1) and _
                punkt_arr_double_1(2) = punkt_arr_double_2(2) 
                    infowindow.writeline(punkt_index)
                end if
            next
        next
    end function

    function func_index_ermittlung_1(punkte_arr)
        for k as integer = 0 to 10

        dim selObj as nxobject
        dim prompt As String = "wähle punkt"
        Dim theUI As UI = UI.GetUI
        Dim title As String = "Selection"
        Dim includeFeatures As Boolean = False
        Dim keepHighlighted As Boolean = False
        Dim selAction As nxopen.Selection.SelectionAction = nxopen.Selection.SelectionAction.ClearAndEnableSpecific
        Dim cursor As Point3d
        Dim scope As nxopen.Selection.SelectionScope = nxopen.Selection.SelectionScope.WorkPart
        Dim selectionMask_array(1) As nxopen.Selection.MaskTriple
            With selectionMask_array(0)
            .Type = UFConstants.UF_point_type
                .Subtype = UFConstants.UF_point_subtype
                .SolidBodySubtype = 0
            End With

        Dim resp As nxopen.Selection.Response = theUI.SelectionManager.SelectObject( _
            prompt, title, scope, selAction, _
            includeFeatures, keepHighlighted, selectionMask_array, selobj, cursor)

            dim index_arr() as integer
            dim punkt_arr_double_1(2) as double
            dim punkt_arr_double_2(2) as double
            dim obj_1 as nxopen.point = directcast(selobj,nxopen.point)
            dim p1 as new point3d(obj_1.coordinates.X,obj_1.coordinates.Y,obj_1.coordinates.Z)

            for punkt_index as integer = 0 to punkte_arr.length-1
                dim p2 as new point3d(punkte_arr(punkt_index).coordinates.X,punkte_arr(punkt_index).coordinates.Y,punkte_arr(punkt_index).coordinates.Z)
                if p1.equals(p2)
                    infowindow.writeline(punkt_index)
                end if
            next
        next
    end function

    function func_bsurf_vorbereitung_knoten(gradu, nu,knotenu)
        'dim knotenu as double() = {}'{0,0,0,0,0.5,1,1,1,1} 'Grad+1 wiederholungen ::::: Grad+1+N items
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


    function func_SelectBody(prompt As String, body_boolean as Boolean)
      dim selObj as nxobject
      Dim theUI As UI = UI.GetUI
       Dim title As String = "Selection"
       Dim includeFeatures As Boolean = False
       Dim keepHighlighted As Boolean = False
       Dim selAction As nxopen.Selection.SelectionAction = nxopen.Selection.SelectionAction.ClearAndEnableSpecific
       Dim cursor As Point3d
       Dim scope As nxopen.Selection.SelectionScope = nxopen.Selection.SelectionScope.WorkPart
       Dim selectionMask_array(1) As nxopen.Selection.MaskTriple
        if body_boolean = true then
            'Dim selectionMask_array(0) As nxopen.Selection.MaskTriple
            With selectionMask_array(0)
            .Type = UFConstants.UF_solid_type
            .Subtype = NXOpen.UF.UFConstants.UF_solid_body_subtype
            '.SolidBodySubtype = UFConstants.UF_UI_SEL_FEATURE_ANY_EDGE 
            .SolidBodySubtype = 0
            End With
        else 
            
            With selectionMask_array(0)
            .Type = UFConstants.UF_solid_type
            .Subtype = NXOpen.UF.UFConstants.UF_solid_body_subtype
            .SolidBodySubtype = UFConstants.UF_UI_SEL_FEATURE_ANY_EDGE 
            end with

            With selectionMask_array(1)
                .Type = UFConstants.UF_datum_axis_type
                .Subtype = 0
        End With
        end if
        Dim resp As nxopen.Selection.Response = theUI.SelectionManager.SelectObject( _
            prompt, title, scope, selAction, _
            includeFeatures, keepHighlighted, selectionMask_array, selobj, cursor)
        return selObj
    end function


    function func_getting_a_number(nu as boolean)
        dim cue = ""
        dim title as String
        if nu = true then 
            title = "number of planes (nu)"
        else
            title = "number of points on intersecting curve (nv)"
        end if
            dim label = "Integer"
            Dim initialvalue As Integer = 100
            dim number As Integer = GetInteger(cue,title,label,initialvalue)
        return number
    end function
    
    
    function func_CreatePointSet_line(line as NXOpen.line, number_points as Integer)
        dim section_0 = workPart.Sections.CreateSection(0,0,0)
        Dim helpPoint As New NXOpen.Point3d(0,0,0)
        Dim nullObj As NXOpen.NXObject = Nothing
        Dim noChain As Boolean = False
        Dim createMode As NXOpen.Section.Mode = Section.Mode.Create
        Dim r1 As NXOpen.CurveDumbRule = workPart.ScRuleFactory.CreateRuleBaseCurveDumb({line})
        Dim pointSetBuilder1 = workPart.Features.CreatePointSetBuilder(nothing)
        pointSetBuilder1.Type = NXOpen.Features.PointSetBuilder.Types.CurvePoints
        pointSetBuilder1.SingleCurveOrEdgeCollector.AddToSection({r1},line,nullObj, nullObj, helpPoint, createMode, noChain)
        pointSetBuilder1.NumberOfPointsExpression.RightHandSide = CStr(number_points)
        Dim pointset As NXOpen.NXObject = pointSetBuilder1.Commit()
        pointSetBuilder1.Destroy()
        return pointset
    End function


    function func_CreatePointSet_feature(intersectionCurve as NXOpen.Features.IntersectionCurve, number_points as Integer)
        Dim helpPoint1 As New NXOpen.Point3d(0,0,0)
        Dim pointSetBuilder1 = workPart.Features.CreatePointSetBuilder(nothing)
        pointSetBuilder1.SingleCurveOrEdgeCollector.SetAllowedEntityTypes(NXOpen.Section.AllowTypes.OnlyCurves)
        pointSetBuilder1.SingleCurveOrEdgeCollector.AllowSelfIntersection(True)
        pointSetBuilder1.Type = NXOpen.Features.PointSetBuilder.Types.CurvePoints
        Dim features1(0) As NXOpen.Features.Feature
        features1(0) = intersectionCurve
        dim curveFeatureRule1 As NXOpen.CurveFeatureRule = workPart.ScRuleFactory.CreateRuleCurveFeature(features1)
        pointSetBuilder1.SingleCurveOrEdgeCollector.AllowSelfIntersection(True)
        Dim rules1(0) As NXOpen.SelectionIntentRule
        rules1(0) = curveFeatureRule1
        Dim nullNXOpen_NXObject As NXOpen.NXObject = Nothing
        pointSetBuilder1.SingleCurveOrEdgeCollector.AddToSection(rules1, nullNXOpen_NXObject, nullNXOpen_NXObject, nullNXOpen_NXObject, helpPoint1, NXOpen.Section.Mode.Create, False)
        pointSetBuilder1.NumberOfPointsExpression.RightHandSide = CStr(number_points)
        Dim pointset As NXOpen.NXObject = pointSetBuilder1.Commit()
        pointSetBuilder1.Destroy()
        return pointset
    End function


    function func_CreateDatumPlane(origin as NXOpen.point, normal as nxopen.direction)
        Dim datumPlaneBuilder1 As Features.DatumPlaneBuilder = workPart.Features.CreateDatumPlaneBuilder(nullNXOpen_Features_Feature)
        dim plane1 As nxopen.Plane = datumPlaneBuilder1.GetPlane()
        plane1.SetMethod(PlaneTypes.MethodType.PointDir)
        Dim geom1(1) As NXObject
        geom1(0) = origin
        geom1(1) = normal
        plane1.SetGeometry(geom1)
        'plane1.SetAlternate(NXOpen.PlaneTypes.AlternateType.One)
        plane1.Evaluate()
        dim feature1 As Features.Feature = datumPlaneBuilder1.CommitFeature()
        Dim datumPlaneFeature1 As Features.DatumPlaneFeature = directcast(feature1, Features.DatumPlaneFeature)
        dim datumPlane1 as DatumPlane = datumPlaneFeature1.DatumPlane
        datumPlaneBuilder1.Destroy()
        return datumPlane1
    End function


    function func_CreateIntersectionCurve(body1 as NXOpen.body,datumPlane1 as NXopen.DatumPlane)
        Dim intersectionCurveBuilder1 As NXOpen.Features.IntersectionCurveBuilder = Nothing
        intersectionCurveBuilder1 = workPart.Features.CreateIntersectionCurveBuilder(nullNXOpen_Features_Feature)
        Dim faces1(0) As NXOpen.DatumPlane
        faces1(0) = datumPlane1
        Dim faceDumbRule1 As NXOpen.FaceDumbRule = Nothing
        faceDumbRule1 = workPart.ScRuleFactory.CreateRuleFaceDatum(faces1)
        Dim rules1(0) As NXOpen.SelectionIntentRule
        rules1(0) = faceDumbRule1
        intersectionCurveBuilder1.FirstFace.ReplaceRules(rules1, False)
        Dim faceBodyRule1 As NXOpen.FaceBodyRule = Nothing
        faceBodyRule1 = workPart.ScRuleFactory.CreateRuleFaceBody(body1)
        Dim rules2(0) As NXOpen.SelectionIntentRule
        rules2(0) = faceBodyRule1
        intersectionCurveBuilder1.SecondFace.ReplaceRules(rules2, False)
        Dim objects1 As NXOpen.TaggedObject() = {}
        for index as integer = 0 to body1.getfaces().length-1
            Array.Resize(objects1,objects1.Length+1)
            objects1(objects1.Length-1) = body1.getfaces(index)
        next
        Dim added3 As Boolean = Nothing
        added3 = intersectionCurveBuilder1.SecondSet.Add(objects1)
        intersectionCurveBuilder1.CurveFitData.Tolerance = 0.000001

        Dim nXObject1 As NXOpen.NXObject = Nothing
        nXObject1 = intersectionCurveBuilder1.Commit()
        intersectionCurveBuilder1.Destroy()
        dim intersectingcurve = directcast(nXObject1,NXOpen.Features.IntersectionCurve)
        return intersectingcurve
    End function
 
End Module