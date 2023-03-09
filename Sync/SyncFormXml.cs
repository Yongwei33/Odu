using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Reflection;
using Ede.Uof.Utility.Log;
using Oduna.Lib.Model;
using System.Data;
using Newtonsoft.Json;

namespace Oduna.Lib.Sync
{
    public class SyncFormXml
    {
        public SyncFormXml(string formVersionId, UrgentLevel Urgentlevel, string account, string userGuid, string userName, DataRow head, List<SW_GRID> grid)
        {
            this.FormVersionId = formVersionId;
            this.Urgentlevel = Urgentlevel;
            this.Account = account;
            this.UserGuid = userGuid;
            this.UserName = userName;
            Fields = new FieldList();
            ApplyAttach = new List<string>();
            this.head = head;
            this.grid = grid;
        }

        DataRow head;
        List<string> modellist = new List<string>() { "SZLITM", "SZDSC1", "SZDSC2", "SZLNTY", "SZUORG", "SZPRRC", "SZAEXP", "SZUOM3", "SZFRRC", "SZFEA", "remark" };
        List<SW_GRID> grid;
        double total = 0;
        double foreignTotal = 0;
        #region==================申請資訊==========================
        public string FormVersionId { get; set; } = "";
        public UrgentLevel Urgentlevel { get; set; } = UrgentLevel.Urgent;
        public string Account { get; set; } = "";
        public string UserGuid { get; set; } = "";
        public string UserName { get; set; } = "";
        public string GroupId { get; set; } = "";
        public string JobTitleId { get; set; } = "";
        public string Comment { get; set; } = "";
        public bool IsNeedTransfer { get; set; } = true;
        public bool IsDeleteTemp { get; set; } = true;

        public List<string> ApplyAttach { get; set; } = new List<string>();
        public FieldList Fields { get; set; } = new FieldList();

        public MessageContent MessageContent { get; set; } = new MessageContent();
        public DisplayTitle DisplayTitle { get; set; } = new DisplayTitle();
        #endregion

