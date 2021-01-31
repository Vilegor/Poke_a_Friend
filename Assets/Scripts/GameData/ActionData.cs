namespace GameData
{
    public class ActionData
    {
        public ActionType Type { get; }
        public int Value { get; }
        public float Cooldown { get; }

        public ActionData(ActionType type, int value, float cooldown)
        {
            Type = type;
            Value = value;
            Cooldown = cooldown;
        }
    }
}