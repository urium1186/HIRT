using LibHIRT.Common;
using System.Linq.Expressions;
using System.Reflection;
using static LibHIRT.Assertions;

namespace LibHIRT.Files
{
    internal static class SSpaceFileFactory
    {

        #region Delegates

        private delegate ISSpaceFile CreateFileDelegate(string name, ISSpaceFile parent = null);
        

        #endregion

        #region Data Members

        public static readonly IReadOnlyCollection<string> SupportedFileExtensions;
        public static readonly IReadOnlyCollection<string> NoSupportedFileExtensions;

        private static Dictionary<string, Type> _extensionLookup;
        private static Dictionary<string, Type> _signatureLookup;

        private static Dictionary<Type, CreateFileDelegate> _constructorLookup;

        #endregion

        #region Constructor

        static SSpaceFileFactory()
        {
            _extensionLookup = BuildExtensionLookup();
            _signatureLookup = BuildSignatureLookup();
            _constructorLookup = BuildConstructorLookup();

            SupportedFileExtensions = new HashSet<string>(_extensionLookup.Keys);
            NoSupportedFileExtensions = new HashSet<string>();
            (NoSupportedFileExtensions as HashSet<string>).Add(".module_hd1");
            (NoSupportedFileExtensions as HashSet<string>).Add(".xml");
        }

        #endregion

        #region Public Methods

        public static ISSpaceFile CreateFile(string name, string signature,
          ISSpaceFile parent = null)
        {
            var ext = Path.GetExtension(name);
            //var signature = ReadSignature(baseStream, dataStartOffset);

            if (!_signatureLookup.TryGetValue(signature, out var fileType))
                if (!_extensionLookup.TryGetValue(ext, out fileType))
                    if (!_signatureLookup.TryGetValue("_*.*", out fileType))
                        return FailReturn<ISSpaceFile>($"Could not determine a FileType for '{name}'.");

            if (!_constructorLookup.TryGetValue(fileType, out var ctorDelegate))
                return FailReturn<ISSpaceFile>($"FileType '{fileType.Name}' does not have a constructor delegate!");

            return ctorDelegate(name, parent);
        }

        #endregion

        #region Private Methods

        private static string ReadSignature(HIRTStream stream, long dataStartOffset)
        {
            stream.Position = dataStartOffset;
            var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, true);

            var signature = reader.ReadStringNullTerminated(maxLength: 32);
            reader.BaseStream.Position = 0;
            Assert(signature == "mohd4", $"Module wit signature worn");

            return signature;
        }

        private static CreateFileDelegate BuildConstructorDelegate(Type fileType)
        {
            const BindingFlags BINDING_FLAGS =
              BindingFlags.CreateInstance |
              BindingFlags.Instance |
              BindingFlags.Public |
              BindingFlags.NonPublic;

            // Get the constructor
            //var ctorArgTypes = new Type[] { typeof(string), typeof(HIRTStream), typeof(long), typeof(long), typeof(ISSpaceFile) };
            var ctorArgTypes = new Type[] { typeof(string), typeof(ISSpaceFile) };
            var ctorMethod = fileType.GetConstructor(BINDING_FLAGS, null, ctorArgTypes, new ParameterModifier[0]);
            Assert(ctorMethod != null, $"Could not find constructor for {fileType.Name}");

            // Get the initialize method
            var initializeMethod = fileType.GetMethod("Initialize", BINDING_FLAGS);
            Assert(ctorMethod != null, $"Could not find initialize method for {fileType.Name}");

            // Initialize the call arguments
            var nameParameter = Expression.Parameter(typeof(string), "name");
            /*var streamParameter = Expression.Parameter(typeof(HIRTStream), "stream");
            var startOffsetParameter = Expression.Parameter(typeof(long), "startOffset");
            var endOffsetParameter = Expression.Parameter(typeof(long), "endOffset");*/
            var parentParameter = Expression.Parameter(typeof(ISSpaceFile), "parent");
            var parameters = new[] { nameParameter, parentParameter };

            // Initialize the local variables
            var instanceVariable = Expression.Variable(fileType, "instance");

            var expressionBlock = Expression.Block(
              variables: new[] { instanceVariable },
              new Expression[]
              {
          // Call the constructor and set the instance variable
          Expression.Assign( instanceVariable, Expression.New( ctorMethod, parameters ) ),

          // Call the initialize method
          Expression.Call( instanceVariable, initializeMethod ),

          // Push the instance to the stack for return
          instanceVariable
              });

            return Expression.Lambda<CreateFileDelegate>(expressionBlock, parameters).Compile();
        }

        private static IEnumerable<Type> GetDefinedFileTypes()
        {
            return typeof(SSpaceFile).Assembly.GetTypes()
              .Where(x => !x.IsAbstract && typeof(ISSpaceFile).IsAssignableFrom(x));
        }

        private static Dictionary<Type, CreateFileDelegate> BuildConstructorLookup()
        {
            var ctorLookup = new Dictionary<Type, CreateFileDelegate>();

            foreach (var fileType in GetDefinedFileTypes())
                ctorLookup.Add(fileType, BuildConstructorDelegate(fileType));

            return ctorLookup;
        }

        private static Dictionary<string, Type> BuildExtensionLookup()
        {
            var extLookup = new Dictionary<string, Type>();

            foreach (var fileType in GetDefinedFileTypes())
            {
                var extAttributes = fileType.GetCustomAttributes(typeof(FileExtensionAttribute), false)
                  .Cast<FileExtensionAttribute>();

                foreach (var extAttribute in extAttributes)
                {
                    var extension = extAttribute.FileExtension;
                    extLookup.Add(extension, fileType);
                }
            }

            return extLookup;
        }

        private static Dictionary<string, Type> BuildSignatureLookup()
        {
            var sigLookup = new Dictionary<string, Type>();

            foreach (var fileType in GetDefinedFileTypes())
            {
                var signatureAttribute = fileType.GetCustomAttributes(typeof(FileSignatureAttribute), false).FirstOrDefault() as FileSignatureAttribute;
                if (signatureAttribute is null)
                    continue;

                sigLookup.Add(signatureAttribute.Signature, fileType);
            }

            return sigLookup;
        }

        #endregion

    }

}
