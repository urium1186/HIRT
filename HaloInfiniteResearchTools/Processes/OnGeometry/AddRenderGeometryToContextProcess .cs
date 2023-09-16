using Assimp;
using HaloInfiniteResearchTools.Assimport;
using LibHIRT.Domain;
using System.Threading;
using System.Threading.Tasks;


namespace HaloInfiniteResearchTools.Processes.OnGeometry
{

    public class AddRenderGeometryToContextProcess : ProcessBase<Scene>
    {

        #region Data Members



        private HISceneContext _context;
        private readonly RenderGeometry _renderGeometry;
        private readonly string _prefixMeshName;
        private readonly object _statusLock;

        private Matrix4x4 _initialTransform;

        #endregion

        #region Properties

        //public override Scene Result => new Scene();
        public override Scene Result => _context.Scene;

        public CancellationToken GetCancelToken => CancellationToken;

        internal HISceneContext Context { get => _context; set => _context = value; }
        #endregion

        #region Constructor

        public AddRenderGeometryToContextProcess(HISceneContext context, RenderGeometry renderGeometry, string prefixMeshName, Matrix4x4 initialTransform = default)
        {
            _renderGeometry = renderGeometry;
            _prefixMeshName = prefixMeshName;
            _context = context;
            _statusLock = new object();
            _initialTransform = initialTransform;
        }

        #endregion

        #region Overrides

        protected override async Task OnExecuting()
        {
            Node temp = _context.AddRenderGeometry(_prefixMeshName, _renderGeometry);
            _context.Scene.RootNode = temp;
        }

        #endregion


    }


}
