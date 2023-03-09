using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oduna.Lib.Model
{
    public class SW_GRID
    {
        public string PDDOCO { get; set; }
        public string PDDCTO { get; set; }
        public string PDKCOO { get; set; }
        public string PDSFXO { get; set; }
        public string PDLNID { get; set; }
        public string 下單公司 { get; set; }
        public string 訂單號碼 { get; set; }
        public string 訂單類型 { get; set; }
        public string 項次 { get; set; }
        public string 分支 { get; set; }
        public string 貨品編號 { get; set; }
        public string 貨品名稱 { get; set; }
        public string 規格 { get; set; }
        public string 行類型 { get; set; }
        public string 數量 { get; set; }
        public string 單價 { get; set; }
        public string 金額 { get; set; }
        public string 單位 { get; set; }
        public string 外幣單位成本 { get; set; }
        public string 外幣總價 { get; set; }
        public string CDEC { get; set; }
    }

    public class SW_DATAGRID
    {
        //Dictionary<明細ID, 明細每列資料>
        public Dictionary<string, List<SW_ROW>> DicDataGrid { get; set; }
    }
    public class SW_ROW
    {
        public string ORDER { get; set; }
        public Dictionary<string, string> CELLS { get; set; }
    }
}
