using Ede.Uof.Utility.Task;
using Ede.Uof.Utility.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ede.Uof.Utility.Configuration;
using System.Text.RegularExpressions;
using Oduna.Lib.Service;
using Oduna.Lib.PurchaseSync;
using System.Xml.Linq;
using System.Data;
using Ede.Uof.EIP.Organization.Util;

namespace Oduna.Lib.Sync
{
    public class SyncTask : BaseTask
    {
        public override void Run(params string[] args)
        {
            DataService service = new DataService();
            UserUCO usrUco = new UserUCO();
            try
            {
                Logger.Write("Sync", "排程開始");

                var formHeadR = service.GetRequisitionsHeadData();
                Logger.Write("Sync", "有" + formHeadR.Rows.Count.ToString() + "筆請購單要從ERP起單");
                int form = 0;
                foreach (DataRow head in formHeadR.Rows)
                {
                    string PDDOCO = head["SYDOCO"].ToString();
                    string PDDCTO = head["SYDCTO"].ToString();
                    string PDKCOO = head["SYKCOO"].ToString();
                    string USERACCOUNT = head["PHTKBY"].ToString();
                    string applicantAc = head["SYANBY"].ToString();
                    string USERGUID = "";
                    if (!string.IsNullOrEmpty(USERACCOUNT.Trim()))
                    {
                        USERACCOUNT = USERACCOUNT.TrimStart();
                        USERACCOUNT = USERACCOUNT.Substring(2);
                        USERGUID = usrUco.GetGUID(USERACCOUNT);
                    }
                    Logger.Write("Sync", "請購單" + PDDCTO + " " + PDDOCO + " " + PDKCOO + " " + "起單人是 " + USERACCOUNT);

                    if (string.IsNullOrEmpty(USERACCOUNT.Trim()) || string.IsNullOrEmpty(USERGUID))
                    {
                        Logger.Write("Sync", "UOF找不到請購單起單人 " + USERACCOUNT);
                        continue;
                    }

                    string USERNAME = "";
                    if (!string.IsNullOrEmpty(USERGUID))
                        USERNAME = usrUco.GetEBUser(USERGUID).Name;

                    string applicantId = "";
                    string applicantNa = "";
                    if (!string.IsNullOrEmpty(applicantAc))
                    {
                        applicantId = service.GetUserId(applicantAc);
                        applicantNa = service.GetApplicantNa(applicantId);
                    }

                    var grid = service.GetRequisitionsGridData(PDDOCO, PDDCTO, PDKCOO);
                    string USING_VERSION_ID = service.GetUSINGVERSIONID("請購單");
                    SyncFormXml formXml = new SyncFormXml(USING_VERSION_ID, (Sync.UrgentLevel)UrgentLevel.Normal, USERACCOUNT, USERGUID, USERNAME, head, grid);
                    //測試區SyncFormXml formXml = new SyncFormXml("9b6a279c-51ad-43ff-8d34-6de463f87af2", (Sync.UrgentLevel)UrgentLevel.Normal, USERACCOUNT, USERGUID, USERNAME, head, grid);
                    //SyncFormXml formXml = new SyncFormXml("971e8a77-26b7-4006-9189-7a47b202e416", UrgentLevel.Normal, "user01", "6ab54fc8-8e3c-4c39-a225-d149ec60712a", "基X同", head, grid);
                    //SyncFormXml formXml = new SyncFormXml("f3a2b832-864e-4285-a773-905dca86613c", UrgentLevel.Normal, "Tony", "c496e32b-0968-4de5-95fc-acf7e5a561c0", "Tony", head, grid);
                    string xml = formXml.ConvertToFormInfoXml(applicantId, applicantAc, applicantNa);

                    service.SaveTaskData(xml);
                    form++;
                }
                Logger.Write("Sync", "總共起請購單" + form.ToString() + "筆");

                var formHeadP = service.GetPurchaseHeadData();
                Logger.Write("Sync", "有" + formHeadP.Rows.Count.ToString() + "筆採購單要從ERP起單");
                form = 0;
                foreach (DataRow head in formHeadP.Rows)
                {
                    string PDDOCO = head["SYDOCO"].ToString();
                    string PDDCTO = head["SYDCTO"].ToString();
                    string PDKCOO = head["SYKCOO"].ToString();
                    string USERACCOUNT = head["PHTKBY"].ToString();
                    string USERGUID = "";
                    if (!string.IsNullOrEmpty(USERACCOUNT.Trim()))
                    {
                        USERACCOUNT = USERACCOUNT.TrimStart();
                        USERACCOUNT = USERACCOUNT.Substring(2);
                        USERGUID = usrUco.GetGUID(USERACCOUNT);
                    }
                    Logger.Write("Sync", "採購單" + PDDCTO + " " + PDDOCO + " " + PDKCOO + " " + "起單人是 " + USERACCOUNT);

                    if (string.IsNullOrEmpty(USERACCOUNT.Trim()) || string.IsNullOrEmpty(USERGUID))
                    {
                        Logger.Write("Sync", "UOF找不到採購單起單人 " + USERACCOUNT);
                        continue;
                    }

                    string USERNAME = "";
                    if (!string.IsNullOrEmpty(USERGUID))
                        USERNAME = usrUco.GetEBUser(USERGUID).Name;

                    Logger.Write("Sync", "原請購單是 " + head["SYOKCO"].ToString() + "-" + head["SYOORN"].ToString() + "-" + head["SYOCTO"].ToString());
                    string SYURRF = "";
                    if (!string.IsNullOrEmpty(head["SYOKCO"].ToString().Trim()) && !string.IsNullOrEmpty(head["SYOORN"].ToString().Trim()) && !string.IsNullOrEmpty(head["SYOCTO"].ToString().Trim()))
                        SYURRF = service.GetFormNo(head["SYOKCO"].ToString(), head["SYOORN"].ToString(), head["SYOCTO"].ToString());
                    string appendix = "";
                    string applicantId = "";
                    string applicantAc = "";
                    string applicantNa = "";
                    if (!string.IsNullOrEmpty(SYURRF))
                    {
                        appendix = service.GetAppendixId(SYURRF);
                        applicantId = service.GetApplicantId(SYURRF);
                        applicantAc = service.GetApplicantAc(applicantId);
                        applicantNa = service.GetApplicantNa(applicantId);
                    }
                    
                    var grid = service.GetPurchaseGridData(PDDOCO, PDDCTO, PDKCOO);
                    string USING_VERSION_ID = service.GetUSINGVERSIONID("採購單");
                    SyncPurchaseFormXml formXml = new SyncPurchaseFormXml(USING_VERSION_ID, (PurchaseSync.UrgentLevel)UrgentLevel.Normal, USERACCOUNT, USERGUID, USERNAME, head, grid);
                    //測試區SyncPurchaseFormXml formXml = new SyncPurchaseFormXml("bbae55f4-03ca-4322-9bf1-c0c688985f41", (PurchaseSync.UrgentLevel)UrgentLevel.Normal, USERACCOUNT, USERGUID, USERNAME, head, grid);
                    //SyncFormXml formXml = new SyncFormXml("971e8a77-26b7-4006-9189-7a47b202e416", UrgentLevel.Normal, "user01", "6ab54fc8-8e3c-4c39-a225-d149ec60712a", "基X同", head, grid);
                    //SyncFormXml formXml = new SyncFormXml("f3a2b832-864e-4285-a773-905dca86613c", UrgentLevel.Normal, "Tony", "c496e32b-0968-4de5-95fc-acf7e5a561c0", "Tony", head, grid);
                    string xml = formXml.ConvertToFormInfoXml(appendix, applicantId, applicantAc, applicantNa);

                    service.SaveTaskData(xml);
                    form++;
                }
                Logger.Write("Sync", "總共起採購單" + form.ToString() + "筆");
                ClearAllCache();
            }
            catch (Exception ex)
            {
                Logger.Write("Sync", ex.Message);
            }
        }

        public void ClearAllCache()
        {
            Setting setting = new Setting();
            Group.Group group = new Group.Group();
            group.Url = setting["SiteUrl"] + "/WebService/Group.asmx";
            group.ClearAllCache();
        }
    }
}
