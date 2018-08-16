using CodeSanook.AppRelease.Models;
using Orchard.Events;

namespace CodeSanook.AppRelease.Handlers
{
    public interface IAppDownloadPartEventHandler:IEventHandler
    {
        void OnInitialized(AppDownloadPart part);
    }
}