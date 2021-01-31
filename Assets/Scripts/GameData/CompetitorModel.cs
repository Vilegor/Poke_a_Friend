namespace GameData
{
    public class CompetitorModel
    {
        private int _id;
        private BaseCompetitorComponent _view;

        public BaseCompetitorComponent View => _view;

        public CompetitorModel(int id, BaseCompetitorComponent view)
        {
            _id = id;
            _view = view;
        }

        public void Reset(int startLevel, int skinId, bool isPlayer = false)
        {
            _view.currentLevel = startLevel;
            _view.skinId = skinId;
            _view.isPlayer = isPlayer;
            _view.isWinner = false;
        }
    }
}