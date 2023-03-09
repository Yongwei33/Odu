using Ede.Uof.Utility.Log;
using Ede.Uof.WKF.ExternalUtility;
using Oduna.Lib.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Oduna.Lib.Sync
{
    public class EndFormTrigger : ICallbackTriggerPlugin
    {
        public string GetFormResult(ApplyTask applyTask)
        {
            DataService service = new DataService();
            var formDoc = applyTask.Task.CurrentDocument;
            //Logger.Write("SW_Requisitions", applyTask.Task.CurrentDocXml);
            string SYKCOO = formDoc.Fields["SYKCOO"].FieldValue;
            string SYDOCO = formDoc.Fields["SYDOCO"].FieldValue;
            string SYDCTO = formDoc.Fields["SYDCTO"].FieldValue;

            if (applyTask.FormResult == Ede.Uof.WKF.Engine.ApplyResult.Adopt)
            {
                service.UpdateFlag2(SYKCOO, SYDOCO, SYDCTO);
                service.UpdateAdoptQ1ToQ3(SYKCOO, SYDOCO, SYDCTO);
            }
            else if (applyTask.FormResult == Ede.Uof.WKF.Engine.ApplyResult.Reject || applyTask.FormResult == Ede.Uof.WKF.Engine.ApplyResult.Cancel)
            {
                service.UpdateFlag3(SYKCOO, SYDOCO, SYDCTO);
                if (SYDCTO == "Q1" || SYDCTO == "Q2" || SYDCTO == "Q3")
                    service.UpdateRejectQ1ToQ3(SYKCOO, SYDOCO, SYDCTO);
                else if (SYDCTO == "Q4")
                    service.UpdateRejectQ4(SYKCOO, SYDOCO, SYDCTO);
            }
            
            Logger.Write("SW_InfoLog", string.Format("{0} 申請結案: {1}", applyTask.Task.FormName, applyTask.FormNumber));

            return "";
        }

        public void OnError(Exception errorException)
        {
            Logger.Write("SW_ErrorLog", errorException.ToString());
        }
        public void Finally()
        {
           
        }
        /*<Form formVersionId="d6ec7f6c-237b-40c5-a818-38c5c5d9f751">
    <Applicant userGuid="admin" account="admin" name="系統管理者" />
    <FormFieldValue>
        <FieldItem fieldId="letterhead" fieldValue="&lt;p style=&quot;text-align: center;&quot;&gt;[img alt=&quot;&quot; src=&quot;/common/filecenter/v3/handler/FileControlHandler.ashx?id=c6814d47-3c66-4818-b43c-aa9313f492e6&amp;amp;path=WKF%5C2022%5C06&amp;amp;contentType=image%2Fjpeg&amp;amp;name=LOGOX1.jpg&amp;amp;e=NLXhNp9IKX5sjykncJgbRw%3d%3d&amp;amp;l=GLLCVf4SmYKMD8XEyY6GOU10snAznjZ4glzIHKsyUmk%3d&quot; class=&quot;UOF&quot; style=&quot;width: 54px; height: 46px;&quot; /]&lt;span style=&quot;font-family: 微軟正黑體; font-size: 32px;&quot;&gt;&lt;strong&gt;&lt;span style=&quot;box-sizing: border-box; margin: 0px; padding: 0px; border: 0px; vertical-align: baseline; background: #ffffff; font-family: 標楷體; font-size: 32px; color: #333333;&quot;&gt;&lt;strong style=&quot;box-sizing: border-box; margin: 0px; padding: 0px; border: 0px; vertical-align: baseline; background: transparent; font-family: inherit;&quot;&gt;霖 宏 科 技 股 份 有 限 公&amp;nbsp;&lt;/strong&gt;&lt;/span&gt;&lt;strong style=&quot;box-sizing: border-box; margin: 0px; padding: 0px; border: 0px; font-size: 32px; vertical-align: baseline; background: #ffffff; font-family: 標楷體; color: #333333;&quot;&gt;司&lt;/strong&gt;&lt;/strong&gt;&lt;/span&gt;&lt;/p&gt;&#xD;&#xA;&lt;p style=&quot;text-align: center;&quot;&gt;&lt;span style=&quot;background-color: #ffffff; font-family: 'Times New Roman'; font-size: 24px; color: #333333;&quot;&gt;&amp;nbsp;&lt;/span&gt;&lt;/p&gt;&#xD;&#xA;&lt;p style=&quot;text-align: center;&quot;&gt;&lt;span style=&quot;background-color: #ffffff; font-family: 'Times New Roman'; font-size: 24px; color: #333333;&quot;&gt;Lin Horn Technology Co., Ltd&lt;/span&gt;&lt;/p&gt;&#xD;&#xA;&lt;p style=&quot;text-align: center;&quot;&gt;&lt;span style=&quot;background-color: #ffffff; font-family: 'Times New Roman'; font-size: 24px; color: #333333;&quot;&gt;&amp;nbsp;&lt;/span&gt;&lt;/p&gt;&#xD;&#xA;" realValue="" enableSearch="True" fillerName="Tony" fillerUserGuid="c496e32b-0968-4de5-95fc-acf7e5a561c0" fillerAccount="Tony" fillSiteId="" />
        <FieldItem fieldId="purchaseNum" fieldValue="BPM220600003" realValue="" enableSearch="True" />
        <FieldItem fieldId="buy_unit" fieldValue="總務課" realValue="" customValue="@null" enableSearch="True" fillerName="Tony" fillerUserGuid="c496e32b-0968-4de5-95fc-acf7e5a561c0" fillerAccount="Tony" fillSiteId="" />
        <FieldItem fieldId="Purchase_department" fieldValue="" realValue="&lt;UserSet&gt;&lt;/UserSet&gt;&#xD;&#xA;" enableSearch="True" />
        <FieldItem fieldId="filling_date" fieldValue="2022/06/22" realValue="" enableSearch="True" fillerName="Tony" fillerUserGuid="c496e32b-0968-4de5-95fc-acf7e5a561c0" fillerAccount="Tony" fillSiteId="" />
        <FieldItem fieldId="needDay" fieldValue="2022/06/22" realValue="" enableSearch="True" fillerName="Tony" fillerUserGuid="c496e32b-0968-4de5-95fc-acf7e5a561c0" fillerAccount="Tony" fillSiteId="" />
        <FieldItem fieldId="purchaseDetail" ConditionValue="" realValue="" enableSearch="True">
            <purchaseDetail>
                <PRODUCT_GRIDITEM xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Part_Seq="0" Part_No="2" part_name="b" part_type="b" price="2.000" unit="kg" require_qty="10" />
            </purchaseDetail>
        </FieldItem>
        <FieldItem fieldId="remark" fieldValue="" realValue="" enableSearch="True" />
        <FieldItem fieldId="appendix" fieldValue="" realValue="" enableSearch="True" />
        <FieldItem fieldId="shelf_life" fieldValue="AD-2Q-002-09-01&#xD;&#xA;" realValue="" enableSearch="True" fillerName="Tony" fillerUserGuid="c496e32b-0968-4de5-95fc-acf7e5a561c0" fillerAccount="Tony" fillSiteId="" />
        <FieldItem fieldId="archive" fieldValue="一式二聯：①請購(白)  ②採購(綠)&#xD;&#xA;" realValue="" enableSearch="True" fillerName="Tony" fillerUserGuid="c496e32b-0968-4de5-95fc-acf7e5a561c0" fillerAccount="Tony" fillSiteId="" />
    </FormFieldValue>
</Form>*/
    }
}
