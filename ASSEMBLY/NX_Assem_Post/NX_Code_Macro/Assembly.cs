using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices; //For Marshal

using NXOpen;
using NXOpen.Assemblies;
using NXOpen.Positioning;
using NXOpen.UF;

namespace NX_Code_Macro
{
    class Assembly:NewNXfile
    {
        public TransCAD.Application tApp = null;

        public TransCAD.AssemDocument tAssemDoc = null;
        public TransCAD.Assem tAssem = null;
        public TransCAD.Component tComp = null;
        public TransCAD.StdAssemConstraints[] tConstraints = null;
        public TransCAD.Part[] tcad_PartList;
        public Component rootComponent;
        public Component[] nxComponents;
        public ComponentPositioner nxPositioner;
        public Constraint[] nxConstraints;
        public int numComponents;
        public Point3d basepoint1;
        public Matrix3x3 orientation1;
        public string[] nxCompNames;
        public NXOpen.Part nxRootAssem;
        public int compCount;
        public TransCAD.IStdAssemConstraint[] GetConstraints;
        public string[] partname = null;
        
        public bool InitializeTransCAD()
        {
            try
            {
                tApp = (TransCAD.Application)Marshal.GetActiveObject("TransCAD.Application");
            }
            catch
            {
                tApp = (TransCAD.Application)Activator.CreateInstance(Type.GetTypeFromProgID("TransCAD.Application"));
            }
            if (tApp == null)
                return false;

            tApp.Visible = true;
            return true;

        }
        public void NXRootFile(Session nxSession, NewNXfile fileDir)
        {
            NXOpen.FileNew nxRootCompFile = nxSession.Parts.FileNew();
            fileDir.NXfile(nxRootCompFile, fileDir.rootDir());
            NXOpen.NXObject nXObject_RootComp = nxRootCompFile.Commit();
            nxRootCompFile.MakeDisplayedPart = true;
            Part nxRootAssem = nxSession.Parts.Work;
            nxRootCompFile.Destroy();
        }
        public void loadAssemblyInfo(Session nxSession)
        {
            tAssemDoc = (TransCAD.AssemDocument)tApp.ActiveDocument;
            tAssem = tAssemDoc.Assem;
            NXOpen.Part nxSubAssem ;
            var newfile = new NewNXfile();
            var fileDir = new NewNXfile();
            //Count Component Number
            compCount = tAssem.GetSize();
            for (int i = 0; i < compCount; i++)
            {
                nxRootAssem = nxSession.Parts.Work;
                TransCAD.Component tComp = tAssem.GetComponent(i);
                string compName = tComp.get_Name();
                Console.WriteLine("Name of Component[ " + i + " ] : " + tComp.get_Name());
                
                //Count Part Number in Component
                int partCount = tComp.GetSize();
                partname = new string[partCount];
                if (compName == "Default SubAssembly")
                {
                    continue;
                }

                NXOpen.FileNew compFile = nxSession.Parts.FileNew();
                newfile.NXfile(compFile, fileDir.subassemdir(compName));

                compFile.MakeDisplayedPart = false;
                
                NXOpen.Assemblies.CreateNewComponentBuilder createNewComponentBuilder1;
                createNewComponentBuilder1 = nxRootAssem.AssemblyManager.CreateNewComponentBuilder();
                createNewComponentBuilder1.NewComponentName = compName;
                createNewComponentBuilder1.ReferenceSet = NXOpen.Assemblies.CreateNewComponentBuilder.ComponentReferenceSetType.EntirePartOnly;
                createNewComponentBuilder1.ReferenceSetName = "Entire Part";
                createNewComponentBuilder1.NewFile = compFile;

                NXOpen.NXObject nXObject_SubAssem = createNewComponentBuilder1.Commit();

                createNewComponentBuilder1.Destroy();

                nxSubAssem = (NXOpen.Part)nxSession.Parts.FindObject(compName);
                tcad_PartList = new TransCAD.Part[partCount];
             
                NXObject[] nxGeom = new NXObject[partCount];
                for (int j = 0; j < partCount; j++)
                {
                    TransCAD.Part tPart = tComp.GetPart(j);
                    partname[j] = tPart.Name;
                    tcad_PartList[j] = tPart;
    
                    int facecount = tPart.Solid.Faces.Count;
                    string facenm = tPart.Solid.Faces[1].Name;
                    Console.WriteLine(facenm);
              
               
     
                    Console.WriteLine(" Name of Part[ " + j + " ] : " + tPart.Name);
                    //tcad_PartList[j] = tComp.GetPart(j);
                    //tcad_PartList[i] = tComp.GetPart(i);
                    //TransCAD.Face face = tcad_PartList[i].SelectFeatureByName("SubAssembly1,Body,Extrude2,Sketch2,Circle1,0,0,0,ExtrudeFeature:0,0:0;0");
            
                    NXOpen.BasePart basePart1;
                    NXOpen.PartLoadStatus partLoadStatus1;
                    string partFileName = tPart.Name;
                    //string partFileDir = A1_PostFileLocation + tPart.Name + fileExtension;
                    basePart1 = nxSession.Parts.OpenBase(fileDir.partdir(tPart.Name), out partLoadStatus1);
                    partLoadStatus1.Dispose();
                    
                    basepoint1 = new NXOpen.Point3d(0.0, 0.0, 0.0);
                    orientation1 = new NXOpen.Matrix3x3();
                    orientation1.Xx = 1.0;
                    orientation1.Xy = 0.0;
                    orientation1.Xz = 0.0;
                    orientation1.Yx = 0.0;
                    orientation1.Yy = 1.0;
                    orientation1.Yz = 0.0;
                    orientation1.Zx = 0.0;
                    orientation1.Zy = 0.0;
                    orientation1.Zz = 1.0;
                    
                    NXOpen.PartLoadStatus partLoadStatus2;
                    NXOpen.Assemblies.Component component1;
                    component1 = nxRootAssem.ComponentAssembly.AddComponent(fileDir.partdir(tPart.Name), "MODEL", "A1-1", basepoint1, orientation1, -1, out partLoadStatus2, true);
                    partLoadStatus2.Dispose();

                    NXOpen.Assemblies.Component[] origComponents1 = new NXOpen.Assemblies.Component[1];
                    origComponents1[0] = component1;
                    NXOpen.Assemblies.Component component2 = (NXOpen.Assemblies.Component)nXObject_SubAssem;
                    NXOpen.Assemblies.Component[] newComponents1;
                    NXOpen.ErrorList errorList1;
                    nxSubAssem.ComponentAssembly.RestructureComponents(origComponents1, component2, true, out newComponents1, out errorList1);
                    errorList1.Dispose();
                    nxGeom[i] = origComponents1[0];
                    string reef = nxGeom[i].OwningComponent.DisplayName;
                    Console.WriteLine("***********" + reef);
                }
                /*
                NXOpen.Part workPart = nxSession.Parts.Work;
                NXOpen.Assemblies.Component component001;
                NXOpen.Point3d basePoint01 = new NXOpen.Point3d(0.0, 0.0, 0.0);
                NXOpen.Matrix3x3 orientation01 = new NXOpen.Matrix3x3();
                orientation01.Xx = 1.0;
                orientation01.Xy = 0.0;
                orientation01.Xz = 0.0;
                orientation01.Yx = 0.0;
                orientation01.Yy = 1.0;
                orientation01.Yz = 0.0;
                orientation01.Zx = 0.0;
                orientation01.Zy = 0.0;
                orientation01.Zz = 1.0;
                NXOpen.PartLoadStatus partLoadStatus02;
                component001 = workPart.ComponentAssembly.AddComponent("D:\\Macro\\A1post\\Body.prt", "MODEL", "BODY", basePoint01, orientation01, -1, out partLoadStatus02, true);

                NXOpen.Point3d basePoint2 = new NXOpen.Point3d(0.0, 0.0, 0.0);
                NXOpen.Matrix3x3 orientation2 = new NXOpen.Matrix3x3();
                orientation2.Xx = 1.0;
                orientation2.Xy = 0.0;
                orientation2.Xz = 0.0;
                orientation2.Yx = 0.0;
                orientation2.Yy = 1.0;
                orientation2.Yz = 0.0;
                orientation2.Zx = 0.0;
                orientation2.Zy = 0.0;
                orientation2.Zz = 1.0;
                NXOpen.PartLoadStatus partLoadStatus4;
                NXOpen.Assemblies.Component component002;
                component002 = workPart.ComponentAssembly.AddComponent("D:\\Macro\\A1post\\RotationPart.prt", "MODEL", "ROTATIONPART", basePoint2, orientation2, -1, out partLoadStatus4, true);
                NXOpen.Face face1 = (NXOpen.Face)component001.FindObject("PROTO#.Features|EXTRUDE(6)|FACE 140 {(25,0,30) EXTRUDE(6)}");
                NXOpen.Face face2 = (NXOpen.Face)component002.FindObject("PROTO#.Features|EXTRUDE(6)|FACE 140 {(-25,0,7.5) EXTRUDE(3)}");
                NXOpen.Positioning.Constraint constraint1;
                NXOpen.Positioning.ComponentPositioner componentPositioner1;
                componentPositioner1 = workPart.ComponentAssembly.Positioner;
                constraint1 = componentPositioner1.CreateConstraint(true);

                NXOpen.Positioning.ComponentConstraint componentConstraint1 = (NXOpen.Positioning.ComponentConstraint)constraint1;
                componentConstraint1.ConstraintAlignment = NXOpen.Positioning.Constraint.Alignment.ContraAlign;

                componentConstraint1.ConstraintType = NXOpen.Positioning.Constraint.Type.Touch;

                NXOpen.Positioning.ConstraintReference constraintReference1;
                constraintReference1 = componentConstraint1.CreateConstraintReference(component001, face1, true, false, false);
                NXOpen.Positioning.ConstraintReference constraintReference2;
                constraintReference2 = componentConstraint1.CreateConstraintReference(component002, face2, true, false, false);
                 */
                
                ComponentPositioner componentPositioner1;
                componentPositioner1 = nxSubAssem.ComponentAssembly.Positioner;
                componentPositioner1.BeginAssemblyConstraints();
                Constraint constraint1;
                constraint1 = componentPositioner1.CreateConstraint(true);
                ComponentConstraint componentConstraint1 = (NXOpen.Positioning.ComponentConstraint)constraint1;
                componentConstraint1.ConstraintAlignment = NXOpen.Positioning.Constraint.Alignment.ContraAlign;
                componentConstraint1.ConstraintType = NXOpen.Positioning.Constraint.Type.Touch;
                NXOpen.Assemblies.Component component01 = (NXOpen.Assemblies.Component)nxSubAssem.ComponentAssembly.RootComponent.FindObject("COMPONENT Body 1");
                string faceNameC1 = "PROTO#.Features|EXTRUDE(3)|FACE 130 {(0,0,20) EXTRUDE(3)}";
                NXOpen.Face face1 = (NXOpen.Face)component01.FindObject(faceNameC1);
                Face face4 = (NXOpen.Face)component01.FindObject("PROTO#.Features|EXTRUDE(6)|FACE 140 {(25,0,30) EXTRUDE(6)}");
                //NXOpen.Face face1 = (NXOpen.Face)component01.FindObject("PROTO#.Features|EXTRUDE(3)|FACE 130 {(0,0,20) EXTRUDE(3)}");
                NXOpen.Positioning.ConstraintReference constraintReference1;
                constraintReference1 = componentConstraint1.CreateConstraintReference(component01, face1, false, false, false);
                NXOpen.Assemblies.Component component02 = (NXOpen.Assemblies.Component)nxSubAssem.ComponentAssembly.RootComponent.FindObject("COMPONENT RotationPart 1");
               
                NXOpen.Face face2 = (NXOpen.Face)component02.FindObject("PROTO#.Features|EXTRUDE(3)|FACE 120 {(0,0,0) EXTRUDE(3)}");
                Face face3 = (NXOpen.Face)component02.FindObject("PROTO#.Features|EXTRUDE(6)|FACE 140 {(-25,0,7.5) EXTRUDE(3)}");
                NXOpen.Positioning.ConstraintReference constraintReference2;
                constraintReference2 = componentConstraint1.CreateConstraintReference(component02, face2, false, false, false);

                ComponentPositioner componentPositioner2;
                componentPositioner2 = nxSubAssem.ComponentAssembly.Positioner;
                componentPositioner2.BeginAssemblyConstraints();
                Constraint constraint2;
                constraint2 = componentPositioner2.CreateConstraint(true);
                ComponentConstraint componentConstraint2 = (NXOpen.Positioning.ComponentConstraint)constraint2;
                componentConstraint2.ConstraintType = NXOpen.Positioning.Constraint.Type.Touch;
                ConstraintReference constraintReference3 = componentConstraint2.CreateConstraintReference(component02,face3,true,false,false);
                ConstraintReference constraintReference4 = componentConstraint2.CreateConstraintReference(component01, face4, true, false, false);


                /*
                ComponentPositioner componentPositioner2;
                componentPositioner2 = nxSubAssem.ComponentAssembly.Positioner;
                componentPositioner2.BeginAssemblyConstraints();
                Constraint constraint2;
                constraint2 = componentPositioner2.CreateConstraint(true);
                ComponentConstraint componentConstraint2 = (NXOpen.Positioning.ComponentConstraint)constraint2;
                componentConstraint2.ConstraintAlignment = NXOpen.Positioning.Constraint.Alignment.ContraAlign;
                componentConstraint2.ConstraintType = NXOpen.Positioning.Constraint.Type.Touch;
                NXOpen.Assemblies.Component component001 = (NXOpen.Assemblies.Component)nxSubAssem.ComponentAssembly.RootComponent.FindObject("COMPONENT Body 1");
                string faceNameC3 = "PROTO#.Features|EXTRUDE(6)|FACE 140 {(25,0,30) EXTRUDE(6)}";
                NXOpen.Face face3 = (NXOpen.Face)component01.FindObject(faceNameC3);
                //NXOpen.Face face1 = (NXOpen.Face)component01.FindObject("PROTO#.Features|EXTRUDE(3)|FACE 130 {(0,0,20) EXTRUDE(3)}");
                NXOpen.Positioning.ConstraintReference constraintReference3;
                constraintReference3 = componentConstraint1.CreateConstraintReference(component001, face3, false, false, false);
                NXOpen.Assemblies.Component component002 = (NXOpen.Assemblies.Component)nxSubAssem.ComponentAssembly.RootComponent.FindObject("COMPONENT RotationPart 1");

                NXOpen.Face face4 = (NXOpen.Face)component02.FindObject("PROTO#.Features|EXTRUDE(6)|FACE 140 {(-25,0,7.5) EXTRUDE(3)}");
                NXOpen.Positioning.ConstraintReference constraintReference4;
                constraintReference4 = componentConstraint1.CreateConstraintReference(component002, face4, false, false, false);
                */

            }

            ///////////////////////
            PartSaveStatus fileSave;



            fileSave = nxRootAssem.Save(BasePart.SaveComponents.True, BasePart.CloseAfterSave.False);
        }
        public void Comparison()
        {
            for (int i = 0; i < 2; i++)
            {
                string partnam = tcad_PartList[i].Name;
                Console.WriteLine(partnam);
                int facecount = tcad_PartList[i].Solid.Faces.Count;
            
                for (int k = 1; k < facecount+1; k++)
                {
                  string name = tcad_PartList[i].Solid.Faces[k].Name;
                  Console.WriteLine(name);
                }
            }
        }
        
    }
}