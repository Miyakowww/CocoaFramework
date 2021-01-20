using CocoaFramework.Model;

namespace CocoaFramework.Core.Route
{
    internal interface IRouteInfo
    {
        public bool Run(MessageSource src, QMessage msg);
    }
}
