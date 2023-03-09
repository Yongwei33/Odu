using Oduna.Lib.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Configuration;
using System.Data.SqlClient;

namespace Oduna.Lib.Service
{
    public class DataService : BaseService
    {
        public DataTable GetRequisitionsHeadData()
        {
            string sSql = @"SELECT DISTINCT a.SYEV01, a.SYKCOO, a.SYDOCO, a.SYDCTO,d.DRDL01, B.PHMCU as SYMCU, B.PHOKCO as SYOKCO, B.PHOORN as SYOORN, B.PHOCTO as SYOCTO, e.ABAN8, 
e.ABALPH as SYAN8, b.PHSHAN as 出貨地址代碼, f.ABALPH as SYSHAN , b.PHDRQJ as SYDRQJ, b.PHTRDJ as SYTRDJ ,b.PHDESC as SYDESC, b.PHPTC, b.PHTXA1 as SYTXA1, 
g.DRDL01, b.PHPTC as 付款條件代碼,h.PNPTD as SYPTC, b.PHANBY as 採購人員代碼, i.ABALPH as SYANBY, b.PHTKBY, b.PHCRCD as SYCRCD, 
b.PHPOHC01 as POHC01,g.DRDL01 FROM PRODDTA.F554301Z a JOIN PRODDTA.F4301 b ON SYKCOO = PHKCOO AND SYDCTO = PHDCTO AND SYDOCO = PHDOCO 
JOIN (SELECT * FROM PRODDTA.F4311 WHERE PDDCTO LIKE 'Q%' AND PDLTTR <> '980' AND PDNXTR = '110') c ON SYKCOO = PDKCOO AND SYDCTO = PDDCTO AND SYDOCO = PDDOCO 
LEFT JOIN (SELECT * FROM PRODCTL.F0005 WHERE DRSY = '00' AND DRRT = 'DT') d on a.SYDCTO = trim(d.DRKY) LEFT JOIN PRODDTA.F0101 e ON b.PHAN8 = e.ABAN8 
LEFT JOIN PRODDTA.F0101 f ON b.PHSHAN = f.ABAN8 LEFT JOIN PRODCTL.F0005 g ON g.DRSY = '43' AND g.DRRT = 'C1' 
AND TRIM(g.DRKY) = TRIM(PHPOHC01) LEFT JOIN PRODDTA.F0014 h ON h.PNPTC = b.PHPTC LEFT JOIN PRODDTA.F0101 i ON b.PHANBY = i.ABAN8 
WHERE a.SYEV01='0' AND SYDCTO LIKE 'Q%'";//PRODDTA CRPDTA
            DataTable item = new DataTable();
            using (var conn = GetConnection())
            {
                conn.Open();
                var dr = conn.ExecuteReader(sSql);
                item.Load(dr);
            }
            return item;
        }

        public DataTable GetPurchaseHeadData()
        {
            string sSql = @"SELECT DISTINCT a.SYEV01, a.SYKCOO, a.SYDOCO, a.SYDCTO,d.DRDL01, B.PHMCU as SYMCU, B.PHOKCO as SYOKCO, B.PHOORN as SYOORN, B.PHOCTO as SYOCTO, e.ABAN8, 
e.ABALPH as SYAN8, b.PHSHAN as 出貨地址代碼, f.ABALPH as SYSHAN , b.PHDRQJ as SYDRQJ, b.PHTRDJ as SYTRDJ ,b.PHDESC as SYDESC, b.PHPTC, b.PHTXA1 as SYTXA1, 
g.DRDL01, b.PHPTC as 付款條件代碼,h.PNPTD as SYPTC, b.PHANBY as 採購人員代碼, i.ABALPH as SYANBY, b.PHTKBY, b.PHCRCD as SYCRCD, 
b.PHPOHC01 as POHC01,g.DRDL01 FROM PRODDTA.F554301Z a JOIN PRODDTA.F4301 b ON SYKCOO = PHKCOO AND SYDCTO = PHDCTO AND SYDOCO = PHDOCO 
JOIN (SELECT * FROM PRODDTA.F4311 WHERE PDDCTO LIKE 'B%' AND PDLTTR <> '980' AND PDNXTR = '230') c ON SYKCOO = PDKCOO AND SYDCTO = PDDCTO AND SYDOCO = PDDOCO 
LEFT JOIN (SELECT * FROM PRODCTL.F0005 WHERE DRSY = '00' AND DRRT = 'DT') d on a.SYDCTO = trim(d.DRKY) LEFT JOIN PRODDTA.F0101 e ON b.PHAN8 = e.ABAN8 
LEFT JOIN PRODDTA.F0101 f ON b.PHSHAN = f.ABAN8 LEFT JOIN PRODCTL.F0005 g ON g.DRSY = '43' AND g.DRRT = 'C1' 
AND TRIM(g.DRKY) = TRIM(PHPOHC01) LEFT JOIN PRODDTA.F0014 h ON h.PNPTC = b.PHPTC LEFT JOIN PRODDTA.F0101 i ON b.PHANBY = i.ABAN8 
WHERE a.SYEV01='0' AND SYDCTO LIKE 'B%'";//PRODDTA CRPDTA
            DataTable item = new DataTable();
            using (var conn = GetConnection())
            {
                conn.Open();
                var dr = conn.ExecuteReader(sSql);
                item.Load(dr);
            }
            return item;
        }

