using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;
using UnityEditorInternal;
using UnityEditorInternal.VR;
using System.Text;

namespace Weaver
{
    class WeaverBuildBot : EditorWindow
    {
        private const string kDefaultAppName = "UNCONFIGURED_APP_NAME";
        private const string kDebugParameterName = "-configuration";
        private const string kExecutableParameterName = "-executableName";
        private const string kNoPDBParameterName = "-noPDBOutput";
        private const string kDefaultCompanyName = "ghostpepper";

        private static string m_executableName = EditorPrefs.GetString(string.Format("Weaver.{0}_ApplicationName", PlayerSettings.productName), kDefaultAppName);
        private static string m_targetDirectory = EditorPrefs.GetString(string.Format("Weaver.{0}_OutputDirectory", PlayerSettings.productName), "weaver_buildtargets/");
        private static bool m_makeDebugBuild = EditorPrefs.GetBool(string.Format("Weaver.{0}_DebugBuild", PlayerSettings.productName), false);

        [MenuItem("Weaver/Configure", false, 1)]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            WeaverBuildBot window = (WeaverBuildBot)GetWindow(typeof(WeaverBuildBot));
            window.Show();
        }

        void OnGUI()
        {
            GUI.changed = false;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Output Application Name:");
            m_executableName = GUILayout.TextField(m_executableName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Output Directory Root:");
            m_targetDirectory = GUILayout.TextField(m_targetDirectory);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            m_makeDebugBuild = EditorGUILayout.ToggleLeft(new GUIContent("Debug Build"), m_makeDebugBuild);

            if (GUI.changed)
            {
                EditorPrefs.SetString(string.Format("Weaver.{0}_ApplicationName", PlayerSettings.productName), m_executableName);
                EditorPrefs.SetString(string.Format("Weaver.{0}_OutputDirectory", PlayerSettings.productName), m_targetDirectory);
                EditorPrefs.SetBool(string.Format("Weaver.{0}_DebugBuild", PlayerSettings.productName), m_makeDebugBuild);
            }
        }

        [MenuItem("Weaver/Build Windows (x86)", false, 15)]
        static void PerformWindowsx86Build()
        {
            CheckAndOverwriteExecutableName();

            string platformDirectory = Path.Combine(m_targetDirectory, "Win32");
            string outputName = Path.Combine(platformDirectory, m_executableName) + ".exe";
            DeleteBuildIfExists(outputName);

            GenericBuild(FindEnabledEditorScenes(), outputName, BuildTarget.StandaloneWindows, BuildOptions.AcceptExternalModificationsToPlayer | BuildOptions.SymlinkLibraries, BuildTargetGroup.Standalone);
        }

        [MenuItem("Weaver/Build Windows (x64)", false, 15)]
        static void PerformWindowsx64Build()
        {
            CheckAndOverwriteExecutableName();

            string platformDirectory = Path.Combine(m_targetDirectory, "Win64");
            string outputName = Path.Combine(platformDirectory, m_executableName) + ".exe";
            DeleteBuildIfExists(outputName);

            GenericBuild(FindEnabledEditorScenes(), outputName, BuildTarget.StandaloneWindows64, BuildOptions.AcceptExternalModificationsToPlayer | BuildOptions.SymlinkLibraries, BuildTargetGroup.Standalone);
        }

        [MenuItem("Weaver/Build Mac OS X", false, 31)]
        static void PerformMacOSXBuild()
        {
            Debug.LogWarning("[Weaver] Warning No one has built OSX Builds using Weaver yet, so this is untested. Once tested and determined working, remove this line!");

            CheckAndOverwriteExecutableName();

            string platformDirectory = Path.Combine(m_targetDirectory, "Mac");
            string outputName = Path.Combine(platformDirectory, m_executableName) + ".app";

            GenericBuild(FindEnabledEditorScenes(), outputName, BuildTarget.StandaloneOSXIntel, BuildOptions.None, BuildTargetGroup.Standalone);
        }

        [MenuItem("Weaver/Build Mac OS X", true)]
        static bool ValidatePerformMacOSXBuild()
        {
#if UNITY_EDITOR_OSX
            return true;
#else
            return false;
#endif
        }

        [MenuItem("Weaver/Export iOS", false, 32)]
        static void PerformiOSExport()
        {
            Debug.LogWarning("[Weaver] Warning No one has built iOS Builds using Weaver yet, so this is untested. Once tested and determined working, remove this line!");

            CheckAndOverwriteExecutableName();
            string outputDirectory = Path.Combine(m_targetDirectory, "iOS/");

            // ToDo: The BuildOptions flags seem to only sort of work the first time.
            // I think you can't specify some of them if the directory exists/it has been outputted before.
            GenericBuild(FindEnabledEditorScenes(), outputDirectory, BuildTarget.iOS, BuildOptions.AcceptExternalModificationsToPlayer | BuildOptions.SymlinkLibraries, BuildTargetGroup.iOS);
        }
        [MenuItem("Weaver/Export iOS (Cardboard)", false, 33)]
        static void PerformiOSCardboardExport()
        {
            Debug.LogWarning("[Weaver] Warning No one has built iOS (Cardboard) Builds using Weaver yet, so this is untested. Once tested and determined working, remove this line!");

            CheckAndOverwriteExecutableName();
            string outputDirectory = Path.Combine(m_targetDirectory, "iOS_Cardboard/");

            // ToDo: The BuildOptions flags seem to only sort of work the first time.
            // I think you can't specify some of them if the directory exists/it has been outputted before.
            GenericBuild(FindEnabledEditorScenes(), outputDirectory, BuildTarget.iOS, BuildOptions.AcceptExternalModificationsToPlayer | BuildOptions.SymlinkLibraries, BuildTargetGroup.iOS);
        }

        [MenuItem("Weaver/Build Android", false, 55)]
        static void PerformAndroidBuild()
        {
            CheckAndOverwriteExecutableName();
            PlayerSettings.virtualRealitySupported = false;

            string platformDirectory = Path.Combine(m_targetDirectory, "Android");
            string executableName = string.Format("{0}_Android.apk", m_executableName);
            string outputName = Path.Combine(platformDirectory, executableName);

            // Delete old versions of files
            DeleteBuildIfExists(outputName);
            DeleteFileIfExists(Path.Combine(platformDirectory, "install_android.bat"));

            // Generate new install bat and build.
            GenericBuild(FindEnabledEditorScenes(), outputName, BuildTarget.Android, BuildOptions.None, BuildTargetGroup.Android);
            GenerateAPKInstallBat(executableName, Path.Combine(platformDirectory, "install_android.bat"));
        }

        [MenuItem("Weaver/Build Android (Google Cardboard)", false, 57)]
        static void PerformAndroidGoogleCardboardBuild()
        {
            CheckAndOverwriteExecutableName();
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "GOOGLE_CARDBOARD");

            PlayerSettings.virtualRealitySupported = true;
            string[] VRSDKs = new string[] { "cardboard", "None" };
            VREditor.SetVREnabledDevicesOnTargetGroup(BuildTargetGroup.Android, VRSDKs);

            // Force Daydream Audio Spatializer
            SetAudioSpatializerPlugin("GVR Audio Spatializer");

            string platformDirectory = Path.Combine(m_targetDirectory, "Android");
            string executableName = string.Format("{0}_GCardboard.apk", m_executableName);
            string outputName = Path.Combine(platformDirectory, executableName);

            // Delete old versions of files
            DeleteBuildIfExists(outputName);
            DeleteFileIfExists(Path.Combine(platformDirectory, "install_gcardboard.bat"));

            // Generate new install bat and build.
            GenericBuild(FindEnabledEditorScenes(), outputName, BuildTarget.Android, BuildOptions.None, BuildTargetGroup.Android);
            GenerateAPKInstallBat(executableName, Path.Combine(platformDirectory, "install_gcardboard.bat"));
        }