        public string ConvertToFormInfoXml(string applicantId, string applicantAc, string applicantNa)
        {
            //表單表頭資訊
            XElement formXe = new XElement("Form", new XAttribute("formVersionId", this.FormVersionId)
                , new XAttribute("urgentLevel", (int)this.Urgentlevel));

            //申請者區塊
            XElement applicantXe = new XElement("Applicant", new XAttribute("account", this.Account)
                , new XAttribute("groupId", this.GroupId), new XAttribute("jobTitleId", this.JobTitleId));
            //申請者意見
            XElement CommentXe = new XElement("Comment", this.Comment);

            //申請附件
            if (ApplyAttach.Count > 0)
            {
                XElement attachXe = new XElement("Attach", new XAttribute("IsNeedTransfer", this.IsNeedTransfer)
              , new XAttribute("IsDeleteTemp", this.IsDeleteTemp));

                applicantXe.Add(attachXe);

                foreach (var path in ApplyAttach)
                {
                    XElement attachItemXe = new XElement("AttachItem",
                        new XAttribute("filePath", path));

                    attachXe.Add(attachItemXe);
                }
            }

            formXe.Add(applicantXe);
            applicantXe.Add(CommentXe);

            //Logger.Write("Sync", JsonConvert.SerializeObject(grid, Formatting.Indented));

            //欄位
            XElement FormFieldValueXE = new XElement("FormFieldValue");
            formXe.Add(FormFieldValueXE);
            foreach (var Propertie in this.Fields.GetType().GetProperties())
            {

                var field = ConvertToChildObject(Propertie, this.Fields);
                XElement FieldItemXE = new XElement("FieldItem", new XAttribute("fieldId", field.FieldId));
                FormFieldValueXE.Add(FieldItemXE);

                //排除掉用不到fieldValue的欄位
                switch (field.FieldType)
                {
                    case FieldType.optionalField:
                        FileField optionalField = (FileField)field;
                        break;
                    case FieldType.dataGrid:
                        XElement DataGridElement = new XElement("DataGrid");
                        FieldItemXE.Add(DataGridElement);
                        if (grid.Count > 0)
                        {
                            int rowIndex = 0;
                            foreach (var row in grid)
                            {
                                XElement rowElement = new XElement("Row", new XAttribute("order", rowIndex.ToString()));
                                rowIndex++;
                                foreach (var row2 in modellist)
                                {
                                    XElement cellElement = new XElement("Cell", new XAttribute("fieldId", row2));
                                    switch (row2)
                                    {
                                        case "SZLITM":
                                            cellElement.Add(new XAttribute("fieldValue", row.貨品編號));
                                            break;
                                        case "SZDSC1":
                                            cellElement.Add(new XAttribute("fieldValue", row.貨品名稱));
                                            break;
                                        case "SZDSC2":
                                            cellElement.Add(new XAttribute("fieldValue", row.規格));
                                            break;
                                        case "SZLNTY":
                                            cellElement.Add(new XAttribute("fieldValue", row.行類型));
                                            break;
                                        case "SZUORG":
                                            if (row.數量.All(char.IsDigit)) 
                                                cellElement.Add(new XAttribute("fieldValue", Convert.ToDouble(row.數量) / 10000));
                                            else
                                                cellElement.Add(new XAttribute("fieldValue", row.數量));
                                            break;
                                        case "SZPRRC":
                                            if (row.單價.All(char.IsDigit))
                                                cellElement.Add(new XAttribute("fieldValue", Convert.ToDouble(row.單價) / 10000));
                                            else
                                                cellElement.Add(new XAttribute("fieldValue", row.單價));
                                            break;
                                        case "SZAEXP":
                                            cellElement.Add(new XAttribute("fieldValue", row.金額));
                                            if(!string.IsNullOrEmpty(row.金額))
                                                total += Convert.ToDouble(row.金額);
                                            break;
                                        case "SZUOM3":
                                            cellElement.Add(new XAttribute("fieldValue", row.單位));
                                            break;
                                        case "SZFRRC":
                                            //Logger.Write("Sync", "外幣單位成本 " + row.外幣單位成本);
                                            if (row.外幣單位成本.All(char.IsDigit))
                                                cellElement.Add(new XAttribute("fieldValue", Convert.ToDouble(row.外幣單位成本) / 10000));
                                            else
                                                cellElement.Add(new XAttribute("fieldValue", row.外幣單位成本));
                                            break;
                                        case "SZFEA":
                                            //Logger.Write("Sync", "外幣總價 " + row.外幣總價);
                                            double tot;
                                            if (row.CDEC.Trim() == "0")
                                            {
                                                tot = Convert.ToDouble(row.外幣總價);
                                                foreignTotal += Convert.ToDouble(row.外幣總價);
                                                cellElement.Add(new XAttribute("fieldValue", tot));
                                            }
                                            else if (row.CDEC.Trim() == "1")
                                            {
                                                tot = Convert.ToDouble(row.外幣總價) / 10;
                                                foreignTotal += Convert.ToDouble(row.外幣總價) / 10;
                                                cellElement.Add(new XAttribute("fieldValue", tot.ToString("0.0")));
                                            }
                                            else if (row.CDEC.Trim() == "2")
                                            {
                                                tot = Convert.ToDouble(row.外幣總價) / 100;
                                                foreignTotal += Convert.ToDouble(row.外幣總價) / 100;
                                                cellElement.Add(new XAttribute("fieldValue", tot.ToString("0.00")));
                                            }
                                            else if (row.CDEC.Trim() == "3")
                                            {
                                                tot = Convert.ToDouble(row.外幣總價) / 1000;
                                                foreignTotal += Convert.ToDouble(row.外幣總價) / 1000;
                                                cellElement.Add(new XAttribute("fieldValue", tot.ToString("0.000")));
                                            }
                                            else if (row.CDEC.Trim() == "4")
                                            {
                                                tot = Convert.ToDouble(row.外幣總價) / 10000;
                                                foreignTotal += Convert.ToDouble(row.外幣總價) / 1000;
                                                cellElement.Add(new XAttribute("fieldValue", tot.ToString("0.0000")));
                                            }
                                            else
                                                cellElement.Add(new XAttribute("fieldValue", Convert.ToInt32(row.外幣總價)));
                                            break;
                                        default:
                                            cellElement.Add(new XAttribute("fieldValue", ""));
                                            break;
                                    }
                                    rowElement.Add(cellElement);
                                }
                                
                                DataGridElement.Add(rowElement);
                            }

                            FieldItemXE.Add(new XAttribute("fillerName", this.UserName));
                            FieldItemXE.Add(new XAttribute("fillerUserGuid", this.UserGuid));
                            FieldItemXE.Add(new XAttribute("fillerAccount", this.Account));
                            FieldItemXE.Add(new XAttribute("fillSiteId", ""));
                        }
                        break;
                    case FieldType.fileButton:
                        FileField fileField = (FileField)field;

                        if (fileField.FileAttach.Count > 0)
                        {
                            FieldItemXE.Add(new XAttribute("IsNeedTransfer", fileField.IsNeedTransfer.ToString()));
                            FieldItemXE.Add(new XAttribute("IsDeleteTemp", fileField.IsDeleteTemp.ToString()));
                            FieldItemXE.Add(new XAttribute("fillerName", this.UserName));
                            FieldItemXE.Add(new XAttribute("fillerUserGuid", this.UserGuid));
                            FieldItemXE.Add(new XAttribute("fillerAccount", this.Account));
                            FieldItemXE.Add(new XAttribute("fillSiteId", ""));

                            foreach (var filePath in fileField.FileAttach)
                            {
                                XElement attachItemXE = new XElement("AttachItem", new XAttribute("filePath", filePath));
                                FieldItemXE.Add(attachItemXE);
                            }

                        }

                        break;
                    default:
                        if (field.FieldId == "formNo")
                            FieldItemXE.Add(new XAttribute("fieldValue", ""));
                        else if (field.FieldId == "total")
                            FieldItemXE.Add(new XAttribute("fieldValue", total));
                        else if (field.FieldId == "foreignTotal")
                            FieldItemXE.Add(new XAttribute("fieldValue", foreignTotal.ToString("0.00")));
                        else if (field.FieldId == "SYDRQJ")
                            FieldItemXE.Add(new XAttribute("fieldValue", ConvertDate(head["SYDRQJ"].ToString())));
                        else if (field.FieldId == "SYTRDJ")
                            FieldItemXE.Add(new XAttribute("fieldValue", ConvertDate(head["SYTRDJ"].ToString())));
                        else if (field.FieldId == "080")
                            FieldItemXE.Add(new XAttribute("fieldValue", ""));
                        /*else if (field.FieldId == "SYANBY")
                        {
                            if (string.IsNullOrEmpty(applicantId) || string.IsNullOrEmpty(applicantAc) || string.IsNullOrEmpty(applicantNa))
                            {
                                FieldItemXE.Add(new XAttribute("fieldValue", ""));
                            }
                            else
                            {
                                FieldItemXE.Add(new XAttribute("fieldValue", applicantNa + "(" + applicantAc + ")"));
                                FieldItemXE.Add(new XAttribute("realValue", $"<UserSet><Element type='user'> <userId>" + applicantId + "</userId></Element></UserSet>\r\n"));
                            }
                        }*/
                        else
                            FieldItemXE.Add(new XAttribute("fieldValue", head[field.FieldId].ToString()));
                        break;
                }

                
                if (field.FieldId != "formNo" && field.FieldId != "total" && field.FieldId != "foreignTotal" && field.FieldId != "080" && field.FieldId != "DETAIL" && head[field.FieldId].ToString() != "")
                {
                    FieldItemXE.Add(new XAttribute("fillerName", this.UserName));
                    FieldItemXE.Add(new XAttribute("fillerUserGuid", this.UserGuid));
                    FieldItemXE.Add(new XAttribute("fillerAccount", this.Account));
                    FieldItemXE.Add(new XAttribute("fillSiteId", ""));
                }

                //加殊屬性的欄位
                switch (field.FieldType)
                {
                    case FieldType.autoNumber:
                        // <FieldItem fieldId="NO" fieldValue="" IsNeedAutoNbr ="false" />   
                        FieldItemXE.Add(new XAttribute("IsNeedAutoNbr", ((AutoNumnerField)field).IsNeedAutoNbr));

                        break;
                    case FieldType.numberText:
                    case FieldType.singleLineText:

                        break;
                }

            }

            //起單顯示標題
            //XElement jsonDisplayXE = new XElement("JsonDisplay", Newtonsoft.Json.JsonConvert.SerializeObject(DisplayTitle));
            //formXe.Add(jsonDisplayXE);

            //起單郵件樣板
            /*XElement messageContent = new XElement("MessageContent");
            formXe.Add(messageContent);

            foreach (var Propertie in this.MessageContent.GetType().GetProperties())
            {
                XElement contentXE = new XElement(Propertie.Name.ToString().Substring(1));
              
                contentXE.Value = Propertie.GetValue(this.MessageContent).ToString();
                messageContent.Add(contentXE);
            }*/

            //Logger.Write("FormXml", formXe.ToString());
            return formXe.ToString();
        }

