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
    public class StartFormTrigger : ICallbackTriggerPlugin
    {
        public string GetFormResult(ApplyTask applyTask)
        {
            DataService service = new DataService();
            var formDoc = applyTask.Task.CurrentDocument;
            //Logger.Write("SW_Requisitions", applyTask.Task.CurrentDocXml);
            string SYKCOO = formDoc.Fields["SYKCOO"].FieldValue;
            string SYDOCO = formDoc.Fields["SYDOCO"].FieldValue;
            string SYDCTO = formDoc.Fields["SYDCTO"].FieldValue;
            service.UpdateFlag1(SYKCOO, SYDOCO, SYDCTO);
            service.InsertFormNo(SYKCOO, SYDOCO, SYDCTO, applyTask.FormNumber);

            Logger.Write("SW_InfoLog", string.Format("{0} 申請起單: {1}", applyTask.Task.FormName, applyTask.FormNumber));

            return "";
        }

        public void OnError(Exception errorException)
        {
            Logger.Write("SW_ErrorLog", errorException.ToString());
        }
        public void Finally()
        {
           
        }
    }
}
