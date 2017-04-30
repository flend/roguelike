namespace RogueBasin
{
    public enum CustomInputArgsActions
    {
        MouseMoveToCurrentLocation
    }

    public class CustomInputArgs
    {
        public readonly CustomInputArgsActions action;

        public CustomInputArgs(CustomInputArgsActions action)
        {
            this.action = action;
        }
    }
}
