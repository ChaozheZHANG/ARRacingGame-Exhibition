using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using System.IO;
#endif

namespace ScriptEcho.iOS
{
    /// <summary>
    /// iOS构建管理器 - 处理iOS构建的特殊设置和优化
    /// </summary>
    public class iOSBuildManager
    {
#if UNITY_EDITOR
        [MenuItem("ScriptEcho/Build/Configure for iOS")]
        public static void ConfigureForiOS()
        {
            Debug.Log("开始配置iOS构建设置...");
            
            // 基本Player设置
            PlayerSettings.productName = "ScriptEcho AR赛车";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.iOS.buildNumber = "1";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.scriptecho.arracing");
            
            // iOS特定设置
            PlayerSettings.iOS.targetOSVersionString = "12.0";
            PlayerSettings.iOS.requiresARKit = true;
            PlayerSettings.iOS.cameraUsageDescription = "此应用需要使用相机来提供AR赛车游戏体验";
            PlayerSettings.iOS.locationUsageDescription = "此应用可能需要位置信息来增强AR体验";
            
            // AR设置
            PlayerSettings.iOS.requiresARKit = true;
            PlayerSettings.iOS.automaticallyDetectAndAddCapabilities = true;
            
            // 性能优化
            PlayerSettings.iOS.scriptCallOptimization = ScriptCallOptimizationLevel.SlowAndSafe;
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.iOS, ManagedStrippingLevel.Medium);
            
            // 图形设置
            PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS, new UnityEngine.Rendering.GraphicsDeviceType[] {
                UnityEngine.Rendering.GraphicsDeviceType.Metal
            });
            
            // 屏幕方向
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            
            Debug.Log("iOS构建设置配置完成！");
        }
        
        [MenuItem("ScriptEcho/Build/Build for iOS")]
        public static void BuildForiOS()
        {
            Debug.Log("开始iOS构建...");
            
            // 先配置设置
            ConfigureForiOS();
            
            // 构建设置
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new string[] {
                "Assets/Scenes/ScriptEchoPlatform.unity",
                "Assets/Scenes/ScriptEchoARRacing.unity"
            };
            buildPlayerOptions.locationPathName = "Builds/iOS";
            buildPlayerOptions.target = BuildTarget.iOS;
            buildPlayerOptions.options = BuildOptions.None;
            
            // 执行构建
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("iOS构建成功！路径: " + summary.outputPath);
                Debug.Log("现在可以在Xcode中打开项目并部署到iPhone");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError("iOS构建失败！");
            }
        }
        
        [UnityEditor.Callbacks.PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToBuiltProject)
        {
            if (buildTarget == BuildTarget.iOS)
            {
                ConfigureXcodeProject(pathToBuiltProject);
            }
        }
        
        private static void ConfigureXcodeProject(string buildPath)
        {
            Debug.Log("配置Xcode项目...");
            
            string plistPath = buildPath + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            
            PlistElementDict rootDict = plist.root;
            
            // 添加AR相关权限
            rootDict.SetString("NSCameraUsageDescription", "此应用需要使用相机来提供AR赛车游戏体验");
            rootDict.SetString("NSLocationWhenInUseUsageDescription", "此应用可能需要位置信息来增强AR体验");
            
            // 添加支持的设备方向
            PlistElementArray orientations = rootDict.CreateArray("UISupportedInterfaceOrientations");
            orientations.AddString("UIInterfaceOrientationLandscapeLeft");
            orientations.AddString("UIInterfaceOrientationLandscapeRight");
            
            // ARKit支持
            rootDict.SetBoolean("UIRequiresFullScreen", true);
            rootDict.SetString("UILaunchStoryboardName", "LaunchScreen");
            
            // 写入plist
            plist.WriteToFile(plistPath);
            
            // 配置Xcode项目
            string projPath = PBXProject.GetPBXProjectPath(buildPath);
            PBXProject proj = new PBXProject();
            proj.ReadFromFile(projPath);
            
            string target = proj.GetUnityMainTargetGuid();
            
            // 添加ARKit framework
            proj.AddFrameworkToProject(target, "ARKit.framework", false);
            proj.AddFrameworkToProject(target, "Metal.framework", false);
            proj.AddFrameworkToProject(target, "MetalKit.framework", false);
            
            // 设置构建设置
            proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
            proj.SetBuildProperty(target, "CLANG_ENABLE_MODULES", "YES");
            
            // 写入项目文件
            proj.WriteToFile(projPath);
            
            Debug.Log("Xcode项目配置完成！");
        }
#endif
    }
    
    /// <summary>
    /// iOS运行时优化管理器
    /// </summary>
    public class iOSRuntimeOptimizer : MonoBehaviour
    {
        [Header("性能设置")]
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private bool optimizeForBattery = true;
        [SerializeField] private bool enableMetalRendering = true;
        
        void Start()
        {
            ApplyiOSOptimizations();
        }
        
        private void ApplyiOSOptimizations()
        {
#if UNITY_IOS && !UNITY_EDITOR
            Debug.Log("应用iOS运行时优化...");
            
            // 设置目标帧率
            Application.targetFrameRate = targetFrameRate;
            
            // 电池优化
            if (optimizeForBattery)
            {
                QualitySettings.vSyncCount = 0;
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }
            
            // Metal渲染优化
            if (enableMetalRendering)
            {
                // Metal相关的优化设置
                QualitySettings.antiAliasing = 2; // 适中的抗锯齿
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
            }
            
            // 内存管理
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
            
            Debug.Log("iOS优化设置已应用");
#endif
        }
        
        void OnApplicationFocus(bool hasFocus)
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (hasFocus)
            {
                // 应用恢复时的优化
                Application.targetFrameRate = targetFrameRate;
            }
            else
            {
                // 应用失去焦点时降低帧率节省电池
                Application.targetFrameRate = 30;
            }
#endif
        }
    }
} 