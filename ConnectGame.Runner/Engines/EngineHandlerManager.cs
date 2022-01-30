using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ConnectGame.Runner.Engines
{
    class EngineHandlerManager : IEngineHandlerManager
    {
        private readonly HashSet<EngineHandler> _handlers;

        public EngineHandlerManager()
        {
            _handlers = new HashSet<EngineHandler>();
        }

        public Task AddHandlerAsync(Func<EngineHandlerParameters, Task> action)
        {
            var handler = new EngineHandler(action);
            return AddHandlerAsync(handler);
        }

        public Task AddConditionAsync(Func<string, Task<bool>> action)
        {
            var handler = new EngineHandler(async parameters =>
            {
                var actionResult = await action.Invoke(parameters.Message);
                if (actionResult)
                {
                    parameters.IsCompleted = true;
                }
            });

            return AddHandlerAsync(handler);
        }

        private Task AddHandlerAsync(EngineHandler handler)
        {
            _handlers.Add(handler);
            return handler.Completion.Task;
        }

        public async Task RunHandlersAsync(string message)
        {
            var parameters = new EngineHandlerParameters(message);
            var handlerList = _handlers.ToList();
            foreach (var handler in handlerList)
            {
                await handler.HandleAction.Invoke(parameters);
                if (parameters.IsCompleted)
                {
                    _handlers.Remove(handler);
                    handler.Completion.SetResult();
                    return;
                }

                if (parameters.IsHandled)
                {
                    return;
                }
            }
        }
    }
}