        public List<SW_GRID> GetRequisitionsGridData(string PDDOCO, string PDDCTO, string PDKCOO)
        {
            string sSql = @"SELECT a.PDDOCO, a.PDDCTO, a.PDKCOO, a.PDSFXO, a.PDLNID, a.PDKCOO AS 下單公司, a.PDDOCO AS 訂單號碼, a.PDDCTO AS 訂單類型, 
a.PDLNID AS 項次, a.PDMCU AS 分支, a.PDLITM AS 貨品編號, a.PDDSC1 AS 貨品名稱, a.PDDSC2 AS 規格, a.PDLNTY AS 行類型, a.PDUORG AS 數量, a.PDPRRC AS 單價, 
a.PDAEXP AS 金額, a.PDUOM3 AS 單位, a.PDFRRC AS 外幣單位成本, a.PDFEA AS 外幣總價, a.PDCRCD, b.CDEC FROM PRODDTA.F4311 a
LEFT JOIN (SELECT CVCRCD AS CRCD, CVCDEC AS CDEC FROM F0013) b ON b.CRCD = a.PDCRCD 
WHERE a.PDDOCO= :PDDOCO AND a.PDDCTO = :PDDCTO AND a.PDKCOO = :PDKCOO AND a.PDLTTR <> '980' AND a.PDNXTR = '110'";
            List<SW_GRID> item = null;
            using (var conn = GetConnection())
            {
                conn.Open();
                item = conn.Query<SW_GRID>(sSql, new { PDDOCO, PDDCTO, PDKCOO }).ToList();
            }
            return item;
        }

        public List<SW_GRID> GetPurchaseGridData(string PDDOCO, string PDDCTO, string PDKCOO)
        {
            string sSql = @"SELECT a.PDDOCO, a.PDDCTO, a.PDKCOO, a.PDSFXO, a.PDLNID, a.PDKCOO AS 下單公司, a.PDDOCO AS 訂單號碼, a.PDDCTO AS 訂單類型, 
a.PDLNID AS 項次, a.PDMCU AS 分支, a.PDLITM AS 貨品編號, a.PDDSC1 AS 貨品名稱, a.PDDSC2 AS 規格, a.PDLNTY AS 行類型, a.PDUORG AS 數量, a.PDPRRC AS 單價, 
a.PDAEXP AS 金額, a.PDUOM3 AS 單位, a.PDFRRC AS 外幣單位成本, a.PDFEA AS 外幣總價, a.PDCRCD, b.CDEC FROM PRODDTA.F4311 a
LEFT JOIN (SELECT CVCRCD AS CRCD, CVCDEC AS CDEC FROM F0013) b ON b.CRCD = a.PDCRCD 
WHERE a.PDDOCO= :PDDOCO AND a.PDDCTO = :PDDCTO AND a.PDKCOO = :PDKCOO AND a.PDLTTR <> '980' AND a.PDNXTR = '230'";
            List<SW_GRID> item = null;
            using (var conn = GetConnection())
            {
                conn.Open();
                item = conn.Query<SW_GRID>(sSql, new { PDDOCO, PDDCTO, PDKCOO }).ToList();
            }
            return item;
        }

        public string GetFormNo(string SYOKCO, string SYOORN, string SYOCTO)
        {
            string sSql = @"SELECT SYURRF FROM PRODDTA.F554301Z WHERE SYKCOO = :SYOKCO AND SYDOCO = :SYOORN AND SYDCTO = :SYOCTO";
            string item = null;
            using (var conn = GetConnection())
            {
                conn.Open();
                item = conn.Query<string>(sSql, new { SYOKCO, SYOORN, SYOCTO }).FirstOrDefault();
            }
            return item;
        }

