using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

public class Shortcut : EditorWindow
{
    [MenuItem("Custom/Shortcut")]
    static void ShowWindow()
    {
        EditorWindow.GetWindow<Shortcut>();
    }

    string keyword = "";
    
    List<Line> addMenus = new List<Line>();
    string addMenu_name = "";
    string addMenu_path = "";
    private Vector2 scroll_pos;
    
    void OnGUI()
    {
        Dictionary<string, string> search = new Dictionary<string, string>();
        search.Add("Google検索", "https://www.google.com/search?q=");
        search.Add("翻訳", "https://translate.google.com/?hl=ja#view=home&op=translate&sl=ja&tl=en&text=");
        search.Add("Unityスクリプトリファレンス", "https://docs.unity3d.com/jp/current/ScriptReference/30_search.html?q=");
        search.Add("Qiita", "https://qiita.com/search?utf8=✓&sort=&q=");

        Dictionary<string, string> drive = new Dictionary<string, string>();
        drive.Add("MyDrive", "https://drive.google.com/drive/my-drive");

        //個人で適宜変えてね
        Dictionary<string, string> apps = new Dictionary<string, string>();
        apps.Add("GIMP", @"C:\Program Files\GIMP 2\bin\gimp-2.10.exe");
        apps.Add("Blender", @"C:\Program Files\Blender Foundation\Blender\blender.exe");
        apps.Add("GitHubDesktop", @"C:\Users\Yuga\AppData\Local\GitHubDesktop\GitHubDesktop.exe");
        apps.Add("SouceTree", @"C:\Users\Yuga\AppData\Local\SourceTree\SouceTree.exe");

        GUILayout.Label("検索");
        keyword = GUILayout.TextField(keyword);
        
        scroll_pos = EditorGUILayout.BeginScrollView(scroll_pos); //ここからスクロール表示
        
        foreach(string key in search.Keys)
        {
            if (GUILayout.Button(key))
            {
                Application.OpenURL(search[key] + keyword);
            }
        }

        GUILayout.Label("Googleドライブ");
        foreach (string key in drive.Keys)
        {
            if (GUILayout.Button(key))
            {
                Application.OpenURL(drive[key] + keyword);
            }
        }

        GUILayout.Label("アプリケーション");
        foreach (string key in apps.Keys)
        {
            if (GUILayout.Button(key))
            {
                Process.Start(apps[key]);
            }
        }
        
        
        GUILayout.Label("ユーザーめぬ");

        #region view elements

        // ユーザーメニュー
        foreach (var addMenu in addMenus)
        {
            GUILayout.BeginHorizontal();
            addMenu.ShowLine(keyword);
            if (GUILayout.Button("削除", GUILayout.Width(30))) //ボタンを表示
            {
                addMenus.Remove(addMenu);
                break;
            }
            GUILayout.EndHorizontal();
        }
        
        #endregion
        

        #region AddElement

        // メニューの名前入力
        GUILayout.BeginHorizontal();
        GUILayout.Label("name", GUILayout.Width(40));
        addMenu_name = GUILayout.TextField(addMenu_name);
        GUILayout.EndHorizontal();
        
        // パス入力
        GUILayout.BeginHorizontal();
        GUILayout.Label("path", GUILayout.Width(40));
        addMenu_path = GUILayout.TextField(addMenu_path);
        GUILayout.EndHorizontal();
        
        // ボタンズ
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("検索追加"))
        {
            addMenus.Add(new Search(addMenu_name, addMenu_path));
            addMenu_name = "";
            addMenu_path = "";
        }
        if (GUILayout.Button("アプリ追加"))
        {
            addMenus.Add(new App(addMenu_name, addMenu_path));
            addMenu_name = "";
            addMenu_path = "";
        }
        GUILayout.EndHorizontal();        

        #endregion
        
        
        EditorGUILayout.EndScrollView(); //ここまでスクロール表示
    }


    private abstract class Line
    {
        protected string name { get; private set; }
        protected string path { get; private set; }

        protected Line(string name, string path)
        {
            this.name = name;
            this.path = path;
        }
        public abstract void ShowLine(string keyword);
    }

    private class Search : Line
    {
        public Search(string name, string path) : base(name, path){}
        public override void ShowLine(string keyword)
        {
            if (GUILayout.Button(name))
            {
                Application.OpenURL(path + keyword);
            }
        }
    }

    private class App : Line
    {
        public App(string name, string path) : base(name, path){}
        public override void ShowLine(string keyword)
        {
            if (GUILayout.Button(name))
            {
                Process.Start(path);
            }
        }
    }
}

public class CopyAbsolutePath : MonoBehaviour
{

    [MenuItem("Assets/GetFullPath", false)]
    static void Execute()
    {
        int instanceID = Selection.activeInstanceID;
        string path = AssetDatabase.GetAssetPath(instanceID);
        string fullPath = Path.GetFullPath(path);

        GUIUtility.systemCopyBuffer = fullPath;
    }
}