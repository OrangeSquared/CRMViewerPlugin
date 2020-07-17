using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using NuGet;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Description;
using System.Windows.Forms;
using System.Xml;
using XrmToolBox.Extensibility;

namespace CRMViewerPlugin
{
    class Browser : Result
    {
        public ctlCRMViewer control { get; set; }

        public bool ShowDefaultAttributes
        {
            get
            {
                bool retVal = true;
                if (control != null)
                    retVal = control.SettingValue("showDefaultAttributes") == "true";

                return retVal;
            }
        }

        private string[] defaultAttributes = new string[]
        {
            "createdby",
            "createdon",
            "createdonbehalfby",
            "importsequencenumber",
            "modifiedby",
            "modifiedon",
            "modifiedonbehalfby",
            "overriddencreatedon",
            "ownerid",
            "owningbusinessunit",
            "owningteam",
            "owninguser",
            "timezoneruleversionnumber",
            "utcconversiontimezonecode",
            "versionnumber",
            "statecode",
        };

        public enum SelectionType
        {
            EntityList, Entity, PickList,
            Record
        }
        public string EntityLogicalName { get; set; }
        public Guid EntityRecordId { get; set; }
        public string PicklistLogicalName { get; set; }

        private List<Tuple<string, string, int, string>> _optionSetValues;


        public override string Header => currentSelection;

        private Dictionary<string, string> _cache;
        internal Dictionary<string, string> cache
        {
            get
            {
                if (_cache == null)
                {
                    _cache = new Dictionary<string, string>();
                    string[] input;
                    if (SettingsManager.Instance.TryLoad(GetType(), out input, ((Microsoft.Xrm.Tooling.Connector.CrmServiceClient)service).ConnectedOrgUniqueName + "_cache"))
                        for (int i = 0; i < input.GetUpperBound(0); i += 2)
                            _cache.Add(input[i], input[i + 1]);
                }
                return _cache;
            }
        }

        internal void SaveToCache()
        {
            if (cache.ContainsKey(EntityLogicalName))
                cache.Remove(EntityLogicalName);


            MemoryStream memoryStream = new MemoryStream(500000);
            Data.WriteXml(memoryStream, XmlWriteMode.WriteSchema);
            memoryStream.Flush();
            memoryStream.Position = 0;
            string table = memoryStream.ReadToEnd();
            cache.Add(EntityLogicalName, table);
            memoryStream.Close();
            memoryStream.Dispose();
        }

        internal void LoadFromCache()
        {
            byte[] ba = Encoding.ASCII.GetBytes(cache[EntityLogicalName]);
            MemoryStream memoryStream = new MemoryStream(ba);
            Data = new DataTable();
            Data.ReadXml(memoryStream);
            memoryStream.Close();
            memoryStream.Dispose();
        }

        internal void SaveCache()
        {
            List<string> output = new List<string>();
            foreach (string s in cache.Keys)
            {
                output.Add(s);
                output.Add(cache[s]);
            }

            SettingsManager.Instance.Save(GetType(), output.ToArray(), ((Microsoft.Xrm.Tooling.Connector.CrmServiceClient)service).ConnectedOrgUniqueName + "_cache");
        }


        private string currentSelection = "Entities";
        public SelectionType currentSelectionType { get; set; }

        public override string Status => throw new NotImplementedException();

        public Browser(IOrganizationService Service)
        {
            service = Service;
        }

