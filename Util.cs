using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMViewerPlugin
{
    static public class Util
    {
        internal static string CleanForCSName(string value)
        {
            string retVal = string.Empty;
            for (int i = 0; i < value.Length; i++)
                if ("abcdefghijklmnopqrstuvwxyz0123456789_".Contains(value.Substring(i, 1).ToLower()))
                    retVal += value.Substring(i, 1);
                else
                    retVal += "_";

            if ("1234567890".Contains(retVal.Substring(0, 1)))
                retVal = "_" + retVal;

            return retVal;
        }

        internal static object ConvertToCSType(string CRMType)
        {
            string retVal = CRMType;
            switch (CRMType)
            {
                case "Virtual": retVal = "object"; break;
                case "Uniqueidentifier": retVal = "Guid"; break;
                case "Decimal": retVal = "decimal"; break;
                case "Double": retVal = "double"; break;
                case "Money":
                case "DateTime":
                    break;
                case "Lookup": retVal = "EntityReference"; break;
                case "Boolean": retVal = "bool"; break;
                case "Integer": retVal = "int"; break;
                case "String":
                case "Memo":
                    retVal = "string"; break;
                case "Picklist":
                case "Status":
                    retVal = "OptionSetValue"; break;
                default:
                    System.Diagnostics.Debug.WriteLine(CRMType);
                    break;
            }

            return retVal;
        }
    }
}
