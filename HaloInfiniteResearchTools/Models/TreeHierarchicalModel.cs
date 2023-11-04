using System.Collections.Generic;

namespace HaloInfiniteResearchTools.Models
{
    public class TreeHierarchicalModel : IFileModel
    {
        private bool disposedValue;

        public string Name { get; set; }
        public List<TreeHierarchicalModel> Childrens { get; set; }
        public object Value { get; set; }
        public TreeHierarchicalModel()
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Childrens.Clear();
                    Childrens = null;// TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TreeHierarchicalModel()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            //System.GC.SuppressFinalize(this);
        }
    }
}
