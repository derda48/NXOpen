Imports NXOpen
imports system
Imports System.IO
Imports System.Collections.Generic


Module NXOpenSample
Sub Main()
            Dim theSession As Session = Session.GetSession()
            Dim workPart As Part = theSession.Parts.Work
            Dim line As String
            Dim delim As Char() = {","c}
            dim Punkt as Point3d
            dim ersteLine as boolean = False
            Dim lines() As String = File.ReadAllLines("C:\Users\DaniH\Documents\Uberordner1\Studium\StuMi\TM\VIT-V\nx_open\vb\Punkte.txt")
            Using sr As StreamReader = New StreamReader("C:\Users\DaniH\Documents\Uberordner1\Studium\StuMi\TM\VIT-V\nx_open\vb\Punkte.txt")
            
                    line = sr.ReadLine()
                    for index as integer = 0 to lines.length-1
                        if ersteLine then
                        Dim strings As String() = line.Split(delim)
                        Punkt.x = Double.Parse(strings(0))
                        Punkt.y = Double.Parse(strings(1))
                        Punkt.z = Double.Parse(strings(2))      
                        end if 
                        ersteLine = True                  
                        line = sr.ReadLine()
                    next
                End Using
            dim a as new Point3d(1,2,3)
            workPart.Curves.CreateLine(a,Punkt)

end sub

End Module
