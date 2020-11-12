
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

    theSession.UpdateManager.ClearErrorList()
    dim obj1() As NXOpen.TaggedObject = {}
    dim int1 as integer
    for each theFeature as Features.feature in workPart.Features
        if theFeature.GetType.ToString = "NXOpen.Features.ExtractFace" then 
            Array.Resize(obj1,obj1.Length+1)  
            obj1(obj1.Length-1) = CType(workPart.Features.FindObject(theFeature.journalidentifier), NXOpen.Features.ExtractFace)
        end if
    next
    'infowindow.writeline(obj1.length)
    Dim nErrs1 As Integer = Nothing
    nErrs1 = theSession.UpdateManager.AddObjectsToDeleteList(obj1)
    
    Dim markId1 As NXOpen.Session.UndoMarkId = Nothing
    markId1 = theSession.SetUndoMark(NXOpen.Session.MarkVisibility.Invisible, "Delete")

    int1 = theSession.UpdateManager.DoUpdate(markId1)

    end Sub

End Module


    