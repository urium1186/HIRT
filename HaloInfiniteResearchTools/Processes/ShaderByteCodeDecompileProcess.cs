using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.Processes
{
    public class ShaderByteCodeDecompileProcess : ProcessBase
    {
        byte[] _buffer;
        private bool _isRootSignature;

        public string DecompiledStr { get; private set; }
        public string DecompiledStrGLSL_V { get; private set; }
        public string DecompiledStrGLSL { get; private set; }

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

            string filePath_dxc = AppDomain.CurrentDomain.BaseDirectory + "Resources\\shaders\\dxc\\dxc.exe";
            string filePath_dxil_spirv = AppDomain.CurrentDomain.BaseDirectory + "Resources\\shaders\\dxil-spirv\\dxil-spirv.exe";
            string filePath_spirv_cross = AppDomain.CurrentDomain.BaseDirectory + "Resources\\shaders\\spirv-cross\\spirv-cross.exe";
            string tempFilename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".dxbc";
            string tempFilename_spv = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".spv";
            string out_dxc = "";
            string out_glsl = "";
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
                    Console.WriteLine(err);
                    process.WaitForExit();
                    DecompiledStr = out_dxc;
                    string err_dxil = "";
                    if (true)
                    {
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
                            Console.WriteLine(err_dxil);
                            process_dxil.WaitForExit();

                        }

                        if (!string.IsNullOrEmpty(err_dxil) && File.Exists(filePath_dxil_spirv) && File.Exists(tempFilename))
                        {
                            Process process_dxil_2 = new Process();
                            process_dxil_2.StartInfo.FileName = filePath_dxil_spirv;

                            process_dxil_2.StartInfo.Arguments = tempFilename + "--use-reflection-names --output " + tempFilename_spv; // Note the /c command (*)
                            process_dxil_2.StartInfo.UseShellExecute = false;
                            process_dxil_2.StartInfo.RedirectStandardOutput = true;
                            process_dxil_2.StartInfo.RedirectStandardError = true;
                            process_dxil_2.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            process_dxil_2.StartInfo.CreateNoWindow = true;
                            process_dxil_2.Start();
                            //* Read the output (or the error)
                            string output_dxil = process_dxil_2.StandardOutput.ReadToEnd();

                            Console.WriteLine(output_dxil);
                            err_dxil = process_dxil_2.StandardError.ReadToEnd();
                            Console.WriteLine(err_dxil);
                            process_dxil_2.WaitForExit();

                        }
                        if (File.Exists(filePath_spirv_cross) && File.Exists(tempFilename_spv))
                        {
                            Process process_spirv = new Process();
                            process_spirv.StartInfo.FileName = filePath_spirv_cross;

                            process_spirv.StartInfo.Arguments = tempFilename_spv + " -V"; // Note the /c command (*)
                            process_spirv.StartInfo.UseShellExecute = false;
                            process_spirv.StartInfo.RedirectStandardOutput = true;
                            process_spirv.StartInfo.RedirectStandardError = true;
                            process_spirv.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            process_spirv.StartInfo.CreateNoWindow = true;
                            process_spirv.Start();
                            //* Read the output (or the error)
                            string output_spirv = process_spirv.StandardOutput.ReadToEnd();
                            DecompiledStrGLSL_V = output_spirv;
                            Console.WriteLine(output_spirv);
                            string err_spirv = process_spirv.StandardError.ReadToEnd();
                            Console.WriteLine(err_spirv);
                            process_spirv.WaitForExit();

                        }

                        DecompiledStrGLSL = out_glsl;

                    }
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
