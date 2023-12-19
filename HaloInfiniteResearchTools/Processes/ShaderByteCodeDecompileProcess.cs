using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using WpfHexaEditor.Core.MethodExtention;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace HaloInfiniteResearchTools.Processes
{

    public class ShaderByteCodeDecompileProcess : ProcessBase
    {
        /* 
       // Import the D3DCompile function from dxcompiler.dll
       [DllImport("Resources\\shaders\\dxc\\dxcompiler.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "D3DDisassemble")]
       public static extern int D3DDisassemble(
           IntPtr pShader,
           uint BytecodeLength,
           uint Flags,
           IntPtr pComments,
           out IntPtr ppDisassembly
       );

       // Function to disassemble a DXBC file
       public static string DisassembleDXBC(string filePath)
       {
           // Load the DXBC file into memory
           byte[] shaderBytes = System.IO.File.ReadAllBytes(filePath);

           // Allocate memory for the disassembly
           IntPtr disassemblyPtr;
           int result = D3DDisassemble(
               Marshal.UnsafeAddrOfPinnedArrayElement(shaderBytes, 0),
               (uint)shaderBytes.Length,
               0,
               IntPtr.Zero,
               out disassemblyPtr
           );

           if (result >= 0)
           {
               // Convert the disassembly pointer to a string
               string disassembly = Marshal.PtrToStringAnsi(disassemblyPtr);

               // Free the allocated memory
               Marshal.FreeCoTaskMem(disassemblyPtr);

               return disassembly;
           }
           else
           {
               // Handle the error (for simplicity, printing the error code)
               Console.WriteLine($"Error disassembling the shader: 0x{result:X}");
               return null;
           }
       }

       */

        // Importar DxcCreateInstance desde dxcompiler.dll
        [DllImport("Resources\\shaders\\dxc\\dxcompiler.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi, EntryPoint = "DxcCreateInstance")]
        public static extern int DxcCreateInstance(
            [MarshalAs(UnmanagedType.LPStruct)] Guid rclsid,
            [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppv
        );

        
        public static string DisassembleDXBC(string filePath)
        {
            Guid clsid = new Guid("..."); // Reemplaza con el GUID de la clase específica que deseas crear
            Guid iid = new Guid("...");   // Reemplaza con el GUID de la interfaz que deseas obtener

            // Crear una instancia del objeto DirectX Compiler
            object dxcInstance;
            int result = DxcCreateInstance(clsid, iid, out dxcInstance);

            if (result == 0)
            {
                // La instancia se creó correctamente, ahora puedes utilizarla según tus necesidades

                // Por ejemplo, si deseas obtener una interfaz específica (como IDxcCompiler):
                // IDxcCompiler dxcCompiler = (IDxcCompiler)dxcInstance;
                // Ahora puedes utilizar dxcCompiler para compilar shaders, etc.

                Console.WriteLine("Instancia creada con éxito");
            }
            else
            {
                // Manejar el error
                Console.WriteLine($"Error al crear la instancia. Código de error: {result}");
            }
            return "";
        }


        byte[] _buffer;
        private bool _isRootSignature;

        public string DecompiledStr { get; private set; }
        public string DecompiledDxil { get; private set; }
        public string DecompiledStrHLSL { get; private set; }
        public string DecompiledStrGLSL { get; private set; }
        public string DecompiledStrSPV { get; private set; }

        public ShaderByteCodeDecompileProcess(byte[] buffer, bool isRootSignature = false)
        {
            _buffer = buffer;
            _isRootSignature = isRootSignature;
        }

        protected override async Task OnExecuting()
        {
            if (_isRootSignature)
            {
                await DecompileRootSignature();
            }
            else
            {
                await DecompileByteCode();
            }

        }

        private async Task DecompileRootSignature()
        {
            Process process = new Process();
            string directory = "C:\\Program Files (x86)\\Windows Kits\\10\\bin\\";
            //string filePath_dxc = "C:\\Program Files (x86)\\Windows Kits\\10\\bin\\10.0.19041.0\\x64\\fxc.exe";

            if (!Directory.Exists(directory))
            {
                DecompiledStr = "";
                return;
            }


            var directoryFiles = Directory.GetFiles(directory, "fxc.exe", SearchOption.AllDirectories)
                  .Where(IsFileExtensionRecognized);
            if (directoryFiles.Count() > 0)
            {
                //string lastVersionPath = directoryFiles.ToImmutableSortedSet().ToList()[0];
                string lastVersionPath = directoryFiles.ToImmutableSortedSet().ToList()[directoryFiles.Count() - 1];
                if (File.Exists(lastVersionPath))
                {
                    string tempFilename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".fxo";
                    //string tempFilename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".cso";
                    //string tempFilename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".glsl";
                    //string tempFilename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".fxo";
                    FileStream tempStream = new FileStream(tempFilename, FileMode.CreateNew);
                    tempStream.Write(_buffer);
                    await tempStream.FlushAsync();
                    tempStream.Close();
                    process.StartInfo.FileName = lastVersionPath;
                    //"/dumpbin rs1.fxo /extractrootsignature /Fo rs1.rs.fxo"
                    //" fxc /dumpbin rs1.stripped.fxo /setrootsignature rs1.rs.fxo /Fo rs1.new.fxo"

                    process.StartInfo.Arguments = "/dumpbin " + tempFilename + " /extractrootsignature"; // Note the /c command (*)
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();
                    //* Read the output (or the error)
                    string output = process.StandardOutput.ReadToEnd();
                    DecompiledStr = output;
                    Console.WriteLine(output);
                    string err = process.StandardError.ReadToEnd();
                    Console.WriteLine(err);
                    File.Delete(tempFilename);
                    process.WaitForExit();
                }
            }




        }
        private async Task DecompileRootSignatureOld()
        {
            Process process = new Process();
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "Resources\\shaders\\dxc\\dxc.exe";
            if (File.Exists(filePath))
            {
                string tempFilename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".dxbc";
                //string tempFilename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".cso";
                //string tempFilename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".glsl";
                //string tempFilename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".fxo";
                FileStream tempStream = new FileStream(tempFilename, FileMode.CreateNew);
                tempStream.Write(_buffer);
                await tempStream.FlushAsync();
                tempStream.Close();
                process.StartInfo.FileName = filePath;

                process.StartInfo.Arguments = "-dumpbin /ROOT " + tempFilename; // Note the /c command (*)
                //process.StartInfo.Arguments = "-extractrootsignature /Fo " + tempFilename; // Note the /c command (*)
                //process.StartInfo.Arguments = "- verifyrootsignature " + tempFilename; // Note the /c command (*)
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                //* Read the output (or the error)
                string output = process.StandardOutput.ReadToEnd();
                DecompiledStr = output;
                Console.WriteLine(output);
                string err = process.StandardError.ReadToEnd();
                Console.WriteLine(err);
                File.Delete(tempFilename);
                process.WaitForExit();
            }
        }
        private async Task DecompileByteCode()
        {
            DecompiledStrSPV = "";
            DecompiledDxil = "";
            string filePath_dxc = AppDomain.CurrentDomain.BaseDirectory + "Resources\\shaders\\dxc\\dxc.exe";
            string filePath_dxil_spirv = AppDomain.CurrentDomain.BaseDirectory + "Resources\\shaders\\dxil-spirv\\dxil-spirv.exe";
            string filePath_dxil_extract = AppDomain.CurrentDomain.BaseDirectory + "Resources\\shaders\\dxil-spirv\\dxil-extract.exe";
            string filePath_dxil_extract_args = " --reflection"; //  --verbose
            string filePath_spirv_cross = AppDomain.CurrentDomain.BaseDirectory + "Resources\\shaders\\spirv-cross\\spirv-cross.exe";
            string tempFilename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".dxbc";
            string tempFilename_spv = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".spv";
            string out_dxc = "";
            string out_glsl = "";
            string out_dxil = "";
            if (File.Exists(filePath_dxc))
            {
                try
                {
                    //string tempFilename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".cso";
                    //string tempFilename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".glsl";
                    //string tempFilename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".fxo";
                    FileStream tempStream = new FileStream(tempFilename, FileMode.CreateNew);
                    tempStream.Write(_buffer);
                    await tempStream.FlushAsync();
                    tempStream.Close();


                    //string out_str = ShaderByteCodeDecompileProcess.DisassembleDXBC(tempFilename);

                    Process process = new Process();
                    process.StartInfo.FileName = filePath_dxc;

                    process.StartInfo.Arguments = "-dumpbin /ALL " + tempFilename; // Note the /c command (*)
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();
                    //* Read the output (or the error)
                    string output = process.StandardOutput.ReadToEnd();
                    out_dxc = output;
                    Console.WriteLine(output);
                    string err = process.StandardError.ReadToEnd();
                    if (!string.IsNullOrEmpty(err))
                        StatusList.AddError("error on dumpbin", err);
                    Console.WriteLine(err);
                    process.WaitForExit();
                    DecompiledStr = out_dxc;
                    string err_dxil = "";

                    if (File.Exists(filePath_dxil_extract) && File.Exists(tempFilename))
                    {
                        Process process_dxil_extract = new Process();
                        process_dxil_extract.StartInfo.FileName = filePath_dxil_extract;

                        process_dxil_extract.StartInfo.Arguments =  tempFilename+ filePath_dxil_extract_args; // Note the /c command (*)
                        process_dxil_extract.StartInfo.UseShellExecute = false;
                        process_dxil_extract.StartInfo.RedirectStandardOutput = true;
                        process_dxil_extract.StartInfo.RedirectStandardError = true;
                        process_dxil_extract.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        process_dxil_extract.StartInfo.CreateNoWindow = true;
                        process_dxil_extract.Start();
                        //* Read the output (or the error)

                        err_dxil = process_dxil_extract.StandardError.ReadToEnd();
                        if (!string.IsNullOrEmpty(err_dxil))
                            StatusList.AddError("error on dxil", err_dxil);
                        Console.WriteLine(err_dxil);

                        string output_dxil = process_dxil_extract.StandardOutput.ReadToEnd();
                        out_dxil = output_dxil;
                        DecompiledDxil = err_dxil + output_dxil;
                        Console.WriteLine(output_dxil);
                        
                        process_dxil_extract.WaitForExit();

                    }

                    if (File.Exists(filePath_dxil_spirv) && File.Exists(tempFilename))
                    {
                        Process process_dxil = new Process();
                        process_dxil.StartInfo.FileName = filePath_dxil_spirv;

                        process_dxil.StartInfo.Arguments = "--use-reflection-names --glsl " + tempFilename; // Note the /c command (*)
                        process_dxil.StartInfo.UseShellExecute = false;
                        process_dxil.StartInfo.RedirectStandardOutput = true;
                        process_dxil.StartInfo.RedirectStandardError = true;
                        process_dxil.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        process_dxil.StartInfo.CreateNoWindow = true;
                        process_dxil.Start();
                        //* Read the output (or the error)
                        string output_dxil = process_dxil.StandardOutput.ReadToEnd();
                        out_glsl = output_dxil;
                        Console.WriteLine(output_dxil);
                        err_dxil = process_dxil.StandardError.ReadToEnd();
                        if (!string.IsNullOrEmpty(err_dxil)) {
                            if (err_dxil.IndexOf("Exception")!=-1 || err_dxil.IndexOf("Error") != -1) {
                                StatusList.AddError("error on dxil", err_dxil);
                            }
                        }
                            
                        Console.WriteLine(err_dxil);
                        process_dxil.WaitForExit();

                    }
                    /*
                     Another way is if you can manage to convert the byte code to SPIR-V somehow. There are many great tools to analyze and even decompile SPIR-V back to HLSL and GLSL:

                    SPIRV Cross
                    SPIRV Viewer

                    There is a way to convert DXIL (shader model 6) to SPIRV with dxil-spirv

                     */
                    // SPIRV
                    if (string.IsNullOrEmpty(err_dxil) && File.Exists(filePath_dxil_spirv) && File.Exists(tempFilename))
                    {
                        Process process_dxil_2 = new Process();
                        process_dxil_2.StartInfo.FileName = filePath_dxil_spirv;

                        process_dxil_2.StartInfo.Arguments = tempFilename + " --use-reflection-names --output " + tempFilename_spv; // Note the /c command (*)
                        process_dxil_2.StartInfo.UseShellExecute = false;
                        process_dxil_2.StartInfo.RedirectStandardOutput = true;
                        process_dxil_2.StartInfo.RedirectStandardError = true;
                        process_dxil_2.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        process_dxil_2.StartInfo.CreateNoWindow = true;
                        process_dxil_2.Start();
                        //* Read the output (or the error)
                        string output_dxil = process_dxil_2.StandardOutput.ReadToEnd();
                        if (!string.IsNullOrEmpty(output_dxil)) {
                            DecompiledStrSPV = output_dxil;
                            Console.WriteLine(output_dxil);
                        }

                        
                        err_dxil = process_dxil_2.StandardError.ReadToEnd();
                            
                        Console.WriteLine(err_dxil);
                        if (!string.IsNullOrEmpty(err_dxil))
                            StatusList.AddError("error on dxil2", err_dxil);
                        process_dxil_2.WaitForExit();

                    }
                    if (string.IsNullOrEmpty(err_dxil) && File.Exists(filePath_dxil_spirv) && File.Exists(tempFilename))
                    {
                        Process process_dxil_2 = new Process();
                        process_dxil_2.StartInfo.FileName = filePath_dxil_spirv;

                        process_dxil_2.StartInfo.Arguments = tempFilename + " --use-reflection-names";
                        process_dxil_2.StartInfo.UseShellExecute = false;
                        process_dxil_2.StartInfo.RedirectStandardOutput = true;
                        process_dxil_2.StartInfo.RedirectStandardError = true;
                        process_dxil_2.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        process_dxil_2.StartInfo.CreateNoWindow = true;
                        process_dxil_2.Start();
                        //* Read the output (or the error)
                        string output_dxil = process_dxil_2.StandardOutput.ReadToEnd();
                        if (!string.IsNullOrEmpty(output_dxil))
                        {
                            DecompiledStrSPV = output_dxil;
                            Console.WriteLine(output_dxil);
                        }


                        err_dxil = process_dxil_2.StandardError.ReadToEnd();

                        Console.WriteLine(err_dxil);
                        if (!string.IsNullOrEmpty(err_dxil))
                            StatusList.AddError("error on dxil2", err_dxil);
                        process_dxil_2.WaitForExit();

                    }
                    if (File.Exists(filePath_spirv_cross) && File.Exists(tempFilename_spv))
                    {
                        Process process_spirv = new Process();
                        process_spirv.StartInfo.FileName = filePath_spirv_cross;

                        process_spirv.StartInfo.Arguments = tempFilename_spv + " -V --hlsl --no-es --hlsl-preserve-structured-buffers --shader-model 60"; // Note the /c command (*) --dump-resources 
                        process_spirv.StartInfo.UseShellExecute = false;
                        process_spirv.StartInfo.RedirectStandardOutput = true;
                        process_spirv.StartInfo.RedirectStandardError = true;
                        process_spirv.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        process_spirv.StartInfo.CreateNoWindow = true;
                        process_spirv.Start();
                        //* Read the output (or the error)
                        string output_spirv = process_spirv.StandardOutput.ReadToEnd();
                        DecompiledStrHLSL = output_spirv;
                        Console.WriteLine(output_spirv);
                        string err_spirv = process_spirv.StandardError.ReadToEnd();
                        if (!string.IsNullOrEmpty(err_spirv) && err_spirv != output_spirv) {
                            DecompiledStrHLSL = err_spirv + output_spirv;
                            if (err_spirv.IndexOf("Exception") != -1) {
                                StatusList.AddError("error on spirv", err_spirv);
                            }
                            
                        }
                            
                        Console.WriteLine(err_spirv);
                        process_spirv.WaitForExit();

                    }

                    DecompiledStrGLSL = out_glsl;

                    
                    if (File.Exists(tempFilename))
                        File.Delete(tempFilename);
                    if (File.Exists(tempFilename_spv))
                        File.Delete(tempFilename_spv);
                }
                catch (Exception ex)
                {
                    DecompiledStrGLSL = out_glsl;
                    DecompiledStr = out_dxc;
                    if (File.Exists(tempFilename))
                        File.Delete(tempFilename);
                    if (File.Exists(tempFilename_spv))
                        File.Delete(tempFilename_spv);
                    StatusList.AddError($"Error on shbc", ex);
                    throw ex;
                }





            }
        }

        private static bool IsFileExtensionRecognized(string filePath)
        {

            return filePath.Contains("x64");
        }
    }
}
