using System;

namespace HaloInfiniteResearchTools.Common
{

  public static class GCHelper
  {

    public static void ForceCollect()
    {
      // WPF has odd GC behavior when it comes to bitmaps and some other
      // resource types. To actually clear them from memory, you need to
      // collect twice, while waiting for the finalizers in between collects.
      // Yes, I know this is hacky.
      GC.Collect();
      GC.WaitForPendingFinalizers();
      GC.Collect();
    }

  }

}