        private Field ConvertToChildObject(PropertyInfo propertyInfo, object parent)
        {
            var source = propertyInfo.GetValue(parent);
            var destination = Activator.CreateInstance(propertyInfo.PropertyType);

            foreach (PropertyInfo prop in destination.GetType().GetProperties().ToList())
            {
                var value = source.GetType().GetProperty(prop.Name).GetValue(source, null);
                prop.SetValue(destination, value, null);
            }

            return (Field)destination;
        }

        private string ConvertToChildObjectForString(PropertyInfo propertyInfo, object parent)
        {
            var source = propertyInfo.GetValue(parent);
            var destination = Activator.CreateInstance(propertyInfo.PropertyType);

            foreach (PropertyInfo prop in destination.GetType().GetProperties().ToList())
            {
                var value = source.GetType().GetProperty(prop.Name).GetValue(source, null);
                prop.SetValue(destination, value, null);
            }

            return (string)destination;
        }

        public string ConvertDate(string stringDate)
        {
            string date = "";
            int centry = 0;
            int year;
            if (!string.IsNullOrEmpty(stringDate))
            {
                if (stringDate.Substring(0, 1) == "1")
                    centry = 2000;
                else if (stringDate.Substring(0, 1) == "2")
                    centry = 2100;

                year = centry + Convert.ToInt32(stringDate.Substring(1, 2));
                date = year.ToString() + '-' + "01" + '-' + "01";
                DateTimeOffset dateTime = Convert.ToDateTime(date);
                dateTime = dateTime.AddDays(Convert.ToInt32(stringDate.Substring(3))).AddDays(-1);
                date = dateTime.ToString("yyyy/MM/dd");
            }
            return date;
        }
    }


