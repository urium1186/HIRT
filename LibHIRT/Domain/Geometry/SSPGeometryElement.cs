namespace LibHIRT.Domain.Geometry
{

    public abstract class SSPGeometryElement
    {

        #region Data Members

        private uint _index;

        #endregion

        #region Properties

        public uint Index
        {
            get => _index;
        }

        public abstract SSPGeometryElementType ElementType { get; }

        #endregion

    }

}
