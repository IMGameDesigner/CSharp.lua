

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CSharpLua {
  class Program {
    private const string kHelpCmdString = @"Usage: CSharp.lua [-s srcfolder] [-d dstfolder]
Arguments
-s              : source directory, all *.cs files whill be compiled
-d              : destination  directory, will put the out lua files

Options
-h              : show the help message and exit
-l              : 输入[包含一群dll文件的文件夹]的路径，比如C:\Users\Administrator\Desktop\dll 

-m              : meta files, like System.xml, use ';' to separate
-csc            : csc.exe command argumnets, use ' ' or '\t' to separate

-c              : support classic lua version(5.1), default support 5.3
-a              : attributes need to export, use ';' to separate, if ""-a"" only, all attributes whill be exported
-metadata       : export all metadata, use @CSharpLua.Metadata annotations for precise control
-module         : the currently compiled assembly needs to be referenced, it's useful for multiple module compiled
";
    public static void Main(string[] args) {
      
      if (args.Length > 0) {
        try {
          var cmds = Utility.GetCommondLines(args);
          if (cmds.ContainsKey("-h")) {
            ShowHelpInfo();
            return;
          }

          Console.WriteLine($"start {DateTime.Now}");

          string folder = cmds.GetArgument("-s");
          string output = cmds.GetArgument("-d");
          string libPath = cmds.GetArgument("-l", true);
          
          var files = Directory.GetFiles(libPath, "*.dll");
          string allDll = "";
          for(int i=1;i<=files.Length;i++)
            if (i == files.Length) {
              allDll += files[i-1];
            } else {
              allDll += files[i - 1] + ";";
            }
          
          string meta = cmds.GetArgument("-m", true);
          bool isClassic = cmds.ContainsKey("-c");
          string atts = cmds.GetArgument("-a", true);
          if (atts == null && cmds.ContainsKey("-a")) {
            atts = string.Empty;
          }
          string csc = GetCSCArgument(args);
          bool isExportMetadata = cmds.ContainsKey("-metadata");
          bool isModule = cmds.ContainsKey("-module");
          Compiler c = new Compiler(folder, output, allDll, meta, csc, isClassic, atts) {
            IsExportMetadata = isExportMetadata,
            IsModule = isModule,
          };
          c.Compile();
          Console.WriteLine("all operator success");
          Console.WriteLine($"end {DateTime.Now}");
        } catch (CmdArgumentException e) {
          Console.Error.WriteLine(e.Message);
          ShowHelpInfo();
          Environment.ExitCode = -1;
        } catch (CompilationErrorException e) {
          Console.Error.WriteLine(e.Message);
          Environment.ExitCode = -1;
        } catch (Exception e) {
          Console.Error.WriteLine(e.ToString());
          Environment.ExitCode = -1;
        }
      } else {
        ShowHelpInfo();
        Environment.ExitCode = -1;
      }
    }

    private static void ShowHelpInfo() {
      Console.Error.WriteLine(kHelpCmdString);
    }

    private static HashSet<string> argumnets_; 

    private static bool IsArgumentKey(string key) {
      if (argumnets_ == null) {
        argumnets_ = new HashSet<string>();
        string[] lines = kHelpCmdString.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines) {
          if (line.StartsWith('-')) {
            char[] chars = line.TakeWhile(i => !char.IsWhiteSpace(i)).ToArray();
            argumnets_.Add(new string(chars));
          }
        }
      }
      return argumnets_.Contains(key);
    }

    private static string GetCSCArgument(string[] args) {
      int index = args.IndexOf("-csc");
      if (index != -1) {
        var remains = args.Skip(index + 1);
        int end = remains.IndexOf(IsArgumentKey);
        if (end != -1) {
          remains = remains.Take(end);
        }
        return string.Join(" ", remains);
      }
      return null;
    }
  }
}
