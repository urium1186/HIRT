using DeepCopy;
using HaloInfiniteResearchTools.Models;

namespace HaloInfiniteResearchTools.Common
{

  public interface IDeepCopy<T>
  {
  }

    public static class DeepCopyExtensions
    {

        [DeepCopyExtension]
        public static ModelExportOptionsModel DeepCopy(this ModelExportOptionsModel source)
          => source;

        [DeepCopyExtension]
        public static ModelViewerOptionsModel DeepCopy(this ModelViewerOptionsModel source)
          => source;

        [DeepCopyExtension]
        public static TextureExportOptionsModel DeepCopy(this TextureExportOptionsModel source)
          => source;

        [DeepCopyExtension]
        public static TextureViewerOptionsModel DeepCopy(this TextureViewerOptionsModel source)
          => source;

        [DeepCopyExtension]
        public static TagStructsDumperOptionsModel DeepCopy(this TagStructsDumperOptionsModel source)
                  => source;


    }

}
