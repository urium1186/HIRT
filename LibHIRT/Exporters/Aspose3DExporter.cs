using Aspose.ThreeD.Formats;
using Aspose.ThreeD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibHIRT.Exporters
{
    public static class Aspose3DExporter
    {
        public static SaveOptions GetSaveOptions(string extension = "dae")
        {
            SaveOptions saveOpts = new ColladaSaveOptions();
            
            if (extension == "fbx") {
                saveOpts = new FbxSaveOptions(FileContentType.Binary);
                //FbxSaveOptions _saveOpts = new FbxSaveOptions(FileFormat.FBX7500Binary);
                //_saveOpts.GenerateVertexElementMaterial = true;
                //_saveOpts.ExportLegacyMaterialProperties = true;
            }
            

            return saveOpts;
        }
    }
}
