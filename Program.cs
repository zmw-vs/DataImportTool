using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace DataImportTool
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var name = new AssemblyName(args.Name).Name;
                if (name == "EPPlus" || name == "Newtonsoft.Json")
                {
                    using (var stream = Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream("DataImportTool.Deps." + name + ".dll"))
                    {
                        if (stream == null) return null;
                        var bytes = new byte[stream.Length];
                        stream.Read(bytes, 0, bytes.Length);
                        return Assembly.Load(bytes);
                    }
                }
                return null;
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