        #region===================列舉==========================
        /// <summary>
        /// 表單的緊急程度
        /// </summary>
        public enum UrgentLevel : int
        {
            /// <summary>
            /// 緊急
            /// </summary>
            MostUrgent,
            /// <summary>
            /// 急
            /// </summary>
            Urgent,
            /// <summary>
            /// 普通
            /// </summary>
            Normal

        }//end UrgentLevel


        /// <summary>
        /// 表單版本欄位的欄位格式
        /// </summary>
        /// <remarks></remarks>
        public enum FieldType : int
        {
            /// <summary>
            /// 單行文字欄位singleLineText
            /// </summary>
            singleLineText = 0,

            /// <summary>
            /// 多行文字欄位multiLineText
            /// </summary>
            multiLineText = 1,

            /// <summary>
            /// 數值欄位numberText
            /// </summary>
            numberText = 2,

            /// <summary>
            /// 檔案選取欄位fileButton
            /// </summary>
            fileButton = 3,

            /// <summary>
            /// 日期欄位dateSelect
            /// </summary>
            dateSelect = 4,

            /// <summary>
            /// 時間欄位timeSelect
            /// </summary>
            timeSelect = 5,

            /// <summary>
            /// 核選方塊checkBox
            /// </summary>
            checkBox = 6,

