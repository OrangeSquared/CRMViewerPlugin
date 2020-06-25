using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace CRMViewerPlugin
{
    class TopMenu : Result
    {
        public override string Header => "Main Menu";

        public override string Status => throw new NotImplementedException();

        public TopMenu(IOrganizationService Service)
        {
            service = Service;
            Data = new System.Data.DataTable("Top Menu");
            Data.Columns.AddRange(new DataColumn[]
                {
                    new DataColumn("Key"),
                    new DataColumn("Action")
                });
            DataRow dr = Data.NewRow();
            dr["Key"] = "Browse";
            dr["Action"] = "Browse Schema";
            Data.Rows.Add(dr);
             dr = Data.NewRow();
            dr["Key"] = "nothing";
            dr["Action"] = "nothing";
            Data.Rows.Add(dr);
        }

        public override Result ProcessSelection(object selection, System.ComponentModel.BackgroundWorker worker)
        {
            Result retVal = null;
            switch (selection as string)
            {
                case "Browse":
                    Browser browser = new Browser(service);
                    browser.LoadEntitiesList(worker);
                    retVal = browser;
                    break;

                default:
                    break;
            }
            return retVal;
        }

        public override void Refresh(BackgroundWorker worker)
        {
         
        }
    }
}
