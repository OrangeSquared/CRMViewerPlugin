using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMViewerPlugin
{
    public static class OptionSetTools
    {
        const string OptionSetFormat = "({value}) {label}";

        public static string GetOptionSetValue(IOrganizationService service, string Entity, string Attribute, int Value, Dictionary<string, string> cache)
        {

            string retVal = "UNKNOWN.";
            string key = string.Format("{0}.{1}.{2}", Entity, Attribute, Value);

            if (!cache.ContainsKey(key))
            {
                RetrieveAttributeRequest rar = new RetrieveAttributeRequest()
                {
                    EntityLogicalName = Entity,
                    LogicalName = Attribute,
                    RetrieveAsIfPublished = true
                };
                RetrieveAttributeResponse rarr = (RetrieveAttributeResponse)service.Execute(rar);

                OptionSetMetadata metadata = null;

                if (rarr.AttributeMetadata.GetType().Name == "PicklistAttributeMetadata")
                    if (((PicklistAttributeMetadata)rarr.AttributeMetadata).OptionSet.Options.Count > 0)
                        //op = ((PicklistAttributeMetadata)rarr.AttributeMetadata).OptionSet.Options.First(x => x.Value == Value);
                        metadata = ((PicklistAttributeMetadata)rarr.AttributeMetadata).OptionSet;

                if (rarr.AttributeMetadata.GetType().Name == "StateAttributeMetadata")
                    //op = ((StateAttributeMetadata)rarr.AttributeMetadata).OptionSet.Options.First(x => x.Value == Value);
                    metadata = ((StateAttributeMetadata)rarr.AttributeMetadata).OptionSet;

                if (rarr.AttributeMetadata.GetType().Name == "StatusAttributeMetadata")
                    //op = ((StatusAttributeMetadata)rarr.AttributeMetadata).OptionSet.Options.First(x => x.Value == Value);
                    metadata = ((StatusAttributeMetadata)rarr.AttributeMetadata).OptionSet;

                if (metadata != null)
                {
                    string tkey = null;
                    foreach (OptionMetadata op in metadata.Options)
                    {
                        tkey = string.Format("{0}.{1}.{2}", Entity, Attribute, op.Value.Value);
                        if (!cache.ContainsKey(tkey))
                            cache.Add(tkey, OptionSetFormat
                                .Replace("{entity}", Entity)
                                .Replace("{attribute}", Attribute)
                                .Replace("{value}", Value.ToString())
                                .Replace("{label}", op.Label.LocalizedLabels[0].Label));
                    }
                }
            }


            if (cache.ContainsKey(key))
                retVal = cache[key];

            return retVal;
        }
    }
}
