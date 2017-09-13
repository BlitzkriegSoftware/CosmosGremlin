using System.Diagnostics;

namespace CosmosGremlin.ConsoleApp.Lib
{
    public static class AssemblyHelper
    {
        /// <summary>
        /// Make a version string from AssemblyInfo
        /// </summary>
        /// <returns>formatted string</returns>
        public static string GetVersion()
        {
            int mjr = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileMajorPart;
            int mnr = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileMinorPart;
            int bld = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileBuildPart;
            int prv = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FilePrivatePart;
            return mjr + "." + mnr + "." + bld + "." + prv;
        }

        /// <summary>
        /// Gets title from assembly info
        /// </summary>
        /// <returns>title</returns>
        public static string GetTitle()
        {
            return FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).ProductName;
        }

        /// <summary>
        /// Gets the name of the EXE running
        /// </summary>
        /// <returns>String of name</returns>
        public static string GetExeName()
        {
            return FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileName;
        }

        /// <summary>
        /// Get CopyRight
        /// </summary>
        /// <returns></returns>
        public static string GetCopyright()
        {
            return FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).LegalCopyright;
        }
    }
}
