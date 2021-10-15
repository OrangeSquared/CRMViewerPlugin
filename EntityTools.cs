using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CRMViewerPlugin
{
    public static class EntityTools
    {
        private const string ClassHeaderTemplate = @"
        public {0}()
        {{
            LogicalName = ""{0}"";
        }}

        private void SetOptionSetValue(string attributeLogicalName, object value)
        {{
            if ((int)value == -1)
                this[attributeLogicalName] = null;
            else
                this[attributeLogicalName] = new OptionSetValue((int)value);
        }}

        private int GetOptionSetValue(string attributeLogicalName)
        {{
            if (this.Contains(attributeLogicalName))
                return this.GetAttributeValue<OptionSetValue>(attributeLogicalName).Value;
            else
                return -1;
        }}
";
        private const string FakerTemplate = @"
        public static Faker<{0}> GetFaker(IOrganizationService service, MasterData masterData)
        {{
            Faker faker = new Faker();
            return new Faker<{0}>()
{1}
                ;
        }}
";
        private const string OptionSetPropertyTemplate = @"public {0}_{1}_values {1} {{ get {{ return ({0}_{1}_values)GetOptionSetValue(""{1}""); }} set {{ SetOptionSetValue(""{1}"", value); }} }}";
        private const string StringPropertyTemplate = @"        public {1} {0}
        {{
            get {{ return this.GetAttributeValue<{1}>(""{0}""); }}
            set {{ this[""{0}""] = value; }}
        }}
";
        private const string GenericPropertyTemplate = @"        public {1}? {0}
        {{
            get {{ return this.GetAttributeValue<{1}>(""{0}""); }}
            set
            {{
                if (value.HasValue)
                    this[""{0}""] = value.Value;
                else
                    this[""{0}""] = null;
            }}
        }}
";

        internal static void LoadFromCache(Result result, string cacheString)
        {
            byte[] ba = Encoding.ASCII.GetBytes(cacheString);
            MemoryStream memoryStream = new MemoryStream(ba);
            result.Data = new DataSet();
            result.Data.ReadXml(memoryStream);
            memoryStream.Close();
            memoryStream.Dispose();
        }

        internal static string AnalyzeEntity(IOrganizationService service, BackgroundWorker worker, string entityLogicalName)
        {
            System.Collections.Specialized.ListDictionary counts = new System.Collections.Specialized.ListDictionary();

            //get list of all columns
            RetrieveEntityRequest request = new RetrieveEntityRequest()
            {
                EntityFilters = EntityFilters.Attributes,
                RetrieveAsIfPublished = true,
                LogicalName = entityLogicalName
            };
            RetrieveEntityResponse response = (RetrieveEntityResponse)service.Execute(request);

            //create KVPair list of columns
            foreach (AttributeMetadata am in response.EntityMetadata.Attributes)
                counts.Add(am.LogicalName, 0);

            //get total record count
            int totalRecordCount = 0;
            string xmlCount = string.Format(@"<fetch aggregate='true'>
                  <entity name='{0}'>
                    <attribute name='{0}id' alias='count' aggregate='count' />
                  </entity>
                </fetch>", entityLogicalName);
            EntityCollection ecc = service.RetrieveMultiple(new FetchExpression(xmlCount));
            totalRecordCount = (int)ecc.Entities[0].GetAttributeValue<AliasedValue>("count").Value;


            //get records in batches
            int pos = 0;
            bool moreRecords = false;
            int page = 1;
            string cookie = string.Empty;
            //List<Entity> Entities = new List<Entity>();
            do
            {
                string fetchXml = string.Format(@"<fetch {0}>
                                      <entity name='contact'>
                                        <all-attributes />
                                      </entity>
                                    </fetch>", cookie);
                EntityCollection collection = service.RetrieveMultiple(new FetchExpression(fetchXml));

                if (collection.Entities.Count >= 0)
                {
                    //Entities.AddRange(collection.Entities);
                    foreach (Entity ee in collection.Entities)
                    {
                        foreach (string att in ee.Attributes.Keys)
                            counts[att] = (int)counts[att] + 1;
                        worker.ReportProgress((int)(100.0 * ((double)pos++ / (double)totalRecordCount)));
                        System.Threading.Thread.Sleep(1);
                    }
                }

                moreRecords = collection.MoreRecords;
                if (moreRecords)
                {
                    page++;
                    cookie = string.Format("paging-cookie='{0}' page='{1}'", System.Security.SecurityElement.Escape(collection.PagingCookie), page);
                }
            } while (moreRecords);

            //report results
            StringBuilder sb = new StringBuilder();
            foreach (string key in counts.Keys)
                sb.AppendLine(string.Format("{0}\t{1}", key, (int)counts[key]));

            worker.ReportProgress(100);

            return sb.ToString();
        }

        internal static string CreateEntityClass(IOrganizationService Service, BackgroundWorker worker, string entityLogicalName)
        {
            List<Tuple<string, string, string>> attributes = Browser.GetEntityAttributes(Service, entityLogicalName);

            string header = string.Format(ClassHeaderTemplate, entityLogicalName);
            StringBuilder sbFaker = new StringBuilder();
            StringBuilder sbEnums = new StringBuilder();
            StringBuilder sbAttributes = new StringBuilder();

            attributes.Sort((x, y) => x.Item1.CompareTo(y.Item1));

            sbEnums.AppendLine("        #region enums");
            foreach (Tuple<string, string, string> att in attributes)
                if (att.Item2 == "Picklist" || att.Item2 == "Status")
                {
                    List<Tuple<string, int>> ovs = Browser.GetPicklistValues(Service, entityLogicalName, att.Item1);
                    sbEnums.AppendFormat("public enum {0}_{1}_values\r\n{{\r\n", entityLogicalName, att.Item1);
                    foreach (Tuple<string, int> ov in ovs)
                        sbEnums.AppendFormat("{0} = {1},\r\n", Util.CleanForCSName(ov.Item1), ov.Item2);
                    sbEnums.AppendLine("}");
                }
            sbEnums.AppendLine("        #endregion");

            sbAttributes.AppendLine("        #region attributes");
            foreach (Tuple<string, string, string> att in attributes)
                switch (att.Item2)
                {
                    case "Picklist":
                    case "Status":
                        sbAttributes.AppendLine(string.Format(OptionSetPropertyTemplate, entityLogicalName, Util.CleanForCSName(att.Item1), Util.ConvertToCSType(att.Item2)));
                        sbFaker.AppendFormat("                .RuleFor(u => u.{0}, f => f.PickRandom<{1}_{0}_values>())\r\n", att.Item1, entityLogicalName);
                        break;

                    case "String":
                        sbAttributes.AppendFormat(StringPropertyTemplate, Util.CleanForCSName(att.Item1), Util.ConvertToCSType(att.Item2));
                        sbFaker.AppendFormat("                .RuleFor(u => u.{0}, f => f.Random.AlphaNumeric(10))\r\n", att.Item1);
                        break;

                    case "Memo":
                        sbAttributes.AppendFormat(StringPropertyTemplate, Util.CleanForCSName(att.Item1), Util.ConvertToCSType(att.Item2));
                        sbFaker.AppendFormat("                .RuleFor(u => u.{0}, f => f.Lorem.Paragraphs(2))\r\n", att.Item1);
                        break;

                    case "Money":
                        sbAttributes.AppendFormat(StringPropertyTemplate, Util.CleanForCSName(att.Item1), Util.ConvertToCSType(att.Item2));
                        sbFaker.AppendFormat("                .RuleFor(u => u.{0}, f => new Money(25.0m * (decimal)f.Random.Int(1, 200)))\r\n", att.Item1);
                        break;

                    case "Lookup":
                        sbAttributes.AppendFormat(StringPropertyTemplate, Util.CleanForCSName(att.Item1), Util.ConvertToCSType(att.Item2));
                        sbFaker.AppendFormat("                .RuleFor(u => u.{0}, f => f.PickRandom<EntityReference>(masterData.EntityReferences(\"{0}\")))\r\n", att.Item1);
                        break;

                    case "Virtual":
                        sbAttributes.AppendFormat(StringPropertyTemplate, Util.CleanForCSName(att.Item1), Util.ConvertToCSType(att.Item2));
                        break;

                    case "Boolean":
                        sbAttributes.AppendFormat(GenericPropertyTemplate, Util.CleanForCSName(att.Item1), Util.ConvertToCSType(att.Item2));
                        sbFaker.AppendFormat("                .RuleFor(u => u.{0}, f => f.Random.Bool())\r\n", att.Item1);
                        break;

                    case "Integer":
                        sbAttributes.AppendFormat(GenericPropertyTemplate, Util.CleanForCSName(att.Item1), Util.ConvertToCSType(att.Item2));
                        sbFaker.AppendFormat("                .RuleFor(u => u.{0}, f => f.Random.Int(0, 100))\r\n", att.Item1);
                        break;

                    case "Decimal":
                        sbAttributes.AppendFormat(GenericPropertyTemplate, Util.CleanForCSName(att.Item1), Util.ConvertToCSType(att.Item2));
                        sbFaker.AppendFormat("                .RuleFor(u => u.{0}, f => f.Random.Decimal(0.0, 1.0))\r\n", att.Item1);
                        break;

                    case "DateTime":
                        sbAttributes.AppendFormat(GenericPropertyTemplate, Util.CleanForCSName(att.Item1), Util.ConvertToCSType(att.Item2));
                        sbFaker.AppendFormat("                .RuleFor(u => u.{0}, f => f.Date.Past(10))\r\n", att.Item1);
                        break;

                    case "Uniqueidentifier":
                        sbAttributes.AppendFormat(GenericPropertyTemplate, Util.CleanForCSName(att.Item1), Util.ConvertToCSType(att.Item2));
                        break;

                    default:
                        sbAttributes.AppendFormat(GenericPropertyTemplate, Util.CleanForCSName(att.Item1), Util.ConvertToCSType(att.Item2));
                        sbFaker.AppendFormat("                .RuleFor(u => u.{0}, f => null) // {1}/{2} field\r\n", att.Item1, att.Item2, Util.ConvertToCSType(att.Item2));
                        break;
                }
            sbAttributes.AppendLine("        #endregion");

            return string.Format(FakerTemplate, entityLogicalName, sbFaker.ToString()) + header +  sbEnums.ToString() + sbAttributes.ToString();
        }
    }
}
