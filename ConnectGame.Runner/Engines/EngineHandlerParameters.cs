namespace ConnectGame.Runner.Engines
{
    class EngineHandlerParameters
    {
        public bool IsHandled { get; set; }
        public bool IsCompleted { get; set; }
        public string Message { get; set; }
        //public IReadOnlyList<string> PreviousMessages { get; set; }

        public EngineHandlerParameters(string message)
        {
            Message = message;
        }
    }
}