        public string GetNXTR(string SYKCOO, string SYDOCO, string SYDCTO)
        {
            string sSql = @"SELECT PDNXTR FROM PRODDTA.F4311 WHERE PDKCOO = :SYKCOO AND PDDOCO = :SYDOCO AND PDDCTO = :SYDCTO";
            string item = null;
            using (var conn = GetConnection())
            {
                conn.Open();
                item = conn.Query<string>(sSql, new { SYKCOO, SYDOCO, SYDCTO }).FirstOrDefault();
            }
            return item;
        }

        public string GetAppendixId(string SYURRF)
        {
            string sSql = @"SELECT ATTACH_ID FROM TB_WKF_TASK WHERE DOC_NBR= @SYURRF";
            string item = null;
            using (var conn = GetSqlConnection())
            {
                conn.Open();
                item = conn.Query<string>(sSql, new { SYURRF }).FirstOrDefault();
            }
            return item;
        }

        public string GetApplicantId(string SYURRF)
        {
            string sSql = @"SELECT USER_GUID FROM TB_WKF_TASK WHERE DOC_NBR= @SYURRF";
            string item = null;
            using (var conn = GetSqlConnection())
            {
                conn.Open();
                item = conn.Query<string>(sSql, new { SYURRF }).FirstOrDefault();
            }
            return item;
        }

        public string GetUserId(string ACCOUNT)
        {
            string sSql = @"SELECT USER_GUID FROM TB_EB_USER WHERE ACCOUNT= @ACCOUNT";
            string item = null;
            using (var conn = GetSqlConnection())
            {
                conn.Open();
                item = conn.Query<string>(sSql, new { ACCOUNT }).FirstOrDefault();
            }
            return item;
        }

        public string GetApplicantAc(string USER_GUID)
        {
            string sSql = @"SELECT ACCOUNT FROM TB_EB_USER WHERE USER_GUID= @USER_GUID";
            string item = null;
            using (var conn = GetSqlConnection())
            {
                conn.Open();
                item = conn.Query<string>(sSql, new { USER_GUID }).FirstOrDefault();
            }
            return item;
        }

        public string GetApplicantNa(string USER_GUID)
        {
            string sSql = @"SELECT NAME FROM TB_EB_USER WHERE USER_GUID= @USER_GUID";
            string item = null;
            using (var conn = GetSqlConnection())
            {
                conn.Open();
                item = conn.Query<string>(sSql, new { USER_GUID }).FirstOrDefault();
            }
            return item;
        }

        public string GetUSINGVERSIONID(string FORM_NAME)
        {
            string sSql = @"SELECT USING_VERSION_ID FROM TB_WKF_FORM WHERE FORM_NAME= @FORM_NAME";
            string item = null;
            using (var conn = GetSqlConnection())
            {
                conn.Open();
                item = conn.Query<string>(sSql, new { FORM_NAME }).FirstOrDefault();
            }
            return item;
        }

        public bool SaveTaskData(string formXml)
        {
            string sSql = @"INSERT INTO [dbo].[TB_WKF_EXTERNAL_TASK] ([EXTERNAL_TASK_ID], [FORM_INFO], [STATUS]) VALUES (newid(), @formXml, '2')";
            bool result = false;
            using (var conn = GetSqlConnection())
            {
                conn.Open();
                var tx = conn.BeginTransaction();
                try
                {
                    conn.Execute(sSql, new { formXml }, transaction: tx);
                    result = true;
                    tx.Commit();
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    throw ex;
                }
            }
            return result;
        }

