using Codesanook.AppRelease.Models;
using Orchard.Events;

namespace Codesanook.AppRelease.Handlers
{
    public interface IAppDownloadPartEventHandler:IEventHandler
    {
        void OnInitialized(AppDownloadPart part);
    }
}