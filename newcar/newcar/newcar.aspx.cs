using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace newcar.newcar
{
    public partial class newcar : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            string filePath = FileUpload1.PostedFile.FileName;
            DataTable dt = ImportExcelFile(filePath);
            GridView1.DataSource = dt;
            GridView1.DataBind();
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            DataTable dt = GridViewtoDataTable(GridView1);
            HttpContext curContext = HttpContext.Current;
            // 设置编码和附件格式
            curContext.Response.ContentType = "application/vnd.ms-excel";
            curContext.Response.ContentEncoding = Encoding.UTF8;
            curContext.Response.Charset = "";
            curContext.Response.AppendHeader("Content-Disposition",
                "attachment;filename=" + HttpUtility.UrlEncode("EntyWorkbook.xlsx", Encoding.UTF8));
            curContext.Response.BinaryWrite(WriteExcel(dt, "EntyWorkbook.xlsx").GetBuffer());
            curContext.Response.End();
        }

        IWorkbook xssfworkbook;
        #region  
        public DataTable ImportExcelFile(string filePath)
        {
            #region//初始化信息  
            try
            {
                using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    if (filePath.IndexOf(".xlsx") > 0) // 2007版本
                        xssfworkbook = new XSSFWorkbook(file);
                    else if (filePath.IndexOf(".xls") > 0) // 2003版本
                        xssfworkbook = new HSSFWorkbook(file); 
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            #endregion
            ISheet sheet = xssfworkbook.GetSheetAt(0);
            IEnumerator rows = sheet.GetRowEnumerator();
            DataTable dt = new DataTable();
            IRow headrow = sheet.GetRow(0);
            for (int j = 0; j < (sheet.GetRow(0).LastCellNum); j++)
            {
                //dt.Columns.Add(Convert.ToChar(((int)'A') + j).ToString());
                //首行做表头
                ICell cell = headrow.GetCell(j);
                dt.Columns.Add(cell.ToString());
            }
            for (int x=(sheet.FirstRowNum+1); x<=sheet.LastRowNum;x++)
            {
                IRow row = sheet.GetRow(x);
                DataRow dr = dt.NewRow();
                for (int i = 0; i < row.LastCellNum; i++)
                {
                    ICell cell = row.GetCell(i);
                    if (cell == null)
                    {
                        dr[i] = null;
                    }
                    else
                    {
                        dr[i] = cell.ToString();
                    }
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }
        #endregion
        public static MemoryStream WriteExcel(DataTable dt, string filePath)
        {
                IWorkbook book = null;
                if (filePath.IndexOf(".xlsx") > 0) // 2007版本
                    book = new XSSFWorkbook();
                else if (filePath.IndexOf(".xls") > 0) // 2003版本
                    book = new HSSFWorkbook();
                dt.TableName = "table";
                ISheet sheet = book.CreateSheet(dt.TableName);

                IRow row = sheet.CreateRow(0);
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    row.CreateCell(i).SetCellValue(dt.Columns[i].ColumnName);
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    IRow row2 = sheet.CreateRow(i + 1);
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        row2.CreateCell(j).SetCellValue(Convert.ToString(dt.Rows[i][j]));
                    }
                }
                // 写入到客户端  
                using (MemoryStream ms = new MemoryStream())
                {
                    book.Write(ms);
                    ms.Flush();
                    return ms;
                }  
        }

        public static string GetCellText(TableCell cell)
        {
            string text = cell.Text;
            if (!string.IsNullOrEmpty(text))
            {
                return text;
            }
            foreach (Control control in cell.Controls)
            {
                if (control != null && control is IButtonControl)
                {
                    IButtonControl btn = control as IButtonControl;
                    text = btn.Text.Replace("\r\n", "").Trim();
                    break;
                }
                if (control != null && control is ITextControl)
                {
                    LiteralControl lc = control as LiteralControl;
                    if (lc != null)
                    {
                        continue;
                    }
                    ITextControl l = control as ITextControl;

                    text = l.Text.Replace("\r\n", "").Trim();
                    break;
                }
            }
            return text;
        }
        /// <summary>
        /// 从GridView的数据生成DataTable
        /// </summary>
        /// <param name="gv">GridView对象</param>
        public static DataTable GridViewtoDataTable(GridView gv)
        {
            DataTable table = new DataTable();
            int rowIndex = 0;
            List<string> cols = new List<string>();
            if (!gv.ShowHeader && gv.Columns.Count == 0)
            {
                return table;
            }
            GridViewRow headerRow = gv.HeaderRow;
            int columnCount = headerRow.Cells.Count;
            for (int i = 0; i < columnCount; i++)
            {
                string text = GetCellText(headerRow.Cells[i]);
                cols.Add(text);
            }
            foreach (GridViewRow r in gv.Rows)
            {
                if (r.RowType == DataControlRowType.DataRow)
                {
                    DataRow row = table.NewRow();
                    int j = 0;
                    for (int i = 0; i < columnCount; i++)
                    {
                        string text = GetCellText(r.Cells[i]);
                        if (!String.IsNullOrEmpty(text))
                        {
                            if (rowIndex == 0)
                            {
                                string columnName = cols[i];
                                if (String.IsNullOrEmpty(columnName))
                                {
                                    continue;
                                }
                                if (table.Columns.Contains(columnName))
                                {
                                    continue;
                                }
                                DataColumn dc = table.Columns.Add();
                                dc.ColumnName = columnName;
                                dc.DataType = typeof(string);
                            }
                            row[j] = text;
                            j++;
                        }
                    }
                    rowIndex++;
                    table.Rows.Add(row);
                }
            }
            return table;
        }
    }
}