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

        public string DecompiledStr { get; set; }

        public ShaderByteCodeDecompileProcess(byte[] buffer, bool isRootSignature = false)
        {
            _buffer = buffer;
            _isRootSignature = isRootSignature;
        }

        protected override async Task OnExecuting()
        {
            if (_isRootSignature) {
                await DecompileRootSignature();
            }
            else {
                await DecompileByteCode();
            }
            
        }

        private async Task DecompileRootSignatureOld()
        {
            Process process = new Process();
            string directory = "C:\\Program Files (x86)\\Windows Kits\\10\\bin\\";
            //string filePath = "C:\\Program Files (x86)\\Windows Kits\\10\\bin\\10.0.19041.0\\x64\\fxc.exe";

            if (!Directory.Exists(directory))
            {
                DecompiledStr = "";
                return;
            }


            var directoryFiles = Directory.GetFiles(directory, "fxc.exe", SearchOption.AllDirectories)
                  .Where(IsFileExtensionRecognized);
            if (directoryFiles.Count() > 0) {
                string lastVersionPath = directoryFiles.ToImmutableSortedSet().ToList()[0];
                //string lastVersionPath = directoryFiles.ToImmutableSortedSet().ToList()[directoryFiles.Count()-1];
                if (File.Exists(lastVersionPath))
                {
                    string tempFilename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString();// + ".fxo";
                    //string tempFilename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".cso";
                    //string tempFilename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".glsl";
                    //string tempFilename = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".fxo";
                    FileStream tempStream = new FileStream(tempFilename, FileMode.CreateNew);
                    tempStream.Write(_buffer);
                    await tempStream.FlushAsync();
                    tempStream.Close();
                    process.StartInfo.FileName = lastVersionPath;

                    process.StartInfo.Arguments = "-dumpbin " + tempFilename; // Note the /c command (*)
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
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
        private async Task DecompileRootSignature()
        {
            Process process = new Process();
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "Resources\\dxc\\dxc.exe";
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
            Process process = new Process();
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "Resources\\dxc\\dxc.exe";
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

                process.StartInfo.Arguments = "-dumpbin /ALL " + tempFilename; // Note the /c command (*)
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
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

        private static bool IsFileExtensionRecognized(string filePath)
        {
            
            return filePath.Contains("x64");
        }
    }
}
