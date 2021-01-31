namespace GameData
{
    public class ActionRequest
    {
        public int PlayerId { get; }
        public ActionType Type { get; }
        public float Time { get; }

        public ActionRequest(int playerId, ActionType type, float time = 0.0f)
        {
            PlayerId = playerId;
            Type = type;
            Time = time;
        }
        
        public int CompareTo(ActionRequest c)
        {
            if (Time < c.Time)
            {
                return 1;
            }
            if (Time > c.Time)
            {
                return -1;
            }

            return 0;
        }
    }
}