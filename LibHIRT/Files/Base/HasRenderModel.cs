using LibHIRT.Files.FileTypes;

namespace LibHIRT.Files.Base
{
    public interface HasRenderModel
    {
        RenderModelFile GetRenderModel();
    }
    public interface HasModel
    {
        ModelFile GetModel();
    }
}
