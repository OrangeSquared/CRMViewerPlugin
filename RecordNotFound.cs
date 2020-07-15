using NuGet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CRMViewerPlugin
{
    class RecordNotFound : Result
    {
        public override string Header => "Not found";

        public override string Status => "done.";

        public override Result ProcessSelection(object sender, object selection, BackgroundWorker worker)
        {
            return null;
        }

        public override void Refresh(BackgroundWorker worker)
        {
            
        }

        public override MenuItem[] GetContextMenu(object selection)
        {
            return null;
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