            /// <summary>
            /// 單選鈕radioButton
            /// </summary>
            radioButton = 7,

            /// <summary>
            /// 下拉式選單dropDownList
            /// </summary>
            dropDownList = 8,

            /// <summary>
            /// 明細欄位dataGrid
            /// </summary>
            dataGrid = 9,

            /// <summary>
            /// 超連結hyperLink
            /// </summary>
            hyperLink = 10,

            /// <summary>
            /// 自動編號autoNumber
            /// </summary>
            autoNumber = 11,

            /// <summary>
            /// 表單計算欄位calculateText
            /// </summary>
            calculateText = 12,

            /// <summary>
            /// 申請者(特定值欄位)userProposer
            /// </summary>
            userProposer = 13,

            /// <summary>
            /// 申請者部門(特定值欄位)userDept
            /// </summary>
            userDept = 14,

            /// <summary>
            /// 申請者職級(特定值欄位)userRank
            /// </summary>
            userRank = 15,

            /// <summary>
            /// 所有部門(特定值欄位)allDept
            /// </summary>
            allDept = 16,

            /// <summary>
            /// 所有職級(特定值欄位)allRank
            /// </summary>
            allRank = 17,

            /// <summary>
            /// 所有職務(特定值欄位)allFunction
            /// </summary>
            allFunction = 18,

            /// <summary>
            /// 所有人員(特定值欄位)allUser
            /// </summary>
            allUser = 19,

            /// <summary>
            /// 加總平均欄位
            /// </summary>
            aggregateText = 20,

            /// <summary>
            /// 隱藏欄位
            /// </summary>
            hiddenField = 21,

            /// <summary>
            /// 外掛欄位
            /// </summary>
            optionalField = 22,


            /// <summary>
            /// 人員組織欄位
            /// </summary>
            /// <remarks>
            ///2011/12/29 add by cloudmikado
            /// </remarks>
            userSetField = 23,

            /// <summary>
            /// 申請者代理人
            /// </summary>
            userAgent = 24,
            /// <summary>
            /// 文字編輯欄位
            /// </summary>
            htmlEditor = 25,

            /// <summary>
            /// 純顯示欄位
            /// </summary>
            displayField = 26,

            /// <summary>
            /// 所有會員欄位
            /// </summary>
            allMember = 27,
            /// <summary>
            /// 所有群組
            /// </summary>
            allMemberGroup = 28,
            /// <summary>
            /// 手寫簽名
            /// </summary>
            canvas = 29,

            /// <summary>
            /// 申請者職務
            /// </summary>
            userFunction = 30,

            /// <summary>
            /// 申請者資訊
            /// (非實際存在的欄位，條件比對用)
            /// </summary>
            applyInformation = 31,

            none = 999
        }
        #endregion

        #region==================欄位物件==========================
        /// <summary>
        /// 表單編號欄位
        /// </summary>
        public class AutoNumnerField : Field
        {
            public AutoNumnerField()
            {
                base.FieldType = FieldType.autoNumber;
            }
            public string IsNeedAutoNbr { get; set; } = "false";
        }


