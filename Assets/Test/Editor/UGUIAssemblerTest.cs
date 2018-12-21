using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UGUIAssembler;
using UnityEditor;

public class UGUIAssemblerTest
{
    private static CsvTable table;
    private System.Text.Encoding encoding
    {
        get
        {
            return System.Text.Encoding.GetEncoding("GB2312");
        }
    }
    [Test]
    public void CsvTableLoadTest()
    {
        var time1 = System.DateTime.Now;
        var configPath = EditorUtility.OpenFilePanel("选择配制规则文件（csv）", PreferHelper.configFolderPath, "csv");
        var time2 = System.DateTime.Now;
        table = CsvHelper.ReadCSV(configPath, encoding);
        var time3 = System.DateTime.Now;
        Debug.Log( "打开面板用时： "+ (time2 - time1).TotalMilliseconds);
        Debug.Log( "解析Table用时： "+ (time3 - time2).TotalMilliseconds);

        Debug.Log(string.Join("-", table.Columns.ToArray()));

        for (int i = 0; i < table.Rows.Count; i++)
        {
            var row = table.Rows[i];
            Debug.Log(string.Join("-", row));
        }
    }
    [Test]
    public void CsvTableSaveTest()
    {
        if (table != null)
        {
            var path = EditorUtility.SaveFilePanel("保存规则文件（csv）", PreferHelper.configFolderPath, "newtable", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                CsvHelper.SaveCSV(table, path, encoding);
            }
        }
    }
    [Test]
    public void TableToUIInfoTest()
    {
        var configPath = EditorUtility.OpenFilePanel("选择配制规则文件（csv）", PreferHelper.configFolderPath, "csv");
        table = CsvHelper.ReadCSV(configPath, encoding);

        if (table.IsUIInfoTable(true))
        {
            var uiInfo = table.LoadUIInfo();
            Debug.Log(uiInfo.name);
        }
        else
        {
            EditorUtility.DisplayDialog("错误提示", "表为或标题不匹配", "ok");
        }

    }
    [Test]
    public void ConventAbleTypesTest()
    {
        //System.Convert
        var intType = typeof(int);
        var floatType = typeof(float);
        var singleType = typeof(System.Single);
        var boolType = typeof(bool);

        Debug.Log("int 是 single 的子类？" + intType.IsSubclassOf(singleType));
        Debug.Log("int 从 single 的派生？" + singleType .IsAssignableFrom(intType));
        Debug.Log("int 和 single 类型一样?" + (singleType == intType));
        Debug.Log("float 和 single 类型一样?" + (floatType == singleType));

        Debug.Log("inttype name:" + intType.FullName);
        Debug.Log("floatType name:" + floatType.FullName);
        Debug.Log("singleType name:" + singleType.FullName);
        Debug.Log("boolType name:" + boolType.FullName);
    }

    [System.Serializable]
    public class JsonTestClass
    {
        public int x;
        public string[] y;
    }
    [Test]
    public void JsonStringTest()
    {
        Debug.Log(JsonUtility.ToJson(Vector3.zero));

        var clas = new JsonTestClass();
        clas.x = 123;
        clas.y = new string[10];
        for (int i = 0; i < clas.y.Length; i++)
        {
            clas.y[i] = i.ToString();
        }

        Debug.Log(JsonUtility.ToJson(clas));
    }
}