        public int InsertFormNo(string SYKCOO, string SYDOCO, string SYDCTO, string FormNo)
        {
            string sSql = @"UPDATE PRODDTA.F554301Z SET SYURRF = :FormNo WHERE SYKCOO = :SYKCOO AND SYDOCO = :SYDOCO AND SYDCTO = :SYDCTO";
            int effRows = -1;
            using (var conn = this.GetConnection())
            {
                conn.Open();
                try
                {
                    conn.Execute(sSql, new { FormNo, SYKCOO, SYDOCO, SYDCTO });
                    effRows = 1;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return effRows;
        }

        public int UpdateFlag1(string SYKCOO, string SYDOCO, string SYDCTO)
        {
            string sSql = @"UPDATE PRODDTA.F554301Z SET SYEV01 = '1' WHERE SYKCOO = :SYKCOO AND SYDOCO = :SYDOCO AND SYDCTO = :SYDCTO";
            int effRows = -1;
            using (var conn = this.GetConnection())
            {
                conn.Open();
                try
                {
                    conn.Execute(sSql, new { SYKCOO, SYDOCO, SYDCTO });
                    effRows = 1;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return effRows;
        }

        public int UpdateFlag2(string SYKCOO, string SYDOCO, string SYDCTO)
        {
            string sSql = @"UPDATE PRODDTA.F554301Z SET SYEV01 = '2' WHERE SYKCOO = :SYKCOO AND SYDOCO = :SYDOCO AND SYDCTO = :SYDCTO";
            int effRows = -1;
            using (var conn = this.GetConnection())
            {
                conn.Open();
                try
                {
                    conn.Execute(sSql, new { SYKCOO, SYDOCO, SYDCTO });
                    effRows = 1;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return effRows;
        }

        public int UpdateFlag3(string SYKCOO, string SYDOCO, string SYDCTO)
        {
            string sSql = @"UPDATE PRODDTA.F554301Z SET SYEV01 = '3' WHERE SYKCOO = :SYKCOO AND SYDOCO = :SYDOCO AND SYDCTO = :SYDCTO";
            int effRows = -1;
            using (var conn = this.GetConnection())
            {
                conn.Open();
                try
                {
                    conn.Execute(sSql, new { SYKCOO, SYDOCO, SYDCTO });
                    effRows = 1;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return effRows;
        }

        public int UpdateAdoptQ1ToQ3(string SYKCOO, string SYDOCO, string SYDCTO)
        {
            string sSql = @"UPDATE PRODDTA.F4311 SET PDLTTR = PDNXTR, PDNXTR = '140' WHERE PDKCOO = :SYKCOO AND PDDOCO = :SYDOCO AND PDDCTO = :SYDCTO AND PDLTTR <> '980'
 AND PDNXTR <> '999'";
            int effRows = -1;
            using (var conn = this.GetConnection())
            {
                conn.Open();
                try
                {
                    conn.Execute(sSql, new { SYKCOO, SYDOCO, SYDCTO });
                    effRows = 1;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return effRows;
        }

        public int UpdateAdoptQ4(string SYKCOO, string SYDOCO, string SYDCTO)
        {
            string sSql = @"UPDATE F4311 SET PDLTTR = PDNXTR, PDNXTR = '115' WHERE PDKCOO = :SYKCOO AND PDDOCO = :SYDOCO AND PDDCTO = :SYDCTO";
            int effRows = -1;
            using (var conn = this.GetConnection())
            {
                conn.Open();
                try
                {
                    conn.Execute(sSql, new { SYKCOO, SYDOCO, SYDCTO });
                    effRows = 1;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return effRows;
        }

        public int UpdateRejectQ1ToQ3(string SYKCOO, string SYDOCO, string SYDCTO)
        {
            string sSql = @"UPDATE PRODDTA.F4311 SET PDLTTR = PDNXTR, PDNXTR = '110' WHERE PDKCOO = :SYKCOO AND PDDOCO = :SYDOCO AND PDDCTO = :SYDCTO";
            int effRows = -1;
            using (var conn = this.GetConnection())
            {
                conn.Open();
                try
                {
                    conn.Execute(sSql, new { SYKCOO, SYDOCO, SYDCTO });
                    effRows = 1;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return effRows;
        }

        public int UpdateRejectQ4(string SYKCOO, string SYDOCO, string SYDCTO)
        {
            string sSql = @"UPDATE PRODDTA.F4311 SET PDLTTR = PDNXTR, PDNXTR = '110' WHERE PDKCOO = :SYKCOO AND PDDOCO = :SYDOCO AND PDDCTO = :SYDCTO";
            int effRows = -1;
            using (var conn = this.GetConnection())
            {
                conn.Open();
                try
                {
                    conn.Execute(sSql, new { SYKCOO, SYDOCO, SYDCTO });
                    effRows = 1;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return effRows;
        }

        public int UpdatePurchaseB1ToB4(string SYKCOO, string SYDOCO, string SYDCTO)
        {
            string sSql = @"UPDATE PRODDTA.F4311 SET PDLTTR = PDNXTR, PDNXTR = '280' WHERE PDKCOO = :SYKCOO AND PDDOCO = :SYDOCO AND PDDCTO = :SYDCTO AND PDLTTR <> '980'
 AND PDNXTR <> '999'";
            int effRows = -1;
            using (var conn = this.GetConnection())
            {
                conn.Open();
                try
                {
                    conn.Execute(sSql, new { SYKCOO, SYDOCO, SYDCTO });
                    effRows = 1;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return effRows;
        }

        public DataTable GetTestHeadData()
        {
            string sSql = @"SELECT DISTINCT a.SYEV01, a.SYKCOO, a.SYDOCO, a.SYURRF, a.SYDCTO,d.DRDL01, B.PHMCU as SYMCU, B.PHOKCO as SYOKCO, B.PHOORN as SYOORN, B.PHOCTO as SYOCTO, 
e.ABAN8, e.ABALPH as SYAN8, b.PHSHAN as 出貨地址代碼, f.ABALPH as SYSHAN , b.PHDRQJ as SYDRQJ, b.PHDRQJ as SYTRDJ 
,b.PHDESC as SYDESC, b.PHPTC, b.PHTXA1 as SYTXA1, g.DRDL01, b.PHPTC as 付款條件代碼,h.PNPTD as SYPTC, b.PHANBY as 採購人員代碼, i.ABALPH as SYANBY, b.PHTKBY, 
b.PHCRCD as SYCRCD, b.PHPOHC01 as POHC01,g.DRDL01 FROM PRODDTA.F554301Z a 
JOIN PRODDTA.F4301 b ON SYKCOO = PHKCOO AND SYDCTO = PHDCTO AND SYDOCO = PHDOCO 
LEFT JOIN PRODCTL.F0005 d on a.SYDCTO = trim(d.DRKY) LEFT JOIN PRODDTA.F0101 e ON b.PHAN8 = e.ABAN8 
LEFT JOIN PRODDTA.F0101 f ON b.PHSHAN = f.ABAN8 LEFT JOIN PRODCTL.F0005 g ON g.DRSY = '43' AND g.DRRT = 'C1' AND TRIM(g.DRKY) = TRIM(PHPOHC01) 
LEFT JOIN PRODDTA.F0014 h ON h.PNPTC = b.PHPTC LEFT JOIN PRODDTA.F0101 i ON b.PHAN8 = i.ABAN8 WHERE a.SYEV01='0' AND SYDCTO LIKE 'B%' ";
            DataTable item = new DataTable();
            using (var conn = GetConnection())
            {
                conn.Open();
                var dr = conn.ExecuteReader(sSql);
                item.Load(dr);
            }
            return item;
        }

        public List<SW_GRID> GetTestGridData(string PDDOCO, string PDDCTO, string PDKCOO)
        {
            string sSql = @"SELECT a.PDDOCO, a.PDDCTO, a.PDKCOO, a.PDSFXO, a.PDLNID, a.PDKCOO AS 下單公司, a.PDDOCO AS 訂單號碼, a.PDDCTO AS 訂單類型, 
a.PDLNID AS 項次, a.PDMCU AS 分支, a.PDLITM AS 貨品編號, a.PDDSC1 AS 貨品名稱, a.PDDSC2 AS 規格, a.PDLNTY AS 行類型, a.PDUORG AS 數量, a.PDPRRC AS 單價, 
a.PDAEXP AS 金額, a.PDUOM3 AS 單位, a.PDFRRC AS 外幣單位成本, a.PDFEA AS 外幣總價, a.PDCRCD, a.CDEC FROM F4311 a 
WHERE a.PDDOCO= :PDDOCO AND a.PDDCTO = :PDDCTO AND a.PDKCOO = :PDKCOO";
            List<SW_GRID> item = null;
            using (var conn = GetConnection())
            {
                conn.Open();
                item = conn.Query<SW_GRID>(sSql, new { PDDOCO, PDDCTO, PDKCOO }).ToList();
            }
            return item;
        }

        public DataTable GetData()
        {
            string sSql = @"SELECT PDDOCO, PDDCTO, PDKCOO, PDSFXO, PDLNID, PDKCOO AS 下單公司, PDDOCO AS 訂單號碼, PDDCTO AS 訂單類型, 
PDLNID AS 項次, PDMCU AS 分支, PDLITM AS 貨品編號, PDDSC1 AS 貨品名稱, PDDSC2 AS 規格, PDLNTY AS 行類型, PDUORG AS 數量, PDPRRC AS 單價, 
PDAEXP AS 金額, PDUOM3 AS 單位, PDFRRC AS 外幣單位成本, PDFEA AS 外幣總價 FROM CRPDTA.F4311";
            DataTable item = new DataTable();
            using (var conn = GetConnection())
            {
                conn.Open();
                var dr = conn.ExecuteReader(sSql);
                item.Load(dr);
            }
            return item;
        }

        public int UpdateTest()
        {
            string sSql = @"UPDATE Table1 SET Class = '10' WHERE Name = '123             '";
            int effRows = -1;
            using (var conn = GetSqlConnection())
            {
                conn.Open();
                try
                {
                    conn.Execute(sSql);
                    effRows = 1;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return effRows;
        }

        public static SqlConnection GetSqlConnection()
        {
            string dbConnStr = ConfigurationManager.ConnectionStrings["connectionstring"].ConnectionString;

            return new SqlConnection(dbConnStr);
        }
    }
}
