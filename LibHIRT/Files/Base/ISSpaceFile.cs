namespace LibHIRT.Files
{
    public interface ISSpaceFile : IDisposable, IEquatable<ISSpaceFile>
    {

        #region Properties

        ISSpaceFile Parent { get; }
        IEnumerable<ISSpaceFile> Children { get; }

        ISSpaceFile RefParent { get; set; }
        Dictionary<int, ISSpaceFile> RefChildren { get; }

        string Name { get; }
        string Extension { get; }
        long SizeInBytes { get; }

        long Hash { get; }
        string Path_string { get; }
        string InDiskPath { get; set; }
        string TagGroup { get; }
        string FileTypeDisplay { get; }

        #endregion

        #region Public Methods

        HIRTStream GetStream();

        int TryGetGlobalId();

        #endregion

    }
}
