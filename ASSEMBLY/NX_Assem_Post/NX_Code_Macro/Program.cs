using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices; //For Marshal

using NXOpen;
using NXOpen.Assemblies;
using NXOpen.Positioning;
using NXOpen.UF;

namespace NX_Code_Macro
{

    class Program
    {
        private static NXOpen.Session nxSession;
        private static NXOpen.UF.UFSession ufSession;
       // private static TransCAD.Application tApp;

        static void Main(string[] args)
        {
            

            System.Console.WriteLine("parameter count = {0}", args.Length);

            


            var AssemManager = new Assembly();//Instance of Assembly Class
            var fileDir = new NewNXfile(); //Contains File Directory Infromation
            var newfile = new NewNXfile(); // Contains New File Creation Code
            var constraint = new ConstraintClass(); // Containts Code of Constraint Class
            fileDir.initiatefl(args[0]);
            nxSession = Session.GetSession();
            AssemManager.NXRootFile(nxSession, fileDir);
            
/*
            NXOpen.FileNew nxRootCompFile = nxSession.Parts.FileNew(); 
           
            newfile.NXfile(nxRootCompFile, fileDir.rootDir());
            NXOpen.NXObject nXObject_RootComp = nxRootCompFile.Commit();
            nxRootCompFile.MakeDisplayedPart = true;

            Part nxRootAssem = nxSession.Parts.Work;
           
            nxRootCompFile.Destroy();
*/
            AssemManager.InitializeTransCAD();
            AssemManager.loadAssemblyInfo(nxSession);
            //AssemManager.Comparison();
            TransCAD.IStdAssemConstraint[] GetConstraints= new TransCAD.IStdAssemConstraint[constraint.NumOfConstraint(AssemManager)];
            TransCAD.StdAssemConstraintCoaxial ConstraintCoaxial;
            TransCAD.StdAssemConstraintIncidence ConstraintIncidence;
            string[] ConsPrtName=new string[constraint.NumOfConstraint(AssemManager)];
            string[] ConsGeoName = new string[constraint.NumOfConstraint(AssemManager)];
            string[] RefPrtName = new string[constraint.NumOfConstraint(AssemManager)];
            string[] RefGeoName = new string[constraint.NumOfConstraint(AssemManager)];
            

            for (int i = 0; i < constraint.NumOfConstraint(AssemManager); i++)
            {
                GetConstraints[i] = AssemManager.tAssem.Constraints.GetConstraint(i + 1);
                if (i == 0)
                {
                    ConstraintCoaxial = (TransCAD.StdAssemConstraintCoaxial)GetConstraints[i];
                    ConsPrtName[i] = ConstraintCoaxial.ConstrainedPart.Name;
                    ConsGeoName[i] = ConstraintCoaxial.ConstrainedGeometry.ReferenceeName;
                    RefPrtName[i] = ConstraintCoaxial.ReferencePart.Name;
                    RefGeoName[i] = ConstraintCoaxial.ReferenceGeometry.ReferenceeName;
                }
                else
                {
                    ConstraintIncidence = (TransCAD.StdAssemConstraintIncidence)GetConstraints[i];
                    ConsPrtName[i] = ConstraintIncidence.ConstrainedPart.Name;         
                    ConsGeoName[i] = ConstraintIncidence.ConstrainedGeometry.ReferenceeName;
                    RefPrtName[i] = ConstraintIncidence.ReferencePart.Name;
                    RefGeoName[i] = ConstraintIncidence.ReferenceGeometry.ReferenceeName;
                    
                }
                Console.WriteLine("Constraints Part" + i + " Name:: " + ConsPrtName[i]);
                Console.WriteLine("Constraints Geometry" + i + "Geometry:: " + ConsGeoName[i]);
                Console.WriteLine("Reference Part" + i + " Name:: " + RefPrtName[i]);
                Console.WriteLine("Reference Geometry" + i + "Geometry:: " + RefGeoName[i]);
               
            }
            for (int i = 0; i < 2; i++)
            {
                string partnam = AssemManager.tcad_PartList[i].Name;
                Console.WriteLine(partnam);
                int facecount = AssemManager.tcad_PartList[i].Solid.Faces.Count;
                for (int l = 0; l < 2; l++)
                {
                    for (int k = 1; k < facecount + 1; k++)
                    {
                        string name = AssemManager.tcad_PartList[i].Solid.Faces[k].Name;
                        if (ConsGeoName[l] == name)
                        {
                            Console.WriteLine("The maching face in " + partnam + " is " + k + " for constraint " + l);
                            Console.WriteLine(name);
                            
                            int numVertex = AssemManager.tcad_PartList[i].Solid.Faces[k].Vertices.Count;
                            Console.WriteLine("Number of Vertices in Constrained Geometry" + numVertex);
                            {
                                Console.WriteLine("Constrainted Geometry is :::");
                                for (int m = 1; m < numVertex; m++)
                                {
                                    
                                    TransCAD.Vertex tv = AssemManager.tcad_PartList[i].Solid.Faces[k].Vertices[m];
                                    Console.WriteLine(" (" + tv.X + "," + tv.Y + "," + tv.z + ") ");
                                }
                            }
                            break;
                        }
                        if (RefGeoName[l] == name)
                        {
                            Console.WriteLine("The matching face in " + partnam + " is " + k  +" for constraint " + l);
                            Console.WriteLine(name);
                            Console.WriteLine("Reference Geometry is :::");
                            int numVertex = AssemManager.tcad_PartList[i].Solid.Faces[k].Vertices.Count;
                            Console.WriteLine("Number of Vertices in Reference Geometry" + numVertex);
                            {
                                for (int m = 1; m < numVertex; m++)
                                {
                                    TransCAD.Vertex tv = AssemManager.tcad_PartList[i].Solid.Faces[k].Vertices[m];
                                    Console.WriteLine(" (" + tv.X + "," + tv.Y + "," + tv.z + ") ");
                                }
                            }
                            break;
                        }

                    }
                }
               
            }

            

       }
    }

}
