namespace USITools
{
    public interface IEfficiencyBonusConsumer
    {
        float GetEfficiencyBonus();
        void SetEfficiencyBonus(string bonName, float bonValue);
    }
}