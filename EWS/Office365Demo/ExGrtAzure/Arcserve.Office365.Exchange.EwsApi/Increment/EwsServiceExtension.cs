using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.EwsApi.Increment
{
    public static class EwsServiceExtension
    {
        public static string GetDetailInformation(this ServiceResponse response)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Response details: errorMsg:").Append(response.ErrorMessage).Append(" errorCode:").Append(response.ErrorCode).Append(" result:").Append(response.Result.ToString()).Append(" errorDetais:");
            if(response.ErrorDetails != null && response.ErrorDetails.Count > 0)
            {
                int index = 0;
                foreach(var item in response.ErrorDetails)
                {
                    sb.Append(" [").Append(index++).Append("]: key:").Append(item.Key).Append(" value:").Append(item.Value);
                }
            }

            sb.Append(" errorProperties:");
            if(response.ErrorProperties != null && response.ErrorProperties.Count > 0)
            {
                int index = 0;
                foreach(var item in response.ErrorProperties)
                {
                    sb.Append(" [").Append(index++).Append("]:").Append(item.Type.FullName).Append(" strProperty:").Append(item.ToString());
                }
            }
            return sb.ToString();
        }
    }
}
