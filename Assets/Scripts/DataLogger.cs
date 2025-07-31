using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

public class DataLogger
{
    private const string uninitializedValuePlaceholder = "uninitialized";
    public List<string[]> LoggedData { get; private set; }
    public Dictionary<string, int> ColumnsIndex { get; private set; }
    private (string, Func<string>)[] dataGetterFuncTuples;

    #region DataLogger class constructors
    public DataLogger(string[] columnNames, Func<string>[] dataGetterFuncs) : this(columnNames.Zip(dataGetterFuncs, (a, b) => (a, b)).ToArray())
    {
        if (columnNames.Length != dataGetterFuncs.Length) throw new Exception("columnNames and dataGetterFuncs have to be the same length!");
    }

    public DataLogger((string, Func<string>)[] dataGetterFuncTuples)
    {
        LoggedData = new List<string[]>();

        this.dataGetterFuncTuples = dataGetterFuncTuples;
        ColumnsIndex = new Dictionary<string, int>();
        for (int i = 0; i < dataGetterFuncTuples.Length; i++)
        {
            ColumnsIndex.Add(dataGetterFuncTuples[i].Item1, i);
        }
    }
    #endregion

    #region AddColumns()
    public void AddColumn(string newColumnName, Func<string> newDataGetterFunc)
    {
        AddColumn((newColumnName, newDataGetterFunc));
    }

    public void AddColumn((string, Func<string>) newDataGetterFuncTuple)
    {
        AddColumns(new (string, Func<string>)[] { newDataGetterFuncTuple });
    }

    public void AddColumns(string[] newColumnsNames, Func<string>[] newDataGetterFuncs)
    {
        if (newColumnsNames.Length != newDataGetterFuncs.Length) throw new Exception("newColumnsNames and  newDataGetterFuncs have to be the same length!");
        AddColumns(newColumnsNames.Zip(newDataGetterFuncs, (a, b) => (a, b)).ToArray());
    }
    public void AddColumns((string, Func<string>)[] newDataGetterFuncTuples)
    {
        Array.Resize(ref this.dataGetterFuncTuples, this.dataGetterFuncTuples.Length + newDataGetterFuncTuples.Length);
        for (int i = 0; i < newDataGetterFuncTuples.Length; i++)
        {
            this.dataGetterFuncTuples[this.dataGetterFuncTuples.Length - newDataGetterFuncTuples.Length + i] = newDataGetterFuncTuples[i];
        }
    }
    #endregion

    public void LogData()
    {
        string[] newRowData = new string[dataGetterFuncTuples.Length];
        for (int i = 0; i < newRowData.Length; i++)
        {
            newRowData[i] = dataGetterFuncTuples[i].Item2.Invoke();
        }
        LoggedData.Add(newRowData);
    }

    public void ClearData()
    {
        LoggedData.Clear();
    }

    #region Exporting Data to CSV
    /// <summary>
    /// Exports data to csv, does NOT escape commas and quotes. Consider using a proper C# library eg. https://joshclose.github.io/CsvHelper/
    /// </summary>
    /// <param name="filePath">filePath from Application.persistentDataPath</param>
    public void ExportDataToCSV(string filePath, bool isIncludeHeaders = true, bool isAddSurroundingQuotes = true, bool isAppendDatetimeToFileName = true)
    {

        // Change file names to include datetime for tracking and (n) to guarentee no overriding files.
        if (isAppendDatetimeToFileName) filePath = string.Format("{0} {1}", filePath, DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        string _fullFilePath = GetIndexedFilePath(Path.Join(Application.persistentDataPath, filePath), "csv");

        // Write file
        StringBuilder csv = new StringBuilder();

        if (isIncludeHeaders) csv.AppendLine(ProcessToCSVRow(dataGetterFuncTuples.Select(x => x.Item1), isAddSurroundingQuotes: isAddSurroundingQuotes, isAddDelimiter: false));
        string _uninitializedValuePlaceHolder = isAddSurroundingQuotes ? string.Format("\"{0}\"", uninitializedValuePlaceholder) : uninitializedValuePlaceholder;

        int _numColumns = dataGetterFuncTuples.Length;
        int _numMissingVals = 0;
        string[] _rowData = new string[dataGetterFuncTuples.Length];
        Array.Fill(_rowData, _uninitializedValuePlaceHolder);

        for (int row_index = 0; row_index < LoggedData.Count; row_index++)
        {
            _numMissingVals = _numColumns - LoggedData[row_index].Length;
            if (_numMissingVals > 0)
            {
                Array.Copy(LoggedData[row_index], 0, _rowData, 0, LoggedData[row_index].Length);
                csv.AppendLine(ProcessToCSVRow(_rowData, isAddSurroundingQuotes: isAddSurroundingQuotes, isAddDelimiter: false));
            }
            else csv.AppendLine(ProcessToCSVRow(LoggedData[row_index], isAddSurroundingQuotes: isAddSurroundingQuotes, isAddDelimiter: false));
        }

        Debug.Log(string.Format("CSV Exported to \"{0}\"", _fullFilePath));
        File.WriteAllText(_fullFilePath, csv.ToString());
    }

    private string ProcessToCSVRow(IEnumerable<string> strings, bool isAddSurroundingQuotes = true, bool isAddDelimiter = true)
    {
        // From CSV "standard" https://www.rfc-editor.org/rfc/rfc4180
        const string csvItemSeparator = ",";
        const string csvDelimiter = "\r\n";

        if (isAddSurroundingQuotes) strings = strings.Select(x => String.Format("\"{0}\"", x));
        return string.Format("{0}{1}", string.Join(csvItemSeparator, strings), (isAddDelimiter ? csvDelimiter : String.Empty));
    }

    private string GetIndexedFilePath(string fileStub, string fileExtension)
    {
        // Appends (n) to file stub if file of same name already exists
        string filename = String.Format("{0}.{1}", fileStub, fileExtension);
        int ix = 0;
        while (File.Exists(filename))
        {
            ix++;
            filename = string.Format("{0} ({1}).{2}", fileStub, ix, fileExtension);
        }
        return filename;
    }
    #endregion
}
