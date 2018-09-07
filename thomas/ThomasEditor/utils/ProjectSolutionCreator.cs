using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ThomasEngine;
namespace ThomasEditor.utils
{

    class ScriptAssemblyManager
    {
        static string assemblyPath = "";
                
        [STAThread]
        public static bool CreateSolution(string path, string name)
        {
            Type type = Type.GetTypeFromProgID("VisualStudio.DTE");
            object obj = Activator.CreateInstance(type, true);
            EnvDTE.DTE dte = (EnvDTE.DTE)obj;
            MessageFilter.Register();
            try
            {
                
                dte.MainWindow.Visible = false; // optional if you want to See VS doing its thing

                string template = System.IO.Path.GetFullPath("../Data/assemblyFiles/MyTemplate.vstemplate");
                dte.Solution.AddFromTemplate(template, path, name);
                //create a new solution
                dte.Solution.Create(path, name + ".sln");
                var solution = dte.Solution;
                EnvDTE.Project project = solution.AddFromFile(path + "\\" + name + ".csproj");

                // create a C# class library
                System.IO.Directory.CreateDirectory(path + "\\Assets");
                project.ProjectItems.AddFromDirectory(path + "\\Assets");
                assemblyPath = path + "\\" + name + ".sln";


                solution.SolutionBuild.Build(true);
                // save and quit
                dte.ExecuteCommand("File.SaveAll");
                dte.Quit();
                MessageFilter.Revoke();
                return true;

            }
            catch(Exception e)
            {
                Debug.Log("Error creating project: " + e.Message);
                MessageFilter.Revoke();
                return false;
            }
        }

        public static bool OpenSolution(string path)
        {
            assemblyPath = path;
            return BuildSolution();
        }

        public static bool BuildSolution()
        {

            var properties = new Dictionary<string, string>();
            properties["Configuration"] = "Debug";
            properties["Platform"] = "Any CPU";
            var request = new BuildRequestData(assemblyPath, properties, null, new string[] { "Build" }, null);
            ProjectCollection pc = new ProjectCollection();
            BuildParameters bp = new BuildParameters(pc);
            var result = BuildManager.DefaultBuildManager.Build(bp, request);
            if (result.OverallResult == BuildResultCode.Success)
                return true;
            else
            {
                Debug.Log("Failed to build project assembly.... :(");
                return false;
            }
            //Type type = Type.GetTypeFromProgID("VisualStudio.DTE");
            //object obj = Activator.CreateInstance(type, true);
            //EnvDTE.DTE dte = (EnvDTE.DTE)obj;
            //MessageFilter.Register();
            //try
            //{
            //    dte.MainWindow.Visible = false; // optional if you want to See VS doing its thing
            //    dte.Solution.Open(assemblyPath);
            //    var solution = dte.Solution;
            //    solution.SolutionBuild.Build(true);
            //    dte.Quit();
            //    MessageFilter.Revoke();
            //    return true;
            //}catch(Exception e)
            //{
            //    Debug.Log("Failed to open/build project: " + e.Message);
            //    dte.Quit();
            //    MessageFilter.Revoke();
            //    return false;
            //}
        }

        public static void AddScript(string script)
        {
            Type type = Type.GetTypeFromProgID("VisualStudio.DTE");
            object obj = Activator.CreateInstance(type, true);
            EnvDTE.DTE dte = (EnvDTE.DTE)obj;
            MessageFilter.Register();
            try
            {
                dte.MainWindow.Visible = false; // optional if you want to See VS doing its thing
                dte.Solution.Open(assemblyPath);
                var solution = dte.Solution;
                solution.Projects.Item(1).ProjectItems.AddFromFile(script);
                dte.ExecuteCommand("File.SaveAll");
                solution.SolutionBuild.Build(true);
            }catch(Exception e)
            {
                Debug.Log("Failed to add file to solution: " + e.Message);
            }

            dte.Quit();
            MessageFilter.Revoke();
        }

    }

    public class MessageFilter : IOleMessageFilter
    {
        //
        // Class containing the IOleMessageFilter
        // thread error-handling functions.

        // Start the filter.
        public static void Register()
        {
            IOleMessageFilter newFilter = new MessageFilter();
            IOleMessageFilter oldFilter = null;
            CoRegisterMessageFilter(newFilter, out oldFilter);
        }

        // Done with the filter, close it.
        public static void Revoke()
        {
            IOleMessageFilter oldFilter = null;
            CoRegisterMessageFilter(null, out oldFilter);
        }

        //
        // IOleMessageFilter functions.
        // Handle incoming thread requests.
        int IOleMessageFilter.HandleInComingCall(int dwCallType,
          System.IntPtr hTaskCaller, int dwTickCount, System.IntPtr
          lpInterfaceInfo)
        {
            //Return the flag SERVERCALL_ISHANDLED.
            return 0;
        }

        // Thread call was rejected, so try again.
        int IOleMessageFilter.RetryRejectedCall(System.IntPtr
          hTaskCallee, int dwTickCount, int dwRejectType)
        {
            if (dwRejectType == 2)
            // flag = SERVERCALL_RETRYLATER.
            {
                // Retry the thread call immediately if return >=0 & 
                // <100.
                return 99;
            }
            // Too busy; cancel call.
            return -1;
        }

        int IOleMessageFilter.MessagePending(System.IntPtr hTaskCallee,
          int dwTickCount, int dwPendingType)
        {
            //Return the flag PENDINGMSG_WAITDEFPROCESS.
            return 2;
        }

        // Implement the IOleMessageFilter interface.
        [DllImport("Ole32.dll")]
        private static extern int
          CoRegisterMessageFilter(IOleMessageFilter newFilter, out
          IOleMessageFilter oldFilter);
    }

    [ComImport(), Guid("00000016-0000-0000-C000-000000000046"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    interface IOleMessageFilter
    {
        [PreserveSig]
        int HandleInComingCall(
            int dwCallType,
            IntPtr hTaskCaller,
            int dwTickCount,
            IntPtr lpInterfaceInfo);

        [PreserveSig]
        int RetryRejectedCall(
            IntPtr hTaskCallee,
            int dwTickCount,
            int dwRejectType);

        [PreserveSig]
        int MessagePending(
            IntPtr hTaskCallee,
            int dwTickCount,
            int dwPendingType);
    }
}