        internal void LoadEntitiesList(System.ComponentModel.BackgroundWorker worker)
        {
            currentSelectionType = SelectionType.EntityList;

            Data = new DataTable();
            Data.Columns.AddRange(new DataColumn[]
            {
                        new DataColumn("Key"),
                        new DataColumn("Logical Name"),
                        new DataColumn("Display Name")
            });
            Data.PrimaryKey = new DataColumn[] { Data.Columns["Key"] };

            RetrieveAllEntitiesRequest retrieveAllEntitiesRequest = new RetrieveAllEntitiesRequest()
            {
                EntityFilters = EntityFilters.Entity,
                RetrieveAsIfPublished = true
            };

            RetrieveAllEntitiesResponse retrieveAllEntitiesResponse = (RetrieveAllEntitiesResponse)service.Execute(retrieveAllEntitiesRequest);

            IEnumerable<EntityMetadata> subset = retrieveAllEntitiesResponse.EntityMetadata;

            int pos = 1;
            int max = subset.Count();
            Console.WriteLine(string.Format("\r\nLoading {0} entities", max));
            foreach (EntityMetadata e in subset)
            {
                if (e.IsCustomizable.Value && e.DisplayName.LocalizedLabels.Count > 0)
                {
                    DataRow newDR = Data.NewRow();
                    newDR[0] = e.LogicalName;
                    newDR[1] = e.LogicalName;
                    newDR[2] = e.DisplayName.LocalizedLabels.Count > 0 ? e.DisplayName.LocalizedLabels.First(x => x.LanguageCode == 1033).Label : e.LogicalName;
                    Data.Rows.Add(newDR);
                    //Util.ShowProgress(pos++, max);
                }
                worker.ReportProgress((int)(100 * ((double)pos++ / (double)max)));
            }
            Data.DefaultView.Sort = "Key ASC";
        }

        public void LoadEntityAttributes(string entityLogicalName, bool showDefaultAttributes, System.ComponentModel.BackgroundWorker worker)
        {
            //SettingsManager.Instance.TryLoad(GetType(),out )
            //SettingsManager.Instance.TryLoad(GetType(), out mySettings)
            currentSelectionType = SelectionType.Entity;
            EntityLogicalName = entityLogicalName;
            currentSelection = "Entity " + entityLogicalName;

            FromCache = cache.ContainsKey(entityLogicalName);
            if (FromCache)
            {
                LoadFromCache();
            }
            else
            {
                Data = new DataTable(EntityLogicalName);
                Data.Columns.AddRange(
                    new DataColumn[] {
                    new DataColumn("Key", typeof(string)),
                    new DataColumn("Logical Name", typeof(string)),
                    new DataColumn("Display Name", typeof(string)),
                    new DataColumn("Required", typeof(string)),
                    new DataColumn("Data Type", typeof(string)),
                    new DataColumn("Metadata", typeof(string))
                    });
                Data.PrimaryKey = new DataColumn[] { Data.Columns["Key"] };


                RetrieveEntityRequest request = new RetrieveEntityRequest()
                {
                    EntityFilters = EntityFilters.Attributes,
                    RetrieveAsIfPublished = true,
                    LogicalName = EntityLogicalName
                };
                RetrieveEntityResponse response = (RetrieveEntityResponse)service.Execute(request);

                int pos = 1;
                int max = response.EntityMetadata.Attributes.Count();

                string primaryKey = EntityLogicalName + "id";
                foreach (AttributeMetadata am in response.EntityMetadata.Attributes)
                {
                    if (am.DisplayName.LocalizedLabels.Count > 0 &&
                        (ShowDefaultAttributes || (!defaultAttributes.Contains(am.LogicalName))))
                    {
                        DataRow newDR = Data.NewRow();
                        newDR["Key"] = am.LogicalName;
                        newDR["Display Name"] = am.DisplayName.LocalizedLabels[0].Label;
                        newDR["Logical Name"] = am.LogicalName;
                        newDR["Data Type"] = am.AttributeType.ToString();
                        switch (am.RequiredLevel.Value)
                        {
                            case AttributeRequiredLevel.None: newDR["Required"] = string.Empty; break;
                            case AttributeRequiredLevel.SystemRequired: newDR["Required"] = "Req'd"; break;
                            case AttributeRequiredLevel.ApplicationRequired: newDR["Required"] = "Req'd"; break;
                            case AttributeRequiredLevel.Recommended: newDR["Required"] = string.Empty; break;
                            //case AttributeRequiredLevel.Recommended: newDR["Required"] = "Rec'd"; break;
                            default:
                                break;
                        }
                        switch (am.AttributeType.ToString())
                        {
                            case "Lookup":
                                RetrieveAttributeRequest rar = new RetrieveAttributeRequest()
                                {
                                    EntityLogicalName = EntityLogicalName,
                                    LogicalName = am.LogicalName,
                                    RetrieveAsIfPublished = true
                                };
                                RetrieveAttributeResponse rarr = (RetrieveAttributeResponse)service.Execute(rar);
                                newDR["Metadata"] = string.Join(", ", ((LookupAttributeMetadata)rarr.AttributeMetadata).Targets);
                                break;

                            case "Picklist":
                                RetrieveAttributeRequest rarpl = new RetrieveAttributeRequest()
                                {
                                    EntityLogicalName = EntityLogicalName,
                                    LogicalName = am.LogicalName,
                                    RetrieveAsIfPublished = true
                                };
                                RetrieveAttributeResponse rarplr = (RetrieveAttributeResponse)service.Execute(rarpl);
                                newDR["Metadata"] = ((PicklistAttributeMetadata)rarplr.AttributeMetadata).OptionSet.Name;
                                break;

                            default:
                                newDR["Metadata"] = string.Empty;
                                break;
                        }
                        Data.Rows.Add(newDR);

                        worker.ReportProgress((int)(100 * ((double)pos++ / (double)max)));
                    }
                }

                SaveToCache();
                SaveCache();
            }

            Data.DefaultView.Sort = "Key ASC";
        }