        [MenuItem("Weaver/Build Android (GearVR)", false, 56)]
        static void PerformAndroidGearVRBuild()
        {
            CheckAndOverwriteExecutableName();
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "GEAR_VR");

            // Force Oculus SDK support
            PlayerSettings.virtualRealitySupported = true;
            string[] VRSDKs = new string[] { "Oculus", "None" };
            VREditor.SetVREnabledDevicesOnTargetGroup(BuildTargetGroup.Android, VRSDKs);

            // Force Oculus Audio Spatializer
            SetAudioSpatializerPlugin("OculusSpatializer");

            string platformDirectory = Path.Combine(m_targetDirectory, "Android");
            string executableName = string.Format("{0}_GearVR.apk", m_executableName);
            string outputName = Path.Combine(platformDirectory, executableName);

            // Delete old versions of files
            DeleteBuildIfExists(outputName);
            DeleteFileIfExists(Path.Combine(platformDirectory, "install_gearvr.bat"));

            // Generate new install bat and build.
            GenericBuild(FindEnabledEditorScenes(), outputName, BuildTarget.Android, BuildOptions.None, BuildTargetGroup.Android);
            GenerateAPKInstallBat(executableName, Path.Combine(platformDirectory, "install_gearvr.bat"));
        }

        [MenuItem("Weaver/Build Android (Daydream)", false, 58)]
        static void PerformAndroidDaydreamBuild()
        {
            CheckAndOverwriteExecutableName();
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "GOOGLE_DAYDREAM");

            // Force daydream SDK support
            PlayerSettings.virtualRealitySupported = true;
            string[] VRSDKs = new string[] { "daydream", "None" };
            VREditor.SetVREnabledDevicesOnTargetGroup(BuildTargetGroup.Android, VRSDKs);
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;

            // Force Daydream Audio Spatializer
            SetAudioSpatializerPlugin("GVR Audio Spatializer");

            string platformDirectory = Path.Combine(m_targetDirectory, "Android");
            string executableName = string.Format("{0}_Daydream.apk", m_executableName);
            string outputName = Path.Combine(platformDirectory, executableName);

            // Delete old versions of files
            DeleteBuildIfExists(outputName);
            DeleteFileIfExists(Path.Combine(platformDirectory, "install_daydream.bat"));

            // Generate new install bat and build.
            GenericBuild(FindEnabledEditorScenes(), outputName, BuildTarget.Android, BuildOptions.None, BuildTargetGroup.Android);
            GenerateAPKInstallBat(executableName, Path.Combine(platformDirectory, "install_daydream.bat"));
        }

        static void GenericBuild(string[] scenes, string targetDirectory, BuildTarget buildTarget, BuildOptions buildOptions, BuildTargetGroup buildTargetGroup)
        {
            Debug.LogFormat("[Weaver] Starting Build to File/Directory \"{0}\".", targetDirectory);
            PlayerSettings.companyName = kDefaultCompanyName;

            // Determine if they want to make a Debug Build.
            bool debugBuild = GetArgumentValueFromCLI(kDebugParameterName) == "debug";
            Debug.LogFormat("[Weaver] Making Debug Build: {0}", debugBuild);

            // Use this hack to make it copy PDB files even if the UI hasn't checked that before.
            bool noPDB = GetArgumentContainedInCLI(kNoPDBParameterName);
            if (!noPDB)
            {
                EditorUserBuildSettings.SetPlatformSettings("Standalone", "CopyPDBFiles", "true");
            }

            PlayerSettings.usePlayerLog = true;

            if (debugBuild)
            {
                buildOptions |= BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler;
            }

            // Ensure the output directory exists, otherwise Unity dies.
            Directory.CreateDirectory(targetDirectory);

            // We can only switch the build target this way if we're not in batch mode
            if (!InternalEditorUtility.inBatchMode)
            {
                // Switch our active build target (potentially reimporting content...)
                EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);
            }

