using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XrmToolBox.Extensibility;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using McTools.Xrm.Connection;
using System.Dynamic;
using XrmToolBox.Extensibility.Interfaces;
using XrmToolBox.Extensibility.Args;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography.X509Certificates;
using System.Activities;

namespace CRMViewerPlugin
{
    //comment
    public partial class ctlCRMViewer : PluginControlBase, IStatusBarMessenger
    {
        private Settings mySettings;
        private Stack<Result> results;
        private DataTable activeList;
        private Menu popupMenu;
        private bool insetup = true;

        public event EventHandler<StatusBarMessageEventArgs> SendMessageToStatusBar;

        #region cache
        private Dictionary<string, string> _cache;
        internal Dictionary<string, string> cache
        {
            get
            {
                if (_cache == null)
                {
                    _cache = new Dictionary<string, string>();
                    string[] input;
                    if (SettingsManager.Instance.TryLoad(GetType(), out input, ((Microsoft.Xrm.Tooling.Connector.CrmServiceClient)Service).ConnectedOrgUniqueName + "_cache"))
                        for (int i = 0; i < input.GetUpperBound(0); i += 2)
                            _cache.Add(input[i], input[i + 1]);
                }
                return _cache;
            }
        }
        private void SaveCache()
        {
            List<string> output = new List<string>();
            foreach (string s in cache.Keys)
            {
                output.Add(s);
                output.Add(cache[s]);
            }
            SettingsManager.Instance.Save(GetType(), output.ToArray(), ((Microsoft.Xrm.Tooling.Connector.CrmServiceClient)Service).ConnectedOrgUniqueName + "_cache");
        }
        #endregion


        #region plugin stuff
        private void MyPluginControl_Load(object sender, EventArgs e)
        {
            insetup = true;
            //ShowInfoNotification("This is a notification that can lead to XrmToolBox repository", new Uri("https://github.com/MscrmTools/XrmToolBox"));

            // Loads or creates the settings for the plugin
            if (!SettingsManager.Instance.TryLoad(GetType(), out mySettings))
            {
                mySettings = new Settings();

                LogWarning("Settings not found => a new settings file has been created!");

                SettingsManager.Instance.Save(GetType(), mySettings);
            }
            else
            {
                LogInfo("Settings found and loaded");
            }

            results = new Stack<Result>();
            //ExecuteMethod(LoadEntityList);
        }

        private void tsbClose_Click(object sender, EventArgs e) { CloseTool(); }

        private void MyPluginControl_OnCloseTool(object sender, EventArgs e) { SettingsManager.Instance.Save(GetType(), mySettings); }

        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName, object parameter)
        {
            base.UpdateConnection(newService, detail, actionName, parameter);

            if (mySettings != null && detail != null)
            {
                mySettings.LastUsedOrganizationWebappUrl = detail.WebApplicationUrl;
                LogInfo("Connection has changed to: {0}", detail.WebApplicationUrl);
                results = new Stack<Result>();
                ExecuteMethod(LoadEntityList);
            }
        }
        private void ctlCRMViewer_ConnectionUpdated(object sender, ConnectionUpdatedEventArgs e)
        {
            if (!SettingsManager.Instance.TryLoad(GetType(), out mySettings))
            {
                mySettings = new Settings();

                LogWarning("Settings not found => a new settings file has been created!");

                SettingsManager.Instance.Save(GetType(), mySettings);
            }
            else
            {
                LogInfo("Settings found and loaded");
            }

            results = new Stack<Result>();
            //ExecuteMethod(LoadEntityList);
        }


        #endregion

        #region Events
        private void ProgressChanged(ProgressChangedEventArgs progressChangedEventArgs)
        {
            SetWorkingMessage(string.Format("{0}% complete", progressChangedEventArgs.ProgressPercentage));
            int tic = 1;
            if (progressChangedEventArgs.ProgressPercentage > 0)
                tic += progressChangedEventArgs.ProgressPercentage % 3;
            SendMessageToStatusBar?.Invoke(this, new StatusBarMessageEventArgs(progressChangedEventArgs.ProgressPercentage, "Loading" + new string('.', tic)));
        }

