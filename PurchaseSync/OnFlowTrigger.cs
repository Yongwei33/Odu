using Ede.Uof.Utility.Log;
using Ede.Uof.WKF.Engine;
using Ede.Uof.WKF.ExternalUtility;
using Oduna.Lib.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Oduna.Lib.PurchaseSync
{
    public class OnFlowTrigger : ICallbackTriggerPlugin
    {
        public string GetFormResult(ApplyTask applyTask)
        {
            if (applyTask.SignResult == SignResult.Approve && applyTask.SiteCode == "B280")
            {
                DataService service = new DataService();
                var formDoc = applyTask.Task.CurrentDocument;
                string SYKCOO = formDoc.Fields["SYKCOO"].FieldValue;
                string SYDOCO = formDoc.Fields["SYDOCO"].FieldValue;
                string SYDCTO = formDoc.Fields["SYDCTO"].FieldValue;
                service.UpdatePurchaseB1ToB4(SYKCOO, SYDOCO, SYDCTO);
            }
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