#if UNITY_5_5_OR_NEWER
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = scenes;
            buildPlayerOptions.locationPathName = targetDirectory;
            buildPlayerOptions.target = buildTarget;
            buildPlayerOptions.options = buildOptions;
            buildPlayerOptions.targetGroup = buildTargetGroup;

            // Build the application.
            string res = BuildPipeline.BuildPlayer(buildPlayerOptions).ToString();
#else
            string res = BuildPipeline.BuildPlayer(scenes, targetDirectory, buildTarget, buildOptions);
#endif

            if (!string.IsNullOrEmpty(res))
            {
                throw new Exception("[Weaver] Build Failed. Result:\n" + res);
            }

            Debug.Log("[Weaver] Finished Successful Build to Directory: " + targetDirectory);

            // Open the directory in windows explorer if we're being built from the GUI.
            if (InternalEditorUtility.isHumanControllingUs)
            {
                EditorUtility.RevealInFinder(targetDirectory);
            }
        }

        private static void CheckAndOverwriteExecutableName()
        {
            // Determine if they've passed a project name via the CLI, if so override the one that comes from the configuration.
            string projectName = GetArgumentValueFromCLI(kExecutableParameterName);
            if (!string.IsNullOrEmpty(projectName))
                m_executableName = projectName;

            // If they haven't configured it, warn them...
            if (m_executableName == kDefaultAppName)
            {
                Debug.LogWarningFormat("[Weaver] Warning - No application name configured. Using Default \"{0}\"\nIf using the UI, use Weaver > Configuration to set it, and if using Jenkins pass \"{1} <name>\" argument on CLI.",
                    kDefaultAppName, kExecutableParameterName);
            }
        }

        /// <summary>
        /// This function checks CLI arguments for the specified argument, and will return
        /// the matching value. If the argument is not found, or the argument is not followed
        /// by a value then it will return null! Search is case insensitive.
        /// </summary>
        /// <param name="argumentName">Case insensitive argument to look for.</param>
        /// <returns>Value if found, null otherwise.</returns>
        private static string GetArgumentValueFromCLI(string argumentName)
        {
            // Lowercase everything so we don't have to be case sensitive
            argumentName = argumentName.ToLowerInvariant();

            var cmdArgs = new List<string>(Environment.GetCommandLineArgs());
            for (int i = 0; i < cmdArgs.Count; i++)
                cmdArgs[i] = cmdArgs[i].ToLowerInvariant();

            int argumentIndex = cmdArgs.IndexOf(argumentName);
            if (argumentIndex >= 0 && argumentIndex < cmdArgs.Count - 1)
            {
                return cmdArgs[argumentIndex + 1];
            }

            // Didn't find the argument in the CLI, or it was the last entry without a matching value.
            return null;
        }

        /// <summary>
        /// This function can be used to check if an arbitrary argument has been specified
        /// in the command line. This is useful for flags you might wish to use instead of
        /// a key/value pair. Search is case insensitive.
        /// </summary>
        /// <param name="argumentName">Case insensitive argument to look for.</param>
        /// <returns>True if it is an argument on the command line, false otherwise. </returns>
        private static bool GetArgumentContainedInCLI(string argumentName)
        {
            // Lowercase everything so we don't have to be case sensitive
            argumentName = argumentName.ToLowerInvariant();

            var cmdArgs = new List<string>(Environment.GetCommandLineArgs());
            for (int i = 0; i < cmdArgs.Count; i++)
                cmdArgs[i] = cmdArgs[i].ToLowerInvariant();

            return cmdArgs.IndexOf(argumentName) >= 0;
        }

        private static string[] FindEnabledEditorScenes()
        {
            List<string> editorScenes = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled)
                    continue;

                editorScenes.Add(scene.path);
            }

            return editorScenes.ToArray();
        }

        private static void SetAudioSpatializerPlugin(string pluginName)
        {
            SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/AudioManager.asset")[0]);
            serializedObject.FindProperty("m_SpatializerPlugin").stringValue = pluginName;
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// This generates an "Install.bat" file which can be double clicked to install the APK generated by 
        /// weaver to a connected device in ADB. It will automatically uninstall the old version if required.
        /// </summary>
        private static void GenerateAPKInstallBat(string packageFileName, string outputFilePath)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("@Echo Off");
            sb.AppendLine("echo Preparing to uninstall previous version of APK if installed.");
            // Search the return text for an OK message
            string uninstallErrorCheck =
                "| findstr /I /C:\"Unknown package:\"" + Environment.NewLine +
                "if %errorlevel% == 0 (" + Environment.NewLine +
                "echo Succesfully uninstalled previous version." + Environment.NewLine +
                ") else (" + Environment.NewLine +
                "echo \"An exception was thrown while uninstalling, most likely there was no previous version found to uninstall. Only worry about this if the install fails.\"" + Environment.NewLine +
                ")" + Environment.NewLine;

            sb.AppendFormat("adb uninstall {0} {1}{2}", PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android), uninstallErrorCheck, Environment.NewLine);

            // Now install the new version
            sb.AppendLine("echo Preparing to install new version of APK...");
            sb.AppendFormat("adb install {0}{1}", packageFileName, Environment.NewLine);
            sb.AppendLine("pause");

            File.WriteAllText(outputFilePath, sb.ToString());
        }


        private static void DeleteBuildIfExists(string outputName)
        {
            // Can't overwrite automatically, have to manually handle.
            if (File.Exists(outputName))
            {
                Debug.Log("[Weaver] Build already exists, deleting old build...");

                // Delete the exe
                File.Delete(outputName);

                // Delete the Data Folder.
                string dataFolderName = outputName.Substring(0, outputName.Length - 3) + "_Data";
                if (Directory.Exists(dataFolderName))
                    Directory.Delete(dataFolderName);
            }
        }

        private static void DeleteFileIfExists(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("[Weaver] Caught exception while deleting file: {0}\n{1}", ex);
            }
        }

    }
}