        private void NewResultsAvailable(RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            if (runWorkerCompletedEventArgs.Result != null && runWorkerCompletedEventArgs.Result.GetType().Name == "String")
                Clipboard.SetText(runWorkerCompletedEventArgs.Result.ToString());
            else
                PaintResults();
        }

        private void tsbSearch_TextChanged(object sender, EventArgs e)
        {
            DataTable dt = results.Peek().Data.Tables[0];
            List<string> newFilter = new List<string>();
            for (int i = 1; i < dt.Columns.Count; i++)
                newFilter.Add(string.Format("[{0}] LIKE '%{1}%'", dt.Columns[i].ColumnName, tsbSearch.Text));

            dt.DefaultView.RowFilter = string.Join(" OR ", newFilter);
            results.Peek().SearchText = tsbSearch.Text;
        }

        private void dgvMain_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            NavigateIn(e.RowIndex);
        }
        private void dgvRelationships_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            WorkAsyncInfo wai = new WorkAsyncInfo
            {
                Message = "Retrieving info from CRM...",
                Work = (worker, args) =>
                {
                    Result result = null;
                    result = Browser.GetEntityResult(Service, cache, dgvRelationships.Rows[e.RowIndex].Cells[3].Value.ToString(), worker);
                    if (result != null)
                    {
                        results.Push(result);
                        if (!result.FromCache)
                            SaveCache();
                    }

                },
                ProgressChanged = ProgressChanged,
                PostWorkCallBack = NewResultsAvailable,
                AsyncArgument = null,
                MessageHeight = 150,
                MessageWidth = 340
            };
            WorkAsync(wai);
        }

        private void tsbBack_Click(object sender, EventArgs e) { PageBack(); }

        private void dgvMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                PageBack();

            else if (e.KeyCode == Keys.Enter)
                NavigateIn(dgvMain.SelectedRows[0].Index);

            else if (e.KeyCode == Keys.F2)
                tsbSearch.Focus();

            else if (e.KeyCode == Keys.F5)
                Refresh();

            else if ("ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890_-".Contains(e.KeyCode.ToString()))
            {
                tsbSearch.Text += e.KeyCode.ToString();
            }

            e.SuppressKeyPress = true;
        }

        private void tsbRefresh_Click(object sender, EventArgs e) { Refresh(); }


        private void dgvMain_MouseDown(object sender, MouseEventArgs e)
        {
            if (dgvMain.HitTest(e.X, e.Y).RowIndex >= 0)
                dgvMain.Rows[dgvMain.HitTest(e.X, e.Y).RowIndex].Selected = true;
        }

        private void dgvMain_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                MenuItem miCopyKey = new MenuItem("Copy Key");
                miCopyKey.Click += (object ss, EventArgs ee) => Clipboard.SetText(dgvMain.SelectedRows[0].Cells[0].Value.ToString());
                MenuItem miOpenInBrowser = new MenuItem("Open in Browser");
                miOpenInBrowser.Click += MiOpenInBrowser_Click;
                MenuItem miOpenRecord = new MenuItem("Open Record");
                miOpenRecord.Click += MiOpenRecord_Click;
                MenuItem miCreateEnum = new MenuItem("Create Enum");
                miCreateEnum.Click += MiCreateEnum_Click;
                MenuItem miCreateClass = new MenuItem("Create Class");
                miCreateClass.Click += MiCreateClass_Click;
                MenuItem miDocumentEntity = new MenuItem("Document Entity");
                miDocumentEntity.Click += MiDocumentEntity_Click;
                MenuItem miAnalyzeEntity = new MenuItem("Analyze Entity");
                miAnalyzeEntity.Click += MiAnalyzeEntity_Click;

                ContextMenu contextMenu = new ContextMenu();

                switch (results.Peek().DataType)
                {
                    case Result.ResultType.EntityList:
                        contextMenu = new ContextMenu(new MenuItem[]
                            {
                                miCopyKey,
                                miOpenInBrowser,
                                miCreateClass,
                                miDocumentEntity,
                                miAnalyzeEntity,
                            });
                        break;
                    case Result.ResultType.Entity:
                        contextMenu = new ContextMenu(new MenuItem[]
                            {
                                miCopyKey,
                                miOpenInBrowser,
                            }); break;
                    case Result.ResultType.PickList:
                        contextMenu = new ContextMenu(new MenuItem[]
                            {
                                miCopyKey,
                                miCreateEnum,
                            });
                        break;
                    case Result.ResultType.Record:
                        contextMenu = new ContextMenu(new MenuItem[]
                            {
                                miCopyKey,
                                miOpenInBrowser,
                            });
                        break;
                    default:
                        break;
                }

                contextMenu.Show(dgvMain, e.Location);
            }

        }

        private void MiAnalyzeEntity_Click(object sender, EventArgs e)
        {
            string entityLogicalName = dgvMain.SelectedRows[0].Cells[0].Value.ToString();

            WorkAsyncInfo wai = new WorkAsyncInfo
            {
                Message = "Analyzing Entity " + entityLogicalName,
                Work = (worker, args) =>
                {
                    args.Result = EntityTools.AnalyzeEntity(Service, worker, entityLogicalName);
                },
                ProgressChanged = ProgressChanged,
                PostWorkCallBack = NewResultsAvailable,
                AsyncArgument = null,
                MessageHeight = 150,
                MessageWidth = 340,
            };
            WorkAsync(wai);
        }

        private void MiDocumentEntity_Click(object sender, EventArgs e)
        {
            WorkAsyncInfo wai = new WorkAsyncInfo
            {
                Message = "Documenting entity...",
                Work = (worker, args) =>
                {
                    StringBuilder sb = new StringBuilder();
                    string entityLogicalName = dgvMain.SelectedRows[0].Cells[0].Value.ToString();
                    string entityDisplayName = dgvMain.SelectedRows[0].Cells[2].Value.ToString();
                    List<Tuple<string, string, string>> attributes = Browser.GetEntityAttributes(Service, entityLogicalName);

                    sb.AppendLine(string.Format("Entity Logical Name\t{0}", entityLogicalName));
                    sb.AppendLine(string.Format("Entity Display Name\t{0}", entityDisplayName));
                    sb.AppendLine("");
                    sb.AppendLine("Attributes");
                    sb.AppendLine("Logical Name\tDisplay Name\tType");

                    attributes.Sort((x, y) => x.Item1.CompareTo(y.Item1));

                    foreach (Tuple<string, string, string> att in attributes)
                        sb.AppendLine(string.Format("{0}\t{1}\t{2}", att.Item1, att.Item3, att.Item2));

                    args.Result = sb.ToString();
                },
                ProgressChanged = ProgressChanged,
                PostWorkCallBack = NewResultsAvailable,
                AsyncArgument = null,
                MessageHeight = 150,
                MessageWidth = 340
            };
            WorkAsync(wai);
        }

        private void MiCreateClass_Click(object sender, EventArgs e)
        {
            WorkAsyncInfo wai = new WorkAsyncInfo
            {
                Message = "Building class...",
                Work = (worker, args) =>
                {
                    args.Result = EntityTools.CreateEntityClass(Service, worker, dgvMain.SelectedRows[0].Cells[0].Value.ToString());
                },
                ProgressChanged = ProgressChanged,
                PostWorkCallBack = NewResultsAvailable,
                AsyncArgument = null,
                MessageHeight = 150,
                MessageWidth = 340
            };
            WorkAsync(wai);
        }



        private void MiCreateEnum_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("    public enum Enum" + results.Peek().Key);
            sb.AppendLine("        {");
            for (int i = 0; i < dgvMain.Rows.Count; i++)
            {
                string name = dgvMain.Rows[i].Cells[2].Value.ToString();
                name = name.Replace(" ", "_");
                string number = dgvMain.Rows[i].Cells[1].Value.ToString();
                sb.AppendLine(string.Format("            {0} = {1},", name, number));
            }
            sb.AppendLine("        }");

            Clipboard.SetText(sb.ToString());
        }

        private void MiOpenRecord_Click(object sender, EventArgs e)
        {
            string possibleid = Clipboard.ContainsText() ? Clipboard.GetText() : string.Empty;
            Guid id = Guid.Empty;

            if (!Guid.TryParse(possibleid, out id))
            {
                string fetchXML = string.Format(@"<fetch top='1' >  <entity name='{0}' >    <attribute name='{0}id' />  </entity></fetch>", results.Peek().EntityLogicalName);
                EntityCollection ec = Service.RetrieveMultiple(new FetchExpression(fetchXML));
                if (ec.Entities.Count > 0)
                    id = ec.Entities[0].Id;
            }
            if (id != Guid.Empty)
            {
                WorkAsyncInfo wai = new WorkAsyncInfo
                {
                    Message = "Retrieving info from CRM...",
                    Work = (worker, args) =>
                    {
                        Result result = Browser.GetRecordResult(Service, cache, results.Peek().EntityLogicalName, id, worker);
                        result.Header = string.Format("{0}{{{1}}}", results.Peek().EntityLogicalName, id);
                        if (result != null)
                        {
                            results.Push(result);
                            if (!result.FromCache)
                                SaveCache();
                        }

                    },
                    ProgressChanged = ProgressChanged,
                    PostWorkCallBack = NewResultsAvailable,
                    AsyncArgument = null,
                    MessageHeight = 150,
                    MessageWidth = 340
                };
                WorkAsync(wai);
            }
        }

        private string OpenLatest(bool LatestUpdated)
        {
            string retVal = "";


            Guid id = Guid.Empty;
            string fetchXML = string.Format(@"<fetch top='1' >  <entity name='{0}' >      <order attribute='{1}' descending='true' />  </entity></fetch>",
            results.Peek().EntityLogicalName,
            LatestUpdated ? "createdon" : "modifiedon");
            EntityCollection ec = Service.RetrieveMultiple(new FetchExpression(fetchXML));
            if (ec.Entities.Count > 0)
                id = ec.Entities[0].Id;


            if (id != Guid.Empty)
                retVal = id.ToString();
            else
                retVal = String.Empty;

            return retVal;
        }

        private void OpenRecord()
        {
            Guid id = Guid.Empty;
            if (Guid.TryParse(tstbRecordId.Text, out id))
            {
                string entln = results.Peek().EntityLogicalName;
                OpenRecord(entln, id);
            }
        }

        private void OpenRecord(string EntityLogicalName, Guid id)
        {
            string possibleid = tstbRecordId.Text;

            if (Guid.TryParse(possibleid, out id))
            {
                WorkAsyncInfo wai = new WorkAsyncInfo
                {
                    Message = "Retrieving info from CRM...",
                    Work = (worker, args) =>
                    {
                        Result result = Browser.GetRecordResult(Service, cache, EntityLogicalName, id, worker);
                        result.Header = string.Format("{0}{{{1}}}", EntityLogicalName, id);
                        if (result != null)
                        {
                            results.Push(result);
                            if (!result.FromCache)
                                SaveCache();
                        }

                    },
                    ProgressChanged = ProgressChanged,
                    PostWorkCallBack = NewResultsAvailable,
                    AsyncArgument = null,
                    MessageHeight = 150,
                    MessageWidth = 340
                };
                WorkAsync(wai);
            }
        }

        private void MiOpenInBrowser_Click(object sender, EventArgs e)
        {
            string rootURI = string.Format("https://{0}:{1}",
                ((Microsoft.Xrm.Tooling.Connector.CrmServiceClient)Service).CrmConnectOrgUriActual.Host.Replace("api.", ""),
                ((Microsoft.Xrm.Tooling.Connector.CrmServiceClient)Service).CrmConnectOrgUriActual.Port);
            string targetUri = null;

            switch (results.Peek().DataType)
            {
                case Result.ResultType.EntityList:
                    targetUri = string.Format("{0}//main.aspx?etn={1}&pagetype=entitylist", rootURI, dgvMain.SelectedRows[0].Cells[0].Value.ToString());
                    break;
                case Result.ResultType.Entity:
                    targetUri = string.Format("{0}//main.aspx?etn={1}&pagetype=entitylist", rootURI, results.Peek().EntityLogicalName);
                    break;
                case Result.ResultType.PickList:
                    break;
                case Result.ResultType.Record:
                    targetUri = string.Format("{0}//main.aspx?etn={1}&pagetype=entityrecord&id={2}", rootURI, results.Peek().EntityLogicalName, results.Peek().EntityRecordId);
                    break;
                default:
                    break;
            }

            System.Diagnostics.Process.Start(targetUri);
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (mySettings != null && !insetup)
            {
                mySettings.SplitterPosition = splitContainer1.SplitterDistance;
                SettingsManager.Instance.Save(GetType(), mySettings);
            }
        }

        private void ctlCRMViewer_Paint(object sender, PaintEventArgs e)
        {
            if (insetup)
            {
                splitContainer1.SplitterDistance = mySettings.SplitterPosition;
                insetup = false;
            }
        }

        #endregion



        #region Functions
        private void LoadEntityList()
        {
            WorkAsyncInfo wai = new WorkAsyncInfo
            {
                Message = "Retrieving info from CRM...",
                Work = (worker, args) =>
                {
                    Result result = Browser.GetEntityListResult(Service, worker);

                    if (result != null)
                        results.Push(result);
                },
                ProgressChanged = ProgressChanged,
                PostWorkCallBack = NewResultsAvailable,
                AsyncArgument = null,
                MessageHeight = 150,
                MessageWidth = 340
            };
            WorkAsync(wai);
        }

        private void PaintResults()
        {
            activeList = results.Peek().Data.Tables[0];
            tslCached.Visible = results.Peek().FromCache;

            List<string> header = new List<string>();
            foreach (Result r in results)
                header.Add(r.Header);
            gbMain.Text = string.Join(" <= ", header);

            dgvMain.DataSource = activeList;

            dgvMain.Columns["Key"].Visible = false;
            dgvMain.RowHeadersWidth = dgvMain.ColumnHeadersHeight;
            string search = results.Peek().SearchText;
            tsbSearch.Text = "";
            tsbSearch.Text = search;

            if (results.Peek().Data.Tables.Count > 1)
            {
                DataTable det = results.Peek().Data.Tables[1];
                dgvRelationships.DataSource = det;
                dgvRelationships.Columns["key"].Visible = false;
                dgvRelationships.RowHeadersWidth = dgvRelationships.ColumnHeadersHeight;
            }
            else
            {
                dgvRelationships.DataSource = null;
            }

            tslRecordId.Visible = results.Peek().DataType == Result.ResultType.Entity;
            tstbRecordId.Visible = results.Peek().DataType == Result.ResultType.Entity;
            tsbLoadRecordId.Visible = results.Peek().DataType == Result.ResultType.Entity;
            tsbLoadLatest.Visible = results.Peek().DataType == Result.ResultType.Entity;
            tsbLoadNewest.Visible = results.Peek().DataType == Result.ResultType.Entity;

            SendMessageToStatusBar?.Invoke(this, new StatusBarMessageEventArgs(string.Format("{0} results loaded", dgvMain.Rows.Count)));
        }

        private void PageBack()
        {
            if (results.Count > 1)
            {
                results.Pop();
                tsbLoadRecordId.Enabled = false;
                if (results.Peek().EntityRecordId != null && results.Peek().EntityRecordId != Guid.Empty)
                {
                    tstbRecordId.Text = results.Peek().EntityRecordId.ToString();
                    tsbLoadRecordId.Enabled = true;
                }

                PaintResults();
            }
        }

        #endregion



        public ctlCRMViewer()
        {
            InitializeComponent();
            results = new Stack<Result>();
        }




        #region Navigation
        private void NavigateIn(int row)
        {
                        tstbRecordId.Text = string.Empty;
            WorkAsyncInfo wai = new WorkAsyncInfo
            {
                Message = "Retrieving info from CRM...",
                Work = (worker, args) =>
                {
                    Result result = null;

                    switch (results.Peek().DataType)
                    {
                        case Result.ResultType.EntityList:
                            result = Browser.GetEntityResult(Service, cache, dgvMain.Rows[row].Cells[0].Value.ToString(), worker);
                            break;

                        case Result.ResultType.Entity:
                            string datatype = dgvMain.Rows[row].Cells[4].Value.ToString();
                            if (datatype == "Picklist" || datatype == "State" || datatype == "Status")
                                result = Browser.GetPicklistResult(Service, results.Peek().EntityLogicalName, dgvMain.Rows[row].Cells[0].Value.ToString(), worker);
                            else if (datatype == "Lookup")
                                result = Browser.GetEntityResult(Service, cache, dgvMain.Rows[row].Cells[5].Value.ToString(), worker);
                            break;

                        case Result.ResultType.PickList:

                            break;

                        case Result.ResultType.Record:
                            string cell3val = (string)dgvMain.Rows[row].Cells[3].Value;
                            if (cell3val == "Lookup")
                            {
                                string entRef = (string)dgvMain.Rows[row].Cells[4].Value;
                                entRef = entRef.Substring(entRef.IndexOf("(") + 1);
                                entRef = entRef.Substring(0, entRef.Length - 1);
                                string[] vals = entRef.Split(':');

                                result = Browser.GetRecordResult(Service, cache, vals[0], new Guid(vals[1]), worker);
                                result.Header = string.Format("{0}{{{1}}}", vals[0], vals[1]);
                            }
                            break;

                        default:
                            break;
                    }
                    if (result != null)
                    {
                        results.Push(result);
                        if (!result.FromCache)
                            SaveCache();
                    }

                },
                ProgressChanged = ProgressChanged,
                PostWorkCallBack = NewResultsAvailable,
                AsyncArgument = null,
                MessageHeight = 150,
                MessageWidth = 340
            };
            WorkAsync(wai);
        }

        private void Refresh()
        {
            Result r = results.Peek();
            Result newResult = null;
            WorkAsyncInfo wai = new WorkAsyncInfo
            {
                Message = "Retrieving info from CRM...",
                Work = (worker, args) =>
                {

                    switch (r.DataType)
                    {
                        case Result.ResultType.EntityList:
                            newResult = Browser.GetEntityListResult(Service, worker);
                            newResult.SearchText = r.SearchText;
                            break;
                        case Result.ResultType.Entity:
                            cache.Remove(r.Key);
                            newResult = Browser.GetEntityResult(Service, cache, r.Key, worker);
                            newResult.SearchText = r.SearchText;
                            break;
                        case Result.ResultType.PickList:
                            break;
                        case Result.ResultType.Record:
                            Guid rid = results.Peek().EntityRecordId;
                            string entln = results.Peek().EntityLogicalName;
                            newResult = Browser.GetRecordResult(Service, cache, entln, rid, worker);
                            newResult.Header = string.Format("{0}{{{1}}}", entln, rid);

                            break;
                        default:
                            break;
                    }
                    if (newResult != null)
                    {
                        results.Pop();
                        results.Push(newResult);
                    }
                },
                ProgressChanged = ProgressChanged,
                PostWorkCallBack = NewResultsAvailable,
                AsyncArgument = null,
                MessageHeight = 150,
                MessageWidth = 340
            };
            WorkAsync(wai);
        }

        private void tsbLoadNewest_Click(object sender, EventArgs e)
        {
            tstbRecordId.Text = OpenLatest(false);
        }

        private void tsbLoadLatest_Click(object sender, EventArgs e)
        {
            tstbRecordId.Text = OpenLatest(true);
        }

        private void tsbLoadRecordId_Click(object sender, EventArgs e)
        {
            OpenRecord();
        }

        private void tstbRecordId_TextChanged(object sender, EventArgs e)
        {
            Guid guid = Guid.Empty;
            tsbLoadRecordId.Enabled = Guid.TryParse(tstbRecordId.Text, out guid);
        }


        #endregion

    }
}