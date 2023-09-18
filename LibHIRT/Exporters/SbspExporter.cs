using Aspose.ThreeD;
using Aspose.ThreeD.Deformers;
using Aspose.ThreeD.Entities;
using Aspose.ThreeD.Formats;
using Aspose.ThreeD.Shading;
using LibHIRT.Domain;
using LibHIRT.Exporters.Converters;
using LibHIRT.Exporters.Utils;
using LibHIRT.Files.FileTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.Exporters
{
    public class SbspExporter
    {
        static private FbxSaveOptions _saveOpts;

        public SbspExporter()
        {
            _saveOpts = new FbxSaveOptions(FileFormat.FBX7500Binary);
        }

        public static bool Export(ScenarioStructureBspFile file, string path, string name)
        {
            try
            {
                Scene scene = new Scene();

                scene.AssetInfo.ApplicationName = "HIRT";

                // Set application/tool vendor name
                scene.AssetInfo.ApplicationVendor = "urium86";

                // We use ancient egyption measurement unit Pole
                scene.AssetInfo.UnitName = "cm";

                // One Pole equals to 60cm
                scene.AssetInfo.UnitScaleFactor = 1.00;

                scene.AssetInfo.CoordinatedSystem = CoordinatedSystem.RightHanded;

                

                // create a box to which the material will be applied
                SbspConverter temp_convert = new SbspConverter(file);


                scene.RootNode.AddChildNode(temp_convert.BuildFullEntity());
                //temp_convert.BuildFullEntity(scene.RootNode);
                //temp_convert.BuildFullEntity();
                
                //PolygonModifier.Scale(scene, new Aspose.ThreeD.Utilities.Vector3(0.01));
                CoordinateSystemTools.ChangeCoordenate(scene.RootNode);
                //boxNode.Material = mat;
                FbxSaveOptions _saveOpts = new FbxSaveOptions(FileFormat.FBX7500Binary);
                //ObjSaveOptions _saveOpts = new ObjSaveOptions();
                //ColladaSaveOptions _saveOpts = new ColladaSaveOptions();
                //UsdSaveOptions _saveOpts = new UsdSaveOptions();
                // save 3d scene into STL format
                //string out_path = Path.Combine(path, name, name + @".fbx");
                string out_path = Path.Combine(path, name, name + _saveOpts.FileFormat.Extension); 
                Directory.CreateDirectory(Path.GetDirectoryName(out_path));
                scene.Save(out_path, _saveOpts);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }
    }
}