        #region==================明細欄位物件==========================
        /// <summary>
        /// 明細欄位物件
        /// </summary>
        public class DataGridField : Field
        {
            public List<CellCollections> Rows { get; set; } = new List<CellCollections>();
            public DataGridField()
            {
                base.FieldType = FieldType.dataGrid;
            }

        }

        public class CellCollections
        {
        }

        #endregion

        /// <summary>
        /// 明細欄位物件
        /// </summary>
        public class OptionalField : Field
        {
            public string ConditionValue { get; set; } = "";
        }

        /// <summary>
        /// 附件欄位物件
        /// </summary>
        public class FileField : Field
        {
            public bool IsNeedTransfer { get; set; } = true;
            public bool IsDeleteTemp { get; set; } = true;

            public List<string> FileAttach { get; set; } = new List<string>();
        }

        /// <summary>
        /// 通用欄位物件
        /// </summary>
        public class Field
        {

            public string FieldId { get; set; } = "";
            public string FieldName { get; set; } = "";
            public string FieldValue { get; set; } = "";
            public FieldType FieldType { get; set; } = FieldType.singleLineText;
            public string RealValue { get; set; } = "";
            public string FillerName { get; set; } = "";
            public string FillerUserGuid { get; set; } = "";
            public string FillerAccount { get; set; } = "";
        }


        #endregion

    #region==================欄位清單==========================

        public class _DETAILCellCollections : CellCollections
        {
            
            public Field _SZLITMField { get; set; } = new Field() { FieldType = FieldType.singleLineText, FieldId="SZLITM" };

            public Field _SZDSC1Field { get; set; } = new Field() { FieldType = FieldType.singleLineText, FieldId="SZDSC1" };

            public Field _SZDSC2Field { get; set; } = new Field() { FieldType = FieldType.singleLineText, FieldId="SZDSC2" };

            public Field _SZLNTYField { get; set; } = new Field() { FieldType = FieldType.singleLineText, FieldId="SZLNTY" };

            public Field _SZUORGField { get; set; } = new Field() { FieldType = FieldType.numberText, FieldId="SZUORG" };

            public Field _SZPRRCField { get; set; } = new Field() { FieldType = FieldType.numberText, FieldId="SZPRRC" };

            public Field _SZAEXPField { get; set; } = new Field() { FieldType = FieldType.numberText, FieldId="SZAEXP" };

            public Field _SZUOM3Field { get; set; } = new Field() { FieldType = FieldType.singleLineText, FieldId="SZUOM3" };

            public Field _SZFRRCField { get; set; } = new Field() { FieldType = FieldType.numberText, FieldId="SZFRRC" };

            public Field _SZFEAField { get; set; } = new Field() { FieldType = FieldType.numberText, FieldId="SZFEA" };

            public Field _remarkField { get; set; } = new Field() { FieldType = FieldType.multiLineText, FieldId="remark" };
                    
        }



    public class FieldList
    {

        public AutoNumnerField _formNoField { get; set; } = new AutoNumnerField() { FieldId = "formNo" };

        public Field _080Field { get; set; } = new Field() { FieldType = FieldType.singleLineText, FieldId = "080" };

        public Field _SYDOCOField { get; set; } = new Field() { FieldType = FieldType.singleLineText, FieldId = "SYDOCO" };

        public Field _SYDCTOField { get; set; } = new Field() { FieldType = FieldType.singleLineText, FieldId="SYDCTO" };

        public Field _SYKCOOField { get; set; } = new Field() { FieldType = FieldType.singleLineText, FieldId="SYKCOO" };

        public Field _SYMCUField { get; set; } = new Field() { FieldType = FieldType.singleLineText, FieldId="SYMCU" };

        public Field _SYOORNField { get; set; } = new Field() { FieldType = FieldType.singleLineText, FieldId="SYOORN" };

        public Field _SYOCTOField { get; set; } = new Field() { FieldType = FieldType.singleLineText, FieldId="SYOCTO" };

