using System;
using System.Threading.Tasks;

namespace ConnectGame.Runner.Engines
{
    class EngineHandler
    {
        public Func<EngineHandlerParameters, Task> HandleAction { get; }
        public TaskCompletionSource Completion { get; }

        public EngineHandler(Func<EngineHandlerParameters, Task> handleAction)
        {
            HandleAction = handleAction;
            Completion = new TaskCompletionSource();
        }
    }
}