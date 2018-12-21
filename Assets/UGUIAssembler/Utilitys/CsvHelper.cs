using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
namespace UGUIAssembler
{
    public class CsvTable
    {
        public string name;
        public List<string> Columns = new List<string>();
        public List<string[]> Rows = new List<string[]>();

        public CsvTable(string name)
        {
            this.name = name;
        }

        public string this[string title, int row]
        {
            get
            {
                if (Columns == null || Rows == null)
                {
                    return null;
                }
                if (string.IsNullOrEmpty(title) || row < 0 || row >= Rows.Count)
                {
                    return null;
                }
                var index = Columns.IndexOf(title);
                if (index >= 0)
                {
                    return Rows[index][row];
                }
                else
                {
                    return null;
                }
            }
        }
        public string this[int col, int row]
        {
            get
            {
                if (Rows == null || Rows.Count <= row)
                {
                    return null;
                }
                var line = Rows[row];
                if (line == null || line.Length <= col)
                {
                    return null;
                }
                return line[col];
            }
        }

        public static string[] NewRow(int count)
        {
            var row = new string[count];
            return row;
        }
    }

    public class CsvHelper
    {
        /// <summary>
        /// 将DataTable中数据写入到CSV文件中
        /// </summary>
        /// <param name="dt">提供保存数据的DataTable</param>
        /// <param name="fileName">CSV的文件路径</param>
        public static void SaveCSV(CsvTable dt, string fullPath, System.Text.Encoding encoding)
        {
            FileInfo fi = new FileInfo(fullPath);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }
            FileStream fs = new FileStream(fullPath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, encoding);
            string data = "";
            //写出列名称
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                data += "\"" + dt.Columns[i] + "\"";
                if (i < dt.Columns.Count - 1)
                {
                    data += ",";
                }
            }
            sw.WriteLine(data);
            //写出各行数据
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                data = "";
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    string str = dt.Rows[i][j].ToString();
                    str = string.Format("\"{0}\"", str);
                    data += str;
                    if (j < dt.Columns.Count - 1)
                    {
                        data += ",";
                    }
                }
                sw.WriteLine(data);
            }
            sw.Close();
            fs.Close();
        }
        /// <summary>
        /// 读取CSV文件到DataTable中
        /// </summary>
        /// <param name="filePath">CSV的文件路径</param>
        /// <returns></returns>
        public static CsvTable ReadCSV(string filePath, System.Text.Encoding encoding)
        {
            CsvTable dt = new CsvTable(System.IO.Path.GetFileNameWithoutExtension(filePath));
            var fileInfo = new FileInfo(filePath);
            var fileStream = fileInfo.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            int lineNumber = 0;
            using (CsvFileReader reader = new CsvFileReader(fileStream, encoding))
            {
                var row = new List<string>();

                while (reader.ReadRow(row))
                {
                    if (0 == lineNumber)
                    {
                        foreach (string s in row)
                        {
                            dt.Columns.Add(s.Replace("\"", ""));
                        }
                    }
                    else
                    {
                        int index = 0;
                        string[] dr = CsvTable.NewRow(row.Count);
                        foreach (string s in row)
                        {
                            dr[index] = s.Replace("\"", "");
                            index++;
                        }
                        dt.Rows.Add(dr);
                    }
                    lineNumber++;
                }
            }
            fileStream.Dispose();
            fileStream.Close();
            return dt;
        }
        public class CsvFileReader : StreamReader
        {
            public CsvFileReader(Stream stream, System.Text.Encoding encoding)
                : base(stream, encoding)
            {
            }

            public CsvFileReader(string filename, System.Text.Encoding encoding)
                : base(filename, encoding)
            {
            }
            public override string ReadLine()
            {
                var line = base.ReadLine();
                if(string.IsNullOrEmpty(line))
                {
                    if(EndOfStream)
                    {
                        return null;
                    }
                    else
                    {
                        return ReadLine();
                    }
                }
                else
                {
                    return line;
                }
            }
            /// <summary>  
            /// Reads a row of data from a CSV file  
            /// </summary>  
            /// <param name="row"></param>  
            /// <returns></returns>  
            public bool ReadRow(List<string> row)
            {
                var LineText = ReadLine();

                if (String.IsNullOrEmpty(LineText))
                    return false;

                var count = LineText.Length;
                bool match = true;
                for (int i = 0; i < count; i++)
                {
                    char currentChar = LineText[i];
                    if (currentChar == '"')
                    {
                        match = !match;
                    }
                    if (i == count - 1)
                    {
                        if (!match)
                        {
                            var newLine = ReadLine();
                            if (string.IsNullOrEmpty(newLine)) {
                                return false;
                            }
                            LineText += "\n" + newLine;
                            count = LineText.Length;
                        }
                    }
                }

                int pos = 0;
                int rows = 0;

                while (pos < LineText.Length)
                {
                    string value;

                    // Special handling for quoted field  
                    if (LineText[pos] == '"')
                    {
                        // Skip initial quote  
                        pos++;

                        // Parse quoted value  
                        int start = pos;
                        while (pos < LineText.Length)
                        {
                            // Test for quote character  
                            if (LineText[pos] == '"')
                            {
                                // Found one  
                                pos++;

                                // If two quotes together, keep one  
                                // Otherwise, indicates end of value  
                                if (pos >= LineText.Length || LineText[pos] != '"')
                                {
                                    pos--;
                                    break;
                                }
                            }
                            pos++;
                        }
                        value = LineText.Substring(start, pos - start);
                        value = value.Replace("\"\"", "\"");
                    }
                    else
                    {
                        // Parse unquoted value  
                        int start = pos;
                        while (pos < LineText.Length && LineText[pos] != ',')
                            pos++;
                        value = LineText.Substring(start, pos - start);
                    }

                    // Add field to list  
                    if (rows < row.Count)
                        row[rows] = value;
                    else
                        row.Add(value);
                    rows++;

                    // Eat up to and including next comma  
                    while (pos < LineText.Length && LineText[pos] != ',')
                        pos++;
                    if (pos < LineText.Length)
                        pos++;
                }
                // Delete any unused items  
                while (row.Count > rows)
                    row.RemoveAt(rows);

                // Return true if any columns read  
                return (row.Count > 0);
            }
        }

    }

}