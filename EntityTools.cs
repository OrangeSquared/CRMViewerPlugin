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

    }
}
