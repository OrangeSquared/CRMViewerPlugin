using NuGet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMViewerPlugin
{
    class RecordNotFound : Result
    {
        public override string Header => "Not found";

        public override string Status => "done.";

        public override Result ProcessSelection(object selection, BackgroundWorker worker)
        {
            return null;
        }

        public override void Refresh(BackgroundWorker worker)
        {
            
        }

        public RecordNotFound()
        {
            Data = new DataTable();
            Data.Columns.Add("Key");
            Data.Columns.Add("Record");
            Data.Columns.Add("not");
            Data.Columns.Add("found");
        }
    }
}