        internal void OpenInBrowser()
        {
            string rootURI = string.Format("https://{0}:{1}",
                ((Microsoft.Xrm.Tooling.Connector.CrmServiceClient)service).CrmConnectOrgUriActual.Host,
                ((Microsoft.Xrm.Tooling.Connector.CrmServiceClient)service).CrmConnectOrgUriActual.Port);
            string targetUri = null;
            switch (currentSelectionType)
            {
                case SelectionType.EntityList:
                    targetUri = rootURI;
                    break;
                case SelectionType.Entity:
                    targetUri = string.Format("{0}//main.aspx?etn={1}&pagetype=entitylist", rootURI, EntityLogicalName);
                    break;
                case SelectionType.Record: //              FUTURE
                    targetUri = string.Format("{0}//main.aspx?etn={1}&pagetype=entityrecord&id={2}", rootURI, EntityLogicalName, EntityRecordId);
                    break;

                default:
                    break;
            }
            System.Diagnostics.Process.Start(targetUri);
        }

        private void LoadPicklist(string entityLogicalName, string logicalName)
        {
            currentSelectionType = SelectionType.PickList;
            PicklistLogicalName = logicalName;

            Data = new DataTable();
            Data.Columns.AddRange(
                new DataColumn[] {
                    new DataColumn("Key", typeof(string)),
                    new DataColumn("Value", typeof(string)),
                    new DataColumn("Label", typeof(string)),
                });
            Data.PrimaryKey = new DataColumn[] { Data.Columns["Key"] };
            //need to detmine global or local

            RetrieveAttributeRequest rar = new RetrieveAttributeRequest()
            {
                EntityLogicalName = EntityLogicalName,
                LogicalName = logicalName,
                RetrieveAsIfPublished = true
            };
            RetrieveAttributeResponse rarr = (RetrieveAttributeResponse)service.Execute(rar);


            EntityLogicalName = entityLogicalName;
            currentSelection = string.Format("PickList {0}", logicalName);


            if (rarr.AttributeMetadata.GetType().Name == "PicklistAttributeMetadata")
                foreach (OptionMetadata om in ((PicklistAttributeMetadata)rarr.AttributeMetadata).OptionSet.Options)
                {
                    DataRow dr = Data.NewRow();
                    dr["Key"] = om.Value ?? 0;
                    dr["Value"] = om.Value ?? 0;
                    dr["Label"] = om.Label.LocalizedLabels[0].Label;
                    Data.Rows.Add(dr);
                }

            else if (rarr.AttributeMetadata.GetType().Name == "StateAttributeMetadata")
                foreach (OptionMetadata om in ((StateAttributeMetadata)rarr.AttributeMetadata).OptionSet.Options)
                {
                    DataRow dr = Data.NewRow();
                    dr["Key"] = om.Value ?? 0;
                    dr["Value"] = om.Value ?? 0;
                    dr["Label"] = om.Label.LocalizedLabels[0].Label;
                    Data.Rows.Add(dr);
                }

            else if (rarr.AttributeMetadata.GetType().Name == "StatusAttributeMetadata")
                foreach (OptionMetadata om in ((StatusAttributeMetadata)rarr.AttributeMetadata).OptionSet.Options)
                {
                    DataRow dr = Data.NewRow();
                    dr["Key"] = om.Value ?? 0;
                    dr["Value"] = om.Value ?? 0;
                    dr["Label"] = om.Label.LocalizedLabels[0].Label;
                    Data.Rows.Add(dr);
                }

            Data.DefaultView.Sort = "Value ASC";
        }

