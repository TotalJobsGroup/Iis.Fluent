using System.Linq;
using Microsoft.Web.Administration;

namespace IisConfiguration
{
    /// <summary>
    /// 
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bindingCollection"></param>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public static Binding ByProtocol(this BindingCollection bindingCollection, string protocol)
        {
            return bindingCollection.Where(x => x.Protocol == protocol).FirstOrDefault();
        }
    }
}