        public Field _SYOKCOField { get; set; } = new Field() { FieldType = FieldType.singleLineText, FieldId="SYOKCO" };

        public Field _SYAN8Field { get; set; } = new Field() { FieldType = FieldType.singleLineText, FieldId="SYAN8" };

        public Field _SYDRQJField { get; set; } = new Field() { FieldType = FieldType.singleLineText, FieldId="SYDRQJ" };

        public Field _SYSHANField { get; set; } = new Field() { FieldType = FieldType.singleLineText, FieldId="SYSHAN" };

        public Field _SYTRDJField { get; set; } = new Field() { FieldType = FieldType.singleLineText, FieldId="SYTRDJ" };

        public Field _SYANBYField { get; set; } = new Field() { FieldType = FieldType.singleLineText, FieldId="SYANBY" };

        public Field _SYPTCField { get; set; } = new Field() { FieldType = FieldType.singleLineText, FieldId="SYPTC" };

        public Field _SYTXA1Field { get; set; } = new Field() { FieldType = FieldType.singleLineText, FieldId="SYTXA1" };

        public Field _SYCRCDField { get; set; } = new Field() { FieldType = FieldType.singleLineText, FieldId="SYCRCD" };

        public Field _POHC01Field { get; set; } = new Field() { FieldType = FieldType.singleLineText, FieldId="POHC01" };

        public Field _SYDESCField { get; set; } = new Field() { FieldType = FieldType.multiLineText, FieldId="SYDESC" };

        public DataGridField _DETAILField { get; set; } = new DataGridField() { FieldId = "DETAIL" };



        public Field _totalField { get; set; } = new Field() { FieldType = FieldType.aggregateText, FieldId="total" };

        public Field _foreignTotalField { get; set; } = new Field() { FieldType = FieldType.aggregateText, FieldId = "foreignTotal" };

    }

    public class MessageContent
    {
        
        public string _formNo { get; set; } = "";

        public string _SYDOCO { get; set; } = "";

        public string _SYDCTO { get; set; } = "";

        public string _SYKCOO { get; set; } = "";

        public string _SYMCU { get; set; } = "";

        public string _SYOORN { get; set; } = "";

        public string _SYOCTO { get; set; } = "";

        public string _SYOKCO { get; set; } = "";

        public string _SYAN8 { get; set; } = "";

        public string _SYDRQJ { get; set; } = "";

        public string _SYSHAN { get; set; } = "";

        public string _SYTRDJ { get; set; } = "";

        public string _SYANBY { get; set; } = "";

        public string _SYPTC { get; set; } = "";

        public string _SYTXA1 { get; set; } = "";

        public string _SYCRCD { get; set; } = "";

        public string _POHC01 { get; set; } = "";

        public string _SYDESC { get; set; } = "";

        public string _DETAIL { get; set; } = "";

        public string _total { get; set; } = "";

    }

    public class DisplayTitle
    {
        
        public string _formNo { get; set; } = "";

        public string _SYDOCO { get; set; } = "";

        public string _SYDCTO { get; set; } = "";

        public string _SYKCOO { get; set; } = "";

        public string _SYMCU { get; set; } = "";

        public string _SYOORN { get; set; } = "";

        public string _SYOCTO { get; set; } = "";

        public string _SYOKCO { get; set; } = "";

        public string _SYAN8 { get; set; } = "";

        public string _SYDRQJ { get; set; } = "";

        public string _SYSHAN { get; set; } = "";

        public string _SYTRDJ { get; set; } = "";

        public string _SYANBY { get; set; } = "";

        public string _SYPTC { get; set; } = "";

        public string _SYTXA1 { get; set; } = "";

        public string _SYCRCD { get; set; } = "";

        public string _POHC01 { get; set; } = "";

        public string _SYDESC { get; set; } = "";

        public string _DETAIL { get; set; } = "";

        public string _total { get; set; } = "";

    }
    #endregion
}