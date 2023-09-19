using Aspose.ThreeD;
using Aspose.ThreeD.Deformers;
using Aspose.ThreeD.Formats;
using Aspose.ThreeD.Shading;
using LibHIRT.Domain;
using LibHIRT.Exporters.Converters;

namespace LibHIRT.Exporters
{
    public static class RenderGeometryExporter
    {
        
        static RenderGeometryExporter()
        {
            
            //_saveOpts.GenerateVertexElementMaterial = true;
            /*
             // Generates the legacy material properties.
            _saveOpts.ExportLegacyMaterialProperties = true;
            // Fold repeated curve data using FBX's animation reference count
            _saveOpts.FoldRepeatedCurveData = true;
            // Always generates material mapping information for geometries if the attached node contains materials.
            _saveOpts.GenerateVertexElementMaterial = true;
            // Configure the look up paths to allow importer to find external dependencies.
            _saveOpts.LookupPaths = new List<string>(new string[] { dataDir });
            // Generates a video object for texture.
            _saveOpts.VideoForTexture = true;*/

        }

        public static bool Export(RenderGeometry renderGeometry, string path, string name, string format = "dae")
        {
            return Export(renderGeometry, path, name,  null, null, format);
        }
        public static bool Export(RenderGeometry renderGeometry, string path, string name, List<Material> materials, string format = "dae")
        {
            return Export(renderGeometry, path, name, materials, null, format);
        }
        public static bool Export(RenderGeometry renderGeometry, string path, string name, List<Bone> bones, string format = "dae")
        {
            return Export(renderGeometry, path, name, null, bones, format);
        }

        public static bool Export(RenderGeometry renderGeometry, string path, string name, List<Material> materials, List<Bone> bones, string format = "dae")
        {
            try
            {
                Scene scene = new Scene();



                // create a box to which the material will be applied
                RenderGeometryConverter temp_convert = new RenderGeometryConverter(renderGeometry, materials);


                scene.RootNode.AddChildNode(temp_convert.BuildFullEntity());

                //boxNode.Material = mat;
                
                
                SaveOptions _saveOpts = Aspose3DExporter.GetSaveOptions(format);
                // save 3d scene into STL format
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
