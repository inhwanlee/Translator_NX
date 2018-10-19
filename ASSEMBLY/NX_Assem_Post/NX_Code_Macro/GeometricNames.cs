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
    class GeometricNames
    {
        private static string rename, featureName;
        public string LoadName(Session nxSession,string dir,Point3d[] fromtcad)
        {
            var loadpartinfo = new NXPartFile();
            int count = 01;
            loadpartinfo.FileLoad(nxSession,dir);
            loadpartinfo.BodzinPart(dir);
            string feature="";
            Point3d[] fromNX;
            int i = 0;
            for (int z = 0; z < loadpartinfo.NumOfAtribs(); z++)
            {
                count = 1;
                fromNX = loadpartinfo.FVrtM(z);
                Console.WriteLine(fromNX.Length + "\n" + fromtcad.Length);
                if ((fromNX.Length == fromtcad.Length) || (fromNX.Length == fromtcad.Length-1) && (fromNX.Length>1))
                {
                    Point3d checkchange;
                    Console.WriteLine("Condition satisfies");
                    for (int j = 0; j < fromNX.Length; j++)
                    {
                        for (int k = 0; k < fromNX.Length; k++)
                        {
                            if ((fromtcad[j].X == fromNX[k].X ) && (fromtcad[j].Y == fromNX[k].Y ) && (fromtcad[j].Z == fromNX[k].Z ))
                            {
                                count = count + 1;
                                break;
                            }
                        }
                    }
                }
                if ((count >= fromNX.Length)&&(fromNX.Length>1))
                {
                    rename = loadpartinfo.nxName(loadpartinfo.FVrtM(z));
                    break;
                }
            }
            for (int y = 0; y < loadpartinfo.BodzinPart(dir); y++)
            {
                string fName = loadpartinfo.featurename(y);
                string[] fFeatureN = loadpartinfo.faceinfeature(fName);
                for (int x = 0; x < fFeatureN.Length; x++)
                {
                    if (fFeatureN[x].Contains(rename))
                    {
                        featureName = loadpartinfo.featurename(y);
                        break;
                    }
                }
            }
            string macthedFace = "PROTO#.Features|" + featureName + "|" + rename;
            return macthedFace;
        }
    }
}
