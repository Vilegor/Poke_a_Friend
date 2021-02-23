using GameData;

namespace CarGame
{
    public class CarCompetitorModel : CompetitorModel
    {
        private readonly CarCompetitorComponent _carView;

        public int SkinId
        {
            get => _carView.skinId;
            set => _carView.skinId = value;
        } 
        
        public CarCompetitorModel(int id, CarCompetitorComponent view) : base(id, view)
        {
            _carView = view;
        }
    }
}