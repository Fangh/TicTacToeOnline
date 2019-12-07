// Minor adjustments by Arshd and then tsny and finaly RKar
// Version Incrementor Script for Unity by Francesco Forno (Fornetto Games)
// Inspired by http://forum.unity3d.com/threads/automatic-version-increment-script.144917/

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

[InitializeOnLoad]
class VersionIncrementor : IPreprocessBuildWithReport
{
    [MenuItem("Build/Increase Current Build Version")]
    private static void IncreaseBuild()
    {
        IncrementVersion(new int[] { 0, 0, 1 });
    }

    [MenuItem("Build/Increase Minor Version")]
    private static void IncreaseMinor()
    {
        IncrementVersion(new int[] { 0, 1, 0 });
    }

    [MenuItem("Build/Increase Major Version")]
    private static void IncreaseMajor()
    {
        IncrementVersion(new int[] { 1, 0, 0 });
    }

    static void IncrementVersion(int[] versionIncr)
    {
        string[] lines = PlayerSettings.bundleVersion.Split('.');

        for (int i = lines.Length - 1; i >= 0; i--)
        {
            bool isNumber = int.TryParse(lines[i], out int numberValue);

            if (isNumber && versionIncr.Length - 1 >= i)
            {
                if (i > 0 && versionIncr[i] + numberValue > 9)
                {
                    versionIncr[i - 1]++;

                    versionIncr[i] = 0;
                }
                else
                {
                    versionIncr[i] += numberValue;
                }
            }
        }

        PlayerSettings.bundleVersion = versionIncr[0] + "." + versionIncr[1] + "." + versionIncr[2];
        UnityEngine.GameObject.FindGameObjectWithTag("VersionLabel").GetComponent<TMPro.TMP_Text>().text = $"Version {PlayerSettings.bundleVersion}";
    }

    public static string GetLocalVersion()
    {
        return PlayerSettings.bundleVersion.ToString();
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        IncreaseBuild();
    }

    public int callbackOrder { get { return 0; } }
}
#endif