        internal void LoadRecord(string entityLogicalName, Guid recordId, bool customOnly, BackgroundWorker worker)
        {
            this.EntityLogicalName = entityLogicalName;
            this.currentSelectionType = SelectionType.Record;
            Data = new DataTable();

            Data.Columns.AddRange(
                new DataColumn[]
                {
                    new DataColumn("Key", typeof(string)),
                    new DataColumn("Logical Name", typeof(string)),
                    new DataColumn("Display Name", typeof(string)),
                    new DataColumn("Data Type", typeof(string)),
                    new DataColumn("Value", typeof(string)),
                    new DataColumn("Metadata", typeof(string)),
                });
            Data.PrimaryKey = new DataColumn[] { Data.Columns["Key"] };

            string fetchXml = null;

            if (recordId != Guid.Empty)
                fetchXml = string.Format(@"
                    <fetch top='1'>
                      <entity name='{0}'>
                        <filter>
                          <condition attribute='{0}id' operator='eq' value='{1}'/>
                        </filter>
                      </entity>
                    </fetch>", EntityLogicalName, recordId);
            else
                fetchXml = string.Format(@"
                    <fetch top='1'>
                      <entity name='{0}'>
                      </entity>
                    </fetch>", EntityLogicalName);

            EntityCollection ec = service.RetrieveMultiple(new FetchExpression(fetchXml));

            if (ec == null || ec.Entities.Count <= 0)
            {
                DataRow dr = Data.NewRow();
                dr[0] = "RNF";
                dr[1] = "Record";
                dr[2] = "not";
                dr[3] = "found.";
                Data.Rows.Add(dr);
            }
            else
            {
                Entity entity = ec[0];
                this.EntityRecordId = entity.Id;
                Browser entityResult = new Browser(service);
                entityResult.LoadEntityAttributes(entityLogicalName, customOnly, worker);

                //////// do sorting

                foreach (DataRow edr in entityResult.Data.Rows)
                {
                    DataRow dr = Data.NewRow();
                    dr[0] = edr[0];
                    dr[1] = edr[1];
                    dr[2] = edr[2];
                    dr[3] = edr[4];
                    if (entity.Contains((string)edr[1]))
                        dr[4] = EntityValueFormat(entity, (string)edr[1]);




                    switch (edr[4].ToString())
                    {
                        case "Lookup":
                            RetrieveAttributeRequest rar = new RetrieveAttributeRequest()
                            {
                                EntityLogicalName = EntityLogicalName,
                                LogicalName = edr[0].ToString(),
                                RetrieveAsIfPublished = true
                            };
                            RetrieveAttributeResponse rarr = (RetrieveAttributeResponse)service.Execute(rar);
                            dr["Metadata"] = string.Join(", ", ((LookupAttributeMetadata)rarr.AttributeMetadata).Targets);
                            break;

                        case "Picklist":
                            RetrieveAttributeRequest rarpl = new RetrieveAttributeRequest()
                            {
                                EntityLogicalName = EntityLogicalName,
                                LogicalName = edr[0].ToString(),
                                RetrieveAsIfPublished = true
                            };
                            RetrieveAttributeResponse rarplr = (RetrieveAttributeResponse)service.Execute(rarpl);
                            dr["Metadata"] = ((PicklistAttributeMetadata)rarplr.AttributeMetadata).OptionSet.Name;
                            break;

                        default:
                            dr["Metadata"] = string.Empty;
                            break;
                    }





                    //else
                    //  dr[2] = "<NULL>";
                    Data.Rows.Add(dr);
                }
            }
        }

        public override Result ProcessSelection(object sender, object selection, System.ComponentModel.BackgroundWorker worker)
        {
            Browser retVal = null;
            DataRow dataRow = Data.Rows.Find(selection);


            switch (currentSelectionType)
            {
                case SelectionType.EntityList:
                    retVal = new Browser(service);
                    retVal.EntityLogicalName = EntityLogicalName;
                    retVal.control = (ctlCRMViewer)sender;
                    retVal.LoadEntityAttributes((string)selection, ShowDefaultAttributes, worker);
                    break;

                case SelectionType.Entity:
                case SelectionType.Record:
                    string entityLogicalName = (string)selection;
                    string dataType = (string)dataRow["Data Type"];
                    switch (dataType)
                    {
                        case "Lookup":
                            retVal = new Browser(service);
                            retVal.EntityLogicalName = EntityLogicalName;
                            retVal.control = (ctlCRMViewer)sender;
                            retVal.LoadEntityAttributes((string)dataRow["Metadata"], ShowDefaultAttributes, worker);
                            break;

                        case "Picklist":
                        case "State":
                        case "Status":
                            retVal = new Browser(service);
                            retVal.EntityLogicalName = EntityLogicalName;
                            retVal.control = (ctlCRMViewer)sender;
                            string logicalName = (string)dataRow["Logical Name"];
                            string listName = (string)dataRow["Metadata"];
                            retVal.LoadPicklist(EntityLogicalName, logicalName);
                            break;
                    }
                    break;

                default:
                    break;
            }

            return retVal;
        }

        public override void Refresh(System.ComponentModel.BackgroundWorker worker)
        {
            if (cache.ContainsKey(EntityLogicalName))
                cache.Remove(EntityLogicalName);
            switch (currentSelectionType)
            {
                case SelectionType.EntityList: LoadEntitiesList(worker); break;
                case SelectionType.Entity: LoadEntityAttributes(EntityLogicalName, ShowDefaultAttributes, worker); break;
                case SelectionType.PickList: LoadPicklist(EntityLogicalName, PicklistLogicalName); break;
                case SelectionType.Record: LoadRecord(EntityLogicalName, EntityRecordId, ShowDefaultAttributes, worker); break;
                default:
                    break;
            }
        }

        internal string EntityValueFormat(Entity entity, string AttributeLogicalName)
        {
            switch (entity.Attributes[AttributeLogicalName].GetType().Name)
            {
                case "Int32": return entity.GetAttributeValue<Int32>(AttributeLogicalName).ToString(); break;
                case "String": return entity.GetAttributeValue<string>(AttributeLogicalName).Replace("\r\n", "<p>"); break;
                case "Boolean": return entity.GetAttributeValue<bool>(AttributeLogicalName) ? "True" : "False"; break;
                case "DateTime": return entity.GetAttributeValue<DateTime>(AttributeLogicalName).ToString("yyyy-MM-ddTHH:mm:ss"); break;
                case "Guid": return entity.GetAttributeValue<Guid>(AttributeLogicalName).ToString(); break;
                case "OptionSetValue":
                    return GetOptionSetValue(entity.LogicalName, AttributeLogicalName, entity.GetAttributeValue<OptionSetValue>(AttributeLogicalName).Value);
                    break;

                case "EntityReference":
                    EntityReference er = entity.GetAttributeValue<EntityReference>(AttributeLogicalName);
                    return string.Format("{0} ({1}:{2})", er.Name, er.LogicalName, er.Id);
                    break;

                case "Money":
                    return entity.GetAttributeValue<Money>(AttributeLogicalName).Value.ToString("c");

                default:
                    return entity[AttributeLogicalName].GetType().Name;
                    break;
            }
        }

        public string GetOptionSetValue(string Entity, string Attribute, int Value)
        {
            if (_optionSetValues == null) _optionSetValues = new List<Tuple<string, string, int, string>>();

            if (!_optionSetValues.Any(x => (x.Item1 == Entity && x.Item2 == Attribute && x.Item3 == Value)))
            {
                RetrieveAttributeRequest rar = new RetrieveAttributeRequest()
                {
                    EntityLogicalName = Entity,
                    LogicalName = Attribute,
                    RetrieveAsIfPublished = true
                };
                RetrieveAttributeResponse rarr = (RetrieveAttributeResponse)service.Execute(rar);

                OptionMetadata op = null;
                if (rarr.AttributeMetadata.GetType().Name == "PicklistAttributeMetadata")
                    if (((PicklistAttributeMetadata)rarr.AttributeMetadata).OptionSet.Options.Count > 0)
                        op = ((PicklistAttributeMetadata)rarr.AttributeMetadata).OptionSet.Options.First(x => x.Value == Value);

                if (rarr.AttributeMetadata.GetType().Name == "StateAttributeMetadata")
                    op = ((StateAttributeMetadata)rarr.AttributeMetadata).OptionSet.Options.First(x => x.Value == Value);

                if (rarr.AttributeMetadata.GetType().Name == "StatusAttributeMetadata")
                    op = ((StatusAttributeMetadata)rarr.AttributeMetadata).OptionSet.Options.First(x => x.Value == Value);

                if (op != null)
                    _optionSetValues.Add(new Tuple<string, string, int, string>(Entity, Attribute, Value, op.Label.LocalizedLabels[0].Label));
                else
                    _optionSetValues.Add(new Tuple<string, string, int, string>(Entity, Attribute, Value, "UNKNOWN"));
            }

            Tuple<string, string, int, string> record = _optionSetValues.Find(x => (x.Item1 == Entity && x.Item2 == Attribute && x.Item3 == Value));
            return string.Format("({0}) {1}", record.Item3, record.Item4);
        }

        public override MenuItem[] GetContextMenu(object selection)
        {
            switch (currentSelectionType)
            {
                case SelectionType.EntityList:
                    return null;
                    break;
                case SelectionType.Entity:
                    MenuItem miCreateClass = new MenuItem("C# Class");
                    miCreateClass.Click += MiCreateClass_Click;
                    return new MenuItem[] { miCreateClass };
                    break;
                case SelectionType.PickList:
                    MenuItem miMakeUnum = new MenuItem("Copy Enum");
                    miMakeUnum.Click += miMakeUnum_Click;
                    return new MenuItem[] { miMakeUnum };
                    break;
                case SelectionType.Record:
                    return null;
                    break;
                default:
                    return null;
                    break;
            }
        }

        private void MiCreateClass_Click(object sender, EventArgs e)
        {
            StringBuilder header = new StringBuilder();
            StringBuilder body = new StringBuilder();
            header.Append(string.Format(@"using Microsoft.Xrm.Sdk;
using System;

namespace CHANGETHIS
{{
    public class {0}
    {{
", EntityLogicalName));

            foreach (DataRow dr in Data.Rows)
            {                
                switch (dr["Data type"].ToString())
                {
                    case "String":
                    case "Memo":
                        body.AppendLine(string.Format("        public string {0} {{ get; set; }}", dr["Logical Name"].ToString()));
                        break;

                    case "Boolean":
                        body.AppendLine(string.Format("        public bool {0} {{ get; set; }}", dr["Logical Name"].ToString()));
                        break;

                    case "DateTime":
                        body.AppendLine(string.Format("        public DateTime {0} {{ get; set; }}", dr["Logical Name"].ToString()));
                        break;

                    case "Decimal":                        
                        body.AppendLine(string.Format("        public decimal {0} {{ get; set; }}", dr["Logical Name"].ToString()));
                        break;

                    case "Double":
                        body.AppendLine(string.Format("        public double {0} {{ get; set; }}", dr["Logical Name"].ToString()));
                        break;

                    case "Integer":
                        body.AppendLine(string.Format("        public int {0} {{ get; set; }}", dr["Logical Name"].ToString()));
                        break;

                    case "Money":
                        body.AppendLine(string.Format("        public Money {0} {{ get; set; }}", dr["Logical Name"].ToString()));
                        break;

                    case "UniqueIdentifier":
                        body.AppendLine(string.Format("        public Guid {0} {{ get; set; }}", dr["Logical Name"].ToString()));
                        break;

                    case "Picklist":
                    case "State":
                    case "Status":
                        header.AppendLine(UnimFromPicklist(dr["Logical Name"].ToString()));
                        body.AppendLine(string.Format("        public {0}Values {0} {{ get; set; }}", dr["Logical Name"].ToString()));
                        break;

                    case "Lookup":
                        body.AppendLine(string.Format("        public EntityReference {0} {{ get; set; }}", dr["Logical Name"].ToString()));
                        break;

                    default:
                        break;
                }
                //new DataColumn("Logical Name", typeof(string)),
                //    new DataColumn("Display Name", typeof(string)),
                //    new DataColumn("Required", typeof(string)),
                //    new DataColumn("Data Type", typeof(string)),
                //    new DataColumn("Metadata", typeof(string))
            }

            body.Append("}\r\n}");
            Clipboard.SetText(header.ToString() + body.ToString());
        }

        private void miMakeUnum_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("enum {0}\r\n{{", PicklistLogicalName));
            foreach (DataRow dr in Data.Rows)
                sb.AppendLine(string.Format("\t{0} = {1},", dr["Label"].ToString().Replace(" ","_"), dr["value"]));
            sb.AppendLine("}");
            Clipboard.SetText(sb.ToString());
        }

        private string UnimFromPicklist(string AttributeName)
        {
            RetrieveAttributeRequest rar = new RetrieveAttributeRequest()
            {
                EntityLogicalName = EntityLogicalName,
                LogicalName = AttributeName,
                RetrieveAsIfPublished = true
            };
            RetrieveAttributeResponse rarr = (RetrieveAttributeResponse)service.Execute(rar);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("public enum {0}Values\r\n{{", AttributeName));

            if (rarr.AttributeMetadata.GetType().Name == "PicklistAttributeMetadata")
                foreach (OptionMetadata om in ((PicklistAttributeMetadata)rarr.AttributeMetadata).OptionSet.Options)
                    sb.AppendLine(string.Format("\t{0} = {1},", om.Label.LocalizedLabels[0].Label.Replace(" ", "_"), om.Value ?? 0));

            else if (rarr.AttributeMetadata.GetType().Name == "StateAttributeMetadata")
                foreach (OptionMetadata om in ((StateAttributeMetadata)rarr.AttributeMetadata).OptionSet.Options)
                    sb.AppendLine(string.Format("\t{0} = {1},", om.Label.LocalizedLabels[0].Label.Replace(" ", "_"), om.Value ?? 0));

            else if (rarr.AttributeMetadata.GetType().Name == "StatusAttributeMetadata")
                foreach (OptionMetadata om in ((StatusAttributeMetadata)rarr.AttributeMetadata).OptionSet.Options)
                    sb.AppendLine(string.Format("\t{0} = {1},", om.Label.LocalizedLabels[0].Label.Replace(" ", "_"), om.Value ?? 0));

            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
