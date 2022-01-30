using System;
using System.Threading.Tasks;

namespace ConnectGame.Runner.Engines
{
    internal interface IEngineHandlerManager
    {
        Task AddHandlerAsync(Func<EngineHandlerParameters, Task> action);
        Task AddConditionAsync(Func<string, Task<bool>> action);
        Task RunHandlersAsync(string message);
    }
}